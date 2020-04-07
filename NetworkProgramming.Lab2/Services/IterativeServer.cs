using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkProgramming.Lab1;
using NetworkProgramming.Lab2.Models;

namespace NetworkProgramming.Lab2.Services
{
   public class IterativeServer
   {
      private ClientHandler _handler;
      private bool _activeClient;
      private bool _serving;
      private Socket _serverSocket;

      public event EventHandler<InternalMessageModel> OnLogEvent;
      public event EventHandler<ClientModel> OnNewClient;
      public event EventHandler OnDisconnect;

      public IterativeServer()
      {

      }

      public void StartService(string ip, int port, string interfaceName = "") => DisposeCurrentSession().StartListen(ip, port, interfaceName);

      private IterativeServer DisposeCurrentSession()
      {
         if (_serving)
         {
            _serving = false;
         }

         if (_activeClient)
         {
            _activeClient = false;
            _handler?.Disconnect();
         }

         try
         {
            _serverSocket?.Close();
         }
         catch (Exception e)
         {
            var msg = InternalMessageModel.Builder().AttachExceptionData(e)
               .AttachTextMessage("Can't properly dispose current server session\n").WithType(InternalMessageType.Error)
               .AttachTimeStamp(true).BuildMessage();
            OnLogEvent?.Invoke(this, msg);
         }


         return this;
      }

      public void StopService()
      {
         DisposeCurrentSession();
      }

      private void GetMessageFromClient(object[] args)
      {
         var builder = InternalMessageModel.Builder();
         try
         {
            foreach (var arg in args)
            {
               builder = arg switch
               {
                  Exception e => builder.AttachExceptionData(e),
                  string s => builder.AttachTextMessage(s),
                  int num => builder.WithType((InternalMessageType)num),
                  _ => throw new ArgumentException("Unrecognized data received from client handler")
               };
            }

            var msg = builder.AttachTimeStamp(true).BuildMessage();


            if (msg.Type == InternalMessageType.Error && !_handler.IsConnected())
            {
               AcceptNextPendingConnection();
            }
            else if (msg.Type == InternalMessageType.Server)
            {
               _handler.Send(msg.Data);
            }
            OnLogEvent?.Invoke(this, msg);

         }
         catch (Exception e)
         {
            var msg = InternalMessageModel.Builder().AttachExceptionData(e).AttachTimeStamp(true)
               .WithType(InternalMessageType.Error)
               .AttachTextMessage($"Can't parse provided {args} of length {args?.Length ?? 0}").BuildMessage();
            OnLogEvent?.Invoke(this, msg);
         }
      }

      private void StartListen(string ip, int port, string interfaceName = "")
      {
         _serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

         try
         {
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _serverSocket.Bind(endPoint);
         }
         catch (Exception e)
         {
            var msg = InternalMessageModel.Builder().AttachExceptionData(e).AttachTimeStamp(true)
               .WithType(InternalMessageType.Error)
               .AttachTextMessage($"Can't bind socket to provided address: {ip} and port {port} for provided interface: {interfaceName}").BuildMessage();
            OnLogEvent?.Invoke(this, msg);
         }

         try
         {
            _serverSocket.Listen(1);
            _serving = true;
            AcceptNextPendingConnection();
            var msg = InternalMessageModel.Builder().AttachTimeStamp(true).WithType(InternalMessageType.Info)
               .AttachTextMessage(
                  $"Server is currently listening on {((IPEndPoint)_serverSocket.LocalEndPoint).Address} address on {((IPEndPoint)_serverSocket.LocalEndPoint).Port} port")
               .BuildMessage();
            OnLogEvent?.Invoke(this, msg);
         }
         catch (Exception e)
         {
            var msg = InternalMessageModel.Builder().AttachExceptionData(e).AttachTimeStamp(true)
               .WithType(InternalMessageType.Error)
               .AttachTextMessage(
                  $"Can't move socket into listening state on provided address: {ip}, port {port} and interface: {interfaceName}\n")
               .BuildMessage();
            OnLogEvent?.Invoke(this, msg);
            _serving = false;
         }
      }

      private void AcceptNextPendingConnection()
      {
         try
         {
            _serverSocket.BeginAccept(OnAcceptCallback, null);
         }
         catch (Exception e)
         {
            var msg = InternalMessageModel.Builder().AttachExceptionData(e).AttachTimeStamp(true)
               .WithType(InternalMessageType.Error)
               .AttachTextMessage(
                  "Can't accept new pending connection\n")
               .BuildMessage();

            OnLogEvent?.Invoke(this, msg);
         }

      }

      private void OnAcceptCallback(IAsyncResult asyncResult)
      {
         try
         {
            var client = _serverSocket is { } listener
               ? listener.EndAccept(asyncResult)
               : throw new Exception("Server socket is null");

            var clientModel = new ClientModel(client.RemoteEndPoint is IPEndPoint end ? (end.Port, end.Address.ToString()).ToTuple() : (0, "").ToTuple());
            OnNewClient?.Invoke(this, clientModel);

            _handler = new ClientHandler(client, clientModel);
            _handler.OnLogEvent += (sender, objects) => GetMessageFromClient(objects);
            _handler.ClientDisconnected += (sender, args) => OnDisconnect?.Invoke(this, args);
            _activeClient = true;

            var msg = InternalMessageModel.Builder().WithType(InternalMessageType.Success).AttachTimeStamp(true)
               .AttachTextMessage($"Successfully accepted new client: {clientModel}").BuildMessage();
            OnLogEvent?.Invoke(this, msg);
         }
         catch (Exception e)
         {
            if (!_serverSocket.IsDisposed())
            {
               var msg = InternalMessageModel.Builder().AttachExceptionData(e).AttachTimeStamp(true)
                  .WithType(InternalMessageType.Error)
                  .AttachTextMessage(
                     "Can't finish accepting new pending connection\n")
                  .BuildMessage();

               OnLogEvent?.Invoke(this, msg);
            }
         }
      }

      public void AcceptNext()
      {
         AcceptNextPendingConnection();
      }
   }
}
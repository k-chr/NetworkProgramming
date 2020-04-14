using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkProgramming.Lab4.Models;
using NetworkProgramming.Lab4.Extensions;

namespace NetworkProgramming.Lab4.Services
{
   public class MultithreadingServer
   {
      private List<ClientHandler> _clients;
      private bool _activeClient;
      private bool _serving;
      private Socket _serverSocket;
      private int _clientsCount;

      public event EventHandler<InternalMessageModel> OnLogEvent;
      public event EventHandler<ClientModel> OnNewClient;
      public event EventHandler<ClientModel> OnDisconnect;

      public void StartService(string ip, int port, string interfaceName = "", int maxClientsQueue = 3) => DisposeCurrentSession().StartListen(ip, port, interfaceName, maxClientsQueue);

      private MultithreadingServer DisposeCurrentSession()
      {
         if (_serving)
         {
            _serving = false;
         }

         if (_activeClient)
         {
            _activeClient = false;
            foreach (var clientHandler in _clients)
            {
               clientHandler.Disconnect();
            }
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
                  ClientModel m => builder.AttachClientData(m),
                  Exception e => builder.AttachExceptionData(e),
                  string s => builder.AttachTextMessage(s),
                  int num => builder.WithType((InternalMessageType)num),
                  _ => throw new ArgumentException("Unrecognized data received from client handler")
               };
            }

            var msg = builder.AttachTimeStamp(true).BuildMessage();

            OnLogEvent?.Invoke(this, msg);

            if (msg.Type == InternalMessageType.Server)
            {
               var handler = _clients.FirstOrDefault(clientHandler => clientHandler.Data.Equals(msg.ClientModelData));
               handler?.Send(msg.Data);
            }
         }
         catch (Exception e)
         {
            var msg = InternalMessageModel.Builder().AttachExceptionData(e).AttachTimeStamp(true)
               .WithType(InternalMessageType.Error)
               .AttachTextMessage($"Can't parse provided {args} of length {args?.Length ?? 0}").BuildMessage();
            OnLogEvent?.Invoke(this, msg);
         }
      }

      private void StartListen(string ip, int port, string interfaceName = "", int maxClientsCount = 3)
      {
         _serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
         _clients = new List<ClientHandler>(maxClientsCount);
         _clientsCount = maxClientsCount;
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

            if (_clients.Count >= _clientsCount)
            {
               client.Send(Encoding.UTF8.GetBytes("Rejected connection"));
               client.Shutdown(SocketShutdown.Both);
               client.Close(1000);
            }
            else
            {
               var clientModel = new ClientModel(client.RemoteEndPoint is IPEndPoint end
                  ? (end.Port, end.Address.ToString()).ToTuple()
                  : (0, "").ToTuple())
               { Connected = "Online" };

               OnNewClient?.Invoke(this, clientModel);

               var handler = new ClientHandler(client, clientModel);
               handler.OnLogEvent += (sender, objects) => GetMessageFromClient(objects);
               handler.ClientDisconnected += (sender, args) => RemoveClient(args);
               _clients.Add(handler);
               _activeClient = true;

               var msg = InternalMessageModel.Builder().WithType(InternalMessageType.Success).AttachTimeStamp(true)
                  .AttachTextMessage($"Successfully accepted new client: {clientModel}").BuildMessage();
               OnLogEvent?.Invoke(this, msg);
            }
            AcceptNextPendingConnection();
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

      private void RemoveClient(ClientModel args)
      {
         var toRemove = _clients.FirstOrDefault(handler => handler.Data.Equals(args));
         _clients.Remove(toRemove);
         OnDisconnect?.Invoke(this, args);
      }

      public void AcceptNext()
      {
         AcceptNextPendingConnection();
      }
   }
}
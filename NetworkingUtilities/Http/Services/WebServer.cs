using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Http.Routing;

namespace NetworkingUtilities.Http.Services
{
	public class WebServer : IService
	{
		private HttpListener _listener;
		private readonly string _baseRoute;
		private readonly IRouter _router;
		private readonly int? _port;
		private readonly string _403;
		private readonly string _404;

		internal WebServer(WebServerServiceBuilder serviceBuilder)
		{
			_router = serviceBuilder.Router;
			_port = serviceBuilder.Port;
			_baseRoute = serviceBuilder.Prefix;
			using var stream = System.Reflection.Assembly.GetExecutingAssembly()
			   .GetManifestResourceStream("NetworkingUtilities.Http.ErrorPages.403.html");
			if (stream != null)
			{
				using var fStreamReader = new StreamReader(stream);
				var str403 = fStreamReader.ReadToEnd();
				_403 = str403;
			}

			using var stream404 = System.Reflection.Assembly.GetExecutingAssembly()
			   .GetManifestResourceStream("NetworkingUtilities.Http.ErrorPages.404.html");
			if (stream404 != null)
			{
				using var fStreamReader = new StreamReader(stream404);
				var str404 = fStreamReader.ReadToEnd();
				_404 = str404;
			}
		}

		public static WebServerServiceBuilder Builder()
		{
			return new WebServerServiceBuilder();
		}

		public void StopService()
		{
			try
			{
				if (_listener.IsListening)
				{
					_listener?.Stop();
					_listener?.Abort();
					_listener?.Close();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public void StartService()
		{
			_listener = new HttpListener();
			_listener.Prefixes.Add(_baseRoute + (_port.HasValue ? $":{_port.Value}/" : "/"));

			Start();
		}

		private void Start()
		{
			try
			{
				_listener.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Environment.Exit(1);
			}

			_listener.BeginGetContext(OnBeginGetContext, _listener);
		}

		private void OnBeginGetContext(IAsyncResult ar)
		{
			ThreadPool.QueueUserWorkItem((_) =>
			{
				try
				{
					var ctx = _listener.EndGetContext(ar);
					try
					{
						var response = _router.Route(ctx.Request.Url.Segments, ctx.Request.HttpMethod);

						if (response != null)
						{
							ctx.Response.StatusCode = 200;
							using var output = ctx.Response.OutputStream;
							output.Write(Encoding.UTF8.GetBytes(response));
						}
						else
						{
							ctx.Response.StatusCode = 404;
							using var output = ctx.Response.OutputStream;
							output.Write(Encoding.UTF8.GetBytes(_404));
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						ctx.Response.StatusCode = 403;
						using var output = ctx.Response.OutputStream;
						output.Write(Encoding.UTF8.GetBytes(_403));
					}
					finally
					{
						ctx.Response.Close();
					}

					_listener.BeginGetContext(OnBeginGetContext, _listener);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			});
		}
	}
}
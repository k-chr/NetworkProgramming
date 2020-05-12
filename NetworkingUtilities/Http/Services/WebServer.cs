using System;
using System.Net;
using System.Text;
using System.Threading;
using NetworkingUtilities.Http.Routing;

namespace NetworkingUtilities.Http.Services
{
	public class WebServer : IService
	{
		private HttpListener _listener;
		private readonly string _baseRoute;
		private readonly IRouter _router;
		private readonly int? _port;
		private readonly bool _async;

		internal WebServer(WebServerServiceBuilder serviceBuilder)
		{
			_router = serviceBuilder.Router;
			_port = serviceBuilder.Port;
			_async = serviceBuilder.Async;
			_baseRoute = serviceBuilder.Prefix;
		}

		public static WebServerServiceBuilder Builder()
		{
			return new WebServerServiceBuilder();
		}

		public void StopService()
		{
			_listener?.Stop();
		}

		public void StartService()
		{
			_listener = new HttpListener();
			_listener.Prefixes.Add(_baseRoute + (_port.HasValue ? $":{_port.Value}" : ""));

			if (_async)
			{
				//TODO implement async action
			}
			else
			{
				Start();
			}
		}

		private void Start()
		{
			while (_listener.IsListening)
			{
				var ctx = _listener.GetContext();
				ThreadPool.QueueUserWorkItem((_) =>
				{
					try
					{
						var response = _router.Route(ctx.Request.Url.Segments, ctx.Request.HttpMethod);

						if (response != null)
						{
							ctx.Response.StatusCode = 200;
							using var output = ctx.Response.OutputStream;
							output.Write(Encoding.Unicode.GetBytes(response));
						}
						else
						{
							ctx.Response.StatusCode = 404;
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						ctx.Response.StatusCode = 403;
					}
				});
			}
		}
	}
}
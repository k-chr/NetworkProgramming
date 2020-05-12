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
		private string _baseRoute;
		private IRouter _router;
		private int? _port;
		private bool _async;

		internal WebServer(WebServerServiceBuilder serviceBuilder)
		{

		}

		public void StartService()
		{
			_listener = new HttpListener();
			_listener.Prefixes.Add(_baseRoute + (_port.HasValue? $":{_port.Value}" : ""));

			if (_async)
			{

			}
			else
			{
				while (true)
				{
					var ctx = _listener.GetContext();
					ThreadPool.QueueUserWorkItem((_) =>
					{
						try
						{
							var response = _router.Route(ctx.Request.Url.Segments);

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
}

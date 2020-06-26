using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Http.Routing;

namespace NetworkingUtilities.Http.Services
{
	public class WebServerServiceBuilder : IServiceBuilder
	{
		internal WebServerServiceBuilder()
		{
			Router = new Router();
			Router.BuildEndPoints();
		}

		public IService Build()
		{
			return new WebServer(this);
		}

		public WebServerServiceBuilder WithPrefix(string prefix)
		{
			Prefix = prefix;
			return this;
		}

		public WebServerServiceBuilder WithPort(int port)
		{
			Port = port;
			return this;
		}

		internal IRouter Router { get; }
		internal int? Port { get; private set; }
		internal string Prefix { get; private set; }
	}
}
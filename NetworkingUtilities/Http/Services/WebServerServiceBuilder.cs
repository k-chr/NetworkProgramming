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

		public WebServerServiceBuilder UseAsyncInvocations(bool value)
		{
			Async = value;
			return this;
		}

		internal IRouter Router { get;}
		internal int? Port { get; private set; }
		internal string Prefix { get; private set; }
		internal bool Async { get; private set; }

	}
}

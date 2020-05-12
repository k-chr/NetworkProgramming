namespace NetworkingUtilities.Http.Services
{
	public class WebServerServiceBuilder : IServiceBuilder
	{
		public IService Build()
		{
			return new WebServer(this);
		}

	}
}
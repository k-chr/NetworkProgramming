using System.Collections.Generic;
using System.Collections.ObjectModel;
using TimeProjectServices.ViewModels;

namespace TimeClient.ViewModels
{
	public class StartViewModel : ViewModelBase
	{
		public ObservableCollection<string> HelpItems { get; } =
			new ObservableCollection<string>(new List<string>
			{
				"If you want to configure Client module, go to the tab \"Config\". There you are allowed to specify local port, address and port of multicast group of servers, periods for sending DISCOVER / TIME queries.",
				"If you want to choose Server, go to \"Server Selection\" tab and start searching new TimeServers, if algorithm found some of them, " +
				"they will be displayed on the list. If the last connected server is specified, it will be highlighted on the list and exported to configuration file.",
				"If you want to inspect how time-communication works, go to \"Time Communication\" tab, and if you choose server, the communication bubbles should be displayed",
				"If you want to inspect application logs, go to \"Logs\" tab, there will be any info/error/success message"
			});
	}
}
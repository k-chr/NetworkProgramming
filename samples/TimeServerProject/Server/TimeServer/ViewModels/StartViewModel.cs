using System.Collections.Generic;
using System.Collections.ObjectModel;
using TimeProjectServices.ViewModels;

namespace TimeServer.ViewModels
{
	public class StartViewModel : ViewModelBase
	{
		public ObservableCollection<string> HelpItems { get; } =
			new ObservableCollection<string>(new List<string>
			{
				"If you want to configure Server module, go to the tab \"Config\". There you are allowed to specify address and port of multicast group of servers",
				"If you want to look through your Servers, go to \"Servers\" tab and check their statistics.",
				"If you want to inspect application logs, go to \"Logs\" tab, there will be any info/error/success message"
			});
	}
}
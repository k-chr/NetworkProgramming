using System;
using System.Collections.Generic;
using System.Text;

namespace TimeClient.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private Services.TimeClient _timeClient = null;

		public string AppTitle => "TimeClientApp";
	}
}
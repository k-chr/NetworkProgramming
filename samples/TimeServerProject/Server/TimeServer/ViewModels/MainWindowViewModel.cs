using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls.Notifications;
using TimeProjectServices.ViewModels;

namespace TimeServer.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly IManagedNotificationManager _mainWindowNotificationArea;
		private readonly ConfigViewModel _appState;

		public MainWindowViewModel(IManagedNotificationManager mainWindowNotificationArea, ConfigViewModel appState)
		{
			_mainWindowNotificationArea = mainWindowNotificationArea;
			_appState = appState;
		}

		public string Greeting => "Welcome to Avalonia!";

		public void OnClosing()
		{
			
		}
	}
}

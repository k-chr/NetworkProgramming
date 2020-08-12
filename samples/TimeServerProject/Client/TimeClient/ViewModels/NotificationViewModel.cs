using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls.Shapes;

namespace TimeClient.ViewModels
{
	public abstract class NotificationViewModel : ViewModelBase
	{
		public abstract Path Path { get; set; }
	}

	class InfoNotificationViewModel : NotificationViewModel
	{
		public override Path Path { get; set; }
	}

	class ErrorNotificationViewModel : NotificationViewModel
	{
		public override Path Path { get; set; }
	}

	class SuccessNotificationViewModel : NotificationViewModel
	{
		public override Path Path { get; set; }
	}

	public static class NotificationViewModelFactory
	{

	}
}

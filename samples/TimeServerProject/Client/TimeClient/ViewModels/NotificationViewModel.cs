using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls.Shapes;

namespace TimeClient.ViewModels
{
	public abstract class NotificationViewModel : ViewModelBase
	{
		public abstract Path Path { get; set; }
		public abstract string Message { get; set; }
	}

	public static class NotificationViewModelFactory
	{

	}
}

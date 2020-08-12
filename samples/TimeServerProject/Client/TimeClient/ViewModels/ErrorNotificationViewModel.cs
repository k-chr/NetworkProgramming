using Avalonia.Controls.Shapes;

namespace TimeClient.ViewModels
{
	class ErrorNotificationViewModel : NotificationViewModel
	{
		public override Path Path { get; set; }
		public override string Message { get; set; }
	}
}
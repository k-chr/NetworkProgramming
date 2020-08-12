using Avalonia.Controls.Shapes;

namespace TimeClient.ViewModels
{
	class SuccessNotificationViewModel : NotificationViewModel
	{
		public override Path Path { get; set; }
		public override string Message { get; set; }
	}
}
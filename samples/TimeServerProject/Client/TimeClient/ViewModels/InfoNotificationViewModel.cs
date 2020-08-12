using Avalonia.Media;

namespace TimeClient.ViewModels
{
	class InfoNotificationViewModel : NotificationViewModel
	{
		public override Geometry Path { get; }
		public override string Message { get; set; }
		public override string Title { get; set; }
	}
}
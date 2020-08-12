using Avalonia.Media;

namespace TimeClient.ViewModels
{
	internal class ErrorNotificationViewModel : NotificationViewModel
	{
		internal ErrorNotificationViewModel() => Icon = Geometry.Parse(
			"M14.966003,22.147998" +
			"L17.112,22.147998 17.112,24.293016 14.966003,24.293016z " +
			"M14.874008,13.098992" +
			"L17.195007,13.098992 17.195007,15.726005 16.645004,21.407031 15.432007,21.407031 14.874008,15.726005z " +
			"M16,5.1690032" +
			"L4.1110077,27.212022 27.889008,27.212022z " +
			"M16,0" +
			"L24,14.832999 32,29.666 16,29.666 0,29.666 8,14.832999z");

		public override Geometry Icon { get; }
		public override string Message { get; set; }
		public override string Title { get; set; }
	}
}
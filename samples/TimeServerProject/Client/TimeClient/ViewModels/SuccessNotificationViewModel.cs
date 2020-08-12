using Avalonia.Media;

namespace TimeClient.ViewModels
{
	internal class SuccessNotificationViewModel : NotificationViewModel
	{
		internal SuccessNotificationViewModel() => Icon = Geometry.Parse(
			"M22.014996,10.204013" +
			"L23.449993,11.597013 13.512005,21.845022 8.4560118,17.863018 9.6940098,16.292017 13.332006,19.157019z " +
			"M15.937988,2" +
			"C8.2529907,2,2,8.2520142,2,15.937988" +
			"L2,16.062988" +
			"C2,23.747986,8.2529907,30,15.937988,30" +
			"L16.062988,30" +
			"C23.747986,30,30.000977,23.747986,30.000977,16.062988" +
			"L30.000977,15.937988" +
			"C30.000977,8.2520142,23.747986,2,16.062988,2z " +
			"M15.937988,0" +
			"L16.062988,0" +
			"C24.851013,0,32,7.1489868,32,15.937988" +
			"L32,16.062988" +
			"C32,24.851013,24.851013,32,16.062988,32" +
			"L15.937988,32" +
			"C7.1499634,32,0,24.851013,0,16.062988" +
			"L0,15.937988" +
			"C0,7.1489868,7.1499634,0,15.937988,0z");
		

		public override Geometry Icon { get; }
		public override string Message { get; set; }
		public override string Title { get; set; }
	}
}
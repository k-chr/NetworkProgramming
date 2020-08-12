using System;
using Avalonia.Media;
using NetworkingUtilities.Utilities.Events;

namespace TimeClient.ViewModels
{
	public abstract class NotificationViewModel : ViewModelBase
	{
		public abstract Geometry Path { get; }
		public abstract string Message { get; set; }
		public abstract string Title { get; set; }
	}

	public static class NotificationViewModelFactory
	{
		public static NotificationViewModel Create(StatusCode code, string message, string title) =>
			code switch
			{
				StatusCode.Error => new ErrorNotificationViewModel(),
				StatusCode.Success => new SuccessNotificationViewModel(),
				StatusCode.Info => new InfoNotificationViewModel(),
				_ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
			};
	}
}

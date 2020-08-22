using System;
using NetworkingUtilities.Utilities.Events;

namespace TimeProjectServices.ViewModels
{
	public static class NotificationViewModelFactory
	{
		public static NotificationViewModel Create(StatusCode code, string message) =>
			code switch
			{
				StatusCode.Error => new ErrorNotificationViewModel {Message = message, Type = code},
				StatusCode.Success => new SuccessNotificationViewModel {Message = message, Type = code},
				StatusCode.Info => new InfoNotificationViewModel {Message = message, Type = code},
				_ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
			};
	}
}
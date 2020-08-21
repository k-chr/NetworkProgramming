using System;
using Avalonia.Media;
using JetBrains.Annotations;
using NetworkingUtilities.Utilities.Events;

namespace TimeClient.ViewModels
{
	public abstract class NotificationViewModel : ViewModelBase
	{
		[UsedImplicitly] public abstract Geometry Icon { get; }
		[UsedImplicitly] public abstract string Message { get; set; }
		[UsedImplicitly] public abstract string Title { get; }
		public StatusCode Type { [UsedImplicitly] get; set; }
	}

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
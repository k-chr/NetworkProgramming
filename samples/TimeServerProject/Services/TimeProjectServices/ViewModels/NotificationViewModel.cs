using Avalonia.Media;
using JetBrains.Annotations;
using NetworkingUtilities.Utilities.Events;

namespace TimeProjectServices.ViewModels
{
	public abstract class NotificationViewModel : ViewModelBase
	{
		[UsedImplicitly] public abstract Geometry Icon { get; }
		[UsedImplicitly] public abstract string Message { get; set; }
		[UsedImplicitly] public abstract string Title { get; }
		public StatusCode Type { [UsedImplicitly] get; set; }
	}
}
using System.Timers;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;

namespace CustomControls.Controls
{
	public class ClockControl : TemplatedControl
	{
		private const int Interval = 1000;

		private double HourAngle
		{
			set => SetValue(HoursAngleProperty, value);
		}

		private double MinutesAngle
		{
			set => SetValue(HoursAngleProperty, value);
		}

		private double SecondsAngle
		{
			set => SetValue(HoursAngleProperty, value);
		}

		public StyledProperty<double> HoursAngleProperty;
		public StyledProperty<double> SecondsAngleProperty;
		public StyledProperty<double> MinutesAngleProperty;

		public ClockControl()
		{
			var clockTimer = new Timer(Interval);
			clockTimer.Elapsed += (sender, args) => { Dispatcher.UIThread.InvokeAsync(() => { }); };

			HoursAngleProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(HoursAngleProperty), 0, false,
					BindingMode.TwoWay);
			SecondsAngleProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(SecondsAngleProperty), 0, false,
					BindingMode.TwoWay);
			MinutesAngleProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(MinutesAngleProperty), 0, false,
					BindingMode.TwoWay);
		}
	}
}
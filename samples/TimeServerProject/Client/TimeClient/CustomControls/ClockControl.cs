using System;
using System.Timers;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;

namespace TimeClient.CustomControls
{
	public class ClockControl : TemplatedControl
	{
		private const int Interval = 1000;

		public Thickness TickOffset
		{
			get => GetValue(TickOffsetProperty);
			set => SetValue(TickOffsetProperty, value);
		}

		public double HoursAngle
		{
			get => GetValue(HoursAngleProperty);
			set => SetValue(HoursAngleProperty, value);
		}

		public double MinutesAngle
		{
			get => GetValue(MinutesAngleProperty);
			set => SetValue(MinutesAngleProperty, value);
		}

		public double SecondsAngle
		{
			get => GetValue(SecondsAngleProperty);
			set => SetValue(SecondsAngleProperty, value);
		}

		public double Hours
		{
			get => GetValue(HoursProperty);
			set => SetValue(HoursProperty, value);
		}

		public double Minutes
		{
			get => GetValue(MinutesProperty);
			set => SetValue(MinutesProperty, value);
		}

		public double Seconds
		{
			get => GetValue(SecondsProperty);
			set => SetValue(SecondsProperty, value);
		}

		public double Radius
		{
			get => GetValue(RadiusProperty);
			set
			{
				SetValue(RadiusProperty, value); 
				TickOffset = new Thickness(0,0,0, value/2);
			}
		}

		public static readonly StyledProperty<Thickness> TickOffsetProperty;

		public static readonly StyledProperty<double> RadiusProperty;

		public static readonly StyledProperty<double> HoursProperty;
		public static readonly StyledProperty<double> MinutesProperty;
		public static readonly StyledProperty<double> SecondsProperty;

		public static readonly StyledProperty<double> HoursAngleProperty;
		public static readonly StyledProperty<double> SecondsAngleProperty;
		public static readonly StyledProperty<double> MinutesAngleProperty;

		static ClockControl()
		{
			HoursAngleProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(HoursAngle), 0, false,
					BindingMode.TwoWay);
			SecondsAngleProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(SecondsAngle), 0, false,
					BindingMode.TwoWay);
			MinutesAngleProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(MinutesAngle), 0, false,
					BindingMode.TwoWay);

			SecondsProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(Seconds), 0, false,
					BindingMode.TwoWay);
			HoursProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(Hours), 0, false,
					BindingMode.TwoWay);
			MinutesProperty =
				AvaloniaProperty.Register<ClockControl, double>(nameof(Minutes), 0, false,
					BindingMode.TwoWay);

			RadiusProperty = AvaloniaProperty.Register<ClockControl, double>(nameof(Radius), 0, false,
				BindingMode.TwoWay);

			TickOffsetProperty = AvaloniaProperty.Register<ClockControl, Thickness>(nameof(TickOffset), Thickness.Parse("0 0 0 0"), false,
				BindingMode.TwoWay);
		}

		public ClockControl()
		{
			var clockTimer = new Timer(Interval);
			clockTimer.Elapsed += (sender, args) =>
			{
				Dispatcher.UIThread.InvokeAsync(() =>
				{
					var date = DateTime.Now;
					SecondsAngle = date.Second * 6;
					MinutesAngle = date.Minute * 6;
					HoursAngle = date.Hour * 30 + date.Minute * 0.5;
					Hours = date.Hour;
					Seconds = date.Second;
					Minutes = date.Minute;
				});
			};
			clockTimer.Start();
		}
	}
}
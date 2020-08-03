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

		public string Hours
		{
			get => GetValue(HoursProperty);
			set => SetValue(HoursProperty, value);
		}

		public string Minutes
		{
			get => GetValue(MinutesProperty);
			set => SetValue(MinutesProperty, value);
		}

		public string Seconds
		{
			get => GetValue(SecondsProperty);
			set => SetValue(SecondsProperty, value);
		}

		public double Radius
		{
			get => GetValue(RadiusProperty);
			set => SetValue(RadiusProperty, value);
		}

		public static readonly DirectProperty<ClockControl, Thickness> TickOffsetProperty;

		public static readonly StyledProperty<double> RadiusProperty;
		public static readonly DirectProperty<ClockControl, double> DiameterProperty;

		public static readonly StyledProperty<string> HoursProperty;
		public static readonly StyledProperty<string> MinutesProperty;
		public static readonly StyledProperty<string> SecondsProperty;

		public static readonly DirectProperty<ClockControl, double> HoursLengthProperty;
		public static readonly DirectProperty<ClockControl, double> MinutesLengthProperty;
		public static readonly DirectProperty<ClockControl, double> SecondsLengthProperty;

		public static readonly DirectProperty<ClockControl, double> TickWidthProperty;
		public static readonly DirectProperty<ClockControl, double> SecondTickWidthProperty;

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
				AvaloniaProperty.Register<ClockControl, string>(nameof(Seconds), "00", false,
					BindingMode.TwoWay);
			HoursProperty =
				AvaloniaProperty.Register<ClockControl, string>(nameof(Hours), "00", false,
					BindingMode.TwoWay);
			MinutesProperty =
				AvaloniaProperty.Register<ClockControl, string>(nameof(Minutes), "00", false,
					BindingMode.TwoWay);

			RadiusProperty = AvaloniaProperty.Register<ClockControl, double>(nameof(Radius), 0, false,
				BindingMode.TwoWay);
			DiameterProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>("Diameter", control => control.Radius * 2);

			TickOffsetProperty = AvaloniaProperty.RegisterDirect<ClockControl, Thickness>("TickOffset",
				control => new Thickness(0, 0, 0, control.Radius / 2));

			TickWidthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>("TickWidth", control => control.Radius / 40);
			SecondTickWidthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>("SecondTickWidth",
					control => control.Radius / 60);

			HoursLengthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>("HoursLength", control => control.Radius * 0.5);
			MinutesLengthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>("MinutesLength", control => control.Radius * 0.8);
			SecondsLengthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>("SecondsLength", control => control.Radius * 0.9);
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
					Hours = $"{date.Hour:00}";
					Seconds = $"{date.Second:00}";
					Minutes = $"{date.Minute:00}";
				});
			};
			clockTimer.Start();
		}
	}
}
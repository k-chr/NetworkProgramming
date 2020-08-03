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
			set
			{
				SetValue(RadiusProperty, value);
				HoursLength = Radius * 0.5;
				MinutesLength = Radius * 0.8;
				SecondsLength = Radius * 0.9;
				TickWidth = Radius / 40;
				SecondsTickWidth = Radius / 60;
				HoursTransformOrigin = new RelativePoint(TickWidth / 2, HoursLength, RelativeUnit.Absolute);
				MinutesTransformOrigin = new RelativePoint(TickWidth / 2, MinutesLength, RelativeUnit.Absolute);
				SecondsTransformOrigin = new RelativePoint(SecondsTickWidth / 2, SecondsLength, RelativeUnit.Absolute);
				HoursTickOffset = Thickness.Parse($"0 0 0 {HoursLength}");
				MinutesTickOffset = Thickness.Parse($"0 0 0 {MinutesLength}");
				SecondsTickOffset = Thickness.Parse($"0 0 0 {SecondsLength}");
			}
		}

		public static readonly DirectProperty<ClockControl, Thickness> HoursTickOffsetProperty;
		public static readonly DirectProperty<ClockControl, Thickness> MinutesTickOffsetProperty;
		public static readonly DirectProperty<ClockControl, Thickness> SecondsTickOffsetProperty;

		public static readonly StyledProperty<double> RadiusProperty;
		public static readonly DirectProperty<ClockControl, double> DiameterProperty;

		public static readonly StyledProperty<string> HoursProperty;
		public static readonly StyledProperty<string> MinutesProperty;
		public static readonly StyledProperty<string> SecondsProperty;

		public static readonly DirectProperty<ClockControl, double> HoursLengthProperty;
		public static readonly DirectProperty<ClockControl, double> MinutesLengthProperty;
		public static readonly DirectProperty<ClockControl, double> SecondsLengthProperty;

		public static readonly DirectProperty<ClockControl, double> TickWidthProperty;
		public static readonly DirectProperty<ClockControl, double> SecondsTickWidthProperty;

		public static readonly DirectProperty<ClockControl, RelativePoint> HoursTransformOriginProperty;
		public static readonly DirectProperty<ClockControl, RelativePoint> MinutesTransformOriginProperty;
		public static readonly DirectProperty<ClockControl, RelativePoint> SecondsTransformOriginProperty;

		public static readonly StyledProperty<double> HoursAngleProperty;
		public static readonly StyledProperty<double> SecondsAngleProperty;
		public static readonly StyledProperty<double> MinutesAngleProperty;

		private double _hoursLength;
		private double _minutesLength;
		private double _secondsLength;
		private RelativePoint _hoursTransformOrigin;
		private RelativePoint _minutesTransformOrigin;
		private RelativePoint _secondsTransformOrigin;
		private double _tickWidth;
		private double _secondsTickWidth;
		private Thickness _hoursTickOffset;
		private Thickness _minutesTickOffset;
		private Thickness _secondsTickOffset;

		public double SecondsLength
		{
			get => _secondsLength;
			set => SetAndRaise(SecondsLengthProperty, ref _secondsLength, value);
		}

		public double MinutesLength
		{
			get => _minutesLength;
			set => SetAndRaise(MinutesLengthProperty, ref _minutesLength, value);
		}

		public double HoursLength
		{
			get => _hoursLength;
			set => SetAndRaise(HoursLengthProperty, ref _hoursLength, value);
		}

		public RelativePoint SecondsTransformOrigin
		{
			get => _secondsTransformOrigin;
			set => SetAndRaise(SecondsTransformOriginProperty, ref _secondsTransformOrigin, value);
		}

		public RelativePoint MinutesTransformOrigin
		{
			get => _minutesTransformOrigin;
			set => SetAndRaise(MinutesTransformOriginProperty, ref _minutesTransformOrigin, value);
		}

		public RelativePoint HoursTransformOrigin
		{
			get => _hoursTransformOrigin;
			set => SetAndRaise(HoursTransformOriginProperty, ref _hoursTransformOrigin, value);
		}

		public Thickness SecondsTickOffset
		{
			get => _secondsTickOffset;
			set => SetAndRaise(SecondsTickOffsetProperty, ref _secondsTickOffset, value);
		}

		public Thickness MinutesTickOffset
		{
			get => _minutesTickOffset;
			set => SetAndRaise(MinutesTickOffsetProperty, ref _minutesTickOffset, value);
		}

		public Thickness HoursTickOffset
		{
			get => _hoursTickOffset;
			set => SetAndRaise(HoursTickOffsetProperty, ref _hoursTickOffset, value);
		}

		public double SecondsTickWidth
		{
			get => _secondsTickWidth;
			set => SetAndRaise(SecondsTickWidthProperty, ref _secondsTickWidth, value);
		}

		public double TickWidth
		{
			get => _tickWidth;
			set => SetAndRaise(TickWidthProperty, ref _tickWidth, value);
		}

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

			TickWidthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>(nameof(TickWidth), control => control.TickWidth);
			SecondsTickWidthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>(nameof(SecondsTickWidth),
					control => control.SecondsTickWidth);

			HoursLengthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>(nameof(HoursLength),
					control => control.HoursLength);
			MinutesLengthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>(nameof(MinutesLength),
					control => control.MinutesLength);
			SecondsLengthProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, double>(nameof(SecondsLength),
					control => control.SecondsLength);

			HoursTransformOriginProperty = AvaloniaProperty.RegisterDirect<ClockControl, RelativePoint>(
				nameof(HoursTransformOrigin),
				control => control.HoursTransformOrigin);
			MinutesTransformOriginProperty = AvaloniaProperty.RegisterDirect<ClockControl, RelativePoint>(
				nameof(MinutesTransformOrigin),
				control => control.MinutesTransformOrigin);
			SecondsTransformOriginProperty = AvaloniaProperty.RegisterDirect<ClockControl, RelativePoint>(
				nameof(SecondsTransformOrigin),
				control => control.SecondsTransformOrigin);

			HoursTickOffsetProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, Thickness>(nameof(HoursTickOffset),
					control => control.HoursTickOffset);
			MinutesTickOffsetProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, Thickness>(nameof(MinutesTickOffset),
					control => control.MinutesTickOffset);
			SecondsTickOffsetProperty =
				AvaloniaProperty.RegisterDirect<ClockControl, Thickness>(nameof(SecondsTickOffset),
					control => control.SecondsTickOffset);
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
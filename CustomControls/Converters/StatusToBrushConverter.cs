using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using NetworkingUtilities.Utilities.Events;

namespace CustomControls.Converters
{
	public class StatusToBrushConverter : IValueConverter
	{
		
		private static readonly Color DarkRed = Colors.DarkRed;
		private static readonly Color DarkGreen = Colors.DarkGreen;
		private static readonly Color DarkBlue = Colors.DarkBlue;


		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is StatusCode code)
			{
				return code switch
					   {
						   StatusCode.Error => DarkRed,
						   StatusCode.Success => DarkGreen,
						   StatusCode.Info => DarkBlue,
						   _ => throw new ArgumentOutOfRangeException()
					   };
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color color)
			{
				return color switch
					   {
						   { } when color.Equals(DarkRed) => StatusCode.Error,
						   { } when color.Equals(DarkBlue) => StatusCode.Info,
						   { } when color.Equals(DarkGreen) => StatusCode.Success,
						   _ => throw new ArgumentOutOfRangeException()
					   };
			}

			return value;
		}
	}
}
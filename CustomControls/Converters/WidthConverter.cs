using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CustomControls.Converters
{
	public class WidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double i && parameter is string j)
			{
				return i - double.Parse(j);
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double i && parameter is string j)
			{
				return i + double.Parse(j);
			}

			return value;
		}
	}
}
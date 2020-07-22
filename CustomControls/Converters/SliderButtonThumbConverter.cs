using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CustomControls.Converters
{
	public class SliderButtonThumbConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int i)
			{
				return i / 2;
			}

			return 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int i)
			{
				return i * 2;
			}

			return 0;
		}
	}
}
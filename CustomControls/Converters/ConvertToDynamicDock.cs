using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace CustomControls.Converters
{
	public class ConvertToDynamicDock : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is HorizontalAlignment i)
			{
				return i switch
					   {
						   HorizontalAlignment.Left => Dock.Right,
						   _ => Dock.Left
					   };
			}

			return Dock.Left;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Dock i)
			{
				return i switch
					   {
						   Dock.Right => HorizontalAlignment.Left,
						   _ => HorizontalAlignment.Right
					   };
			}

			return HorizontalAlignment.Right;
		}
	}
}
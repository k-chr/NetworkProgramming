using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using UdpClient.Models;

namespace UdpClient.CustomControl
{
   public class InternalMessageTypeToAlignmentConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is InternalMessageType i)
         {
            return i switch
            {
               InternalMessageType.Client => HorizontalAlignment.Right,
               _ => HorizontalAlignment.Left
            };
         }

         return HorizontalAlignment.Left;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is HorizontalAlignment i)
         {
            return i switch
            {
               HorizontalAlignment.Right => InternalMessageType.Client,
               _ => InternalMessageType.Server
            };
         }

         return InternalMessageType.Server;
      }
   }
}

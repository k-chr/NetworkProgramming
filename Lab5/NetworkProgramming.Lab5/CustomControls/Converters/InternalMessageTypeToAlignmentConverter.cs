using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using CustomControls.Models;

namespace CustomControls.Converters
{
   public class InternalMessageTypeToAlignmentConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is InternalMessageType i && parameter is string s)
         {
            return s switch
            {
               "Client" => i switch
               {
                     InternalMessageType.Client => HorizontalAlignment.Right,
                     _ => HorizontalAlignment.Left
               },
               "Server" => i switch
               {
                     InternalMessageType.Server => HorizontalAlignment.Right,
                     _ => HorizontalAlignment.Left
               },
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

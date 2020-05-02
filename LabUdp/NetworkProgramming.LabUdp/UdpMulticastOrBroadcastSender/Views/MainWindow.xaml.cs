using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UdpMulticastOrBroadcastSender.Views
{
   public class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
      }

      private void InitializeComponent()
      {
         AvaloniaXamlLoader.Load(this);
         this.FindControl<Control>("TitleBar").PointerPressed += (i, e) =>
         {
            PlatformImpl?.BeginMoveDrag(e);
         };
      }

      public void Minimize(object sender, RoutedEventArgs args)
      {
         this.WindowState = WindowState.Minimized;
      }

      public void Close(object sender, RoutedEventArgs args)
      {
         base.OnClosing(new CancelEventArgs());
      }
   }
}

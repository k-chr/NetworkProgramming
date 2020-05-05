using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace UdpBroadcastOrMulticastReceiver.Views
{
   public class MainWindow : Window
   {
      private readonly string _maximizeButton = "Restore Down";
      private readonly string _restoreDownIcon = "M0,6H8V14H0ZM3,6V3H11V11H9";
      private readonly string _maximizeIcon = "M0,2H10V12H0Z";
      public MainWindow()
      {
         InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
         this.LayoutUpdated += (sender, args) => PollWindowState();
      }

      private void PollWindowState()
      {
         var butt = this.FindControl<Button>(_maximizeButton);
         if (!(butt.Content is Path cont)) return;
         if (WindowState == WindowState.Normal)
         {
            cont.Data = Geometry.Parse(_maximizeIcon);
            butt.Tag = "Maximize";
         }
         else if(WindowState == WindowState.Maximized)
         {
            cont.Data = Geometry.Parse(_restoreDownIcon);
            butt.Tag = "RestoreDown";
         }
      }


      private void InitializeComponent()
      {
         AvaloniaXamlLoader.Load(this);
         this.FindControl<Control>("TitleBar").PointerPressed += (i, e) => { PlatformImpl?.BeginMoveDrag(e); };
         SetupSide("Left", StandardCursorType.LeftSide, WindowEdge.West);
         SetupSide("Right", StandardCursorType.RightSide, WindowEdge.East);
         SetupSide("Top", StandardCursorType.TopSide, WindowEdge.North);
         SetupSide("Bottom", StandardCursorType.BottomSide, WindowEdge.South);
         SetupSide("TopLeft", StandardCursorType.TopLeftCorner, WindowEdge.NorthWest);
         SetupSide("TopRight", StandardCursorType.TopRightCorner, WindowEdge.NorthEast);
         SetupSide("BottomLeft", StandardCursorType.BottomLeftCorner, WindowEdge.SouthWest);
         SetupSide("BottomRight", StandardCursorType.BottomRightCorner, WindowEdge.SouthEast);
      }

      private void SetupSide(string name, StandardCursorType cursor, WindowEdge edge)
      {
         var ctl = this.FindControl<Control>(name);
         ctl.Cursor = new Cursor(cursor);
         ctl.PointerPressed += (i, e) => { PlatformImpl?.BeginResizeDrag(edge, e); };
      }

      public void Minimize(object sender, RoutedEventArgs args)
      {
         WindowState = WindowState.Minimized;
      }

      public void Close(object sender, RoutedEventArgs args)
      {
         base.OnClosing(new CancelEventArgs());
      }

      public void Maximize(object sender, RoutedEventArgs args)
      {
         WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
      }
   }
}


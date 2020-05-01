using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UdpServer.Views
{
   public class ConversationView : UserControl
   {
      public ConversationView()
      {
         this.InitializeComponent();
      }

      private void InitializeComponent()
      {
         AvaloniaXamlLoader.Load(this);
      }
   }
}

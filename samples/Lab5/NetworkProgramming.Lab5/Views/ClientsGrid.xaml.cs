using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NetworkProgramming.Lab4.Views
{
   public class ClientsGrid : UserControl
   {
      public ClientsGrid()
      {
         this.InitializeComponent();
      }

      private void InitializeComponent()
      {
         AvaloniaXamlLoader.Load(this);
      }
   }
}

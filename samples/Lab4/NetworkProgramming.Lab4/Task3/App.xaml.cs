using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Task3.ViewModels;
using Task3.Views;

namespace Task3
{
   public class App : Application
   {
      public override void Initialize()
      {
         AvaloniaXamlLoader.Load(this);
      }

      public override void OnFrameworkInitializationCompleted()
      {
         if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
         {
            desktop.MainWindow = new MainWindow
            {
               DataContext = new MainWindowViewModel(),
            };
         }

         base.OnFrameworkInitializationCompleted();
      }
   }
}

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Task4.ViewModels;
using Task4.Views;

namespace Task4
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

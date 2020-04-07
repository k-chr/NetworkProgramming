using Avalonia;
using Avalonia.Dialogs;
using Avalonia.Logging;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

namespace NetworkProgramming.Lab2
{
   class Program
   {
      public static void Main(string[] args) => BuildAvaloniaApp()
          .StartWithClassicDesktopLifetime(args);

      // Avalonia configuration, don't remove; also used by visual designer.
      public static AppBuilder BuildAvaloniaApp()
          => AppBuilder.Configure<App>()
             .LogToDebug(LogEventLevel.Verbose)
              .UsePlatformDetect()
              .UseDirect2D1()
              .UseReactiveUI()
              .UseManagedSystemDialogs().LogToDebug().LogToTrace();
   }
}

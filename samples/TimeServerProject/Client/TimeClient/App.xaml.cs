using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using ReactiveUI;
using TimeClient.Services;
using TimeClient.ViewModels;
using TimeClient.Views;

namespace TimeClient
{
	public class App : Application
	{
		private const string ConfigFileName = "Config.bin";
		private AutoSuspendHelper _suspendHelper;

		public static readonly Styles FluentDark = new Styles
		{
			new StyleInclude(new Uri("resm:Styles?assembly=TimeProjectServices"))
			{
				Source = new Uri("avares://TimeProjectServices/Styles/FluentDark.xaml")
			},
		};

		public static readonly Styles FluentLight = new Styles
		{
			new StyleInclude(new Uri("resm:Styles?assembly=TimeProjectServices"))
			{
				Source = new Uri("avares://TimeProjectServices/Styles/FluentLight.xaml")
			},
		};

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
			_suspendHelper = new AutoSuspendHelper(ApplicationLifetime);
			RxApp.SuspensionHost.CreateNewAppState = () => new ConfigViewModel();
			RxApp.SuspensionHost.SetupDefaultSuspendResume(new BinaryConfigurationSuspensionDriver(ConfigFileName));
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				_suspendHelper.OnFrameworkInitializationCompleted();
				var mainWindow = new MainWindow();

				var viewModel = new MainWindowViewModel(mainWindow.NotificationArea,
					RxApp.SuspensionHost.GetAppState<ConfigViewModel>());

				mainWindow.DataContext = viewModel;
				desktop.MainWindow = mainWindow;

				Window.WindowClosedEvent.AddClassHandler(typeof(MainWindow), (sender, args) =>
				{
					if (sender is MainWindow window)
					{
						var mainWindowViewModel = window.DataContext as MainWindowViewModel;
						mainWindowViewModel?.OnClosing();
					}
				});
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using TimeClient.ViewModels;
using TimeClient.Views;

namespace TimeClient
{
	public class App : Application
	{
		public static readonly Styles FluentDark = new Styles
		{
			new StyleInclude(new Uri("resm:Styles?assembly=TimeClient"))
			{
				Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentDark.xaml")
			},
		};

		public static readonly Styles FluentLight = new Styles
		{
			new StyleInclude(new Uri("resm:Styles?assembly=TimeClient"))
			{
				Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentLight.xaml")
			},
		};

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);

			Styles.Insert(0, FluentDark);
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
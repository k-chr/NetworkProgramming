using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;

namespace TimeClient.Views
{
	public class MainWindow : Window
	{
		private readonly StyleInclude _dark;
		private readonly StyleInclude _light;

		public MainWindow()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
			var light = new StyleInclude(new Uri("resm:Styles?assembly=TimeClient"))
			{
				Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentLight.xaml")
			};
			var dark = new StyleInclude(new Uri("resm:Styles?assembly=TimeClient"))
			{
				Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentDark.xaml")
			};
			
			_light = light;
			_dark = dark;
			Styles.Add(dark);
		}

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);
			ExtendClientAreaChromeHints =
				Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome |
				Avalonia.Platform.ExtendClientAreaChromeHints.OSXThickTitleBar;
		}
		public void OnStyleChange(object sender, RoutedEventArgs args)
		{
			var button = (ToggleSwitch) sender;
			Styles[0] = button.IsChecked switch
						{
							true => _light,
							false => _dark,
							_ => _light
						};
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
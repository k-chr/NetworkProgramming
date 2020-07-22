using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using CustomControls.Controls;

namespace UdpServer.Views
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
			var light = new StyleInclude(new Uri("resm:Styles?assembly=UdpServer"))
			{
				Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default")
			};
			var dark = new StyleInclude(new Uri("resm:Styles?assembly=UdpServer"))
			{
				Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default")
			};

			_light = light;
			_dark = dark;

			Styles.Add(light);
		}

		public void OnStyleChange(object sender, RoutedEventArgs args)
		{
			var button = (SliderButton) sender;
			Styles[0] = button.State switch
						{
							0 => _light,
							1 => _dark,
							_ => _light
						};
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
			this.FindControl<Control>("TitleBar").PointerPressed += (i, e) => { PlatformImpl?.BeginMoveDrag(e); };
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeClient.Views
{
	public class MainWindow : FluentWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			this.AttachDevTools();
			var themes = this.Find<ToggleSwitch>("Themes");
			themes.Checked += (sender, args) => Application.Current.Styles[0] = App.FluentLight;
			themes.Unchecked += (sender, args) => Application.Current.Styles[0] = App.FluentDark;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
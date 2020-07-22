using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UdpServer.Views
{
	public class MainMenu : UserControl
	{
		public MainMenu()
		{
			this.InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
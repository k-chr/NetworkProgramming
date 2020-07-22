using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UdpClient.Views
{
	public class AppView : UserControl
	{
		public AppView()
		{
			this.InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
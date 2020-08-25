using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeServer.Views
{
	public class ServersView : UserControl
	{
		public ServersView() => InitializeComponent();

		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}

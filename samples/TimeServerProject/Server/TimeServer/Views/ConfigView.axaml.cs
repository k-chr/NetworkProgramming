using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeServer.Views
{
	public class ConfigView : UserControl
	{
		public ConfigView()
		{
			this.InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}

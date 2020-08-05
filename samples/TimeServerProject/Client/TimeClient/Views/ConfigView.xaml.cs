using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeClient.Views
{
	public class ConfigView : UserControl
	{
		public ConfigView() => InitializeComponent();

		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}
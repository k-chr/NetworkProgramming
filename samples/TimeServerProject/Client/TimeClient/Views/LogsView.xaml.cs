using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeClient.Views
{
	public class LogsView : UserControl
	{
		public LogsView() => InitializeComponent();

		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}

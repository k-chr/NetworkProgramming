using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeServer.Views
{
	public class LogsView : UserControl
	{
		public LogsView()
		{
			this.InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}

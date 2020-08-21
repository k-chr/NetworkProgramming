using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeClient.Views
{
	public class ServerSelectionView : UserControl
	{
		public ServerSelectionView() => InitializeComponent();

		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}
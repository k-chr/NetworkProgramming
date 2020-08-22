using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TimeProjectServices.Views
{
	public class NotificationView : UserControl
	{
		public NotificationView() => InitializeComponent();

		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}

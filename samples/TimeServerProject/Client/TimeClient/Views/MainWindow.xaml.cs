using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

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
			var tabControl = this.Find<TabControl>("TabControl");
			tabControl.SelectionChanged += (sender, args) =>
			{
				var key = "PART_ItemsContentPresenter";
				var children = tabControl.GetVisualChildren();
				foreach (var child in children)
				{
					if (child is ScrollViewer viewer && (viewer.Name?.Equals(key) ?? false))
					{
						viewer.ScrollToHome();
						break;
					}

					if (!(Traverse(child, key) is ScrollViewer scrollViewer)) continue;
					scrollViewer.ScrollToHome();
					break;
				}
			};
		}

		private static IVisual Traverse(IVisual child, string key)
		{
			if (child is ScrollViewer viewer && (viewer.Name?.Equals(key) ?? false))
			{
				return child;
			}

			IVisual result = null;

			foreach (var visualChild in child.GetVisualChildren())
			{
				result = Traverse(visualChild, key);
			}

			return result;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
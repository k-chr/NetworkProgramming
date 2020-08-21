using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using DynamicData;
using JetBrains.Annotations;

namespace TimeClient.Views
{
	public class MainWindow : FluentWindow
	{
		public WindowNotificationManager NotificationArea { get; }

		public static readonly StyledProperty<bool> SideBarCollapsedProperty;
		private bool _sideBarCollapsed;

		public bool SideBarCollapsed
		{
			[UsedImplicitly] get => _sideBarCollapsed;
			set => SetAndRaise(SideBarCollapsedProperty, ref _sideBarCollapsed, value);
		}

		public MainWindow()
		{
			InitializeComponent();
			this.AttachDevTools();
			var themes = this.Find<ToggleSwitch>("Themes");

			var tabControl = this.Find<TabControl>("TabControl");

			themes.Checked += (sender, args) =>
			{
				Application.Current.Styles[0] = App.FluentLight;
				var key2 = "PART_ItemsHeadersPresenter";
				var scrollViewer2 = Traverse<ScrollViewer>(tabControl, key2) as ScrollViewer;
				scrollViewer2?.ScrollToHome();
			};

			themes.Unchecked += (sender, args) =>
			{
				Application.Current.Styles[0] = App.FluentDark;
				var key2 = "PART_ItemsHeadersPresenter";
				var scrollViewer2 = Traverse<ScrollViewer>(tabControl, key2) as ScrollViewer;
				scrollViewer2?.ScrollToHome();
			};

			Classes.Add("sideBarCollapsed");

			tabControl.SelectionChanged += (sender, args) =>
			{
				var key = "PART_ItemsContentPresenter";
				var scrollViewer = Traverse<ScrollViewer>(tabControl, key) as ScrollViewer;
				scrollViewer?.ScrollToHome();
			};

			var sideBar = this.Find<ExperimentalAcrylicBorder>("SideBar");
			sideBar.PointerEnter += OnPointerEnter;
			PointerLeave += OnPointerLeave;

			var content = this.Find<ExperimentalAcrylicBorder>("Content");

			content.PointerEnter += OnPointerLeave;
			NotificationArea = new WindowNotificationManager(this)
			{
				Position = NotificationPosition.BottomRight,
				MaxItems = 3,
				BorderBrush = Brushes.Transparent,
			};
		}

		private void OnPointerEnter([CanBeNull] object sender, PointerEventArgs args)
		{
			if (!(args.Source is ExperimentalAcrylicBorder)) return;
			Classes.ReplaceOrAdd("sideBarCollapsed", "sideBarShown");
			SideBarCollapsed = false;
		}

		private void OnPointerLeave([CanBeNull] object sender, PointerEventArgs args)
		{
			if (!(args.Source is ExperimentalAcrylicBorder || args.Source is MainWindow)) return;
			Classes.ReplaceOrAdd("sideBarShown", "sideBarCollapsed");
			SideBarCollapsed = true;
		}

		static MainWindow() =>
			SideBarCollapsedProperty = AvaloniaProperty.Register<MainWindow, bool>("SideBarCollapsed",
				false, false, BindingMode.TwoWay);

		private static IVisual Traverse<T>(IVisual child, string key) where T : Control
		{
			if (child is T control && (control.Name?.Equals(key) ?? false))
			{
				return child;
			}

			IVisual result = null;

			foreach (var visualChild in child.GetVisualChildren())
			{
				result = Traverse<T>(visualChild, key);
				if (result != null) break;
			}

			return result;
		}

		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}
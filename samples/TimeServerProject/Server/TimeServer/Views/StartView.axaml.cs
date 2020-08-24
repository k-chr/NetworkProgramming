using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace TimeServer.Views
{
	public class StartView : UserControl
	{
		public static readonly StyledProperty<double> InfoWidthProperty;
		private double _infoWidth;

		public double InfoWidth
		{
			get => _infoWidth;
			set
			{
				_infoWidth = value - 70;
				RaisePropertyChanged(InfoWidthProperty, new Optional<double>(InfoWidthProperty.CoerceValue(this, 0)),
					new BindingValue<double>(_infoWidth));
			}
		}

		static StartView() => InfoWidthProperty = AvaloniaProperty.Register<StartView, double>(nameof(InfoWidth), double.NaN);

		public StartView() => InitializeComponent();

		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}
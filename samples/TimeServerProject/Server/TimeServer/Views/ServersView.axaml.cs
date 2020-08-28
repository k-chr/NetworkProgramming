using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;

namespace TimeServer.Views
{
	public class ServersView : UserControl
	{
		[UsedImplicitly] 
		public static readonly StyledProperty<double> InfoWidthProperty =
			AvaloniaProperty.Register<ServersView, double>(nameof(InfoWidth), double.NaN);

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

		public ServersView() => InitializeComponent();

		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using CustomControls.Models;

namespace CustomControls.Controls
{
	public class MessageBubble : TemplatedControl
	{
		public DateTime Date
		{
			set => SetValue(DateProperty, value);
		}

		public IBrush BubbleColor
		{
			set => SetValue(BubbleColorProperty, value);
		}

		public string Message
		{
			set => SetValue(MessageProperty, value);
		}

		public ClientModel Client
		{
			set => SetValue(ClientProperty, value);
		}

		public new static StyledProperty<HorizontalAlignment> HorizontalAlignmentProperty =
			AvaloniaProperty.Register<MessageBubble, HorizontalAlignment>(nameof(HorizontalAlignment));

		public static StyledProperty<Dock> DynamicDockProperty =
			AvaloniaProperty.Register<MessageBubble, Dock>(nameof(DynamicDock));

		public static StyledProperty<ClientModel> ClientProperty =
			AvaloniaProperty.Register<MessageBubble, ClientModel>(nameof(Client));

		public static StyledProperty<DateTime> DateProperty =
			AvaloniaProperty.Register<MessageBubble, DateTime>(nameof(Date), DateTime.Now);

		public static StyledProperty<IBrush> BubbleColorProperty =
			AvaloniaProperty.Register<MessageBubble, IBrush>(nameof(BubbleColor), Brush.Parse("Black"));

		public static StyledProperty<string> MessageProperty =
			AvaloniaProperty.Register<MessageBubble, string>(nameof(Message), string.Empty);

		public Dock DynamicDock
		{
			get => GetValue(DynamicDockProperty);
			set => SetValue(DynamicDockProperty, value);
		}

		public new HorizontalAlignment HorizontalAlignment
		{
			get => GetValue(HorizontalAlignmentProperty);
			set
			{
				SetValue(DynamicDockProperty, value == HorizontalAlignment.Left ? Dock.Right : Dock.Left);
				SetValue(HorizontalAlignmentProperty, value);
				base.SetValue(Button.HorizontalAlignmentProperty, value);
			}
		}
	}
}
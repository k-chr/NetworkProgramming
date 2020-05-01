using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;


namespace CustomControls.Controls
{
   public class SliderButton : Button
   {
      public static readonly StyledProperty<double> RadiusProperty = AvaloniaProperty.Register<Button, double>(nameof(Radius), 20);

      public static readonly StyledProperty<int> StateProperty =
          AvaloniaProperty.Register<Button, int>(nameof(State), 1);

      public static readonly StyledProperty<IBrush> ThumbBackgroundProperty =
         AvaloniaProperty.Register<Button, IBrush>(nameof(ThumbBackground));

      public static readonly StyledProperty<HorizontalAlignment> ButtonStateProperty =
         AvaloniaProperty.Register<Button, HorizontalAlignment>(nameof(ButtonState), HorizontalAlignment.Left);

      public double Radius
      {
         set => SetValue(RadiusProperty, value);
      }

      public  IBrush ThumbBackground
      {
         set => SetValue(ThumbBackgroundProperty, value);
      }

      public SliderButton()
      {
         State = 1;
      }

      protected override void OnClick()
      {
         var e = new RoutedEventArgs(ClickEvent);
         RaiseEvent(e);
         
         State = ~(State) + 2;
         if (!e.Handled && Command?.CanExecute(State) == true)
         {
            Command.Execute(State);
            e.Handled = true;
         }

      }


      public int State
      {
         get => GetValue(StateProperty);
         set 
         { 
            SetValue(StateProperty, value);
            ButtonState = value == 1 ? HorizontalAlignment.Left : HorizontalAlignment.Right;
         }
      }

      public HorizontalAlignment ButtonState
      {
         set
         {
            SetValue(ButtonStateProperty, value);
            InvalidateVisual();
         }
      }


      protected override void OnKeyDown(KeyEventArgs e)
      {
         base.OnKeyDown(e);
         switch (e.Key)
         {
            case Key.Left:
               if(State == 1) return;
               OnClick();
               break;
            case Key.Right:
               if(State == 0) return;
               OnClick();
               break;
            default:
               break;
         }
      }
   }
}

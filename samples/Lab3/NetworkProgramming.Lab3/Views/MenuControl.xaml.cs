﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NetworkProgramming.Lab3.Views
{
   public class MenuControl : UserControl
   {
      public MenuControl()
      {
         this.InitializeComponent();
      }

      private void InitializeComponent()
      {
         AvaloniaXamlLoader.Load(this);
      }
   }
}

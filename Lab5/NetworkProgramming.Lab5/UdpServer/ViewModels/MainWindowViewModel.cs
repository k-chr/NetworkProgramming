using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Interactivity;

namespace UdpServer.ViewModels
{
   public class MainWindowViewModel : ViewModelBase
   {
      private bool _isMainMenu = true;
      public bool IsMainMenu => _isMainMenu;

      public string Version => "1.0.0.0";
   }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using ReactiveUI;
using ThreadingUtilities.KamillimakThreading;
using ThreadingUtilities.KamillimakThreading.Helpers;

namespace Task2.ViewModels
{
   public  class MainWindowViewModel : ViewModelBase
   {
      private readonly Dictionary<string, SpecialThread> _threads; 
      private readonly Dictionary<string, StrRef> _threadsButtons; 

      public MainWindowViewModel()
      {

         Th1 =("Th1", "Start Th1").ToTuple();Th2 = ("Th2", "Start Th2").ToTuple();Th3 = ("Th3","Start Th3").ToTuple();Th4 = ("Th4", "Start Th4").ToTuple();
         Th5 = ("Th5", "Start Th5").ToTuple(); Th6 = ("Th6", "Start Th6").ToTuple(); Th7 = ("Th7", "Start Th7").ToTuple(); Th8 = ("Th8", "Start Th8").ToTuple();
         Th9 = ("Th9", "Start Th9").ToTuple(); Th10 = ("Th10", "Start Th10").ToTuple();
         ConsoleOutput = "";
         _threads = new Dictionary<string, SpecialThread>
         {
            {"Th10", new SpecialThread(UpdateConsole, 0) },
            {"Th1", new SpecialThread(UpdateConsole, 1)},
            {"Th2", new SpecialThread(UpdateConsole, 2) },
            {"Th3", new SpecialThread(UpdateConsole, 3)},
            {"Th4", new SpecialThread(UpdateConsole, 4)},
            {"Th5", new SpecialThread(UpdateConsole, 5)},
            {"Th6", new SpecialThread(UpdateConsole, 6)},
            {"Th7", new SpecialThread(UpdateConsole, 7) },
            {"Th8", new SpecialThread(UpdateConsole, 8)},
            {"Th9", new SpecialThread(UpdateConsole, 9)},
         };

         _threadsButtons = new Dictionary<string, StrRef>
         {
            {"Th10", Th10},
            {"Th1", Th1},
            {"Th2", Th2},
            {"Th3", Th3},
            {"Th4", Th4},
            {"Th5", Th5},
            {"Th6", Th6},
            {"Th7", Th7},
            {"Th8", Th8},
            {"Th9", Th9},
         };


      }

      protected override void ExecuteClosing(CancelEventArgs args)
      {
         foreach (var (key,value) in _threads)
         {
            value.Stop();
         }

         base.ExecuteClosing(args);
      }

      private void OnClick(string arg)
      {
         var arr = arg.Split(' ');
         switch (arr[0])
         {
            case "Start":
               _threads[arr[1]].Start();
               Update(arr[1], _threadsButtons[arr[1]].Replace("Start", "Suspend"));
               
               break;
            case "Suspend":
               _threads[arr[1]].Suspend();

               Update(arr[1],_threadsButtons[arr[1]].Replace("Suspend", "Resume"));
               break;
            case "Resume":
               _threads[arr[1]].Resume();
               Update(arr[1] ,_threadsButtons[arr[1]].Replace("Resume", "Suspend"));
               break;
            default:
               break;
         }
      }

      private void Update(string s, StrRef replace)
      {
         _threadsButtons[s] = replace;
         switch (s)
         {
            case "Th1":
               Th1 = replace;
               break;
            case "Th2":
               Th2 = replace;
               break;
            case "Th3":
               Th3 = replace;
               break;
            case "Th4":
               Th4 = replace;
               break;
            case "Th5":
               Th5 = replace;
               break;
            case "Th6":
               Th6 = replace;
               break;
            case "Th7":
               Th7 = replace;
               break;
            case "Th8":
               Th8 = replace;
               break;
            case "Th9":
               Th9 = replace;
               break;
            case "Th10":
               Th10 = replace;
               break;

         }
      }


      public StrRef Th10
      {
         get => _th10;
         set => this.RaiseAndSetIfChanged(ref _th10, value);
      }

      public StrRef Th1
      {
         get => _th1;
         set => this.RaiseAndSetIfChanged(ref _th1, value);
      }


      public StrRef Th2
      {
         get => _th2;
         set => this.RaiseAndSetIfChanged(ref _th2, value);
      }

      public StrRef Th3
      {
         get => _th3;
         set => this.RaiseAndSetIfChanged(ref _th3, value);
      }

      public StrRef Th4
      {
         get => _th4;
         set => this.RaiseAndSetIfChanged(ref _th4, value);
      }

      public StrRef Th5
      {
         get => _th5;
         set => this.RaiseAndSetIfChanged(ref _th5, value);
      }

      public StrRef Th6
      {
         get => _th6;
         set => this.RaiseAndSetIfChanged(ref _th6, value);
      }

      public StrRef Th7
      {
         get => _th7;
         set => this.RaiseAndSetIfChanged(ref _th7, value);
      }

      public StrRef Th8
      {
         get => _th8;
         set => this.RaiseAndSetIfChanged(ref _th8, value);
      }

      public StrRef Th9
      {
         get => _th9;
         set => this.RaiseAndSetIfChanged(ref _th9, value);
      }

      public string ConsoleOutput
      {
         get => _console;
         set => this.RaiseAndSetIfChanged(ref _console, value);
      }

      public string Greeting => "Welcome to Task2 Click on buttons to suspend or resume threads!";

      private StrRef _th10;
      private StrRef _th1;
      private StrRef _th2;
      private StrRef _th3;
      private StrRef _th4;
      private StrRef _th5;
      private StrRef _th6;
      private StrRef _th7;
      private StrRef _th8;
      private StrRef _th9;
      private string _console;


      public void UpdateConsole(string s)
      {
         ConsoleOutput += s;
      }
   }
}

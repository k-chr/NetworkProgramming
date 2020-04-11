using System;
using System.Collections.Generic;
using System.ComponentModel;
using ReactiveUI;
using ThreadingUtilities.KamillimakThreading;

namespace Task3.ViewModels
{
   public class MainWindowViewModel : ViewModelBase
   {
      private readonly Dictionary<string, SynchronizedThread> _threads;
      private object _lock;

      public MainWindowViewModel()
      {
         ConsoleOutput = "";
         _lock = new object();
         
         _threads = new Dictionary<string, SynchronizedThread>
         {
            {"Th10", new SynchronizedThread(UpdateConsole, 0, _lock) },
            {"Th1", new SynchronizedThread(UpdateConsole, 1, _lock)},
            {"Th2", new SynchronizedThread(UpdateConsole, 2, _lock) },
            {"Th3", new SynchronizedThread(UpdateConsole, 3, _lock)},
            {"Th4", new SynchronizedThread(UpdateConsole, 4, _lock)},
            {"Th5", new SynchronizedThread(UpdateConsole, 5, _lock)},
            {"Th6", new SynchronizedThread(UpdateConsole, 6, _lock)},
            {"Th7", new SynchronizedThread(UpdateConsole, 7, _lock) },
            {"Th8", new SynchronizedThread(UpdateConsole, 8, _lock)},
            {"Th9", new SynchronizedThread(UpdateConsole, 9, _lock)},
         };

         Start();
      }

      private void Start()
      {
         foreach (var (_, thread) in _threads)
         {
            thread.Start();
         }
      }

      protected override void ExecuteClosing(CancelEventArgs args)
      {
         foreach (var (key, value) in _threads)
         {
            value.Stop();
         }

         base.ExecuteClosing(args);
      }

      public string ConsoleOutput
      {
         get => _console;
         set => this.RaiseAndSetIfChanged(ref _console, value);
      }

      public string Greeting => "Welcome to Task3, this time only one thread in one time will do work specified in its body!";

      private string _console;


      public void UpdateConsole(string s)
      {
         ConsoleOutput += s;
      }
   }
}

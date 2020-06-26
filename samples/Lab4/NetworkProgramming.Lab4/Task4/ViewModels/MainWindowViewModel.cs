using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Threading;
using ReactiveUI;
using ThreadingUtilities.KamillimakThreading;

namespace Task4.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private Dictionary<string, CancellableTask> _tasks;
		public ObservableCollection<string> TaskNames { get; set; }
		public string SelectedTask { get; set; }

		public void OnCancelTask()
		{
			if (string.IsNullOrEmpty(SelectedTask)) return;
			_tasks[SelectedTask].CancelTask();
			TaskNames.Remove(SelectedTask);
			SelectedTask = "";
		}

		public MainWindowViewModel()
		{
			TaskNames = new ObservableCollection<string>(new List<string>
			{
				"Th1",
				"Th2",
				"Th3",
				"Th4",
				"Th5",
				"Th6",
				"Th7",
				"Th8",
				"Th9",
				"Th10"
			});

			ConsoleOutput = "";

			Start();
		}

		private void Start()
		{
			_tasks = new Dictionary<string, CancellableTask>
			{
				{"Th10", new CancellableTask(UpdateConsole, RemoveTask, 0)},
				{"Th1", new CancellableTask(UpdateConsole, RemoveTask, 1)},
				{"Th2", new CancellableTask(UpdateConsole, RemoveTask, 2)},
				{"Th3", new CancellableTask(UpdateConsole, RemoveTask, 3)},
				{"Th4", new CancellableTask(UpdateConsole, RemoveTask, 4)},
				{"Th5", new CancellableTask(UpdateConsole, RemoveTask, 5)},
				{"Th6", new CancellableTask(UpdateConsole, RemoveTask, 6)},
				{"Th7", new CancellableTask(UpdateConsole, RemoveTask, 7)},
				{"Th8", new CancellableTask(UpdateConsole, RemoveTask, 8)},
				{"Th9", new CancellableTask(UpdateConsole, RemoveTask, 9)},
			};

			foreach (var (_, task) in _tasks)
			{
				task.StartTask();
			}
		}

		protected override void ExecuteClosing(CancelEventArgs args)
		{
			foreach (var (_, value) in _tasks)
			{
				value.CancelTask();
			}

			base.ExecuteClosing(args);
		}

		public string ConsoleOutput
		{
			get => _console;
			set => this.RaiseAndSetIfChanged(ref _console, value);
		}

		public string Greeting =>
			"Welcome to Task4, this time every task will work asynchronously and user can cancel chosen task!";

		private string _console;

		private void UpdateConsole(string s)
		{
			Dispatcher.UIThread.InvokeAsync(() => ConsoleOutput += s);
		}

		private void RemoveTask(string taskId)
		{
			if (string.IsNullOrEmpty(taskId)) return;
			_tasks[taskId].CancelTask();
			Dispatcher.UIThread.InvokeAsync(() => TaskNames.Remove(taskId));
		}
	}
}
using System.ComponentModel;
using System.Reactive;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;

namespace NetworkProgramming.Lab5.ViewModels
{
	public class ViewModelBase : ReactiveObject
	{
		public ReactiveCommand<CancelEventArgs, Unit> ClosingCommand { get; }

		public ViewModelBase()
		{
			ClosingCommand = ReactiveCommand.Create<CancelEventArgs>(ExecuteClosing);
		}

		protected virtual void ExecuteClosing(CancelEventArgs args)
		{
			args.Cancel = false;
			((ClassicDesktopStyleApplicationLifetime) Avalonia.Application.Current.ApplicationLifetime).Shutdown(0);
		}
	}
}
using System.ComponentModel;
using System.Reactive;
using Avalonia.Controls.ApplicationLifetimes;
using JetBrains.Annotations;
using ReactiveUI;

namespace Task4.ViewModels
{
	public class ViewModelBase : ReactiveObject
	{
		[UsedImplicitly] 
		private ReactiveCommand<CancelEventArgs, Unit> _closingCommand;

		protected ViewModelBase()
		{
			_closingCommand = ReactiveCommand.Create<CancelEventArgs>(ExecuteClosing);
		}

		protected virtual void ExecuteClosing(CancelEventArgs args)
		{
			args.Cancel = false;
			((ClassicDesktopStyleApplicationLifetime) Avalonia.Application.Current.ApplicationLifetime).Shutdown();
		}
	}
}
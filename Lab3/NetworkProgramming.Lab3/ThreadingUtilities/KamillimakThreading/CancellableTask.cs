using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingUtilities.KamillimakThreading
{
   public class CancellableTask
   {
      private readonly CancellationToken _token;
      private readonly CancellationTokenSource _tokenSource;
      private Task _task;
      private readonly Action<string> _start;
      private readonly Action<string> _end;
      private readonly int _num;

      public CancellableTask(Action<string> startAction, Action<string> endAction,int num)
      {
         _end = endAction;
         _start = startAction;
         _tokenSource = new CancellationTokenSource();
         _token = _tokenSource.Token;
         _num = num;
      }

      public async void StartTask()
      {
         try
         {
            _task = Task.Run(Run, _token);
            await _task.ConfigureAwait(true);
         }
         catch
         {
            _start($" Task {_num} cancelled ");
         }
      }

      public async void Run()
      {
         var c = 'A';
         var stop = 'Z';
         while (c <= stop )
         {
            try
            {
               _start?.Invoke($"{c}{_num}");
               await Task.Delay(1000, _token).ConfigureAwait(true);
            }
            catch (Exception)
            {
               _start?.Invoke($" Task Th{(_num == 0 ? 10 : _num)} cancelled ");
               return;
            }

            ++c;
         }

         _end?.Invoke($"Th{(_num == 0 ? 10 : _num)}");
      }


      public void CancelTask()
      {
         _tokenSource.Cancel();
      }

   }
}

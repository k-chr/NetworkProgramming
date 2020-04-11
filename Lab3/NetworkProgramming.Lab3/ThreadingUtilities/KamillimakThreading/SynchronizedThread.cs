using System;
using System.Threading;

namespace ThreadingUtilities.KamillimakThreading
{
   public class SynchronizedThread
   {
      private readonly Thread _thread;
      private readonly ManualResetEvent _resetEvent = new ManualResetEvent(true);
      private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
      private Action<string> _start;
      private object _threadingLock;

      public SynchronizedThread(Action<string> start, int i, object @lock = null)
      {
         _start = start;
         _thread = new Thread(() => Proc(i));
         _threadingLock = @lock;
      }

      private void Proc(int num)
      {
         lock (_threadingLock)
         {
            var c = 'A';
            var stop = 'Z';
            while (c <= stop && !_stopEvent.WaitOne(0))
            {

               try
               {
                  _start?.Invoke( $"{c}{num}");
                  Thread.Sleep(1000);
                  _resetEvent.WaitOne();
               }
               catch (Exception)
               {

                  //ignored
               }

               ++c;
            }
         }
      }

      public void Suspend()
      {
         _resetEvent.Reset();
      }

      public void Resume()
      {
         _resetEvent.Set();
      }

      public void Start()
      {
         _thread.Start();
      }

      public void Stop()
      {
         try
         {
            _stopEvent.Set();
            _resetEvent.Set();
            _thread.Join(10);
         }
         catch
         {
            //ignored
         }
      }

   }

}
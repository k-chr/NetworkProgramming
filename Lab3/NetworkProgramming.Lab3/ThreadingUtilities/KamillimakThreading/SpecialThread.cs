using System;
using System.Threading;

namespace ThreadingUtilities.KamillimakThreading
{
   public class SpecialThread
   {
      private readonly Thread _thread;
      private readonly ManualResetEvent _resetEvent = new ManualResetEvent(true);
      private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
      private readonly Action<string> _start;

      public SpecialThread(Action<string> start, int i)
      {
         _start = start;
         _thread = new Thread(() => Proc(i));
      }

      private void Proc(int num)
      {
         var c = 'A';
         var stop = 'Z';

         while (c <= stop && !_stopEvent.WaitOne(0))
         {

            try
            {
               _start( $"{c}{num}");
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
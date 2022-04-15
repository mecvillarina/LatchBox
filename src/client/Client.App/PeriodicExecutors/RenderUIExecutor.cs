using System;
using System.Timers;

namespace Client.App.PeriodicExecutors
{
    public class RenderUIExecutor : IDisposable
    {
        private Timer _timer;
        private bool _running;

        public RenderUIExecutor()
        {
        }

        public void StartExecuting()
        {
            if (!_running)
            {
                _timer = new Timer();
                _timer.Interval = 3000;
                _timer.Elapsed += HandleTimer;
                _timer.AutoReset = true;
                _timer.Enabled = true;
                _running = true;
            }
        }

        async void HandleTimer(object source, ElapsedEventArgs e)
        {

        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}

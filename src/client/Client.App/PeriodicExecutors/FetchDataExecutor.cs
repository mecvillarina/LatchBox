using Client.App.Infrastructure.Managers;
using Client.App.Services;
using System;
using System.Timers;

namespace Client.App.PeriodicExecutors
{
    public class FetchDataExecutor : IDisposable
    {
        private Timer _timer;
        private bool _running;
        bool _isFetching;

        public void StartExecuting()
        {
            if (!_running)
            {
                _timer = new Timer();
                _timer.Interval = 10000;
                _timer.Elapsed += HandleTimer;
                _timer.AutoReset = true;
                _timer.Enabled = true;
                _running = true;
            }
        }

        async void HandleTimer(object source, ElapsedEventArgs e)
        {
            try
            {
                if (_isFetching)
                {
                    return;
                }

                _isFetching = true;
            }
            catch
            {
                Console.WriteLine($"Fetch Wallet Executor: Fetch Error");
            }
            finally
            {
                _isFetching = false;
            }
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }

}

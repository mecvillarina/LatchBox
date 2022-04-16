using MudBlazor;
using MudBlazor.Services;
using System;
using System.Threading.Tasks;

namespace Client.Services
{
    public class AppBreakpointService : IAsyncDisposable
    {
        private readonly IBreakpointService _breakpointService;

        public AppBreakpointService(IBreakpointService breakpointService)
        {
            _breakpointService = breakpointService;
        }

        private bool _isInitialized;
        public Breakpoint CurrentBreakpoint { get; set; }
        public Guid BreakpointSubscriptionId { get; set; }

        public event EventHandler<Breakpoint> BreakpointChanged;

        public async Task InitAsync()
        {
            if (!_isInitialized)
            {
                Console.WriteLine("Init 2");
                var subscriptionResult = await _breakpointService.Subscribe((breakpoint) =>
                {
                    Console.WriteLine("Init 2: Breakpoint Changed");

                    CurrentBreakpoint = breakpoint;
                    BreakpointChanged?.Invoke(this, breakpoint);
                }, new ResizeOptions
                {
                    ReportRate = 100,
                    NotifyOnBreakpointOnly = true,
                });

                CurrentBreakpoint = await _breakpointService.GetBreakpoint();
                BreakpointSubscriptionId = subscriptionResult.SubscriptionId;
                BreakpointChanged?.Invoke(this, CurrentBreakpoint);
                _isInitialized = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _breakpointService.Unsubscribe(BreakpointSubscriptionId);
            _isInitialized = false;
        }
    }
}

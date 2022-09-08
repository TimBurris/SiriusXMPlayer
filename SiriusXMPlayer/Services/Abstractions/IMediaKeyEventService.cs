using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Abstractions
{
    public interface IMediaKeyEventService : Microsoft.Extensions.Hosting.IHostedService
    {
        event EventHandler PlayPausePressed;
        event EventHandler NextTrackPressed;
        event EventHandler PreviousTrackPressed;
    }
}

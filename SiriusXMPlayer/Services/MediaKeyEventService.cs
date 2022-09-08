using Microsoft.Extensions.Logging;
using Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Services;

//this class is responsible for createing "hotkeys" using the windows Api so that it can watch for the media key (playpause, nexttrack, and previoustrack)
//  on start, hook up even handlers for the 3 keys, then on stop detach
public class MediaKeyEventService : Abstractions.IMediaKeyEventService
{

    //This is the message that will be posted when any registered hotkey is pressed
    const int WM_HOTKEY = 0x0312;

    //these are the custom hotkey ids that our app will use.  so when we register a hotkey we choose what id we want it to have
    private const int PLAY_PAUSE_HOTKEY_ID = 9000;
    private const int NEXT_TRACK_HOTKEY_ID = 9001;
    private const int PREV_TRACK_HOTKEY_ID = 9002;

    //Modifiers: you can find these Modifiers documented here: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
    private const uint MOD_NONE = 0x0000; //(none)
    //private const uint MOD_ALT = 0x0001; //ALT
    //private const uint MOD_CONTROL = 0x0002; //CTRL
    //private const uint MOD_SHIFT = 0x0004; //SHIFT
    //private const uint MOD_WIN = 0x0008; //WINDOWS

    //these Virtual key Codes are documented: https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
    private const uint VK_MEDIA_PLAY_PAUSE = 0xB3;
    private const uint VK_MEDIA_NEXT_TRACK = 0xB0;
    private const uint VK_MEDIA_PREV_TRACK = 0xB1;
    private readonly ILogger<MediaKeyEventService> _logger;
    private readonly IAppHandleProvider _appHandleProvider;//this little fella will get us a pointer to the window, which the windows api requires us to provide

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private IntPtr _windowHandle;
    private HwndSource? _source;

    public MediaKeyEventService(ILogger<MediaKeyEventService> logger, IAppHandleProvider appHandleProvider)
    {
        _logger = logger;
        _appHandleProvider = appHandleProvider;
    }

    #region IMediaKeyEventService Implementation

    public event EventHandler? PlayPausePressed;
    public event EventHandler? NextTrackPressed;
    public event EventHandler? PreviousTrackPressed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering the media key hotkeys");

        _windowHandle = _appHandleProvider.GetAppHandle();
        _source = HwndSource.FromHwnd(_windowHandle);
        _source.AddHook(HwndHook);

        RegisterHotKey(_windowHandle, PLAY_PAUSE_HOTKEY_ID, MOD_NONE, VK_MEDIA_PLAY_PAUSE);
        RegisterHotKey(_windowHandle, NEXT_TRACK_HOTKEY_ID, MOD_NONE, VK_MEDIA_NEXT_TRACK);
        RegisterHotKey(_windowHandle, PREV_TRACK_HOTKEY_ID, MOD_NONE, VK_MEDIA_PREV_TRACK);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unregistering the media key hotkeys");

        _source?.RemoveHook(HwndHook);
        UnregisterHotKey(_windowHandle, PLAY_PAUSE_HOTKEY_ID);
        UnregisterHotKey(_windowHandle, NEXT_TRACK_HOTKEY_ID);
        UnregisterHotKey(_windowHandle, PREV_TRACK_HOTKEY_ID);

        return Task.CompletedTask;
    }

    #endregion

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_HOTKEY:
                switch (wParam.ToInt32())
                {
                    case PLAY_PAUSE_HOTKEY_ID:
                        this.PlayPausePressed?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;
                    case NEXT_TRACK_HOTKEY_ID:
                        this.NextTrackPressed?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;

                    case PREV_TRACK_HOTKEY_ID:
                        this.PreviousTrackPressed?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;
                }
                break;
        }
        return IntPtr.Zero;
    }


}

using Microsoft.Extensions.Logging;
using Services.Abstractions;
using System;
using System.Windows;

namespace SiriusXMPlayer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    //these button classes are specific to the HTML for siriusxm. if they change their code we'll have to adjust
    private const string _siriusPreviousTrackButtonClassName = "skip-back-btn";
    private const string _siriusNextTrackButtonClassName = "skip-forward-btn";
    private const string _siriusPlayPauseButtonClassName = "play-pause-btn";

    private readonly MainWindowViewModel _viewModel;
    private readonly IMediaKeyEventService _mediaKeyEventService;
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(MainWindowViewModel viewModel,
        Services.Abstractions.IMediaKeyEventService mediaKeyEventService,
        ILogger<MainWindow> logger)
    {
        InitializeComponent();

        this.DataContext = viewModel;
        _viewModel = viewModel;
        _mediaKeyEventService = mediaKeyEventService;
        _logger = logger;


        _mediaKeyEventService.PlayPausePressed += _mediaKeyEventService_PlayPausePressed;
        _mediaKeyEventService.NextTrackPressed += _mediaKeyEventService_NextTrackPressed;
        _mediaKeyEventService.PreviousTrackPressed += _mediaKeyEventService_PreviousTrackPressed;


        _viewModel.OnBound();

        //startuplocation does not support binding, so we have to manually set it, we are banking on it not changing after initial load, so we aren't watching for property change
        this.WindowStartupLocation = _viewModel.WindowStartupLocation;
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.OnUnbound();

        if (_mediaKeyEventService != null)
        {
            _mediaKeyEventService.PlayPausePressed -= _mediaKeyEventService_PlayPausePressed;
            _mediaKeyEventService.NextTrackPressed -= _mediaKeyEventService_NextTrackPressed;
            _mediaKeyEventService.PreviousTrackPressed -= _mediaKeyEventService_PreviousTrackPressed;
        }
        base.OnClosed(e);
    }


    private void _mediaKeyEventService_NextTrackPressed(object? sender, EventArgs e)
    {
        _logger.LogInformation("Next Track");
        this.PressButton(_siriusNextTrackButtonClassName);
    }

    private void _mediaKeyEventService_PreviousTrackPressed(object? sender, EventArgs e)
    {
        _logger.LogInformation("Previous Track");
        this.PressButton(_siriusPreviousTrackButtonClassName);
    }

    private void _mediaKeyEventService_PlayPausePressed(object? sender, EventArgs e)
    {
        _logger.LogInformation("PlayPause");
        this.PressButton(_siriusPlayPauseButtonClassName);
    }

    private void PressButton(string className)
    {
        //sirius is not using jquery so we are using vanilla JS, which is fine
        //   we are invoking click on the first matching element... of course what if there are more than one or none... 
        //   that would mean they changed their code and we'll have to adjust this
        var t = browser.ExecuteScriptAsync($"document.getElementsByClassName('{className}')[0].click();");
    }
}

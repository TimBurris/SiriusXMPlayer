using Microsoft.Extensions.Logging;
using Services.Abstractions;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace SiriusXMPlayer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    //these button classes are specific to the HTML for siriusxm. if they change their code we'll have to adjust
    private const string _siriusPreviousTrackSelector = "button[aria-label='Skip Back']";
    private const string _siriusNextTrackButtonSelector= "button[aria-label='Skip Forward']";
    private const string _siriusPlayPauseButtonSelector = "button[aria-label='Play'], button[aria-label='Pause']";

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
        _viewModel.OnBound();
        _mediaKeyEventService = mediaKeyEventService;
        _logger = logger;

        browser.CoreWebView2InitializationCompleted += Browser_CoreWebView2InitializationCompleted;

        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var t = InitializeBrowserAndSetupAsync();
    }

    private void Browser_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
        {
            _logger.LogError(e.InitializationException, "WebView failed to intialize");

            if (e.InitializationException is Microsoft.Web.WebView2.Core.WebView2RuntimeNotFoundException)
            {
                browser.Visibility = Visibility.Collapsed;
                NeedWebViewDownloadPane.Visibility = Visibility.Visible;
            }
        }
    }

    protected async Task InitializeBrowserAndSetupAsync()
    {
        await browser.EnsureCoreWebView2Async();

        //webview needs to be initialized before we start doing stuff with it, so that's why this code is here, rather than in ctor
        _mediaKeyEventService.PlayPausePressed += _mediaKeyEventService_PlayPausePressed;
        _mediaKeyEventService.NextTrackPressed += _mediaKeyEventService_NextTrackPressed;
        _mediaKeyEventService.PreviousTrackPressed += _mediaKeyEventService_PreviousTrackPressed;

        //startuplocation does not support binding, so we have to manually set it, we are banking on it not changing after initial load, so we aren't watching for property change
        this.WindowStartupLocation = _viewModel.WindowStartupLocation;

        //now that we are initialized, setup the binding
        browser.SetBinding(Microsoft.Web.WebView2.Wpf.WebView2.SourceProperty, nameof(MainWindowViewModel.SiteUrl));
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.OnUnbound();
        this.Loaded -= MainWindow_Loaded;
        browser.CoreWebView2InitializationCompleted -= Browser_CoreWebView2InitializationCompleted;

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
        this.PressButton(_siriusNextTrackButtonSelector);
    }

    private void _mediaKeyEventService_PreviousTrackPressed(object? sender, EventArgs e)
    {
        _logger.LogInformation("Previous Track");
        this.PressButton(_siriusPreviousTrackSelector);
    }

    private void _mediaKeyEventService_PlayPausePressed(object? sender, EventArgs e)
    {
        _logger.LogInformation("PlayPause");
        this.PressButton(_siriusPlayPauseButtonSelector);
    }

    private void PressButton(string selectorName)
    {
        //sirius is not using jquery so we are using vanilla JS, which is fine
        //   we are invoking click on the first matching element... of course what if there are more than one or none... 
        //   that would mean they changed their code and we'll have to adjust this
        var t = browser.ExecuteScriptAsync($"document.querySelectorAll(\"{selectorName}\")[0].click();");
    }

    private void Exit_MenuItem_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}

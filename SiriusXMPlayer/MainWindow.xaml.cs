using Microsoft.Extensions.Logging;
using Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SiriusXMPlayer
{
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

        public MainWindow(Jot.Tracker tracker, MainWindowViewModel viewModel,
            Services.Abstractions.IMediaKeyEventService mediaKeyEventService,
            ILogger<MainWindow> logger)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            tracker.Track(this);
            _viewModel = viewModel;
            _mediaKeyEventService = mediaKeyEventService;
            _logger = logger;


            _mediaKeyEventService.PlayPausePressed += _mediaKeyEventService_PlayPausePressed;
            _mediaKeyEventService.NextTrackPressed += _mediaKeyEventService_NextTrackPressed;
            _mediaKeyEventService.PreviousTrackPressed += _mediaKeyEventService_PreviousTrackPressed;
        }

        protected override void OnClosed(EventArgs e)
        {
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
            _logger.LogInformation(className);
            var t = browser.ExecuteScriptAsync($"document.getElementsByClassName('{className}')[0].click();");
        }
    }
}

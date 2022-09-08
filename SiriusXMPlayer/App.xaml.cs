using Jot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SiriusXMPlayer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    internal static IHost? AppHost { get; set; }

    public App()
    {
        AppHost = HostBuilder.Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        //apphost will be null only if there was an error initializing
        if (AppHost == null)
        {
            this.Shutdown();
            return;
        }

        var logger = AppHost.Services.GetRequiredService<ILogger<App>>();

        try
        {
            SetupExceptionHandling(logger);

            await AppHost.StartAsync();

            SetupWindowTracking();
            logger.LogInformation("start form");

            var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
            startupForm.Show();

            //i know this is a "hosted service" but it depends on our main windows being initialized, so we are manually running this service
            var mediaKeyEvenService = AppHost.Services.GetRequiredService<Services.Abstractions.IMediaKeyEventService>();

            await mediaKeyEvenService.StartAsync(cancellationToken: System.Threading.CancellationToken.None);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "you are a terrible developer");

            //   we must flush, otherwise the console might close out before logs are done writing
            Serilog.Log.CloseAndFlush();

            this.Shutdown();
        }
        base.OnStartup(e);
    }


    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            var mediaKeyEvenService = AppHost.Services.GetService<Services.Abstractions.IMediaKeyEventService>();
            //the only reason it would be null is if there was an issue booting up
            if (mediaKeyEvenService != null)
            {
                await mediaKeyEvenService.StopAsync(cancellationToken: System.Threading.CancellationToken.None);
            }
        }
        catch { }

        //   we must flush, otherwise the console might close out before logs are done writing
        Serilog.Log.CloseAndFlush();

        base.OnExit(e);

    }

    private void SetupWindowTracking()
    {
        // 1. tell the tracker how to track Window objects 
        var tracker = AppHost!.Services.GetRequiredService<Tracker>();

        tracker.Configure<Window>()
            .Id(w => w.Name)
            .Properties(w => new { w.Top, w.Width, w.Height, w.Left, w.WindowState })
            .PersistOn(nameof(Window.Closing))
            .StopTrackingOn(nameof(Window.Closing));
    }

    private void SetupExceptionHandling(ILogger logger)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            logger.LogError((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        DispatcherUnhandledException += (s, e) =>
        {
            logger.LogError(e.Exception, "Application.Current.DispatcherUnhandledException");
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            logger.LogError(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        };

        logger.LogInformation("exception handling is setup");
    }

}

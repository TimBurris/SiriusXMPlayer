using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Jot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SiriusXMPlayer;

// i pulled this out into it's own separate file because there was jsut so much going on,
//  and there were conflicts with ILogger because for startup we use serilog until DI is ready,
//  then we switch to Microsoft ILogger
public class HostBuilder
{
    public static IHost? Build()
    {
        //do this first because serilogger wants to use it to get it's settings
        var configuration = CreateConfigurationRoot();

        //create an instance of serilogger, using appsettings, so we can log what happens on startup
        //NOTE:  this is just creating one instance, it is NOT setting it up for DI, or hooking it up to microsoft loggin.  that work is done in CreateHostBuilder
        var seriLogger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            seriLogger.Information("Starting app...");
            var builder = Host.CreateDefaultBuilder();

            //tell the builder that we want to use our configuration that we initialized from appsettings
            builder.ConfigureAppConfiguration(x => x.AddConfiguration(configuration));

            //we put configure services into a separate method because its so verbose
            builder.ConfigureServices(ConfigureServices);

            //useserilog plugs serilog in so that we can user Microsoft.Extensions.Logger for logging
            builder.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                   .ReadFrom.Configuration(context.Configuration)//this is what lets serilog get it's settings from appsetting
                   .Enrich.FromLogContext());
            //builder.Logging.AddSerilog();

            //create the host, after this point, builder cannot be used because it will have been built
            return builder.Build();
        }
        catch (Exception ex)
        {
            //use seriLogger because if host builder failed, then microsoft logger is not ready
            seriLogger.Error(ex, "bad news bro");

            //   we must flush, otherwise the console might close out before logs are done writing
            Log.CloseAndFlush();
            return null;
        }
    }


    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddTransient<Services.Abstractions.IAppHandleProvider, Services.AppHandleProvider>();
        services.AddTransient<MainWindowViewModel>();

        //register our PlayerSettings so it can be injected as needed 
        services.Configure<PlayerSettings>(context.Configuration.GetSection("PlayerSettings"));

        //not a fan of singletons, but these really need to be singleton
        services.AddSingleton<Tracker>(new Tracker());
        services.AddSingleton<MainWindow>();
        services.AddSingleton<Services.Abstractions.IMediaKeyEventService, Services.MediaKeyEventService>();
    }

    private static IConfigurationRoot CreateConfigurationRoot()
    {
        //tell our app it's configuation comes from appsettings.json
        return new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();
    }
}

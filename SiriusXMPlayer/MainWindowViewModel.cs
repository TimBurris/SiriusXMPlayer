using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;

namespace SiriusXMPlayer;

public class MainWindowViewModel : NinjaMvvm.NotificationBase
{
	private readonly ILogger<MainWindowViewModel> _logger;
	private readonly Jot.Tracker _tracker;

	public MainWindowViewModel(ILogger<MainWindowViewModel> logger,
		IOptions<PlayerSettings> playerSettings,
		Jot.Tracker tracker)
	{
		_logger = logger;
		_tracker = tracker;
		this.SiteUrl = playerSettings.Value.SiriusXMStartupUrl;
		this.WebViewDownloadUrl = playerSettings.Value.WebView2DownloadUrl;

		//defaults
		this.WindowState = System.Windows.WindowState.Normal;
		this.WindowLeft = null;
		this.WindowTop = null;
		this.WindowWidth = 500;
		this.WindowHeight = 400;

		//tell tracker which properties we want saved/restored
		tracker.Configure<MainWindowViewModel>()
			.Id(w => "MainWindow")
			.Properties(w => new { w.WindowTop, w.WindowWidth, w.WindowHeight, w.WindowLeft, w.WindowState });//do not save startup location, we'll compute that

		_logger.LogInformation("ViewModel Ready, site is {siteurl}", this.SiteUrl);
	}

	public void OnBound()
	{
		//restore settings
		_tracker.Apply(this);

		//compute the startuplocation based on whether or not ther are coordinates stored; essentially first run will be center screen, after that manual
		if (this.WindowLeft == null || this.WindowTop == null)
		{
			this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
		}
		else
		{
			this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
		}
	}

	public void OnUnbound()
	{
		//persist settings
		_tracker.Persist(this);
	}

	#region Binding Properties

	public string WebViewDownloadUrl
	{
		get { return GetField<string>(); }
		set { SetField(value); }
	}

	public string SiteUrl
	{
		get { return GetField<string>(); }
		set { SetField(value); }
	}

	public string WindowTitle { get; } = "SiriusXM Player";

	public double? WindowTop
	{
		get { return GetField<double>(); }
		set { SetField(value); }
	}

	public double? WindowLeft
	{
		get { return GetField<double>(); }
		set { SetField(value); }
	}

	public double WindowWidth
	{
		get { return GetField<double>(); }
		set { SetField(value); }
	}

	public double WindowHeight
	{
		get { return GetField<double>(); }
		set { SetField(value); }
	}

	public System.Windows.WindowState WindowState
	{
		get { return GetField<System.Windows.WindowState>(); }
		set
		{
			if (SetField(value))
			{
				this.WindowShowInTaskbar = (value != System.Windows.WindowState.Minimized);//if not minimized we should show it in taskbar
			}
		}
	}

	public System.Windows.WindowStartupLocation WindowStartupLocation
	{
		get { return GetField<System.Windows.WindowStartupLocation>(); }
		set { SetField(value); }
	}

	public bool WindowShowInTaskbar
	{
		get { return GetField<bool>(); }
		set { SetField(value); }
	}

	#endregion

	#region ToggleMinimized Command

	public RelayCommand ToggleMinimizedCommand => new RelayCommand((param) => this.ToggleMinimized(), (param) => this.CanToggleMinimized());

	public bool CanToggleMinimized()
	{
		return true;
	}

	/// <summary>
	/// Executes the ToggleMinimized command 
	/// </summary>
	public void ToggleMinimized()
	{
		if (this.WindowState == System.Windows.WindowState.Minimized)
		{
			this.WindowState = System.Windows.WindowState.Normal;
		}
		else
		{
			this.WindowState = System.Windows.WindowState.Minimized;
		}
	}

	#endregion

	#region DownloadWebView Command

	public RelayCommand DownloadWebViewCommand => new RelayCommand((param) => this.DownloadWebView(), (param) => this.CanDownloadWebView());

	public bool CanDownloadWebView()
	{
		return true;
	}

	/// <summary>
	/// Executes the DownloadWebView command 
	/// </summary>
	public void DownloadWebView()
	{
		var info = new ProcessStartInfo(this.WebViewDownloadUrl);
		info.UseShellExecute = true;//you have to useshellexecute in .net core
		Process.Start(info);
	}

	#endregion

}

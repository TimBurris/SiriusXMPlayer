using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

	public string SiteUrl
	{
		get { return GetField<string>(); }
		set { SetField(value); }
	}

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
		set { SetField(value); }
	}

	public System.Windows.WindowStartupLocation WindowStartupLocation
	{
		get { return GetField<System.Windows.WindowStartupLocation>(); }
		set { SetField(value); }
	}

}

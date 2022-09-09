using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

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

	#region Binding Properties

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

	private RelayCommand _toggleMinimizedCommand;
	public RelayCommand ToggleMinimizedCommand
	{
		get
		{
			if (_toggleMinimizedCommand == null)
				_toggleMinimizedCommand = new RelayCommand((param) => this.ToggleMinimized(), (param) => this.CanToggleMinimized());
			return _toggleMinimizedCommand;
		}
	}

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



}
/// <summary>
/// A command whose sole purpose is to 
/// relay its functionality to other
/// objects by invoking delegates. The
/// default return value for the CanExecute
/// method is 'true'.
/// </summary>
public class RelayCommand<T> : NinjaMvvm.RelayCommandBase<T>
{
	public RelayCommand(Action<T> execute) : base(execute) { }

	public RelayCommand(Action<T> execute, Predicate<T> canExecute) : base(execute, canExecute) { }

	public RelayCommand(Action<T> execute, Predicate<T> canExecute, string label) : base(execute, canExecute, label) { }

	public override event EventHandler CanExecuteChanged
	{
		add { System.Windows.Input.CommandManager.RequerySuggested += value; }
		remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
	}

	public void Execute(T? parameter)
	{
		base.Execute((object?)parameter);
	}

	public bool CanExecute(T? parameter)
	{
		return base.CanExecute((object?)parameter);
	}
}

/// <summary>
/// provides a generic object implementation of <see cref="RelayCommandBase&lt;T&gt;"/>
/// </summary>
public class RelayCommand : RelayCommand<object>
{
	public RelayCommand(Action execute) : base(new Action<object>((param) => execute())) { }
	public RelayCommand(Action execute, Func<bool> canExecute) : base(new Action<object>((param) => execute()), new Predicate<object>((param) => canExecute())) { }
	public RelayCommand(Action<object> execute) : base(execute) { }

	public RelayCommand(Action<object> execute, Predicate<object> canExecute) : base(execute, canExecute) { }

	public RelayCommand(Action<object> execute, Predicate<object> canExecute, string label) : base(execute, canExecute, label) { }

	public void Execute()
	{
		base.Execute(parameter: null);
	}

	public bool CanExecute()
	{
		return base.CanExecute(parameter: null);
	}
}

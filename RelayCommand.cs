using System;

namespace SiriusXMPlayer;

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

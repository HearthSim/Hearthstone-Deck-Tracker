using System;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class Command : ICommand
	{
		private readonly Action _action;

		public Command(Action action)
		{
			_action = action;
		}

		public bool CanExecute(object parameter) => _action != null;

		public void Execute(object parameter) => _action.Invoke();

		public event EventHandler CanExecuteChanged;
	}

	public class Command<T> : ICommand
	{
		private readonly Action<T> _action;

		public Command(Action<T> action)
		{
			_action = action;
		}
		
		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter) => _action != null;

		public void Execute(object parameter) => _action.Invoke((T)parameter);
	}
}
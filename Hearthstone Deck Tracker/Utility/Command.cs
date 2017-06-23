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

#pragma warning disable 67
		public event EventHandler CanExecuteChanged;
#pragma warning restore 67
	}

	public class Command<T> : ICommand
	{
		private readonly Action<T> _action;

		public Command(Action<T> action)
		{
			_action = action;
		}

#pragma warning disable 67
		public event EventHandler CanExecuteChanged;
#pragma warning restore 67

		public bool CanExecute(object parameter) => _action != null;

		public void Execute(object parameter) => _action.Invoke((T)parameter);
	}
}

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
#pragma warning disable 0067
		public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
	}
}
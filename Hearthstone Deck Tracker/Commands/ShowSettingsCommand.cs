using System;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Commands;

public class ShowSettingsCommand : ICommand
{
	public enum Focus
	{
		Default,
		Account,
		Battlegrounds,
		Notifications,
		Arena,
	}

	private Focus? ParseParameter(object? parameter)
	{
		if(parameter is null)
			return Focus.Default;
		if(parameter is Focus focus)
			return focus;
		if(parameter is string s && Enum.TryParse(s, out focus))
			return focus;
		return null;
	}

	// See Command<T> for why we don't use this.
	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter)
	{
		if(ParseParameter(parameter) is not { } focus)
			return;

		switch(focus)
		{
			case Focus.Default:
			case Focus.Account:
				Core.MainWindow.Options.TreeViewItemHSReplayAccount.IsSelected = true;
				break;
			case Focus.Battlegrounds:
				Core.MainWindow.Options.TreeViewItemOverlayBattlegrounds.IsSelected = true;
				break;
			case Focus.Notifications:
				Core.MainWindow.Options.TreeViewItemTrackerNotifications.IsSelected = true;
				break;
			case Focus.Arena:
				Core.MainWindow.Options.TreeViewItemOverlayArena.IsSelected = true;
				break;
		}
		Core.MainWindow.FlyoutOptions.IsOpen = true;
		Core.MainWindow.ActivateWindow();
	}

	public event EventHandler? CanExecuteChanged;
}

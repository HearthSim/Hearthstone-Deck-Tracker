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
		General,
		Mulligan,
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

		var options = Core.MainWindow.Options;
		options.ShowPage(focus switch
		{
			Focus.Battlegrounds => options.TreeViewItemOverlayBattlegrounds,
			Focus.Notifications => options.TreeViewItemTrackerNotifications,
			Focus.Arena => options.TreeViewItemOverlayArena,
			Focus.General => options.TreeViewItemOverlayGeneral,
			Focus.Mulligan => options.TreeViewItemOverlayMulligan,
			_ => options.TreeViewItemHSReplayAccount,
		});
		Core.MainWindow.FlyoutOptions.IsOpen = true;
		Core.MainWindow.ActivateWindow();
	}

	public event EventHandler? CanExecuteChanged;
}

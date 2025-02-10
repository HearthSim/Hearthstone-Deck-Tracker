using System.Windows.Input;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Commands;

public static class GlobalCommands
{
	public static ICommand OpenUrl { get; } = new Command<string>(url => Helper.TryOpenUrl(url));

	public static ICommand OpenReplay { get; } = new Command<GameStats>(game =>
	{
		if(game != null)
			_ = ReplayLauncher.ShowReplay(game, true);
	});

	public static ICommand ShowSettings { get; } = new ShowSettingsCommand();

	public static ICommand SignInCommand { get; } = new Command(() =>
	{
		ShowSettings.Execute(ShowSettingsCommand.Focus.Account);
		_ = HSReplayNetHelper.TryAuthenticate();
	});
}

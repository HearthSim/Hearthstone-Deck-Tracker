using System.Windows.Input;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class GlobalCommands
	{
		public static ICommand OpenUrl => new Command<string>(url => Helper.TryOpenUrl(url));

		public static ICommand OpenReplay => new Command<GameStats>(game =>
		{
			if(game != null)
				ReplayLauncher.ShowReplay(game, true).Forget();
		});
	}
}

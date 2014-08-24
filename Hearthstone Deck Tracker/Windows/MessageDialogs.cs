using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Windows
{
	public class MessageDialogs
	{
		public static async Task<MessageDialogResult> ShowDeleteGameStatsMessage(MetroWindow window, GameStats stats)
		{
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
			return
				await
				window.ShowMessageAsync("Delete Game",
				                        stats.Result + " vs " + stats.OpponentHero + "\nfrom " + stats.StartTime + "\n\nAre you sure?",
				                        MessageDialogStyle.AffirmativeAndNegative, settings);
		}

		public static async Task<MessageDialogResult> ShowDeleteMultipleGameStatsMessage(MetroWindow window, int count)
		{
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
			return
				await
				window.ShowMessageAsync("Delete Games",
				                        "This will delete the selected games (" + count + ").\n\nAre you sure?",
				                        MessageDialogStyle.AffirmativeAndNegative, settings);
		}
	}
}
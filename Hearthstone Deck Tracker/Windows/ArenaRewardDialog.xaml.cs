using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for ArenaRewardDialog.xaml
	/// </summary>
	public partial class ArenaRewardDialog
	{
		private readonly Deck _deck;
		public ArenaRewardDialog(Deck deck)
		{
			_deck = deck;
			InitializeComponent();
			ArenaRewards.LoadArenaReward(deck.ArenaReward);
		}

		private async void ArenaRewards_OnSave(object sender, RoutedEventArgs e)
		{
			string warning;
			if(!ArenaRewards.Validate(out warning))
			{
				await this.ShowMessage("Error", warning);
				return;
			}
			_deck.ArenaReward = ArenaRewards.Reward;
			DeckList.Save();
			ArenaStats.Instance.UpdateArenaRewards();
			ArenaStats.Instance.UpdateArenaRuns();
			Close();
		}
	}
}

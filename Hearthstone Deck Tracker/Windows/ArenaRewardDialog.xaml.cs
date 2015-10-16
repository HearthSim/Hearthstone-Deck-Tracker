using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for ArenaRewardDialog.xaml
	/// </summary>
	public partial class ArenaRewardDialog
	{
		private readonly Deck _deck;
		public bool SaveButtonWasClicked { get; set; }
		public ArenaRewardDialog(Deck deck)
		{
			_deck = deck;
			InitializeComponent();
			ArenaRewards.LoadArenaReward(deck.ArenaReward);
		}
		private async void ButtonSave_OnClick(object sender, RoutedEventArgs e)
		{
			string warning;
			if(!ArenaRewards.Validate(out warning))
			{
				await this.ShowMessage("Error saving Arena Rewards", warning);
				return;
			}
			_deck.ArenaReward = ArenaRewards.Reward;
			DeckList.Save();
			SaveButtonWasClicked = true;
			Close();
		}
	}
}

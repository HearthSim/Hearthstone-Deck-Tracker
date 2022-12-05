using Hearthstone_Deck_Tracker.Utility;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking
{
	public sealed partial class BattlegroundsHeroPicking
	{
		public BattlegroundsHeroPicking()
		{
			InitializeComponent();
		}

		public ICommand SetSelectedHeroDbfIdCommand => new Command<int>(value => ((BattlegroundsHeroPickingViewModel)DataContext).SelectedHeroDbfId = value);
	}
}

using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPowerPicking;

public partial class BattlegroundsSingleHeroPower
{
	public BattlegroundsSingleHeroPower()
	{
		InitializeComponent();
	}

	private async void AvgPlacementTrigger_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if(await Debounce.WasCalledAgain(100, "AvgPlacementTrigger"))
			return;
		if(DataContext is BattlegroundsHeroHeaderViewModel vm)
			vm.OnPlacementHover?.Invoke(true);
	}

	private async void AvgPlacementTrigger_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if(await Debounce.WasCalledAgain(100, "AvgPlacementTrigger"))
			return;
		if(DataContext is BattlegroundsHeroHeaderViewModel vm)
			vm.OnPlacementHover?.Invoke(false);
	}
}

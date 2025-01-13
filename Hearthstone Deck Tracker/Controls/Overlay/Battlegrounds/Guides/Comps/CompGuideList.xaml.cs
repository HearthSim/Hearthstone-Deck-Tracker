using System;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;

public partial class CompGuideList
{
	public CompGuideList()
	{
		InitializeComponent();
	}

	private void CompGuide_CompClicked(object sender, EventArgs e)
	{
		if (sender is CompButton { DataContext: BattlegroundsCompGuideViewModel selectedComp })
		{
			((BattlegroundsCompsGuidesViewModel)DataContext).SelectedComp = selectedComp;
			Core.Game.Metrics.BattlegroundsCompGuidesClicks++;
		}
	}

	private void CompGuide_BackButtonClicked(object sender, EventArgs e)
	{
		((BattlegroundsCompsGuidesViewModel)DataContext).SelectedComp = null;
	}
}



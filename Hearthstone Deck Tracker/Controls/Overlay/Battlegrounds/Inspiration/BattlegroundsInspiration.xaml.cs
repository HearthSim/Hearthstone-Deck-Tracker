using System.Windows.Controls;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Inspiration;

public partial class BattlegroundsInspiration : UserControl
{
	public BattlegroundsInspiration()
	{
		InitializeComponent();
	}

	private void ToolTipService_OnToolTipOpening(object sender, ToolTipEventArgs e)
	{
		if(sender is not UserControl c)
			return;

		switch (c.DataContext)
		{
			case BattlegroundsMinionViewModel m:
				((CardImage)((ToolTip)c.ToolTip).Content).SetCardIdFromCard(m.Card);
				break;
			case HeroPowerViewModel h:
				((CardImage)((ToolTip)c.ToolTip).Content).SetCardIdFromCard(h.Card);
				break;
			case TrinketViewModel t:
				((CardImage)((ToolTip)c.ToolTip).Content).SetCardIdFromCard(t.Card);
				break;
		}
	}

	private void ToolTipService_OnToolTipClosing(object sender, ToolTipEventArgs e)
	{
		if(sender is not UserControl c)
			return;
		((CardImage)((ToolTip)c.ToolTip).Content).SetCardIdFromCard(null);
	}

	private void BattlegroundsMinion_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if(sender is not UserControl { DataContext: BattlegroundsMinionViewModel m })
			return;
		((BattlegroundsInspirationViewModel)DataContext).SetKeyMinion(m.Card);
		Core.Game.Metrics.BattlegroundsInspirationMinionClicks++;
	}

	private void HeroPower_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if(sender is not UserControl { DataContext: HeroPowerViewModel h })
			return;
		((BattlegroundsInspirationViewModel)DataContext).SetKeyMinion(h.Card);
		Core.Game.Metrics.BattlegroundsInspirationMinionClicks++;
	}
}


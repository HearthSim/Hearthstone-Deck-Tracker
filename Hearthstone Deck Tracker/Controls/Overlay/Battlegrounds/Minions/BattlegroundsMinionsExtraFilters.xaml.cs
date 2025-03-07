using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Commands;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsMinionsExtraFilters : UserControl
{
	public BattlegroundsMinionsExtraFilters()
	{
		InitializeComponent();
	}

	public static readonly DependencyProperty StandAloneProperty =
		DependencyProperty.Register("IsStandAloneMode", typeof(bool), typeof(BattlegroundsMinionsExtraFilters),
			new PropertyMetadata(false));

	public bool IsStandAloneMode
	{
		get { return (bool)GetValue(StandAloneProperty); }
		set { SetValue(StandAloneProperty, value); }
	}

	public ICommand SetActiveKeywordCommand => new Command<GameTag>(value =>
	{
		((BattlegroundsMinionsViewModel)DataContext).ActiveMinionKeyword = ((BattlegroundsMinionsViewModel)DataContext).ActiveMinionKeyword == value ? null : value;
		Core.Game.Metrics.BattlegroundsBrowserMechanicFilterClicks++;
	});

	public ICommand SetActiveMinionTypeCommand => new Command<Race>(value =>
	{
		((BattlegroundsMinionsViewModel)DataContext).ActiveMinionType = ((BattlegroundsMinionsViewModel)DataContext).ActiveMinionType == value ? null : value;
		Core.Game.Metrics.BattlegroundsBrowserTypeFilterClicks++;
	});
}

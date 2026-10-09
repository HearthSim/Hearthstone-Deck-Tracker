using System;
using System.Linq;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPowerPicking;

public class BattlegroundsSingleHeroPowerViewModel : ViewModel
{
	public BattlegroundsHeroHeaderViewModel BgsHeroHeaderVM { get; }

	public int? Armor { get; }

	public Visibility ArmorVisibility => Armor > 0 ? Visibility.Visible : Visibility.Collapsed;

	public CardAssetViewModel HeroRender { get; }

	public BattlegroundsSingleHeroPowerViewModel(int heroPowerDbfId, BattlegroundsHeroPickStats.BattlegroundsSingleHeroPickStats? stats, Action<bool> onPlacementHover)
	{
		Armor = stats?.Armor;
		var heroDbfId = Database.GetCardFromDbfId(heroPowerDbfId, false)?.GetTag(GameTag.BACON_HEROPOWER_BASE_HERO_ID) ?? 0;
		HeroRender = new CardAssetViewModel(heroDbfId > 0 ? Database.GetCardFromDbfId(heroDbfId, false) : null, CardAssetType.Hero);
		BgsHeroHeaderVM = new(stats?.Tier, stats?.AvgPlacement, stats?.PickRate, stats?.PlacementDistribution ?? Enumerable.Repeat(0.0, 8).ToArray(), onPlacementHover);
	}
}

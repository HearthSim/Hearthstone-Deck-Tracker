using System;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking
{
	public class BattlegroundsSingleHeroViewModel : ViewModel
	{
		public BattlegroundsHeroHeaderViewModel BgsHeroHeaderVM { get; }
		public BattlegroundsCompositionPopularityViewModel? BgsCompsPopularityVM { get; }

		public int? HeroDbfId { get; }
		public int? ArmorTier { get; }
		public string? ArmorTierTooltipRange
		{
			get
			{
				if(ArmorTier == null)
					return null;
				var (min, max) = ArmorTier switch
				{
					(>= 2) and (<= 7) => (ArmorTier.Value, ArmorTier.Value + 3),
					_ => (0, 0),
				};
				return string.Format(LocUtil.Get("BattlegroundsHeroPicking_Hero_ArmorTierTooltip_Range"), min, max);
			}
		}

		public BattlegroundsSingleHeroViewModel(BattlegroundsSingleHeroPickStats? stats, Action<bool> onPlacementHover)
		{
			HeroDbfId = stats?.HeroDbfId;
			ArmorTier = stats != null ? Database.GetBattlegroundsHeroFromDbf(stats.HeroDbfId)?.BattlegroundsArmorTier : null;
			BgsHeroHeaderVM = new(stats?.Tier, stats?.AvgPlacement ?? 0, stats?.PickRate ?? 0, stats?.PlacementDistribution ?? Enumerable.Repeat(0.0, 8).ToArray(), onPlacementHover);
			BgsCompsPopularityVM = stats != null ? new(stats.FirstPlaceCompPopularity) : null;
		}

		/// <summary>
		/// When the hero power of a neighboring hero hovers this one we hide
		/// the stats header and armor tier.
		/// </summary>
		public Visibility HeroPowerVisibility { get => GetProp(Visible); set => SetProp(value); }

		public void SetHiddenByHeroPower(bool hidden) => HeroPowerVisibility = (hidden ? Collapsed : Visible);
	}
}

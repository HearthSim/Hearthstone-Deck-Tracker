﻿using System;
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
		public Visibility CompositionsVisibility { get; }

		public int? HeroDbfId { get; }

		public BattlegroundsSingleHeroViewModel(BattlegroundsHeroPickStats.BattlegroundsSingleHeroPickStats? stats, Action<bool> onPlacementHover)
		{
			HeroDbfId = stats?.HeroDbfId;
			BgsHeroHeaderVM = new(stats?.Tier, stats?.AvgPlacement, stats?.PickRate, stats?.PlacementDistribution ?? Enumerable.Repeat(0.0, 8).ToArray(), onPlacementHover);
			BgsCompsPopularityVM = stats?.FirstPlaceCompPopularity != null ? new(stats.FirstPlaceCompPopularity) : null;
			// Hide the "No composition data" message in Duos (unless we start having Comp data there).
			CompositionsVisibility = Core.Game.IsBattlegroundsDuosMatch && BgsCompsPopularityVM == null ? Collapsed : Visible;
		}

		/// <summary>
		/// When the hero power of a neighboring hero hovers this one we hide
		/// the stats header and armor tier.
		/// </summary>
		public Visibility HeroPowerVisibility { get => GetProp(Visible); set => SetProp(value); }

		public void SetHiddenByHeroPower(bool hidden) => HeroPowerVisibility = (hidden ? Collapsed : Visible);
	}
}

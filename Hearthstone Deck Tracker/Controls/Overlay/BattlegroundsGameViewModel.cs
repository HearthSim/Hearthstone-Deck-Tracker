﻿using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System.Windows;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public class BattlegroundsGameViewModel : ViewModel
	{
		public BattlegroundsGameViewModel(GameItem gameItem)
		{
			StartTime = gameItem.StartTime;
			Placement = gameItem.Placement;

			var heroCardId = gameItem.Hero ?? "";
			var heroCard = Database.GetCardFromId(heroCardId);
			if(heroCard?.BattlegroundsSkinParentId > 0)
				heroCard = Database.GetCardFromDbfId(heroCard.BattlegroundsSkinParentId, false);

			CardImage = new CardAssetViewModel(heroCard, Utility.Assets.CardAssetType.Tile);

			var heroShortNameMap = Remote.Config.Data?.BattlegroundsShortNames
				?.Find(sn => sn.DbfId == heroCard?.DbfId);
			HeroName = heroShortNameMap?.ShortName ?? heroCard?.Name ?? "-";

			PlacementText = LocUtil.GetPlacement(gameItem.Placement);

			MMRDelta = gameItem.RatingAfter - gameItem.Rating;
			var signal = MMRDelta > 0 ? "+" : "";
			MMRDeltaText = $"{signal}{MMRDelta}";

			CrownVisibility = gameItem.Placement == 1 ? Visibility.Visible : Visibility.Hidden;
		}

		public string? StartTime { get; }
		public string HeroName { get; }
		public int Placement { get; }
		public string PlacementText { get; }
		public int MMRDelta { get; }
		public string MMRDeltaText { get; }
		public CardAssetViewModel CardImage { get; }
		public Visibility CrownVisibility { get;  }
	}
}

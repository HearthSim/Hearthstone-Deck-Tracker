#region

using System.Linq;
using HearthDb;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using System;
using static HearthDb.Enums.GameTag;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class WotogCounterHelper
	{
		public static Entity PlayerCthun => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.Cthun && x.Info.OriginalZone != null);
		public static Entity PlayerCthunProxy => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.NonCollectible.Neutral.Cthun);
		public static Entity PlayerYogg => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.YoggSaronHopesEnd && x.Info.OriginalZone != null);
		public static Entity PlayerArcaneGiant => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.ArcaneGiant && x.Info.OriginalZone != null);
		public static Entity PlayerGraveHorror => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Priest.GraveHorror && x.Info.OriginalZone != null);
		public static Entity OpponentCthun => Core.Game.Opponent.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.Cthun);
		public static Entity OpponentCthunProxy => Core.Game.Opponent.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.NonCollectible.Neutral.Cthun);
		public static Entity PlayerPogoHopper => Core.Game.Player.RevealedEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Rogue.PogoHopper && x.Info.OriginalZone != null);
		public static Entity OpponentPogoHopper => Core.Game.Opponent.RevealedEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Rogue.PogoHopper && x.Info.OriginalZone != null);
		public static bool PlayerSeenCthun => Core.Game.PlayerEntity?.HasTag(SEEN_CTHUN) ?? false;
		public static bool OpponentSeenCthun => Core.Game.OpponentEntity?.HasTag(SEEN_CTHUN) ?? false;
		public static bool? CthunInDeck => DeckContains(CardIds.Collectible.Neutral.Cthun);
		public static bool? YoggInDeck => DeckContains(CardIds.Collectible.Neutral.YoggSaronHopesEnd);
		public static bool? ArcaneGiantInDeck => DeckContains(CardIds.Collectible.Neutral.ArcaneGiant);
		public static bool? PogoHopperInDeck => DeckContains(CardIds.Collectible.Rogue.PogoHopper);
		public static bool? GraveHorrorInDeck => DeckContains(CardIds.Collectible.Priest.GraveHorror);
		
		public static bool PlayerSeenJade => Core.Game.PlayerEntity?.HasTag(JADE_GOLEM) ?? false;
		public static int PlayerNextJadeGolem => PlayerSeenJade ? Math.Min(Core.Game.PlayerEntity.GetTag(JADE_GOLEM) + 1, 30) : 1;

		public static bool OpponentSeenJade => Core.Game.OpponentEntity?.HasTag(JADE_GOLEM) ?? false;
		public static int OpponentNextJadeGolem => OpponentSeenJade ? Math.Min(Core.Game.OpponentEntity.GetTag(JADE_GOLEM) + 1, 30) : 1;

		public static bool ShowPlayerCthunCounter => !Core.Game.IsInMenu && (Config.Instance.PlayerCthunCounter == DisplayMode.Always
					|| Config.Instance.PlayerCthunCounter == DisplayMode.Auto && PlayerSeenCthun);

		public static bool ShowPlayerPogoHopperCounter => !Core.Game.IsInMenu && (
			Config.Instance.PlayerPogoHopperCounter == DisplayMode.Always
				|| (Config.Instance.PlayerPogoHopperCounter == DisplayMode.Auto && PogoHopperInDeck.HasValue && (PlayerPogoHopper != null || PogoHopperInDeck.Value)));

		public static bool ShowPlayerSpellsCounter => !Core.Game.IsInMenu && (
			Config.Instance.PlayerSpellsCounter == DisplayMode.Always
				|| (Config.Instance.PlayerSpellsCounter == DisplayMode.Auto && YoggInDeck.HasValue && (PlayerYogg != null || YoggInDeck.Value))
				|| (Config.Instance.PlayerSpellsCounter == DisplayMode.Auto && ArcaneGiantInDeck.HasValue && (PlayerArcaneGiant != null || ArcaneGiantInDeck.Value))
				|| (Config.Instance.PlayerSpellsCounter == DisplayMode.Auto && GraveHorrorInDeck.HasValue && (PlayerGraveHorror != null || GraveHorrorInDeck.Value))
			);

		public static bool ShowPlayerJadeCounter => !Core.Game.IsInMenu && (Config.Instance.PlayerJadeCounter == DisplayMode.Always
					|| Config.Instance.PlayerJadeCounter == DisplayMode.Auto && PlayerSeenJade);

		public static bool ShowOpponentCthunCounter => !Core.Game.IsInMenu && (Config.Instance.OpponentCthunCounter == DisplayMode.Always
					|| Config.Instance.OpponentCthunCounter == DisplayMode.Auto && OpponentSeenCthun);

		public static bool ShowOpponentPogoHopperCounter => !Core.Game.IsInMenu && (Config.Instance.OpponentPogoHopperCounter == DisplayMode.Always
					|| Config.Instance.OpponentPogoHopperCounter == DisplayMode.Auto && OpponentPogoHopper != null);

		public static bool ShowOpponentSpellsCounter => !Core.Game.IsInMenu && Config.Instance.OpponentSpellsCounter == DisplayMode.Always;

		public static bool ShowOpponentJadeCounter => !Core.Game.IsInMenu && (Config.Instance.OpponentJadeCounter == DisplayMode.Always
					|| Config.Instance.OpponentJadeCounter == DisplayMode.Auto && OpponentSeenJade);

		private static bool? DeckContains(string cardId) => DeckList.Instance.ActiveDeck?.Cards.Any(x => x.Id == cardId);

	}
}

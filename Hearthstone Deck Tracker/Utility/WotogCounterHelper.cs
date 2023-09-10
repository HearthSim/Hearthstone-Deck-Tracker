﻿using System.Linq;
using HearthDb;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using System;
using static HearthDb.Enums.GameTag;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class WotogCounterHelper
	{
		public static Entity PlayerCthun => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.CthunOG && x.Info.OriginalZone != null);
		public static Entity PlayerCthunProxy => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.NonCollectible.Neutral.Cthun);
		public static Entity PlayerYogg => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.YoggSaronHopesEnd && x.Info.OriginalZone != null);
		public static Entity PlayerArcaneGiant => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.ArcaneGiant && x.Info.OriginalZone != null);
		public static Entity PlayerGraveHorror => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Priest.GraveHorror && x.Info.OriginalZone != null);
		public static Entity OpponentCthun => Core.Game.Opponent.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.CthunOG );
		public static Entity OpponentCthunProxy => Core.Game.Opponent.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.NonCollectible.Neutral.Cthun);
		public static Entity PlayerPogoHopper => Core.Game.Player.RevealedEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Rogue.PogoHopper && x.Info.OriginalZone != null);
		public static Entity OpponentPogoHopper => Core.Game.Opponent.RevealedEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Rogue.PogoHopper && x.Info.OriginalZone != null);

		public static bool PlayerSeenCthun
		{
			get
			{
				var cthun = PlayerCthun;
				return cthun != null;
			}
		}

		public static bool OpponentSeenCthun
		{
			get
			{
				var cthun = OpponentCthun;
				return cthun != null;
			}
		}

		public static bool? CthunInDeck => DeckContains(CardIds.Collectible.Neutral.CthunOG);
		public static bool? YoggInDeck => DeckContains(CardIds.Collectible.Neutral.YoggSaronHopesEnd);
		public static bool? ArcaneGiantInDeck => DeckContains(CardIds.Collectible.Neutral.ArcaneGiant);
		public static bool? PogoHopperInDeck => DeckContains(CardIds.Collectible.Rogue.PogoHopper);
		public static bool? GraveHorrorInDeck => DeckContains(CardIds.Collectible.Priest.GraveHorror);
		
		public static bool PlayerSeenJade => Core.Game.PlayerEntity?.HasTag(JADE_GOLEM) ?? false;
		public static int PlayerNextJadeGolem => PlayerSeenJade ? Math.Min(Core.Game.PlayerEntity!.GetTag(JADE_GOLEM) + 1, 30) : 1;

		public static bool OpponentSeenJade => Core.Game.OpponentEntity?.HasTag(JADE_GOLEM) ?? false;
		public static int OpponentNextJadeGolem => OpponentSeenJade ? Math.Min(Core.Game.OpponentEntity!.GetTag(JADE_GOLEM) + 1, 30) : 1;
		public static int PlayerGalakrondInvokeCounter => Core.Game.PlayerEntity?.GetTag(INVOKE_COUNTER) ?? 0;
		public static int OpponentGalakrondInvokeCounter => Core.Game.OpponentEntity?.GetTag(INVOKE_COUNTER) ?? 0;

		public static int PlayerLibramCounter => Core.Game.Player.LibramReductionCount;
		public static int OpponentLibramCounter => Core.Game.Opponent.LibramReductionCount;

		public static int PlayerAbyssalCurseCounter => Core.Game.Player.AbyssalCurseCount;
		public static int OpponentAbyssalCurseCounter => Core.Game.Opponent.AbyssalCurseCount;

		public static bool ShowPlayerCthunCounter => !Core.Game.IsInMenu && (Config.Instance.PlayerCthunCounter == DisplayMode.Always
					|| Config.Instance.PlayerCthunCounter == DisplayMode.Auto && PlayerSeenCthun);

		public static bool ShowPlayerPogoHopperCounter => !Core.Game.IsInMenu && (
			Config.Instance.PlayerPogoHopperCounter == DisplayMode.Always
				|| (Config.Instance.PlayerPogoHopperCounter == DisplayMode.Auto && PogoHopperInDeck.HasValue && (PlayerPogoHopper != null || PogoHopperInDeck.Value)));

		public static bool ShowPlayerGalakrondCounter => !Core.Game.IsInMenu && (
			Config.Instance.PlayerGalakrondCounter == DisplayMode.Always
				|| (Config.Instance.PlayerGalakrondCounter == DisplayMode.Auto && (Core.Game.PlayerEntity?.HasTag(PROXY_GALAKROND) ?? false)));
		public static bool ShowOpponentGalakrondCounter => !Core.Game.IsInMenu && (
			Config.Instance.OpponentGalakrondCounter == DisplayMode.Always
				|| (Config.Instance.OpponentGalakrondCounter == DisplayMode.Auto && (Core.Game.OpponentEntity?.HasTag(INVOKE_COUNTER) ?? false)));

		public static bool ShowPlayerLibramCounter => !Core.Game.IsInMenu && (
			Config.Instance.PlayerLibramCounter == DisplayMode.Always
				|| (Config.Instance.PlayerLibramCounter == DisplayMode.Auto && Core.Game.Player.LibramReductionCount > 0));
		public static bool ShowOpponentLibramCounter => !Core.Game.IsInMenu && (
			Config.Instance.OpponentLibramCounter == DisplayMode.Always
				|| (Config.Instance.OpponentLibramCounter == DisplayMode.Auto && Core.Game.Opponent.LibramReductionCount > 0));


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

		public static bool ShowPlayerAbyssalCurseCounter => !Core.Game.IsInMenu && (
			Config.Instance.PlayerAbyssalCurseCounter == DisplayMode.Always
				|| (Config.Instance.PlayerAbyssalCurseCounter == DisplayMode.Auto && Core.Game.Player.AbyssalCurseCount > 0));
		public static bool ShowOpponentAbyssalCurseCounter => !Core.Game.IsInMenu && (
			Config.Instance.OpponentAbyssalCurseCounter == DisplayMode.Always
				|| (Config.Instance.OpponentAbyssalCurseCounter == DisplayMode.Auto && Core.Game.Opponent.AbyssalCurseCount > 0));

		private static bool? DeckContains(string cardId) => DeckList.Instance.ActiveDeck?.Cards.Any(x => x.Id == cardId);

	}
}

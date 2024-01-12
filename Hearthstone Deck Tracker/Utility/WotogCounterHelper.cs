using System.Linq;
using HearthDb;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using System;
using static HearthDb.Enums.GameTag;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class WotogCounterHelper
	{
		public static Entity? PlayerCthun => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.CthunOG && x.Info.OriginalZone != null);
		public static Entity? PlayerCthunProxy => Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.NonCollectible.Neutral.Cthun);
		public static Entity? OpponentCthun => Core.Game.Opponent.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Neutral.CthunOG );
		public static Entity? OpponentCthunProxy => Core.Game.Opponent.PlayerEntities.FirstOrDefault(x => x.CardId == CardIds.NonCollectible.Neutral.Cthun);
		public static Entity? PlayerPogoHopper => Core.Game.Player.RevealedEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Rogue.PogoHopper && x.Info.OriginalZone != null);
		public static Entity? OpponentPogoHopper => Core.Game.Opponent.RevealedEntities.FirstOrDefault(x => x.CardId == CardIds.Collectible.Rogue.PogoHopper && x.Info.OriginalZone != null);

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
		public static bool? PogoHopperInDeck => DeckContains(CardIds.Collectible.Rogue.PogoHopper);

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

		public static bool ShowPlayerCthunCounter =>
			Config.Instance.PlayerCthunCounter == DisplayMode.Always ||
			Config.Instance.PlayerCthunCounter == DisplayMode.Auto && PlayerSeenCthun;

		public static bool ShowPlayerPogoHopperCounter =>
			Config.Instance.PlayerPogoHopperCounter == DisplayMode.Always ||
			(Config.Instance.PlayerPogoHopperCounter == DisplayMode.Auto && PogoHopperInDeck.HasValue && (PlayerPogoHopper != null || PogoHopperInDeck.Value));

		public static bool ShowPlayerGalakrondCounter =>
			Config.Instance.PlayerGalakrondCounter == DisplayMode.Always ||
			(Config.Instance.PlayerGalakrondCounter == DisplayMode.Auto && (Core.Game.PlayerEntity?.HasTag(PROXY_GALAKROND) ?? false));
		public static bool ShowOpponentGalakrondCounter =>
			Config.Instance.OpponentGalakrondCounter == DisplayMode.Always ||
			(Config.Instance.OpponentGalakrondCounter == DisplayMode.Auto && (Core.Game.OpponentEntity?.HasTag(INVOKE_COUNTER) ?? false));

		public static bool ShowPlayerLibramCounter =>
			Config.Instance.PlayerLibramCounter == DisplayMode.Always ||
			(Config.Instance.PlayerLibramCounter == DisplayMode.Auto && Core.Game.Player.LibramReductionCount > 0);
		public static bool ShowOpponentLibramCounter =>
			Config.Instance.OpponentLibramCounter == DisplayMode.Always ||
			(Config.Instance.OpponentLibramCounter == DisplayMode.Auto && Core.Game.Opponent.LibramReductionCount > 0);

		public static bool ShowPlayerSpellsCounter =>
			Config.Instance.PlayerSpellsCounter == DisplayMode.Always ||
			(Config.Instance.PlayerSpellsCounter == DisplayMode.Auto && InDeckAndHand(new[] {
				CardIds.Collectible.Neutral.YoggSaronHopesEnd,
				CardIds.Collectible.Neutral.ArcaneGiant,
				CardIds.Collectible.Priest.GraveHorror,
				CardIds.Collectible.Druid.UmbralOwlDARKMOON_FAIRE,
				CardIds.Collectible.Druid.UmbralOwlPLACEHOLDER_202204,
				CardIds.Collectible.Neutral.YoggSaronMasterOfFate,
				CardIds.Collectible.Demonhunter.SaroniteShambler,
				CardIds.Collectible.Druid.ContaminatedLasher,
				CardIds.Collectible.Mage.MeddlesomeServant,
				CardIds.Collectible.Neutral.PrisonBreaker,
			}));

		public static bool ShowPlayerSpellSchoolsCounter =>
			Config.Instance.PlayerSpellSchoolsCounter == DisplayMode.Always ||
			(Config.Instance.PlayerSpellSchoolsCounter == DisplayMode.Auto && InDeckAndHand(new[] {
				CardIds.Collectible.Mage.DiscoveryOfMagic,
				CardIds.Collectible.Mage.InquisitiveCreation,
				CardIds.Collectible.Neutral.Multicaster,
				CardIds.Collectible.Shaman.CoralKeeper,
				CardIds.Collectible.Mage.WisdomOfNorgannon,
				CardIds.Collectible.Mage.Sif,
				CardIds.Collectible.Mage.ElementalInspiration,
				CardIds.Collectible.Mage.MagisterDawngrasp,
			}));

		public static bool ShowPlayerExcavateTier =>
			Config.Instance.PlayerExcavateTierCounter == DisplayMode.Always ||
			(Config.Instance.PlayerExcavateTierCounter == DisplayMode.Auto && InDeckAndHand(new[] {
				CardIds.Collectible.Rogue.BloodrockCoShovel,
				CardIds.Collectible.Warlock.Smokestack,
				CardIds.Collectible.Mage.Cryopreservation,
				CardIds.Collectible.Neutral.KoboldMiner,
				CardIds.Collectible.Warrior.BlastCharge,
				CardIds.Collectible.Deathknight.ReapWhatYouSow,
				CardIds.Collectible.Warrior.ReinforcedPlating,
				CardIds.Collectible.Rogue.DrillyTheKid,
				CardIds.Collectible.Warlock.MoargDrillfist,
				CardIds.Collectible.Deathknight.SkeletonCrew,
				CardIds.Collectible.Neutral.BurrowBuster,
				CardIds.Collectible.Mage.BlastmageMiner
			}));

		public static bool ShowPlayerJadeCounter =>
			Config.Instance.PlayerJadeCounter == DisplayMode.Always ||
			Config.Instance.PlayerJadeCounter == DisplayMode.Auto && PlayerSeenJade;

		public static bool ShowOpponentCthunCounter =>
			Config.Instance.OpponentCthunCounter == DisplayMode.Always ||
			Config.Instance.OpponentCthunCounter == DisplayMode.Auto && OpponentSeenCthun;

		public static bool ShowOpponentPogoHopperCounter =>
			Config.Instance.OpponentPogoHopperCounter == DisplayMode.Always ||
			Config.Instance.OpponentPogoHopperCounter == DisplayMode.Auto && OpponentPogoHopper != null;

		public static bool ShowOpponentSpellCounter => Config.Instance.OpponentSpellsCounter == DisplayMode.Always;
		public static bool ShowOpponentSpellSchoolsCounter => Config.Instance.OpponentSpellSchoolsCounter == DisplayMode.Always;

		public static bool ShowOpponentJadeCounter =>
			Config.Instance.OpponentJadeCounter == DisplayMode.Always ||
			Config.Instance.OpponentJadeCounter == DisplayMode.Auto && OpponentSeenJade;

		public static bool ShowPlayerAbyssalCurseCounter =>
			Config.Instance.PlayerAbyssalCurseCounter == DisplayMode.Always ||
			(Config.Instance.PlayerAbyssalCurseCounter == DisplayMode.Auto && Core.Game.Player.AbyssalCurseCount > 0);
		public static bool ShowOpponentAbyssalCurseCounter =>
			Config.Instance.OpponentAbyssalCurseCounter == DisplayMode.Always ||
			(Config.Instance.OpponentAbyssalCurseCounter == DisplayMode.Auto && Core.Game.Opponent.AbyssalCurseCount > 0);
		public static bool ShowOpponentExcavateCounter =>
			Config.Instance.OpponentExcavateCounter == DisplayMode.Always ||
			(Config.Instance.OpponentExcavateCounter == DisplayMode.Auto && (Core.Game.OpponentEntity?.GetTag((GameTag)2822) ?? 0) > 0);

		private static bool InDeckOrKnown(string cardId)
		{
			var contains = DeckContains(cardId);
			if(!contains.HasValue) return false;
			return contains.Value || Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == cardId && x.Info.OriginalZone != null) != null;
		}
		private static bool InDeckAndHand(string[] cardIds) => cardIds.Any(cardId => InDeckOrKnown(cardId));

		private static bool? DeckContains(string cardId) => DeckList.Instance.ActiveDeck?.Cards.Any(x => x.Id == cardId);
		private static bool? DeckContains(string[] cardIds) => DeckList.Instance.ActiveDeck?.Cards.Any(x => cardIds.Contains(x.Id));
	}
}

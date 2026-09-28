using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static HearthDb.CardIds.NonCollectible;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsDeityStateTest
	{
		private static Entity CreateSigil(GameV2 game, int id, string deityCardId, Zone zone)
		{
			var sigil = new Entity(id) { CardId = Neutral.SecretDeityDnt };
			sigil.SetTag(GameTag.CONTROLLER, game.Player.Id);
			sigil.SetTag(GameTag.ZONE, (int)zone);
			sigil.SetTag(GameTag.BACON_EVOLUTION_CARD_ID, HearthDb.Cards.All[deityCardId].DbfId);
			game.Entities.Add(id, sigil);
			return sigil;
		}

		[TestMethod]
		public void PlayerDeity_IgnoresCombatCopyResetOnRemoval()
		{
			Core._game = null;
			var game = Core._game = new GameV2();
			game.Player.Id = 3;

			CreateSigil(game, 381, Neutral.Yshaarj, Zone.SECRET);
			CreateSigil(game, 761, Neutral.CthunBATTLEGROUNDS, Zone.REMOVEDFROMGAME);

			Assert.AreEqual(Neutral.Yshaarj, game.BattlegroundsPlayerDeity?.Id);
		}

		[TestMethod]
		public void PlayerDeity_PrefersCombatCopyDuringCombat()
		{
			Core._game = null;
			var game = Core._game = new GameV2();
			game.Player.Id = 3;

			CreateSigil(game, 381, Neutral.CthunBATTLEGROUNDS, Zone.SETASIDE);
			CreateSigil(game, 761, Neutral.Yshaarj, Zone.SECRET);

			Assert.AreEqual(Neutral.Yshaarj, game.BattlegroundsPlayerDeity?.Id);
		}
	}
}

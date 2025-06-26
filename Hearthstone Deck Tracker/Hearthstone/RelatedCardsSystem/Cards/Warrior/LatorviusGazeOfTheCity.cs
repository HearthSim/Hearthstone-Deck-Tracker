using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class LatorviusGazeOfTheCity: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Warrior.EntertheLostCity_LatorviusGazeOfTheCityToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.JungleGiants_BarnabusTheStomperToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.TheMarshQueen_QueenCarnassaToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Mage.OpentheWaygate_TimeWarpToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Paladin.TheLastKaleidosaur_GalvadonToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Priest.AwakentheMakers_AmaraWardenOfHopeToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Rogue.TheCavernsBelow_CrystalCoreTokenUNGORO),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Shaman.UnitetheMurlocs_MegafinToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.LakkariSacrifice_NetherPortalToken1),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.FirePlumesHeart_SulfurasToken),
		};
}

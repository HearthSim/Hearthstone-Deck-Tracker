using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class RestInPeace: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.RestInPeace;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.OriginalClass) && GetRelatedCards(opponent).Count > 0;
	}
	private string OpponentHero => Core.Game.CurrentGameStats?.OpponentHeroCardId ?? "";

	private string PlayerHero => Core.Game.CurrentGameStats?.PlayerHeroCardId ?? "";
	public List<Card?> GetRelatedCards(Player player)
	{
		var retval = new List<Card?>();

		var isPlayer = Core.Game.Player.Id == player.Id;
		var opponent = isPlayer ? Core.Game.Opponent : Core.Game.Player;

		var playerMinions =  player.DeadMinionsCards.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is not null).ToList();

		var opponentMinions = opponent.DeadMinionsCards.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, opponent))
			.Where(card => card is not null).ToList();

		if(playerMinions.Any())
		{
			retval.Add(isPlayer ? new Card(PlayerHero) : new Card(OpponentHero));
			var highestCost = playerMinions.Max(c => c?.Cost);
			var minionsWithHighestCost = playerMinions.Where(c => (c?.Cost ?? 0) == highestCost);
			retval.AddRange(minionsWithHighestCost);

		}

		if(opponentMinions.Any())
		{
			retval.Add(isPlayer ? new Card(OpponentHero) : new Card(PlayerHero));
			var highestCost = opponentMinions.Max(c => c?.Cost);
			var minionsWithHighestCost = opponentMinions.Where(c => (c?.Cost ?? 0) == highestCost);
			retval.AddRange(minionsWithHighestCost);

		}

		return retval;
	}
}

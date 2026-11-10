using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Assemble;

/// <summary>
/// An Assemble card, whose related cards are the middle cards it can offer. The pools come from
/// <see cref="AssemblePools"/> rather than from card text, so there is one of these per Assemble card
/// instead of a class per card. Hovering one summarises the middle cards like an Outfinder pool, and
/// right-clicking it opens the Assemble panel instead of the Outfinder's.
/// </summary>
public class AssembleCard : ICardWithRelatedCardsSummary
{
	private readonly string _cardId;

	public AssembleCard(string cardId)
	{
		_cardId = cardId;
	}

	public string GetCardId() => _cardId;

	// One middle card is drawn from the pool per Assemble.
	public int Picks() => 1;

	public int EventCount() => 1;

	public bool IsWithReplacement() => true;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public AssemblePool? GetPool(Player player)
	{
		var card = Database.GetCardFromId(_cardId);
		return card == null ? null : AssemblePools.GetPool(card, player.CurrentClass);
	}

	public List<Card?> GetRelatedCards(Player player) =>
		GetPool(player)?.Cards.Cast<Card?>().ToList() ?? new List<Card?>();
}

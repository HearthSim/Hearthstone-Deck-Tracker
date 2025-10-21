using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public interface ICardGenerator : ICard
{
	bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format);

	bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format);
}

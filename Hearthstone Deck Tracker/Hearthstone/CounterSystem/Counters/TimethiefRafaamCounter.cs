using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class TimethiefRafaamCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.TimethiefRafaam;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Warlock.TimethiefRafaam
	};

	public TimethiefRafaamCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	private string[] _rafaams = {
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_TinyRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_GreenRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_MurlocRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_ExplorerRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_WarchiefRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_CalamitousRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_MindflayerRfaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_GiantRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_ArchmageRafaamToken,
	};

	private readonly SortedList<int, string> _playedRafaams = new();
	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return _playedRafaams.Values.ToArray();
	}

	public override string ValueToShow() => Counter.ToString();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.ZONE || gameState.CurrentBlock?.Type != "PLAY")
			return;

		var isCurrentController = IsPlayerCounter ? entity.IsControlledBy(Game.Player.Id)
			: entity.IsControlledBy(Game.Opponent.Id);

		if(!isCurrentController)
			return;

		if(!_rafaams.Contains(entity.Card.Id) || _playedRafaams.ContainsValue(entity.Card.Id))
			return;

		_playedRafaams.Add(entity.Card.Cost, entity.Card.Id);
		Counter++;
	}
}

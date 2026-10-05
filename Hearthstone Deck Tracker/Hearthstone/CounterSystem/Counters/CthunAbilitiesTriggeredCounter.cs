using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

// C'Thun, Eye of the Empire: "Old God. If you've triggered all 4 C'Thun abilities this game, this minion's abilities are tripled."
public class CthunAbilitiesTriggeredCounter : NumericCounter
{
	private const int AbilityCount = 4;

	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.CthunEyeOfTheEmpire;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Neutral.CthunEyeOfTheEmpire,
	};

	private readonly bool[] _abilitiesUsed = new bool[AbilityCount];

	public CthunAbilitiesTriggeredCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
	}

	public override string ValueToShow() => $"{Counter}/{AbilityCount}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		var index = tag switch
		{
			GameTag.CTHUN_ABILITY1_USED => 0,
			GameTag.CTHUN_ABILITY2_USED => 1,
			GameTag.CTHUN_ABILITY3_USED => 2,
			GameTag.CTHUN_ABILITY4_USED => 3,
			_ => -1,
		};
		if(index < 0 || value <= 0)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(_abilitiesUsed[index])
			return;

		_abilitiesUsed[index] = true;
		Counter++;
	}
}

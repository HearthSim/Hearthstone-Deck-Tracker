using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class PlayedSpellSchoolsCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_PlayedSpellSchools", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.DiscoveryOfMagic;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Neutral.Multicaster,
		HearthDb.CardIds.Collectible.Shaman.CoralKeeper,
		HearthDb.CardIds.Collectible.Shaman.RazzleDazzler,
		HearthDb.CardIds.Collectible.Mage.InquisitiveCreation,
		HearthDb.CardIds.Collectible.Mage.WisdomOfNorgannon,
		HearthDb.CardIds.Collectible.Mage.Sif,
		HearthDb.CardIds.Collectible.Mage.ElementalInspiration,
		HearthDb.CardIds.Collectible.Mage.MagisterDawngrasp
	};

	public PlayedSpellSchoolsCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 2 && OpponentMayHaveRelevantCards(true);
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.Class);
	}

	public override bool IsDisplayValueLong => true;

	public override string ValueToShow() {
		if(Counter == 0)
			return LocUtil.Get("Counter_Spell_School_None", useCardLanguage: true);
		return string.Join(", ", PlayedSpellSchools.Select(HearthDbConverter.GetLocalizedSpellSchool).WhereNotNull().OrderBy(x => x));
	}

	private HashSet<SpellSchool> PlayedSpellSchools { get; set; } = new HashSet<SpellSchool>();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY && value != (int)Zone.SECRET)
			return;

		if(!entity.IsSpell)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(entity.Tags.TryGetValue(GameTag.SPELL_SCHOOL, out var spellSchoolTag))
		{
			if((controller == Game.Player.Id && IsPlayerCounter)
			   || (controller == Game.Opponent.Id && !IsPlayerCounter))
			{
				PlayedSpellSchools.Add((SpellSchool)spellSchoolTag);
				Counter = PlayedSpellSchools.Count;
			}
		}
	}
}

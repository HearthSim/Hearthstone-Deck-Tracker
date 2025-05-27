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
		HearthDb.CardIds.Collectible.Shaman.SirenSong,
		HearthDb.CardIds.Collectible.Shaman.CoralKeeper,
		HearthDb.CardIds.Collectible.Shaman.RazzleDazzler,
		HearthDb.CardIds.Collectible.Mage.DiscoveryOfMagic,
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
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 1 && OpponentMayHaveRelevantCards(true);
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
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

		var controller = entity.GetTag(GameTag.CONTROLLER);
		if(!(controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			return;

		if(DiscountIfCantPlay(tag, value, entity))
		{
			if(entity.Tags.TryGetValue(GameTag.SPELL_SCHOOL, out var schoolTag))
			{
				PlayedSpellSchools.Remove((SpellSchool)schoolTag);
			}
			return;
		}

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY && value != (int)Zone.SECRET)
			return;

		if(gameState.CurrentBlock?.Type != "PLAY")
			return;

		if(!entity.IsSpell)
			return;


		if(entity.Tags.TryGetValue(GameTag.SPELL_SCHOOL, out var spellSchoolTag))
		{
			PlayedSpellSchools.Add((SpellSchool)spellSchoolTag);
			LastEntityToCount = entity;
			Counter = PlayedSpellSchools.Count;
		}
	}
}

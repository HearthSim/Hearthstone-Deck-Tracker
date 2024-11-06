using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class ExcavateTierCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_ExcavateTier", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.KoboldMiner;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Neutral.KoboldMiner,
		HearthDb.CardIds.Collectible.Neutral.BurrowBuster,
		HearthDb.CardIds.Collectible.Rogue.BloodrockCoShovel,
		HearthDb.CardIds.Collectible.Rogue.DrillyTheKid,
		HearthDb.CardIds.Collectible.Warlock.Smokestack,
		HearthDb.CardIds.Collectible.Warlock.MoargDrillfist,
		HearthDb.CardIds.Collectible.Warrior.BlastCharge,
		HearthDb.CardIds.Collectible.Warrior.ReinforcedPlating,
		HearthDb.CardIds.Collectible.Mage.Cryopreservation,
		HearthDb.CardIds.Collectible.Mage.BlastmageMiner,
		HearthDb.CardIds.Collectible.Paladin.Shroomscavate,
		HearthDb.CardIds.Collectible.Paladin.FossilizedKaleidosaur,
		HearthDb.CardIds.Collectible.Deathknight.ReapWhatYouSow,
		HearthDb.CardIds.Collectible.Deathknight.SkeletonCrew,
		HearthDb.CardIds.Collectible.Shaman.DiggingStraightDown,
	};

	public ExcavateTierCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	private string ExcavateTierLabel
	{
		get => Counter switch
		{
			0 => LocUtil.Get("Counter_Excavate_Tier0", useCardLanguage: true),
			1 => LocUtil.Get("Counter_Excavate_Tier1", useCardLanguage: true),
			2 => LocUtil.Get("Counter_Excavate_Tier2", useCardLanguage: true),
			3 => LocUtil.Get("Counter_Excavate_Tier3", useCardLanguage: true),
			_ => (Counter + 1).ToString()
		};
	}

	private bool Excavated { get; set; }

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return Counter > 0 || InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
	}

	public string[] GetExcavateRewards()
	{
		var rewards = Counter switch
		{
			0 => new List<string>
			{
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_EscapingTroggToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_FoolsAzeriteToken,
				HearthDb.CardIds.NonCollectible.Neutral.HeartblossomToken1,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_PouchOfCoinsToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_RockToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_WaterSourceToken
			},
			1 => new List<string>
			{
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_AzeriteChunkToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_CanaryToken,
				HearthDb.CardIds.NonCollectible.Neutral.DeepholmGeodeToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_FallingStalactiteToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_GlowingGlyphToken1,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_LivingStoneToken
			},
			2 => new List<string>
			{
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_AzeriteGemToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_CollapseToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_MotherlodeDrakeToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_OgrefistBoulderToken,
				HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_SteelhideMoleToken,
				HearthDb.CardIds.NonCollectible.Neutral.WorldPillarFragmentToken,
			},
			3 => new List<string>
			{
				HearthDb.CardIds.NonCollectible.Paladin.TheAzeriteDragonToken,
				HearthDb.CardIds.NonCollectible.Mage.KoboldMiner_TheAzeriteHawkToken,
				HearthDb.CardIds.NonCollectible.Warlock.KoboldMiner_TheAzeriteSnakeToken,
				HearthDb.CardIds.NonCollectible.Warrior.KoboldMiner_TheAzeriteOxToken,
				HearthDb.CardIds.NonCollectible.Rogue.KoboldMiner_TheAzeriteScorpionToken,
				HearthDb.CardIds.NonCollectible.Shaman.TheAzeriteMurlocToken,
				HearthDb.CardIds.NonCollectible.Deathknight.KoboldMiner_TheAzeriteRatToken
			},
			_ => new List<string>()
		};

		return rewards.ToArray();
	}


	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			FilterCardsByClassAndFormat(GetExcavateRewards(), Game.Player.OriginalClass) :
			FilterCardsByClassAndFormat(GetExcavateRewards(), Game.Opponent.OriginalClass);
	}

	public override string ValueToShow() => ExcavateTierLabel;

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.CURRENT_EXCAVATE_TIER)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(controller == Game.Player.Id && IsPlayerCounter || controller == Game.Opponent.Id && !IsPlayerCounter)
		{
			Excavated = true;
			Counter = value;
		}
	}
}

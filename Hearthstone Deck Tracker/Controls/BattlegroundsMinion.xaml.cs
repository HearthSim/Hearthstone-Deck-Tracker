using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Controls;

public partial class BattlegroundsMinion
{
	public BattlegroundsMinion()
	{
		InitializeComponent();
	}

	public BattlegroundsMinion(Entity entity) : this()
	{
		DataContext = new BattlegroundsMinionViewModel
		{
			HasPoisonous = entity.HasTag(GameTag.POISONOUS),
			HasVenomous = entity.HasTag(GameTag.VENOMOUS),
			HasDivineShield = entity.HasTag(GameTag.DIVINE_SHIELD),
			HasDeathrattle = entity.HasTag(GameTag.DEATHRATTLE),
			HasReborn = entity.HasTag(GameTag.REBORN),
			IsPremium = entity.HasTag(GameTag.PREMIUM),
			HasTaunt = entity.HasTag(GameTag.TAUNT),
			Attack = entity.Attack,
			Health = entity.Health,
			Card = entity.Card,
		};
	}
}

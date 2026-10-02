using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Controls;

public partial class BattlegroundsMinion
{
	public BattlegroundsMinion()
	{
		InitializeComponent();
	}

	public BattlegroundsMinion(BattlegroundsMinionViewModel viewModel) : this()
	{
		DataContext = viewModel;
	}

	public BattlegroundsMinion(Entity entity) : this()
	{
		DataContext = new BattlegroundsMinionViewModel
		{
			HasPoisonous = entity.HasTag(GameTag.POISONOUS),
			HasVenomous = entity.HasTag(GameTag.VENOMOUS),
			HasDivineShield = entity.GetTag(GameTag.DIVINE_SHIELD) == 1,
			HasEmpoweredDivineShield = entity.GetTag(GameTag.DIVINE_SHIELD) > 1,
			HasDeathrattle = entity.HasTag(GameTag.DEATHRATTLE),
			HasRally = entity.HasTag(GameTag.BACON_RALLY),
			HasReborn = entity.HasTag(GameTag.REBORN),
			IsPremium = entity.HasTag(GameTag.PREMIUM),
			HasTaunt = entity.HasTag(GameTag.TAUNT),
			Attack = entity.Attack,
			Health = entity.Health,
			Card = entity.Card,
		};
	}
}

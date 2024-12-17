using System.Windows.Media;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls;

public class BattlegroundsMinionViewModel : ViewModel
{
	public bool HasPoisonous
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public bool HasVenomous
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public bool HasDivineShield
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public bool HasTaunt
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public bool HasDeathrattle
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public bool HasReborn
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	// Battlegrounds Minions never have the legendary Dragon
	public bool IsLegendary => false; // Card?.Rarity == Rarity.LEGENDARY;

	public bool IsPremium
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public int Attack
	{
		get => GetProp(0);
		set
		{
			SetProp(value);
			AttackBrush = value > Card?.Attack ? Green : White;
		}
	}

	public int Health
	{
		get => GetProp(0);
		set
		{
			SetProp(value);
			HealthBrush = value > Card?.Health ? Green : White;
		}
	}


	public Hearthstone.Card? Card
	{
		get => GetProp<Hearthstone.Card?>(null);
		set
		{
			SetProp(value);
			CardPortrait = new CardAssetViewModel(value, CardAssetType.Portrait);
			AttackBrush = Attack > value?.Attack ? Green : White;
			HealthBrush = Health > value?.Health ? Green : White;
		}
	}

	private static readonly SolidColorBrush White = new(Color.FromScRgb(1, 1, 1, 1));

	private static readonly SolidColorBrush Green = new(Color.FromScRgb(1, .109f, .89f, .109f));

	public Brush AttackBrush
	{
		get => GetProp(White) ?? White;
		private set => SetProp(value);
	}

	public Brush HealthBrush
	{
		get => GetProp(White) ?? White;
		private set => SetProp(value);
	}

	public CardAssetViewModel? CardPortrait
	{
		get => GetProp<CardAssetViewModel?>(null);
		private set => SetProp(value);
	}

	public int Tier
	{
		get => GetProp(1);
		set
		{

			SetProp(value);
			HasTier = true;
		}
	}

	public bool HasTier
	{
		get => GetProp(false);
		set => SetProp(value);
	}
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using HearthDb;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Tooltips;

public partial class CardTooltip : IPlacementAware
{
	private static void Update(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as CardTooltip)?.UpdateViewModel();

	public static readonly DependencyProperty CardProperty = DependencyProperty.Register(nameof(Card), typeof (Hearthstone.Card), typeof(CardTooltip), new PropertyMetadata(null, Update));

	public Hearthstone.Card? Card
	{
		get => (Hearthstone.Card)GetValue(CardProperty);
		set => SetValue(CardProperty, value);
	}

	public static readonly DependencyProperty ShowTripleProperty = DependencyProperty.Register(nameof(ShowTriple), typeof(bool), typeof(CardTooltip), new PropertyMetadata(false, Update));

	public bool ShowTriple
	{
		get => (bool)GetValue(ShowTripleProperty);
		set => SetValue(ShowTripleProperty, value);
	}

	public static readonly DependencyProperty CardAssetTypeProperty = DependencyProperty.Register(nameof(CardAssetType), typeof(CardAssetType), typeof(CardTooltip), new PropertyMetadata(CardAssetType.FullImage, Update));

	public CardAssetType CardAssetType
	{
		get => (CardAssetType)GetValue(CardAssetTypeProperty);
		set => SetValue(CardAssetTypeProperty, value);
	}


	public CardTooltip()
	{
		InitializeComponent();
	}

	public CardTooltipViewModel ViewModel { get; } = new();

	private void UpdateViewModel()
	{
		ViewModel.SetCard(Card, CardAssetType);

		Hearthstone.Card? secondaryCard = null;
		if(CardAssetType == CardAssetType.FullImage && ShowTriple && Card != null && Cards.NormalToTripleCardIds.TryGetValue(Card.Id, out var tripleId))
		{
			secondaryCard = Database.GetCardFromId(tripleId);
			if(secondaryCard != null)
			{
				secondaryCard.BaconCard = true;
				secondaryCard.BaconTriple = true;
			}
		}
		ViewModel.SetSecondaryCard(secondaryCard, CardAssetType);
	}

	public void SetPlacement(PlacementMode placement)
	{
		ViewModel.ImageDock = placement == PlacementMode.Left ? Dock.Right : Dock.Left;
	}
}

public class CardTooltipViewModel : ViewModel
{
	public void SetCard(Hearthstone.Card? card, CardAssetType cardAssetType)
	{
		if(card?.Id != AssetViewModel?.Card?.Id || cardAssetType != AssetViewModel?.CardAssetType)
			AssetViewModel = new CardAssetViewModel(card, cardAssetType);
	}

	public CardAssetViewModel? AssetViewModel
	{
		get => GetProp<CardAssetViewModel?>(null);
		private set => SetProp(value);
	}

	public void SetSecondaryCard(Hearthstone.Card? card, CardAssetType cardAssetType)
	{
		if(card == null)
			SecondaryAssetViewModel = null;
		else if(card.Id != SecondaryAssetViewModel?.Card?.Id || cardAssetType != SecondaryAssetViewModel?.CardAssetType)
			SecondaryAssetViewModel = new CardAssetViewModel(card, CardAssetType.FullImage);
	}

	public CardAssetViewModel? SecondaryAssetViewModel
	{
		get => GetProp<CardAssetViewModel?>(null);
		private set => SetProp(value);
	}

	public string? CreatedBy
	{
		get => GetProp<string?>(null);
		set => SetProp(value);
	}

	public Dock ImageDock
	{
		get => GetProp(Dock.Left);
		set => SetProp(value);
	}
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using HearthDb;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Tooltips;

public partial class CardTooltip : IPlacementAware, IScreenBoundaryAware
{
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(CardTooltip), new PropertyMetadata(null, OnTextChanged));

	private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if(d is CardTooltip cardTooltip)
			cardTooltip.ViewModel.Text = e.NewValue as string;
	}

	public static readonly DependencyProperty RelatedCardsProperty = DependencyProperty.Register(nameof(RelatedCards), typeof(List<Hearthstone.Card>), typeof(CardTooltip), new PropertyMetadata(null, OnRelatedCardsChanged));

	private static void OnRelatedCardsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if(d is CardTooltip cardTooltip)
			cardTooltip.ViewModel.RelatedCards = e.NewValue as List<Hearthstone.Card>;
	}

	public List<Hearthstone.Card> RelatedCards
	{
		get => (List<Hearthstone.Card>)GetValue(RelatedCardsProperty);
		set => SetValue(RelatedCardsProperty, value);
	}

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

	public string? Text
	{
		get => (string?)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

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

	public void SetScreenBoundaryOffset(double x, double y)
	{
		var space = (ActualHeight - PrimaryImage.ActualHeight) / 2;
		var offset = Math.Min(Math.Abs(y), space) * Math.Sign(y);
		ViewModel.CardImageOffset = new Thickness(0, offset, 0, -offset);
	}

	private void CardTooltip_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		// We don't have a great way of detecting whether related cards changed. So we refresh them whenever
		// this tooltip becomes visible.
		if(e.NewValue is true)
			Card?.UpdateRelatedCards();
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

	public List<Hearthstone.Card>? RelatedCards
	{
		get => GetProp<List<Hearthstone.Card>?>(null);
		set => SetProp(value);
	}

	public string? Text
	{
		get => GetProp<string?>(null);
		set => SetProp(value);
	}

	public Dock ImageDock
	{
		get => GetProp(Dock.Left);
		set => SetProp(value);
	}

	public Thickness CardImageOffset
	{
		get => GetProp(new Thickness(0));
		set => SetProp(value);
	}
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using HearthDb;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Tooltips;

public interface ICardTooltip
{
	public void UpdateTooltip(CardTooltipViewModel viewModel);
}

/// <summary>
/// A Card Tooltip with animations, optional secondary image, and text.
/// This component requires it's DataContext to implement ICardTooltip.
/// </summary>
public partial class CardTooltip : IPlacementAware, IScreenBoundaryAware
{
	public CardTooltip()
	{
		InitializeComponent();
	}

	public CardTooltipViewModel ViewModel { get; } = new();

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

	private void CardTooltip_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if(DataContext is ICardTooltip cardTooltip)
			cardTooltip.UpdateTooltip(ViewModel);
	}
}

public class CardTooltipViewModel : ViewModel
{
	public Hearthstone.Card? Card
	{
		get => GetProp<Hearthstone.Card?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(AssetViewModel));
			UpdateSecondaryCard();
		}
	}

	public bool ShowTriple
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			UpdateSecondaryCard();
		}
	}

	private void UpdateSecondaryCard()
	{
		if(!ShowTriple || Card is not { BaconCard: true } || !Cards.NormalToTripleCardIds.TryGetValue(Card.Id, out var tripleId))
			return;
		var secondaryCard = Database.GetCardFromId(tripleId);
		if(secondaryCard != null)
		{
			secondaryCard.BaconCard = true;
			secondaryCard.BaconTriple = true;
		}
		SecondaryCard = secondaryCard;
	}

	public Hearthstone.Card? SecondaryCard
	{
		get => GetProp<Hearthstone.Card?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(SecondaryAssetViewModel));
		}
	}

	public CardAssetType CardAssetType
	{
		get => GetProp(CardAssetType.FullImage);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(AssetViewModel));
			OnPropertyChanged(nameof(SecondaryAssetViewModel));
		}
	}

	public CardAssetViewModel AssetViewModel
	{
		get
		{
			var value = GetProp<CardAssetViewModel?>(null);
			if(value == null || value.Card?.Id != Card?.Id || value.CardAssetType != CardAssetType)
			{
				value = new CardAssetViewModel(Card, CardAssetType);
				SetProp(value);
			}
			return value;
		}
	}

	public CardAssetViewModel SecondaryAssetViewModel
	{
		get
		{
			var value = GetProp<CardAssetViewModel?>(null);
			if(value == null || value.Card?.Id != SecondaryCard?.Id || value.CardAssetType != CardAssetType)
			{
				value = new CardAssetViewModel(SecondaryCard, CardAssetType);
				SetProp(value);
			}
			return value;
		}
	}

	public List<Hearthstone.Card>? RelatedCards
	{
		get => GetProp<List<Hearthstone.Card>?>(null);
		set
		{
			RelatedCardsHeader = LocUtil.Get("Related_Cards", useCardLanguage: true);
			OnPropertyChanged(nameof(RelatedCardsHeader));
			SetProp(value);
		}
	}

	public string? RelatedCardsHeader
	{
		get => GetProp<string?>(null);
		set
		{
			SetProp(value ?? LocUtil.Get("Related_Cards", useCardLanguage: true));
		}
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

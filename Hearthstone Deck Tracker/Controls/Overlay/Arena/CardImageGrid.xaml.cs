using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public partial class CardImageGrid
{

	public static readonly DependencyProperty CardsProperty = DependencyProperty.Register(
		nameof(Cards), typeof(IEnumerable<Hearthstone.Card>), typeof(CardImageGrid), new PropertyMetadata(new List<Hearthstone.Card>(), (d, _) => (d as CardImageGrid)?.Update()));

	public IEnumerable<Hearthstone.Card>? Cards
	{
		get => (IEnumerable<Hearthstone.Card>?)GetValue(CardsProperty);
		set => SetValue(CardsProperty, value);
	}

	public static readonly DependencyProperty MaxCardGridHeightProperty = DependencyProperty.Register(
		nameof(MaxCardGridHeight), typeof(double), typeof(CardImageGrid), new PropertyMetadata(750.0, (d, _) => (d as CardImageGrid)?.Update()));

	public double MaxCardGridHeight
	{
		get => (double)GetValue(MaxCardGridHeightProperty);
		set => SetValue(MaxCardGridHeightProperty, value);
	}

	public static readonly DependencyProperty MaxCardGridWidthProperty = DependencyProperty.Register(
		nameof(MaxCardGridWidth), typeof(double), typeof(CardImageGrid), new PropertyMetadata(600.0, (d, _) => (d as CardImageGrid)?.Update()));

	public double MaxCardGridWidth
	{
		get => (double)GetValue(MaxCardGridWidthProperty);
		set => SetValue(MaxCardGridWidthProperty, value);
	}

	public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
		nameof(Columns), typeof(int), typeof(CardImageGrid), new PropertyMetadata(-1, (d, _) => (d as CardImageGrid)?.Update()));

	public int Columns
	{
		get => (int)GetValue(ColumnsProperty);
		set => SetValue(ColumnsProperty, value);
	}

	public static readonly DependencyProperty MaxCardHeightProperty = DependencyProperty.Register(nameof(MaxCardHeight), typeof(double), typeof(CardImageGrid), new PropertyMetadata(388.0));

	public double MaxCardHeight
	{
		get => (double)GetValue(MaxCardHeightProperty);
		set => SetValue(MaxCardHeightProperty, value);
	}

	public CardImageGrid()
	{
		InitializeComponent();
	}

	public double CardWidth => 256.0;
	public double CardHeight => 388.0;

	public Thickness CardMargin => new(-2, -14, -2, -14);

	private void Update()
	{
		ViewModel.Cards = Cards?.Select(x => new CardAssetViewModel(x, CardAssetType.FullImage)).ToList() ?? new List<CardAssetViewModel>();
		var cardCount = Cards?.Count() ?? 0;
		if(cardCount == 0)
			return;


		var cardHeight = CardHeight + CardMargin.Top + CardMargin.Bottom;
		var cardWidth = CardWidth + CardMargin.Left + CardMargin.Right;
		var maxCardGridWidth = MaxCardGridWidth - 10; // container margin

		var maxScale = Math.Min(1, MaxCardHeight / CardHeight);

		if(cardCount <= 3)
		{
			var cardScale = Math.Min(maxScale, Math.Min(MaxCardGridHeight, maxCardGridWidth / (cardWidth * 3)));
			ViewModel.CardScale = new ScaleTransform { ScaleX = cardScale, ScaleY = cardScale };
			return;
		}

		// Beyond 3 cards: optimize for maximum card scale
		var scale = 0.0;

		if(Columns > 0)
		{
			var cols = Math.Min(Columns, cardCount);
			var rows = Math.Ceiling((double)cardCount / cols);
			scale = Math.Min(maxScale, Math.Min(MaxCardGridHeight / (cardHeight * rows), maxCardGridWidth / (cardWidth * cols)));
		}
		else
		{
			for(var rows = 1; rows < cardCount; rows++)
			{
				var cols = Math.Ceiling((double)cardCount / rows);
				var cardScale = Math.Min(maxScale, Math.Min(MaxCardGridHeight / (cardHeight * rows), maxCardGridWidth / (cardWidth * cols)));
				if(cardScale > scale)
					scale = cardScale;
			}
		}

		ViewModel.CardScale = new ScaleTransform { ScaleX = scale, ScaleY = scale };
	}

	public CardImageGridViewModel ViewModel { get; } = new();
}

public class CardImageGridViewModel : ViewModel
{
	public ScaleTransform CardScale
	{
		get => GetProp<ScaleTransform?>(null) ?? new ScaleTransform(0.7, 0.7);
		set => SetProp(value);
	}

	public List<CardAssetViewModel> Cards
	{
		get => GetProp<List<CardAssetViewModel>?>(null) ?? new List<CardAssetViewModel>();
		set => SetProp(value);
	}
}

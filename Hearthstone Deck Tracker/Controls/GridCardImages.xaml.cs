using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls;

public partial class GridCardImages
{
	public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
		nameof(Title), typeof(string), typeof(GridCardImages), new PropertyMetadata(null));

	public string? Title
	{
		get => (string?)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public static readonly DependencyProperty CardsProperty = DependencyProperty.Register(
		nameof(Cards), typeof(IEnumerable<Hearthstone.Card>), typeof(GridCardImages), new PropertyMetadata(new List<Hearthstone.Card>(), (d, _) => (d as GridCardImages)?.Update()));

	public IEnumerable<Hearthstone.Card>? Cards
	{
		get => (IEnumerable<Hearthstone.Card>?)GetValue(CardsProperty);
		set => SetValue(CardsProperty, value);
	}

	public static readonly DependencyProperty MaxCardGridHeightProperty = DependencyProperty.Register(
		nameof(MaxCardGridHeight), typeof(double), typeof(GridCardImages), new PropertyMetadata(750.0, (d, _) => (d as GridCardImages)?.Update()));

	public double MaxCardGridHeight
	{
		get => (double)GetValue(MaxCardGridHeightProperty);
		set => SetValue(MaxCardGridHeightProperty, value);
	}

	public static readonly DependencyProperty MaxCardGridWidthProperty = DependencyProperty.Register(
		nameof(MaxCardGridWidth), typeof(double), typeof(GridCardImages), new PropertyMetadata(600.0, (d, _) => (d as GridCardImages)?.Update()));

	public double MaxCardGridWidth
	{
		get => (double)GetValue(MaxCardGridWidthProperty);
		set => SetValue(MaxCardGridWidthProperty, value);
	}

	public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
		nameof(Columns), typeof(int), typeof(GridCardImages), new PropertyMetadata(-1, (d, _) => (d as GridCardImages)?.Update()));

	public int Columns
	{
		get => (int)GetValue(ColumnsProperty);
		set => SetValue(ColumnsProperty, value);
	}

	public GridCardImages()
	{
		InitializeComponent();
	}

	//public double MaxGridWidth => 600.0;
	public double CardWidth => 256.0;
	public double CardHeight => 388.0;

	private const double MaxScale = 0.85;
	public Thickness CardMargin => new(-2, -14, -2, -14);

	private void Update()
	{
		ViewModel.Cards = Cards?.Select(x => new CardAssetViewModel(x, CardAssetType.FullImage)).ToList() ?? new List<CardAssetViewModel>();
		var cardCount = Cards?.Count() ?? 0;
		if(cardCount == 0)
		{
			ViewModel.TitleCornerRadius = new CornerRadius(10);
			return;
		}

		ViewModel.TitleCornerRadius = new CornerRadius(10, 10, 0, 0);

		var cardHeight = CardHeight + CardMargin.Top + CardMargin.Bottom;
		var cardWidth = CardWidth + CardMargin.Left + CardMargin.Right;
		var maxCardGridWidth = MaxCardGridWidth - 10; // container margin

		if(cardCount <= 3)
		{
			var cardScale = Math.Min(MaxScale, Math.Min(MaxCardGridHeight, maxCardGridWidth / (cardWidth * 3)));
			ViewModel.CardScale = new ScaleTransform { ScaleX = cardScale, ScaleY = cardScale };
			return;
		}

		// Beyond 3 cards: optimize for maximum card scale
		var scale = 0.0;

		if(Columns > 0)
		{
			var cols = Math.Min(Columns, cardCount);
			var rows = Math.Ceiling((double)cardCount / cols);
			scale = Math.Min(MaxScale, Math.Min(MaxCardGridHeight / (cardHeight * rows), maxCardGridWidth / (cardWidth * cols)));
		}
		else
		{
			for(var rows = 1; rows < cardCount; rows++)
			{
				var cols = Math.Ceiling((double)cardCount / rows);
				var cardScale = Math.Min(MaxScale, Math.Min(MaxCardGridHeight / (cardHeight * rows), maxCardGridWidth / (cardWidth * cols)));
				if(cardScale > scale)
					scale = cardScale;
			}
		}

		ViewModel.CardScale = new ScaleTransform { ScaleX = scale, ScaleY = scale };
	}

	public GridCardImagesViewModel ViewModel { get; } = new();
}

public class GridCardImagesViewModel : ViewModel
{
	public CornerRadius TitleCornerRadius
	{
		get => GetProp(new CornerRadius(10, 10, 0, 0));
		set => SetProp(value);
	}

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

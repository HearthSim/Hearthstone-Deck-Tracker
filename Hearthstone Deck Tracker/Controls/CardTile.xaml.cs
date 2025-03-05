using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Themes;
using Type = System.Type;

namespace Hearthstone_Deck_Tracker.Controls;

public partial class CardTile
{
	public CardTile()
	{
		InitializeComponent();
	}

	private void CardTile_OnLoaded(object sender, RoutedEventArgs e)
	{
		CardDefsManager.CardsChanged += OnCardChanged;
		CardDefsManager.InitialDefsLoaded += OnCardChanged;
		ThemeManager.ThemeChanged += OnCardChanged;
		Helper.CardLanguageChanged += OnCardChanged;
	}

	private void OnCardChanged()
	{
		(DataContext as CardTileViewModel)?.OnCardChanged();
	}

	private void CardTile_OnUnloaded(object sender, RoutedEventArgs e)
	{
		CardDefsManager.CardsChanged -= OnCardChanged;
		CardDefsManager.InitialDefsLoaded -= OnCardChanged;
		ThemeManager.ThemeChanged -= OnCardChanged;
		Helper.CardLanguageChanged -= OnCardChanged;
	}
}

public class CardTileViewModel : CardAssetViewModel, ICardTooltip
{
	private static SolidColorBrush GemDefaultBrush { get; } = new(Color.FromRgb(51, 116, 186));
	private static SolidColorBrush BorderDefaultBrush { get; } = new(Color.FromRgb(10, 10, 10));
	private static SolidColorBrush CommonBrush { get; } = new(Color.FromRgb(255, 255, 255));
	private static SolidColorBrush RareBrush { get; } = new(Color.FromRgb(51, 116, 186));
	private static SolidColorBrush EpicBrush { get; } = new(Color.FromRgb(149,99,206));
	private static SolidColorBrush LegendaryBrush { get; } = new(Color.FromRgb(214, 133, 21));

	public new Hearthstone.Card Card => base.Card!;

	public CardTileViewModel(Hearthstone.Card card) : base(card, CardAssetType.Tile)
	{
		WeakEventManager<Hearthstone.Card, PropertyChangedEventArgs>.AddHandler(Card, nameof(Card.PropertyChanged), CardOnPropertyChanged);
	}

	// @cleanup: We still heavily rely on Hearthstone.Card in many places at the "de-facto card data container".
	// Most of the UI related data could probably be moved here, but that would require a larger refactor.
	// So in the mean time we subscribe to property changes on Hearthstone.Card and update our pass-through
	// properties here accordingly.

	private void CardOnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if(e.PropertyName == nameof(Card.Cost))
			OnPropertyChanged(nameof(Cost));
		else if(e.PropertyName == nameof(Card.Count))
		{
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(nameof(IsCountVisible));
			OnPropertyChanged(nameof(IsCountTextVisible));
			OnPropertyChanged(nameof(IsDarkened));
		}
		else if(e.PropertyName == nameof(Card.Jousted))
		{
			OnPropertyChanged(nameof(IsDarkened));
		}
		else if(e.PropertyName == nameof(Card.Background))
		{
			if(IsPreRendered)
			{
				OnPropertyChanged(nameof(PreRenderedCard));
				OnPropertyChanged(nameof(IsPreRendered));
			}
		}
		else if(e.PropertyName == nameof(Card.IsCreated))
		{
			OnPropertyChanged(nameof(IsCreatedIconVisible));
		}
		else if(e.PropertyName is nameof(Card.HighlightInHand) or nameof(Card.WasDiscarded))
		{
			OnPropertyChanged(nameof(NameColor));
		}
		else if(e.PropertyName is nameof(Card.IsMulliganOption) or nameof(Card.CardWinrates))
		{
			OnPropertyChanged(nameof(MulliganText));
			OnPropertyChanged(nameof(MulliganTextColor));
			OnPropertyChanged(nameof(MulliganBackground));
			OnPropertyChanged(nameof(MulliganBorderColor));
			OnPropertyChanged(nameof(MulliganBorderThickness));
		}
		else if(e.PropertyName == nameof(Card.ExtraInfo))
		{
			OnPropertyChanged(nameof(Name));
		}
	}

	public void OnCardChanged()
	{
		OnPropertyChanged(nameof(Cost));
		OnPropertyChanged(nameof(Tier));
		OnPropertyChanged(nameof(Name));
		OnPropertyChanged(nameof(NameFontFamily));
		OnPropertyChanged(nameof(NameFontWeight));
		OnPropertyChanged(nameof(GemColorBrush));
		OnPropertyChanged(nameof(BorderColorBrush));
		OnPropertyChanged(nameof(BorderOpacity));
		OnPropertyChanged(nameof(IsCountVisible));
		OnPropertyChanged(nameof(IsCountTextVisible));
		OnPropertyChanged(nameof(IsLegendaryIconVisible));
		OnPropertyChanged(nameof(IsCostVisible));
		OnPropertyChanged(nameof(IsBaconSpell));
		OnPropertyChanged(nameof(PreRenderedCard));
		OnPropertyChanged(nameof(IsPreRendered));
	}

	public string Cost
	{
		get
		{
			if(Card.HideStats)
				return string.Empty;
			return Card.IsKnownCard ? Card.Cost.ToString() : CardDefsManager.HasLoadedInitialBaseDefs ? "?" : "";
		}
	}

	public int Count => Card.Count;

	public int Tier => Math.Max(1, Card.TechLevel);

	public string? Name
	{
		get
		{
			var text = Card.IsKnownCard ? Card.LocalizedName : CardDefsManager.HasLoadedInitialBaseDefs ? "???" : "";
			if(Card.ExtraInfo?.CardNameSuffix != null)
				text = $"{text} {Card.ExtraInfo.CardNameSuffix}";
			return text;
		}
	}

	private static FontFamily ChunkfiveFont => new(new Uri("pack://application:,,,/"), "./Resources/#Chunkfive");
	private static FontFamily DefaultFont => new();

	public FontWeight NameFontWeight => Helper.LatinLanguages.Contains(Helper.GetCardLanguage()) ? FontWeights.Normal : FontWeights.Bold;
	public FontFamily NameFontFamily => Helper.LatinLanguages.Contains(Helper.GetCardLanguage()) ? ChunkfiveFont : DefaultFont;
	public SolidColorBrush NameColor => Card.ColorPlayer;

	public SolidColorBrush GemColorBrush
	{
		get
		{
			if(!Config.Instance.RarityCardGems)
				return GemDefaultBrush;
			return Card.Rarity switch
			{
				Rarity.COMMON => CommonBrush,
				Rarity.EPIC => EpicBrush,
				Rarity.LEGENDARY => LegendaryBrush,
				_ => GemDefaultBrush,
			};
		}
	}

	public SolidColorBrush BorderColorBrush
	{
		get
		{
			if(!Config.Instance.RarityCardFrames)
				return BorderDefaultBrush;
			return Card.Rarity switch
			{
				Rarity.COMMON => CommonBrush,
				Rarity.RARE => RareBrush,
				Rarity.EPIC => EpicBrush,
				Rarity.LEGENDARY => LegendaryBrush,
				_ => BorderDefaultBrush,
			};
		}
	}
	public double BorderOpacity
	{
		get
		{
			if(!Config.Instance.RarityCardFrames)
				return 0.3;
			return Card.Rarity switch
			{
				Rarity.COMMON or Rarity.RARE or Rarity.EPIC or Rarity.LEGENDARY => 0.5,
				_ => 0.3,
			};
		}
	}

	public bool IsCountVisible => IsCountTextVisible || IsLegendaryIconVisible;
	public bool IsCountTextVisible => !IsLegendaryIconVisible && Count > 1;
	public bool IsLegendaryIconVisible => Count == 1 && Card.Rarity == Rarity.LEGENDARY;
	public bool IsCostVisible => !Card.BaconCard;
	public bool IsDarkened => Count == 0 || Card.Jousted;

	public DrawingBrush? PreRenderedCard => IsPreRendered ? Card.Background : null;
	public bool IsPreRendered => !Card.BaconCard && ThemeManager.CurrentTheme?.Name != "dark";

	public ImageBrush? PreRenderedHighlight
	{
		get
		{
			if(!IsPreRendered)
				return null;
			return Highlight switch
			{
				HighlightColor.Green => ThemeManager.CurrentTheme?.HighlightImageGreen,
				HighlightColor.Orange => ThemeManager.CurrentTheme?.HighlightImageOrange,
				HighlightColor.Teal => ThemeManager.CurrentTheme?.HighlightImageTeal,
				_ => null
			};
		}
	}

	public HighlightColor Highlight
	{
		get => GetProp(HighlightColor.None);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(PreRenderedHighlight));
			OnPropertyChanged(nameof(HighlightBrush));
		}
	}

	public bool IsCreatedIconVisible => Card.IsCreated;
	public bool IsBaconSpell => Card.TypeEnum == CardType.BATTLEGROUND_SPELL;

	public string? MulliganText => Card.CardWinrates != null ? $"{Card.CardWinrates.Value.MulliganWinrate:0.0}%" : null;

	private static SolidColorBrush DefaultMulliganColor { get; } = new(Color.FromRgb(255, 255, 255));
	public SolidColorBrush MulliganTextColor
	{
		get
		{
			if(Card.CardWinrates == null)
				return DefaultMulliganColor;
			var data = Card.CardWinrates.Value;
			var delta = data.MulliganWinrate - (data.BaseWinrate ?? 50.0f);
			var color = Helper.GetColorString(delta, 75);
			return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
		}
	}

	private static SolidColorBrush DefaultMulliganBorder { get; } = new(Color.FromRgb(79, 86, 91));
	private static SolidColorBrush MulliganOptionBorder { get; } = new(Color.FromRgb(135, 255, 88));
	public SolidColorBrush MulliganBorderColor => Card.IsMulliganOption ? MulliganOptionBorder : DefaultMulliganBorder;

	private static SolidColorBrush DefaultMulliganBackground { get; } = new(Color.FromRgb(22, 22, 22));
	private static SolidColorBrush MulliganOptionBackground { get; } = new(Color.FromRgb(7, 25, 0));
	public SolidColorBrush MulliganBackground => Card.IsMulliganOption ? MulliganOptionBackground : DefaultMulliganBackground;

	private static Thickness DefaultMulliganBorderThickness { get; } = new(1, 1, 1, 1);
	private static Thickness MulliganOptionBorderThickness { get; } = new(2, 2, 2, 2);
	public Thickness MulliganBorderThickness => Card.IsMulliganOption ? MulliganOptionBorderThickness : DefaultMulliganBorderThickness;

	private static SolidColorBrush HighlightGreen { get; } = new(Color.FromRgb(51, 204, 51));
	private static SolidColorBrush HighlightOrange { get; } = new(Color.FromRgb(204, 153, 51));
	private static SolidColorBrush HighlightTeal { get; } = new(Color.FromRgb(51, 204, 204));

	public SolidColorBrush? HighlightBrush => Highlight switch
	{
		HighlightColor.Green => HighlightGreen,
		HighlightColor.Orange => HighlightOrange,
		HighlightColor.Teal => HighlightTeal,
		_ => null
	};

	public void UpdateTooltip(CardTooltipViewModel viewModel)
	{
		Card.UpdateTooltip(viewModel);
	}
}

public class MultiplyConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if(value == null || parameter == null)
			return null;
		try
		{
			return (double)value * double.Parse((string)parameter, CultureInfo.InvariantCulture);;
		}
		catch
		{
			return null;
		}
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class MulliganMarginConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var margin = new Thickness(0, 0, 4, 0);
		if(value == null || parameter == null)
			return margin;
		try
		{
			margin.Right += (double)value * double.Parse((string)parameter, CultureInfo.InvariantCulture);;
			return margin;
		}
		catch
		{
			return margin;
		}
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

using Hearthstone_Deck_Tracker.Annotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsCardsGroup : UserControl, INotifyPropertyChanged
{

	// Unloading (e.g. by being virtualized away in the minions browser) strips the card list, and
	// re-realizing a recycled container may re-bind an unchanged Cards value, which would leave the
	// group empty. Track the unload so the next load repopulates.
	private bool _repopulateOnLoad;

	private readonly LocalizedPropNotifier _localizedPropNotifier;

	public BattlegroundsCardsGroup()
	{
		InitializeComponent();
		_localizedPropNotifier = new LocalizedPropNotifier(GetType(), OnPropertyChanged);
		Unloaded += (_, _) => _repopulateOnLoad = true;
		Loaded += (_, _) =>
		{
			if(!_repopulateOnLoad)
				return;
			_repopulateOnLoad = false;
			if(Cards is not null)
				UpdateCards(Cards.ToList());
		};
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public static readonly DependencyProperty GroupedByMinionTypeProperty = DependencyProperty.Register(
		nameof(GroupedByMinionType),
		typeof(bool),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata(false, TitleInputChanged)
	);

	public static readonly DependencyProperty GroupedByKeywordProperty = DependencyProperty.Register(
		nameof(GroupedByKeyword),
		typeof(bool),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata(false, TitleInputChanged)
	);

	public static readonly DependencyProperty TierProperty = DependencyProperty.Register(
		nameof(Tier),
		typeof(int?),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata(TitleInputChanged)
	);

	public static readonly DependencyProperty MinionTypeProperty = DependencyProperty.Register(
		nameof(MinionType),
		typeof(Race?),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata(TitleInputChanged)
	);

	public static readonly DependencyProperty KeywordProperty = DependencyProperty.Register(
		nameof(Keyword),
		typeof(BattlegroundsKeyword),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata(TitleInputChanged)
	);

	// bindings write dependency properties via SetValue and bypass the CLR setters, so a recycled
	// container would otherwise keep the header of the group it previously showed
	private static void TitleInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var group = (BattlegroundsCardsGroup)d;
		group.OnPropertyChanged(nameof(Title));
		group.OnPropertyChanged(nameof(TitleVisibility));
		group.OnPropertyChanged(nameof(SubTitle));
		group.OnPropertyChanged(nameof(SubTitleVisibility));
		group.OnPropertyChanged(nameof(HeaderCursor));
	}

	public static readonly DependencyProperty ClickFilterCommandProperty = DependencyProperty.Register(
		nameof(ClickMinionTypeCommand),
		typeof(Command<Race>),
		typeof(BattlegroundsCardsGroup)
	);

	public static readonly DependencyProperty IsInspirationEnabledProperty = DependencyProperty.Register(
		nameof(IsInspirationEnabled),
		typeof(bool),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata()
	);

	public Command<Race>? ClickMinionTypeCommand
	{
		get { return (Command<Race>?)GetValue(ClickFilterCommandProperty); }
		set { SetValue(ClickFilterCommandProperty, value); }
	}

	public bool GroupedByMinionType
	{
		get => (bool)GetValue(GroupedByMinionTypeProperty);
		set => SetValue(GroupedByMinionTypeProperty, value);
	}

	public bool GroupedByKeyword
	{
		get => (bool)GetValue(GroupedByKeywordProperty);
		set => SetValue(GroupedByKeywordProperty, value);
	}

	public int Tier
	{
		get => (int)GetValue(TierProperty);
		set => SetValue(TierProperty, value);
	}

	public Race MinionType
	{
		get => (Race)GetValue(MinionTypeProperty);
		set => SetValue(MinionTypeProperty, value);
	}

	public BattlegroundsKeyword? Keyword
	{
		get => (BattlegroundsKeyword?)GetValue(KeywordProperty);
		set => SetValue(KeywordProperty, value);
	}

	public bool IsInspirationEnabled
	{
		get => (bool)GetValue(IsInspirationEnabledProperty);
		set => SetValue(IsInspirationEnabledProperty, value);
	}

	[LocalizedProp]
	public string Title
	{
		get
		{
			if((GroupedByKeyword && (int)MinionType != -1) || GroupedByMinionType)
			{
				// intentionally making this string use HDT language for now, because it's more of a UI term than a game
				// term
				return string.Format(
					LocUtil.Get("BattlegroundsMinions_TavernTier", useCardLanguage: false),
					Tier > 0 ? Tier.ToString() : "?"
				);
			}

			return (int)MinionType == -1
				?  LocUtil.Get("Battlegrounds_Spells", useCardLanguage: true)
				: HearthDbConverter.GetLocalizedRace(MinionType) ?? string.Empty;
		}
	}

	public string SubTitle
	{
		get
		{
			if(GroupedByKeyword)
				return Keyword?.Name ?? string.Empty;

			if(GroupedByMinionType)
				return HearthDbConverter.GetLocalizedRace(MinionType) ?? string.Empty;

			return string.Empty;
		}
	}

	public Visibility TitleVisibility => string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;
	public Visibility SubTitleVisibility => string.IsNullOrEmpty(SubTitle) ? Visibility.Collapsed : Visibility.Visible;

	public string HeaderBackground => "#1d3657";

	public string HeaderCursor => !GroupedByMinionType ? "Hand" : "Arrow";

	public static readonly DependencyProperty CardsProperty = DependencyProperty.Register(
		nameof(Cards),
		typeof(IEnumerable<Hearthstone.Card>),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata(CardsChanged)
	);

	private static void CardsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var group = (BattlegroundsCardsGroup)d;
		group.Cards = (IEnumerable<Hearthstone.Card>)e.NewValue;
	}

	public IEnumerable<Hearthstone.Card> Cards
	{
		get { return (IEnumerable<Hearthstone.Card>)GetValue(CardsProperty); }
		set
		{
			SetValue(CardsProperty, value);
			UpdateCards(value.ToList());
		}
	}

	public void UpdateCards(List<Hearthstone.Card> cards)
	{
		_repopulateOnLoad = false;
		CardsList.ShowTier7InspirationButton = IsInspirationEnabled;
		CardsList.ShowPinButton = true;
		CardsList.UseBattlegroundsTile = true;
		CardsList.Update(cards, true);
	}
}

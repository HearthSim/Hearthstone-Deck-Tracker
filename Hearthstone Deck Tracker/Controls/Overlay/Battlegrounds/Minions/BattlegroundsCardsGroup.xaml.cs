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

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsCardsGroup : UserControl, INotifyPropertyChanged
{

	public BattlegroundsCardsGroup()
	{
		InitializeComponent();
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
		new PropertyMetadata(false, null)
	);

	public static readonly DependencyProperty GroupedByKeywordProperty = DependencyProperty.Register(
		nameof(GroupedByKeyword),
		typeof(bool),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata(false, null)
	);

	public static readonly DependencyProperty TierProperty = DependencyProperty.Register(
		nameof(Tier),
		typeof(int?),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata()
	);

	public static readonly DependencyProperty MinionTypeProperty = DependencyProperty.Register(
		nameof(MinionType),
		typeof(Race?),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata()
	);

	public static readonly DependencyProperty KeywordProperty = DependencyProperty.Register(
		nameof(Keyword),
		typeof(GameTag?),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata()
	);

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
		get { return (bool)GetValue(GroupedByMinionTypeProperty); }
		set
		{
			SetValue(GroupedByMinionTypeProperty, value);
			OnPropertyChanged(nameof(Title));
			OnPropertyChanged(nameof(TitleVisibility));
			OnPropertyChanged(nameof(SubTitle));
			OnPropertyChanged(nameof(SubTitleVisibility));
		}
	}

	public bool GroupedByKeyword
	{
		get { return (bool)GetValue(GroupedByKeywordProperty); }
		set
		{
			SetValue(GroupedByKeywordProperty, value);
			OnPropertyChanged(nameof(Title));
			OnPropertyChanged(nameof(TitleVisibility));
			OnPropertyChanged(nameof(SubTitle));
			OnPropertyChanged(nameof(SubTitleVisibility));
		}
	}

	public int Tier
	{
		get { return (int)GetValue(TierProperty); }
		set
		{
			SetValue(TierProperty, value);
			OnPropertyChanged(nameof(Title));
			OnPropertyChanged(nameof(TitleVisibility));
			OnPropertyChanged(nameof(SubTitle));
			OnPropertyChanged(nameof(SubTitleVisibility));
		}
	}

	public Race MinionType
	{
		get { return (Race)GetValue(MinionTypeProperty); }
		set
		{
			SetValue(MinionTypeProperty, value);
			OnPropertyChanged(nameof(Title));
			OnPropertyChanged(nameof(TitleVisibility));
			OnPropertyChanged(nameof(SubTitle));
			OnPropertyChanged(nameof(SubTitleVisibility));
		}
	}

	public GameTag Keyword
	{
		get { return (GameTag)GetValue(KeywordProperty); }
		set
		{
			SetValue(KeywordProperty, value);
			OnPropertyChanged(nameof(Title));
			OnPropertyChanged(nameof(TitleVisibility));
			OnPropertyChanged(nameof(SubTitle));
			OnPropertyChanged(nameof(SubTitleVisibility));
		}
	}

	public bool IsInspirationEnabled
	{
		get => (bool)GetValue(IsInspirationEnabledProperty);
		set => SetValue(IsInspirationEnabledProperty, value);
	}

	public string Title
	{
		get
		{
			if((GroupedByKeyword && (int)MinionType != -1) || GroupedByMinionType)
			{
				return string.Format(
					LocUtil.Get("BattlegroundsMinions_TavernTier", useCardLanguage: true),
					Tier
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
				return HearthDbConverter.GetLocalizedKeyword(Keyword);

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
		CardsList.ShowTier7InspirationButton = IsInspirationEnabled;
		CardsList.Update(cards, true);
	}
}

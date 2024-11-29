using Hearthstone_Deck_Tracker.Annotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;
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
			var minionTypeName = (int)MinionType == -1
				?  LocUtil.Get("Battlegrounds_Spells", useCardLanguage: true)
				: HearthDbConverter.GetLocalizedRace(MinionType) ?? string.Empty;
			if(!GroupedByMinionType)
				return minionTypeName;

			if(minionTypeName == string.Empty)
				return string.Format(LocUtil.Get("BattlegroundsMinions_TavernTier", useCardLanguage: true), Tier);

			return string.Format(
				LocUtil.Get("BattlegroundsMinions_TavernTierMinionType", useCardLanguage: true),
				Tier, minionTypeName
			);
		}
	}

	public Visibility TitleVisibility => string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;

	private bool _hoveringPanel = false;
	private bool HoveringPanel
	{
		get { return _hoveringPanel; }
		set
		{
			_hoveringPanel = value;
			OnPropertyChanged(nameof(BtnFilterVisibility));
			OnPropertyChanged(nameof(HeaderBackground));
		}
	}

	private bool _hoveringHeader = false;
	private bool HoveringHeader
	{
		get { return _hoveringHeader; }
		set
		{
			_hoveringHeader = value;
			OnPropertyChanged(nameof(BtnFilterVisibility));
			OnPropertyChanged(nameof(HeaderBackground));
		}
	}

	public Visibility BtnFilterVisibility => (HoveringPanel || HoveringHeader) && !GroupedByMinionType ? Visibility.Visible : Visibility.Hidden;

	public string HeaderBackground => HoveringHeader && !GroupedByMinionType
		? "#24436c"
		: "#1d3657";

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

	private void Panel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		HoveringPanel = true;
	}

	private void Panel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		HoveringPanel = false;
	}

	private void Header_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		HoveringHeader = true;
	}

	private void Header_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		HoveringHeader = false;
	}

	private void Header_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if(!GroupedByMinionType)
			ClickMinionTypeCommand?.Execute(MinionType);
	}
}

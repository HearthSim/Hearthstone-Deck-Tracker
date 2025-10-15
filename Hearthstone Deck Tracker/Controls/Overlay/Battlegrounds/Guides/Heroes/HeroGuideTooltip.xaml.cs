using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Heroes;

public partial class HeroGuideTooltip : UserControl
{
	public HeroGuideTooltip()
	{
		InitializeComponent();
	}

	public static readonly DependencyProperty CardsProperty = DependencyProperty.Register(
		nameof(Cards), typeof(IEnumerable<Hearthstone.Card>), typeof(HeroGuideTooltip), new PropertyMetadata(new List<Hearthstone.Card>(), (d, _) => (d as HeroGuideTooltip)?.Update()));

	public IEnumerable<Hearthstone.Card>? Cards
	{
		get => (IEnumerable<Hearthstone.Card>?)GetValue(CardsProperty);
		set => SetValue(CardsProperty, value);
	}

	public static Race GetRace(int raceNumber)
	{
		if (Enum.IsDefined(typeof(Race), raceNumber))
		{
			return (Race)raceNumber;
		}
		return Race.INVALID;
	}

	private void Update()
	{
		var heroPowerDbfId = Cards?.FirstOrDefault()?.DbfId;

		if(heroPowerDbfId == null)
			return;

		var heroPowerCard = HearthDb.Cards.GetFromDbfId(heroPowerDbfId.Value);

		var heroDbfId = heroPowerCard?.Entity.Tags
			.FirstOrDefault(tag => tag.EnumId == (int)GameTag.BACON_HEROPOWER_BASE_HERO_ID)?.Value;

		ViewModel.HoveredHeroDbfid = heroDbfId;

		var availableRaces = BattlegroundsUtils.GetAvailableRaces()?.ToList();
		var heroGuide = Core.Overlay.BattlegroundsHeroGuideListViewModel.GetHeroGuide(heroDbfId ?? 0);

		ViewModel.FavorableTribes = heroGuide?.FavorableTribes?
			.Select(GetRace)
			.Where(race => availableRaces?.Contains(race) == true);

		if (heroGuide != null && !string.IsNullOrEmpty(heroGuide.PublishedGuide))
			ViewModel.PublishedGuide = ReferencedCardRun.ParseCardsFromText(heroGuide.PublishedGuide).FirstOrDefault();
		else
		{
			ViewModel.PublishedGuide = null;
			ViewModel.BuddyGuide = null;
			ViewModel.FavorableTribes = null;
			return;
		}

		ViewModel.BuddyGuide = !string.IsNullOrEmpty(heroGuide.BuddyGuide) ? ReferencedCardRun.ParseCardsFromText(heroGuide.BuddyGuide).FirstOrDefault() : null;
	}

	public BattlegroundsHeroGuideTooltipViewModel ViewModel { get; } = new();
}

public class BattlegroundsHeroGuideTooltipViewModel : ViewModel
{
	public int? HoveredHeroDbfid
	{
		get => GetProp<int?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(HeroGuideVisibility));
		}
	}

	public IEnumerable<Inline>? PublishedGuide
	{
		get => GetProp<IEnumerable<Inline>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(IsGuidePublished));
		}
	}

	public IEnumerable<Race>? FavorableTribes
	{
		get => GetProp<IEnumerable<Race>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(FavorableTribesVisibility));
		}
	}

	public Visibility HeroGuideVisibility
	{
		get
		{
			if(HoveredHeroDbfid == null ||
			   Core.Game.BattlegroundsHeroPickState.OfferedHeroDbfIds == null ||
			   Config.Instance.ShowBattlegroundsBrowser == false ||
			   Config.Instance.ShowBattlegroundsGuides == false
			)
				return Collapsed;

			var offeredHeroDbfIds = Core.Game.BattlegroundsHeroPickState.OfferedHeroDbfIds;

			// The offered hero IDs can be a skins, so we need to get the base hero id.
			var baseHeroDbfIds = offeredHeroDbfIds.Select(dbfId =>
			{
				var heroCard = Database.GetCardFromDbfId(dbfId, false);
				return heroCard?.BattlegroundsSkinParentId > 0 ? heroCard.BattlegroundsSkinParentId : dbfId;
			}).ToList();

			return baseHeroDbfIds.Contains(HoveredHeroDbfid.Value) ? Visible : Collapsed;
		}
	}
	public bool IsGuidePublished => PublishedGuide != null && PublishedGuide.Any();
	public Visibility FavorableTribesVisibility => FavorableTribes != null && FavorableTribes.Any() ? Visible : Collapsed;

	public IEnumerable<Inline>? BuddyGuide
	{
		get => GetProp<IEnumerable<Inline>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(IsBuddyGuidePublished));
		}
	}
	public bool IsBuddyGuidePublished => BuddyGuide != null && BuddyGuide.Any();
}

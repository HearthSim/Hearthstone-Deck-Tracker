using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;
namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Heroes;

public class BattlegroundsHeroGuideViewModel : ViewModel
{
	public BattlegroundsHeroGuide? HeroGuide {
		get => GetProp<BattlegroundsHeroGuide?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(HowToPlay));
			OnPropertyChanged(nameof(FavorableTribes));
			OnPropertyChanged(nameof(FavorableTribesVisibility));
			OnPropertyChanged(nameof(IsGuidePublished));
			OnPropertyChanged(nameof(LastUpdatedFormatted));
		}
	}
	public Hearthstone.Card? HeroCard
	{
		get => GetProp<Hearthstone.Card?>(null);
		set
		{
			SetProp(value);
			CardAsset = new(value, CardAssetType.Portrait);
			OnPropertyChanged(nameof(IsHeroSelected));
		}
	}

	public static Race GetRace(int raceNumber)
	{
		if (Enum.IsDefined(typeof(Race), raceNumber))
		{
			return (Race)raceNumber;
		}
		return Race.INVALID;
	}

	public bool IsHeroSelected => HeroCard != null;

	[LocalizedProp]
	public string? LastUpdatedFormatted => HeroGuide?.LastUpdated != null ? LocUtil.GetAge(HeroGuide.LastUpdated.Value) : null;

	public CardAssetViewModel? CardAsset
	{
		get => GetProp<CardAssetViewModel?>(null);
		set => SetProp(value);
	}

	public IEnumerable<Inline>? HowToPlay =>
		HeroGuide != null ? ReferencedCardRun.ParseCardsFromText(HeroGuide.PublishedGuide).FirstOrDefault() : null;

	public IEnumerable<Race>? FavorableTribes {
		get
		{
			var availableRaces = BattlegroundsUtils.GetAvailableRaces()?.ToList();
			return HeroGuide?.FavorableTribes?
				.Select(GetRace)
				.Where(race => availableRaces?.Contains(race) == true);
		}
	}

	public Visibility FavorableTribesVisibility => FavorableTribes != null && FavorableTribes.Any() ? Visible : Collapsed;

	public bool IsGuidePublished => HowToPlay != null && HowToPlay.Any();
}

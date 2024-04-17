using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public class BattlegroundsMinionsViewModel : ViewModel
{
	private Lazy<BattlegroundsDb> _db = new();

	public IEnumerable<Race>? AvailableRaces
	{
		get => GetProp<IEnumerable<Race>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged();
			OnPropertyChanged(nameof(Groups));
			OnPropertyChanged(nameof(UnavailableRaces));
			OnPropertyChanged(nameof(UnavailableMinionTypesVisibility));
		}
	}

	public List<Race> UnavailableRaces
	{
		get => AvailableRaces is null ? new List<Race>() : _db.Value.Races.Where(x => !AvailableRaces.Contains(x) && x != Race.INVALID && x != Race.ALL).ToList();
	}

	public int? ActiveTier
	{
		get => GetProp<int?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged();
			OnPropertyChanged(nameof(TierButtons));
			OnPropertyChanged(nameof(Groups));
			OnPropertyChanged(nameof(UnavailableMinionTypesVisibility));
		}
	}

	public bool IsDuos
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Groups));
		}
	}

	public string? Anomaly
	{
		get => GetProp<string?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(AvailableTiers));
			OnPropertyChanged(nameof(TierButtons));
			OnPropertyChanged(nameof(Groups));
		}
	}

	public bool IsThorimRelevant
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(AvailableTiers));
			OnPropertyChanged(nameof(TierButtons));
			OnPropertyChanged(nameof(Groups));
		}
	}

	public IEnumerable<string>? BannedMinions
	{
		get => GetProp<IEnumerable<string>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged();
			OnPropertyChanged(nameof(Groups));
		}
	}

	public class TierButton
	{
		public int Tier { get; set; }
		public bool Active { get; set; }
		public bool Available { get; set; }
		public bool Faded { get; set; }
	}

	public List<int> AvailableTiers => BattlegroundsUtils.GetAvailableTiers(Anomaly).ToList();

	public List<TierButton> TierButtons
	{
		get
		{
			var tiers = Enumerable.Range(1, 6).ToList();
			if(AvailableTiers.Contains(7) || IsThorimRelevant)
				tiers.Add(7);
			return tiers.Select(x => new TierButton()
			{
				Tier = x,
				Active = x == ActiveTier,
				Available = AvailableTiers.Contains(x),
				Faded = ActiveTier != null && ActiveTier != x,
			}).ToList();
		}
	}

	public class CardGroup
	{
		public int Order { get; set; } = 0;
		public string? Title { get; set; }
		public IEnumerable<Hearthstone.Card> Cards { get; set; } = new List<Hearthstone.Card>();
	}

	public IEnumerable<CardGroup> Groups
	{
		get
		{
			var groups = new List<CardGroup>();
			if(!(ActiveTier is int tier))
				return groups;

			var isTierAvailable = AvailableTiers.Contains(tier);

			foreach(var race in _db.Value.Races)
			{
				if(AvailableRaces != null && !AvailableRaces.Contains(race) && race != Race.INVALID && race != Race.ALL)
					continue;

				IEnumerable<Hearthstone.Card> cards = _db.Value.GetCards(tier, race, IsDuos);

				if(!cards.Any())
					continue;

				if(!isTierAvailable || (BannedMinions != null && BannedMinions.Any()))
					cards = cards.Select(x =>
					{
						if(isTierAvailable && (BannedMinions == null || !BannedMinions.Contains(x.Id)))
							return x;

						var ret = (Hearthstone.Card)x.Clone();
						ret.Count = 0;
						return ret;
					});

				groups.Add(new CardGroup()
				{
					Order = race switch {
						Race.ALL => -1,
						Race.INVALID => 1,
						_ => 0
					},
					Title = HearthDbConverter.GetLocalizedRace(race),
					Cards = cards.OrderBy(x => x.LocalizedName),
				});
			}

			IEnumerable<Hearthstone.Card> spells = _db.Value.GetSpells(tier, IsDuos);
			if(spells.Any())
			{
				spells = spells.Select(x =>
				{
					if(isTierAvailable)
						return x;

					var ret = (Hearthstone.Card)x.Clone();
					ret.Count = 0;
					return ret;
				});

				groups.Add(new CardGroup()
				{
					Title = LocUtil.Get("Battlegrounds_Spells", useCardLanguage: true),
					Cards = spells.OrderBy(x => x.Cost).ThenBy(x => x.LocalizedName),
					Order = 2,
				});
			}

			return groups.OrderBy(x => x.Order).ThenBy(x => x.Title);
		}
	}

	public void OnHeroPowers(IEnumerable<string> heroPowers)
	{
		IsThorimRelevant = heroPowers.Any(
			x => x is HearthDb.CardIds.NonCollectible.Neutral.ThorimStormlord_ChooseYourChampion or HearthDb.CardIds.NonCollectible.Neutral.ThorimStormlord_ThorimsChampion
		);
	}

	public Visibility UnavailableMinionTypesVisibility => ActiveTier is null || !UnavailableRaces.Any() ? Visibility.Collapsed : Visibility.Visible;

	public void Reset()
	{
		AvailableRaces = null;
		ActiveTier = null;
		IsDuos = false;
		Anomaly = null;
		IsThorimRelevant = false;
		BannedMinions = null;
	}
}

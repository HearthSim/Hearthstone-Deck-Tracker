using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public class BattlegroundsMinionsViewModel : ViewModel
{
	private static BattlegroundsDb Db => BattlegroundsDbSingleton.Instance;

	public IEnumerable<Race>? AvailableRaces
	{
		get => GetProp<IEnumerable<Race>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Groups));
			OnPropertyChanged(nameof(MinionTypeButtons));
			OnPropertyChanged(nameof(UnavailableRaces));
			OnPropertyChanged(nameof(UnavailableMinionTypesVisibility));
		}
	}

	public List<Race> UnavailableRaces
	{
		get => AvailableRaces is null ? new List<Race>() : Db.Races.Where(x => !AvailableRaces.Contains(x) && x != Race.INVALID && x != Race.ALL).ToList();
	}

	public int? ActiveTier
	{
		get => GetProp<int?>(null);
		set
		{
			SetProp(value);
			if(value != null)
			{
				ActiveMinionType = null;
				ActiveMinionKeyword = null;
			}
			OnPropertyChanged(nameof(TierButtons));
			OnPropertyChanged(nameof(KeywordButtons));
			OnPropertyChanged(nameof(Groups));
			OnPropertyChanged(nameof(UnavailableMinionTypesVisibility));
		}
	}

	public Race? ActiveMinionType
	{
		get => GetProp<Race?>(null);
		set
		{
			SetProp(value);
			if(value != null)
			{
				ActiveTier = null;
				ActiveMinionKeyword = null;
			}
			OnPropertyChanged(nameof(TierButtons));
			OnPropertyChanged(nameof(KeywordButtons));
			OnPropertyChanged(nameof(MinionTypeButtons));
			OnPropertyChanged(nameof(IsFilterButtonVisible));
			OnPropertyChanged(nameof(IsExtraFilterSelected));
			OnPropertyChanged(nameof(Groups));
			OnPropertyChanged(nameof(UnavailableMinionTypesVisibility));
		}
	}

	public GameTag? ActiveMinionKeyword
	{
		get => GetProp<GameTag?>(null);
		set
		{
			SetProp(value);
			if(value != null)
			{
				ActiveMinionType = null;
				ActiveTier = null;
			}
			OnPropertyChanged(nameof(TierButtons));
			OnPropertyChanged(nameof(KeywordButtons));
			OnPropertyChanged(nameof(IsFilterButtonVisible));
			OnPropertyChanged(nameof(IsExtraFilterSelected));
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

	public bool IsInspirationEnabled
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
			OnPropertyChanged(nameof(KeywordButtons));
			OnPropertyChanged(nameof(Groups));
		}
	}

	public bool IsThorimRelevant
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			UpdateTavernTier7Visibility();
		}
	}

	public bool IsPaglesFishingRodRelevant
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			UpdateTavernTier7Visibility();
		}
	}

	private bool ShowTavernTier7 => Config.Instance.AlwaysShowBattlegroundsTavernTier7 || IsThorimRelevant || IsPaglesFishingRodRelevant;

	public void UpdateTavernTier7Visibility()
	{
		OnPropertyChanged(nameof(AvailableTiers));
		OnPropertyChanged(nameof(TierButtons));
		OnPropertyChanged(nameof(KeywordButtons));
		OnPropertyChanged(nameof(Groups));
		OnPropertyChanged(nameof(ShowTavernTier7));
	}

	public class TierButton
	{
		public int Tier { get; set; }
		public bool Active { get; set; }
		public bool Available { get; set; }
		public bool Faded { get; set; }

		public int Size { get; set; }
	}

	public List<int> AvailableTiers => BattlegroundsUtils.GetAvailableTiers(Anomaly).ToList();

	public List<TierButton> TierButtons
	{
		get
		{
			var tiers = Enumerable.Range(1, 6).ToList();
			var shouldShowTier7 = AvailableTiers.Contains(7) || ShowTavernTier7;
			if(shouldShowTier7)
				tiers.Add(7);
			return tiers.Select(x => new TierButton()
			{
				Tier = x,
				Active = x == ActiveTier,
				Available = AvailableTiers.Contains(x),
				Faded = (ActiveTier != null && ActiveTier != x) || IsExtraFilterSelected,
				Size = shouldShowTier7 ? 33 : 38
			}).ToList();
		}
	}

	public List<GameTag> AvailableKeywords => BattlegroundsUtils.GetAvailableKeywords();

	public class KeywordButton
	{
		public GameTag Keyword { get; set; }
		public string KeywordName => HearthDbConverter.GetLocalizedKeyword(Keyword);

		public bool Active { get; set; }
		public bool Faded { get; set; }
		public int Size { get; set; }
	}

	public List<KeywordButton> KeywordButtons
	{
		get
		{
			return AvailableKeywords.Select(x => new KeywordButton()
			{
				Keyword = x,
				Active = x == ActiveMinionKeyword,
				Faded = ActiveMinionKeyword != null && ActiveMinionKeyword != x,
				Size = 38
			}).ToList();
		}
	}

	public class MinionTypeButton
	{
		public Race MinionType { get; set; }
		public bool Active { get; set; }
		public bool Available { get; set; }
		public bool Faded { get; set; }

		public int Size { get; set; }
	}

	public List<MinionTypeButton> MinionTypeButtons
	{
		get
		{
			var races = (AvailableRaces ?? Db.Races).ToList();
			// Move OTHER to the end
			races.Remove(Race.INVALID);
			races.Add(Race.INVALID);

			races.Remove(Race.ALL); // Don't show ALL
			races.Add((Race)(-1)); // Spells
			races.Add((Race)(-2)); // Buddies

			return races.Select(x => new MinionTypeButton()
			{
				MinionType = x,
				Active = x == ActiveMinionType,
				Available = races.Contains(x),
				Faded = ActiveMinionType != null && ActiveMinionType != x,
				Size = 34
			}).ToList();
		}
	}

	public bool IsExtraFilterSelected => ActiveMinionType != null || ActiveMinionKeyword != null;
	public bool IsFilterButtonVisible => ActiveMinionType != null || ActiveMinionKeyword != null || IsFilterRegionHovered || IsFiltersOpen;


	public bool IsFilterRegionHovered
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(IsFilterButtonVisible));
		}
	}

	public class CardGroup
	{
		public int Tier { get; set;  }

		/// <summary>
		/// The minion type or -1 for spells
		/// </summary>
		public Race MinionType { get; set; }

		public bool GroupedByMinionType { get; set; } = false;

		public GameTag Keyword { get; set; }

		public bool GroupedByKeyword { get; set; } = false;

		public IEnumerable<Hearthstone.Card> Cards { get; set; } = new List<Hearthstone.Card>();

		public bool IsInspirationEnabled { get; set; }

	}

	public IEnumerable<CardGroup> Groups
	{
		get
		{
			var groups = new List<CardGroup>();
			if(ActiveTier is int tier)
			{
				var isTierAvailable = AvailableTiers.Contains(tier);

				foreach(var race in Db.Races)
				{
					if(AvailableRaces != null && !AvailableRaces.Contains(race) && race != Race.INVALID
					   && race != Race.ALL)
						continue;

					IEnumerable<Hearthstone.Card> cards = Db.GetCards(tier, race, IsDuos);

					if(!cards.Any())
						continue;

					groups.Add(new CardGroup
					{
						Tier = tier,
						MinionType = race,
						Cards = cards.OrderBy(x => x.LocalizedName),
						IsInspirationEnabled = IsInspirationEnabled,
					});
				}

				IEnumerable<Hearthstone.Card> spells = Db.GetSpells(tier, IsDuos);
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

					groups.Add(new CardGroup
					{
						Tier = tier,
						MinionType = (Race)(-1), // Spells are encoded as a -1 race
						Cards = spells.OrderBy(x => x.Cost).ThenBy(x => x.LocalizedName),
						IsInspirationEnabled = IsInspirationEnabled,
					});
				}

				return groups
					.OrderBy(x =>
						(x.MinionType) switch
						{
							Race.ALL => -1, // Always first
							Race.INVALID => 1, // Other
							(Race)(-1) => 2, // Spells
							_ => 0, // Minion Types
						}
					)
					.ThenBy(x => HearthDbConverter.GetLocalizedRace(x.MinionType) ?? string.Empty);
			}
			else if(ActiveMinionType is Race minionType)
			{
				var tiers = AvailableTiers;
				if(ShowTavernTier7)
					tiers.Add(7);
				foreach(var tierGroup in tiers)
				{
					var cards = new List<Hearthstone.Card>();

					// Spells
					if ((int)minionType == -1)
					{
						cards = Db.GetSpells(tierGroup, IsDuos)
							.OrderBy(x => x.Cost)
							.ThenBy(x => x.LocalizedName)
							.ToList();
					}
					// Buddies
					else if ((int)minionType == -2)
					{

						cards = Db.GetBuddies(tierGroup, IsDuos)
							.OrderBy(x => x.LocalizedName)
							.ToList();
					}
					else
					{
						// Get minion cards of the specified type
						var typeCards = Db.GetCards(tierGroup, minionType, IsDuos);

						// Add neutral cards (Race.ALL) if needed
						if (minionType != Race.ALL && minionType != Race.INVALID)
						{
							var neutralCards = Db.GetCards(tierGroup, Race.ALL, IsDuos);
							cards = typeCards.Concat(neutralCards).OrderBy(x => x.LocalizedName).ToList();
						}
						else
						{
							cards = typeCards.OrderBy(x => x.LocalizedName).ToList();
						}
					}

					if(!cards.Any())
						continue;

					groups.Add(new CardGroup
					{
						Tier = tierGroup,
						MinionType = minionType,
						GroupedByMinionType = true,
						Cards = cards,
						IsInspirationEnabled = IsInspirationEnabled,
					});
				}
			}
			else if(ActiveMinionKeyword is GameTag activeMinionKeyword)
			{
				var tiers = AvailableTiers;
				if(ShowTavernTier7)
					tiers.Add(7);
				foreach(var tierGroup in tiers)
				{
					var cards =  Db.GetCards(tierGroup, activeMinionKeyword, AvailableRaces ?? Db.Races, IsDuos)
							.OrderBy(x => x.LocalizedName).ToList();

					if(!cards.Any())
						continue;

					groups.Add(new CardGroup
					{
						Tier = tierGroup,
						Keyword = activeMinionKeyword,
						GroupedByKeyword = true,
						Cards = cards,
						IsInspirationEnabled = IsInspirationEnabled,
					});
				}

				IEnumerable<Hearthstone.Card> spells = Db.GetSpells(activeMinionKeyword, IsDuos);
				if(spells.Any())
				{
					groups.Add(new CardGroup
					{
						Tier = 0,
						Keyword = activeMinionKeyword,
						MinionType = (Race)(-1), // Spells are encoded as a -1 race
						Cards = spells.OrderBy(x => x.Cost).ThenBy(x => x.LocalizedName),
						GroupedByKeyword = true,
						IsInspirationEnabled = IsInspirationEnabled,
					});
				}
			}

			return groups;
		}
	}

	public void OnHeroPowers(IEnumerable<string> heroPowers)
	{
		IsThorimRelevant = heroPowers.Any(
			x => x is HearthDb.CardIds.NonCollectible.Neutral.ThorimStormlord_ChooseYourChampion or HearthDb.CardIds.NonCollectible.Neutral.ThorimStormlord_ThorimsChampion
		);
	}

	public void OnTrinkets(IEnumerable<string> trinkets)
	{
		IsPaglesFishingRodRelevant = trinkets.Contains(HearthDb.CardIds.NonCollectible.Neutral.PaglesFishingRod);
	}

	public Visibility UnavailableMinionTypesVisibility => ActiveTier is null || ActiveMinionType != null || ActiveMinionKeyword != null || !UnavailableRaces.Any() ? Visibility.Collapsed : Visibility.Visible;

	public ICommand FilterClickCommand => new Command(() =>
	{
		IsFiltersOpen = !IsFiltersOpen;
		if(IsFiltersOpen)
			Core.Game.Metrics.BattlegroundsBrowserOpenFilterPanelClicks++;
	});

	public bool IsFiltersOpen
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(IsFilterButtonVisible));
		}
	}

	public void Reset()
	{
		AvailableRaces = null;
		ActiveTier = null;
		ActiveMinionType = null;
		ActiveMinionKeyword = null;
		IsDuos = false;
		Anomaly = null;
		IsThorimRelevant = false;
		IsPaglesFishingRodRelevant = false;
	}

	private bool _preloadedCardTiles;
	public void PreloadCardTiles()
	{
		if(_preloadedCardTiles)
			return;
		_preloadedCardTiles = true;
		var downloader = AssetDownloaders.cardTileDownloader;
		if(downloader == null)
			return;

		var races = Enum.GetValues(typeof(Race)).Cast<Race>();
		foreach(var tier in new [] { 1, 2, 3, 4, 5, 6, 7 })
			foreach(var race in races)
				foreach(var card in Db.GetCards(tier, race, true))
					downloader.GetAssetData(card).Forget();
	}
}

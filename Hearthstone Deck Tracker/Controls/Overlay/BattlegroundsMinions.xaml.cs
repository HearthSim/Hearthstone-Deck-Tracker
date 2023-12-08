using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsMinions : UserControl
	{
		private Lazy<BattlegroundsDb> _db = new();
		private readonly List<BattlegroundsTier> _tierIcons = new();
		//We remove the below cardid from the bg tier list, it appears to have been incorrectly classified as a bg minion by blizzard in patch 20.0.0.
		//Hopefully it will be fixed soon and can be removed.
		private const string NonBgMurlocTidehunterCardId = HearthDb.CardIds.Collectible.Neutral.MurlocTidecallerVanilla;

		public int ActiveTier { get; set; }
		public ObservableCollection<BattlegroundsCardsGroup> Groups { get; set; } = new();
		private BattlegroundsCardsGroup? _spellGroup;
		private Dictionary<Race, BattlegroundsCardsGroup> _groupsByRace = new();

		public BattlegroundsMinions()
		{
			InitializeComponent();
			_tierIcons = BgTierIcons.Children.Cast<BattlegroundsTier>().ToList();
		}

		private void BgTier_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			MinionScrollViewer.ScrollToTop();
			var tier = ((BattlegroundsTier)sender).Tier;
			var races = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats?.GameId) ?? _db.Value.Races;
			Update(tier == ActiveTier ? 0 : tier, races);
			Core.Game.Metrics.IncrementBattlegroundsMinionsTabClick();
		}

		private HashSet<int> _availableTiers = BattlegroundsUtils.GetAvailableTiers(null);
		public void SetAvailableTiers(HashSet<int> tiers)
		{
			_availableTiers = tiers;
			for(var i = 0; i < 7; i++)
			{
				var isAvailable = _availableTiers.Contains(i + 1);
				_tierIcons[i].SetAvailable(isAvailable);
				if(i == 6)
					_tierIcons[i].Visibility = isAvailable ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private List<string> _bannedMinionCardIds = new List<string>();
		public void SetBannedMinions(IEnumerable<string> bannedCardIds)
		{
			_bannedMinionCardIds = bannedCardIds.ToList();
		}

		public void Reset()
		{
			SetAvailableTiers(BattlegroundsUtils.GetAvailableTiers(null));
			Update(0, _db.Value.Races);
		}

		private bool AddOrUpdateBgCardGroup(Race race, List<Hearthstone.Card> cards)
		{
			var addedNew = false;
			if(!_groupsByRace.TryGetValue(race, out var existing))
			{
				existing = new BattlegroundsCardsGroup() { Title = HearthDbConverter.GetLocalizedRace(race) ?? race.ToString()};
				Groups.Add(existing);
				_groupsByRace.Add(race, existing);
				addedNew = true;
			}
			var sortedCards = cards
				.OrderBy(x => x.LocalizedName)
				.ToList();
			existing.UpdateCards(sortedCards);
			return addedNew;
		}

		private bool AddOrUpdateSpellGroup(List<Hearthstone.Card> cards)
		{
			var addedNew = false;
			if(_spellGroup is null)
			{
				_spellGroup = new BattlegroundsCardsGroup() { Title = LocUtil.Get("Battlegrounds_Spells", useCardLanguage: true) };
				Groups.Add(_spellGroup);
				addedNew = true;
			}
			var sortedCards = cards
				.OrderBy(x => x.Cost)
				.ThenBy(x => x.LocalizedName)
				.ToList();
			_spellGroup.UpdateCards(sortedCards);
			return addedNew;
		}

		private Dictionary<Race, List<string>> DifferentTribeClassifiedCards = new Dictionary<Race, List<string>>() { { Race.QUILBOAR, new List<string>() { HearthDb.CardIds.NonCollectible.Neutral.AgamagganTheGreatBoar } } };

		private void Update(int tier, IEnumerable<Race> availableRaces)
		{
			if (ActiveTier == tier)
				return;
			ActiveTier = tier;
			foreach(var item in _tierIcons)
				item.Active = tier == item.Tier;

			if(tier < 1 || tier > 7)
			{
				for(var i = 0; i < 7; i++)
					_tierIcons[i].SetFaded(false);
				Groups.Clear();
				_groupsByRace.Clear();
				_spellGroup = null;
				UnavailableTypes.UnavailableTypesVisibility = System.Windows.Visibility.Collapsed;
				return;
			}
			for(var i = 0; i < 7; i++)
				_tierIcons[i].SetFaded(i != tier - 1);

			var resort = false;

			var unavailableRaces = _db.Value.Races.Where(x => !availableRaces.Contains(x) && x != Race.INVALID && x != Race.ALL)
				.Select(x => HearthDbConverter.GetLocalizedRace(x))
				.OrderBy(x => x)
				.ToList();
			if(unavailableRaces.Count > 0)
			{
				UnavailableTypes.UnavailableTypesVisibility = System.Windows.Visibility.Visible;
				UnavailableTypes.UnavailableRacesText = string.Join(", ", unavailableRaces);
			}
			else
			{
				UnavailableTypes.UnavailableTypesVisibility = System.Windows.Visibility.Collapsed;
				UnavailableTypes.UnavailableRacesText = "";
			}

			var spells = _db.Value.GetSpells(tier);
			if(spells.Any())
				resort |= AddOrUpdateSpellGroup(spells);
			else
				_spellGroup?.Hide();

			foreach(var race in _db.Value.Races)
			{
				var cards = _db.Value.GetCards(tier, race).ToList();

				if(race == Race.MURLOC)
					cards = cards.Where(x => x.Id != NonBgMurlocTidehunterCardId).ToList();

				foreach(var otherTribeCardsToInclude in DifferentTribeClassifiedCards.Where(x => x.Key != race))
					cards = cards.Where(x => !otherTribeCardsToInclude.Value.Contains(x.Id)).ToList();

				foreach(var otherTribeCardsToInclude in DifferentTribeClassifiedCards.Where(x => x.Key == race))
				{
					foreach(var cardId in otherTribeCardsToInclude.Value)
					{
						if(HearthDb.Cards.All.TryGetValue(cardId, out var card))
						{
							if(card.Entity.GetTag(GameTag.TECH_LEVEL) == tier && card.Entity.GetTag(GameTag.IS_BACON_POOL_MINION) > 0)
								cards.Add(new Hearthstone.Card(card, true));
						}
					}
				}

				if(!_availableTiers.Contains(tier))
				{
					// Fade out all minions from unavailable tiers
					cards = cards.Select(x =>
					{
						var ret = (Hearthstone.Card)x.Clone();
						ret.Count = 0;
						return ret;
					}).ToList();
				}
				else if(_bannedMinionCardIds.Count > 0) {
					// Fade out banned minions
					cards = cards.Select(x =>
					{
						if(_bannedMinionCardIds.Contains(x.Id))
						{
							var ret = (Hearthstone.Card)x.Clone();
							ret.Count = 0;
							return ret;
						}
						return x;
					}).ToList();
				}

				if(cards.Count == 0)
				{
					if(_groupsByRace.TryGetValue(race, out var group))
					{
						group.Hide();
					}
				}
				else
				{
					if(race == Race.ALL || race == Race.INVALID || availableRaces.Contains(race))
						resort |= AddOrUpdateBgCardGroup(race, cards);
				}
			}

			if (resort)
			{
				_groupsByRace.TryGetValue(Race.INVALID, out var invalidGroup);
				var items = Groups.ToList()
					.OrderBy(x => x == _spellGroup)
					.ThenBy(x => x == invalidGroup)
					.ThenBy(x => x.Title);
				foreach(var item in items)
				{
					Groups.Remove(item);
					Groups.Add(item);
				}
			}
		}
	}
}

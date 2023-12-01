﻿using HearthDb.Enums;
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
		private Lazy<BattlegroundsDb> _db = new Lazy<BattlegroundsDb>();
		private readonly List<BattlegroundsTier> _tierIcons = new List<BattlegroundsTier>();
		//We remove the below cardid from the bg tier list, it appears to have been incorrectly classified as a bg minion by blizzard in patch 20.0.0.
		//Hopefully it will be fixed soon and can be removed.
		private const string NonBgMurlocTidehunterCardId = HearthDb.CardIds.Collectible.Neutral.MurlocTidecallerVanilla;

		public int ActiveTier { get; set; }
		public ObservableCollection<BattlegroundsCardsGroup> Groups { get; set; } = new ObservableCollection<BattlegroundsCardsGroup>();

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
				_tierIcons[i].SetAvailable(_availableTiers.Contains(i + 1));
		}

		private List<string> _bannedMinionCardIds = new List<string>();
		public void SetBannedMinions(IEnumerable<string> bannedCardIds)
		{
			_bannedMinionCardIds = bannedCardIds.ToList();
		}

		public void Reset()
		{
			_availableTiers = BattlegroundsUtils.GetAvailableTiers(null);
			for(var i = 0; i < 7; i++)
			{
				var isAvailable = _availableTiers.Contains(i + 1);
				_tierIcons[i].SetAvailable(isAvailable);
				if(i == 6)
					_tierIcons[i].Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
			}

			Update(0, _db.Value.Races);
		}

		private bool AddOrUpdateBgCardGroup(Race race, List<Hearthstone.Card> cards)
		{
			var addedNew = false;
			var existing = Groups.FirstOrDefault(x => x.Race == race);
			if(existing == null)
			{
				existing = new BattlegroundsCardsGroup() { Race = race };
				Groups.Add(existing);
				addedNew = true;
			}
			var sortedCards = cards
				.OrderBy(x => x.LocalizedName)
				.ToList();
			existing.UpdateCards(sortedCards);
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
					Groups.FirstOrDefault(x => x.Race == race)?.Hide();
				else
				{
					if(race == Race.ALL || race == Race.INVALID || availableRaces.Contains(race))
						resort |= AddOrUpdateBgCardGroup(race, cards);
				}
			}

			if (resort)
			{
				var items = Groups.ToList()
					.OrderBy(x => x.Race == Race.INVALID)
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

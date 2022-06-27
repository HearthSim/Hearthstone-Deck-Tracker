using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
			var tier = ((BattlegroundsTier)sender).Tier;
			var races = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats?.GameId) ?? _db.Value.Races;
			Update(tier == ActiveTier ? 0 : tier, races);
		}

		public void Reset()
		{
			Update(0, _db.Value.Races);
		}

		private bool AddOrUpdateBgCardGroup(string title, List<Hearthstone.Card> cards)
		{
			var addedNew = false;
			var existing = Groups.FirstOrDefault(x => x.Title == title);
			if(existing == null)
			{
				existing = new BattlegroundsCardsGroup() { Title = title };
				Groups.Add(existing);
				addedNew = true;
			}
			var sortedCards = cards
				.OrderBy(x => x.LocalizedName)
				.ToList();
			existing.UpdateCards(sortedCards);
			return addedNew;
		}

		private static readonly List<Hearthstone.Card> NeutralClassifiedRaceCards = new List<Hearthstone.Card>()
		{
		};

		private IEnumerable<Hearthstone.Card> GetUnavailableRaceCards(IEnumerable<Race> availableRaces)
		{
			return NeutralClassifiedRaceCards.Where(x => x.RaceEnum != null && !availableRaces.Contains(x.RaceEnum.Value)).ToList();
		}

		private Dictionary<Race, List<string>> DifferentTribeClassifiedCards = new Dictionary<Race, List<string>>() { { Race.QUILBOAR, new List<string>() { HearthDb.CardIds.NonCollectible.Neutral.AgamagganTheGreatBoar } } };

		private void Update(int tier, IEnumerable<Race> availableRaces)
		{
			if (ActiveTier == tier)
				return;
			ActiveTier = tier;
			foreach(var item in _tierIcons)
				item.Active = tier == item.Tier;
			if(tier < 1 || tier > 6)
			{
				for(var i = 0; i < 6; i++)
					_tierIcons[i].SetFaded(false);
				Groups.Clear();
				UnavailableTypes.UnavailableTypesVisibility = System.Windows.Visibility.Collapsed;
				return;
			}
			for(var i = 0; i < 6; i++)
				_tierIcons[i].SetFaded(i != tier - 1);

			var resort = false;

			var unavailableRaces = string.Join(
				", ",
				_db.Value.Races.Where(x => !availableRaces.Contains(x) && x != Race.INVALID && x != Race.ALL)
					.Select(x => HearthDbConverter.RaceConverter(x))
					.OrderBy(x => x)
			);
			UnavailableTypes.UnavailableTypesVisibility = System.Windows.Visibility.Visible;
			UnavailableTypes.UnavailableRacesText = unavailableRaces;

			foreach(var race in _db.Value.Races)
			{
				var title = race == Race.INVALID ? "Other" : HearthDbConverter.RaceConverter(race);
				if(title == null)
					continue;

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
							if(card.Entity.GetTag(GameTag.TECH_LEVEL) == tier)
								cards.Add(new Hearthstone.Card(card, true));
						}
					}
				}

				if(race == Race.INVALID)
					cards.AddRange(GetUnavailableRaceCards(availableRaces).Where(x => x.TechLevel == tier));
				if(cards.Count == 0)
					Groups.FirstOrDefault(x => x.Title == title)?.Hide();
				else
				{
					if(race == Race.ALL || race == Race.INVALID || availableRaces.Contains(race))
						resort |= AddOrUpdateBgCardGroup(title, cards);
				}
			}

			if (resort)
			{
				var items = Groups.ToList()
					.OrderBy(x => string.IsNullOrEmpty(x.Title) || x.Title == "Other")
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

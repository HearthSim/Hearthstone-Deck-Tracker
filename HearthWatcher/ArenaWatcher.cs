using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher
{
	public class ArenaWatcher
	{
		public delegate void ChoicesChangedEventHandler(object sender, ChoicesChangedEventArgs args);
		public delegate void CardPickedEventHandler(object sender, CardPickedEventArgs args);
		public delegate void CompleteDeckEventHandler(object sender, CompleteDeckEventArgs args);
		public delegate void RewardsEventHandler(object sender, RewardsEventArgs args);

		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private int _prevSlot = -1;
		private Card[]? _prevChoices;
		private List<List<Card>>? _prevPackages;
		private int _prevChoicesVersion = -1;
		private ArenaInfo? _prevInfo;
		private bool? _prevIsUnderground = null;
		private const int MaxDeckSize = 30;
		private readonly IArenaProvider _arenaProvider;

		public ArenaWatcher(IArenaProvider arenaProvider, int delay = 500)
		{
			_arenaProvider = arenaProvider ?? throw new ArgumentNullException(nameof(arenaProvider));
			_delay = delay;
		}

		public event ChoicesChangedEventHandler OnChoicesChanged;
		public event CardPickedEventHandler OnCardPicked;
		public event CompleteDeckEventHandler OnCompleteDeck;
		public event RewardsEventHandler OnRewards;

		public void Run()
		{
			_watch = true;
			if(!_running)
				Watch();
		}

		public void Stop() => _watch = false;

		private async void Watch()
		{
			_running = true;
			_prevSlot = -1;
			_prevInfo = null;
			_prevChoices = null;
			_prevChoicesVersion = -1;
			_prevPackages = null;
			_prevIsUnderground = null;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				if(Update())
					break;
			}
			_running = false;
		}

		public bool Update()
		{
			var arenaInfo = _arenaProvider.GetArenaInfo();
			if(arenaInfo == null)
				return false;
			var numCards = arenaInfo.Deck.Cards.Sum(x => x.Count);
			if(numCards == MaxDeckSize)
			{
				if(_prevSlot == MaxDeckSize)
					CardPicked(arenaInfo);
				OnCompleteDeck?.Invoke(this, new CompleteDeckEventArgs(arenaInfo));
				if(arenaInfo.Rewards?.Any() ?? false)
					OnRewards?.Invoke(this, new RewardsEventArgs(arenaInfo));
				_watch = false;
				return true;
			}

			// _prevSlot can be related to The arena while currentSlot is Underground and vice-versa
			// so we need to check if _prevIsUnderground is the same as arenaInfo.IsUnderground
			if(_prevInfo != null && arenaInfo.CurrentSlot <= _prevSlot
			  && _prevIsUnderground == arenaInfo.IsUnderground)
				return false;

			var choices = _arenaProvider.GetDraftChoices();
			if(choices == null || choices.Choices.Count == 0)
				return false;

			if(_prevChoicesVersion == choices.Version)
				return false;

			OnChoicesChanged?.Invoke(this,
				new ChoicesChangedEventArgs(choices.Choices.ToArray(), arenaInfo.Deck, arenaInfo.CurrentSlot, arenaInfo.IsUnderground, choices.Packages));

			// we need to check _prevIsUnderground == arenaInfo.IsUnderground
			// otherwise changing arena mode would trigger Hero/CardPicked
			if(_prevSlot == 0 && arenaInfo.CurrentSlot == 1 && _prevIsUnderground == arenaInfo.IsUnderground)
				HeroPicked(arenaInfo);
			else if(_prevSlot > 0 && _prevIsUnderground == arenaInfo.IsUnderground)
				CardPicked(arenaInfo);
			_prevSlot = arenaInfo.CurrentSlot;
			_prevInfo = arenaInfo;
			_prevChoices = choices.Choices.ToArray();
			_prevChoicesVersion = choices.Version;
			_prevPackages = choices.Packages;
			_prevIsUnderground = arenaInfo.IsUnderground;
			return false;
		}

		private void HeroPicked(ArenaInfo arenaInfo)
		{
			var hero = _prevChoices?.FirstOrDefault(x => x.Id == arenaInfo.Deck.Hero);
			if(hero != null)
				OnCardPicked?.Invoke(this, new CardPickedEventArgs(hero, _prevChoices!, arenaInfo.Deck, arenaInfo.CurrentSlot - 1, arenaInfo.IsUnderground, null));
		}

		private void CardPicked(ArenaInfo arenaInfo)
		{
			var prevDeck = _prevInfo?.Deck?.Cards ?? new List<Card>();
			var currDeck = arenaInfo.Deck.Cards;

			var addedCards = currDeck
				.Where(cd => !prevDeck.Any(pd => pd.Id == cd.Id && pd.Count == cd.Count))
				.ToList();

			List<Card>? usedPackage = null;

			if(_prevPackages != null)
			{
				foreach(var package in _prevPackages)
				{
					var packageFullyAdded = package.All(pkgCard =>
					{
						var currCard = currDeck.FirstOrDefault(c => c.Id == pkgCard.Id);
						var prevCard = prevDeck.FirstOrDefault(c => c.Id == pkgCard.Id);

						var currCount = currCard?.Count ?? 0;
						var prevCount = prevCard?.Count ?? 0;

						return (currCount - prevCount) >= pkgCard.Count;
					});

					if(packageFullyAdded)
					{
						usedPackage = package;
						break;
					}
				}
			}

			if(usedPackage != null)
			{
				foreach (var card in usedPackage)
				{
					var match = addedCards.FirstOrDefault(c => c.Id == card.Id);
					if (match != null)
						addedCards.Remove(match);
				}
			}

			var picked = addedCards.FirstOrDefault();
			if(picked != null)
			{
				var pickedCard = new Card(picked.Id, 1, picked.PremiumType);

				OnCardPicked?.Invoke(this, new CardPickedEventArgs(
					pickedCard,
					_prevChoices!,
					arenaInfo.Deck,
					arenaInfo.CurrentSlot - 1,
					arenaInfo.IsUnderground,
					usedPackage));
			}

		}

		private Dictionary<string, string[]>? GetPackagesFromChoices(DraftChoices? choices)
		{
			if(choices?.Packages == null || choices.Packages.Count == 0) return null;

			var packages = new Dictionary<string, string[]>();
			for(var i = 0; i < choices.Choices.Count; i++)
			{
				if(i >= choices.Packages.Count) break;

				packages.Add(choices.Choices[i].Id, choices.Packages[i].Select(c => c.Id).ToArray());
			}

			return packages;
		}
	}
}

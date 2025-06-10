using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror.Enums;
using HearthMirror.Objects;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher
{
	public class ArenaWatcher
	{
		public delegate void ChoicesChangedEventHandler(object sender, ChoicesChangedEventArgs args);
		public delegate void RedraftChoicesChangedEventHandler(object sender, RedraftChoicesChangedEventArgs args);
		public delegate void CardPickedEventHandler(object sender, CardPickedEventArgs args);
		public delegate void RedraftCardPickedEventHandler(object sender, RedraftCardPickedEventArgs args);
		public delegate void CompleteDeckEventHandler(object sender, CompleteDeckEventArgs args);
		public delegate void RewardsEventHandler(object sender, RewardsEventArgs args);

		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private int _prevSlot = -1;
		private int _prevRedraftSlot = -1;
		private Card[]? _prevChoices;
		private List<List<Card>>? _prevPackages;
		private int _prevChoicesVersion = -1;
		private ArenaInfo? _prevInfo;
		private bool? _prevIsUnderground = null;
		private ArenaSessionState _prevArenaSessionState = ArenaSessionState.INVALID;
		private const int MaxDeckSize = 30;
		private const int MaxRedraftDeckSize = 5;
		private readonly IArenaProvider _arenaProvider;

		public ArenaWatcher(IArenaProvider arenaProvider, int delay = 500)
		{
			_arenaProvider = arenaProvider ?? throw new ArgumentNullException(nameof(arenaProvider));
			_delay = delay;
		}

		public event ChoicesChangedEventHandler OnChoicesChanged;
		public event RedraftChoicesChangedEventHandler OnRedraftChoicesChanged;
		public event CardPickedEventHandler OnCardPicked;
		public event RedraftCardPickedEventHandler OnRedraftCardPicked;
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
			_prevRedraftSlot = -1;
			_prevInfo = null;
			_prevChoices = null;
			_prevChoicesVersion = -1;
			_prevPackages = null;
			_prevIsUnderground = null;
			_prevArenaSessionState = ArenaSessionState.INVALID;
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

			if(arenaInfo.SessionState == ArenaSessionState.MIDRUN)
			{
				if(_prevArenaSessionState == ArenaSessionState.DRAFTING)
				{
					var numCards = arenaInfo.Deck.Cards.Sum(x => x.Count);
					if(numCards == MaxDeckSize)
					{
						if(_prevSlot == MaxDeckSize)
							CardPicked(arenaInfo);
					}
				}
				OnCompleteDeck?.Invoke(this, new CompleteDeckEventArgs(arenaInfo));
				if(arenaInfo.Rewards?.Any() ?? false)
					OnRewards?.Invoke(this, new RewardsEventArgs(arenaInfo));
				_watch = false;
				return true;
			}

			if(arenaInfo.SessionState == ArenaSessionState.EDITING_DECK)
			{
				if(_prevArenaSessionState is ArenaSessionState.REDRAFTING or ArenaSessionState.MIDRUN_REDRAFT_PENDING or ArenaSessionState.INVALID)
				{
					var numCards = arenaInfo.RedraftDeck.Cards.Sum(x => x.Count);
					if(numCards == MaxRedraftDeckSize)
					{
						if(_prevRedraftSlot == MaxRedraftDeckSize - 1)
						{
							RedraftLastCardPicked(arenaInfo);
							_prevRedraftSlot = -1;
						}
					}
				}
			}

			if(arenaInfo.SessionState is ArenaSessionState.REDRAFTING or ArenaSessionState.MIDRUN_REDRAFT_PENDING)
			{
				return UpdateRedraft(arenaInfo);
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
			_prevRedraftSlot = -1;
			_prevInfo = arenaInfo;
			_prevChoices = choices.Choices.ToArray();
			_prevChoicesVersion = choices.Version;
			_prevPackages = choices.Packages;
			_prevIsUnderground = arenaInfo.IsUnderground;
			_prevArenaSessionState = arenaInfo.SessionState;
			return false;
		}

		private bool UpdateRedraft(ArenaInfo arenaInfo)
		{
			var redraftDeck = arenaInfo.RedraftDeck;
			var redraftSlot = arenaInfo.RedraftCurrentSlot;

			var choices = _arenaProvider.GetDraftChoices();
			if (choices == null || choices.Choices.Count == 0)
				return false;

			if (_prevInfo != null && redraftSlot <= _prevRedraftSlot
			                      && _prevIsUnderground == arenaInfo.IsUnderground
			                      && _prevChoicesVersion == choices.Version)
				return false;

			OnRedraftChoicesChanged?.Invoke(this,
				new RedraftChoicesChangedEventArgs(choices.Choices.ToArray(), arenaInfo.Deck, redraftDeck, redraftSlot, arenaInfo.Losses, arenaInfo.IsUnderground));

			if(_prevRedraftSlot >= 0 && _prevIsUnderground == arenaInfo.IsUnderground)
				RedraftCardPicked(arenaInfo);

			_prevSlot = -1;
			_prevRedraftSlot = redraftSlot;
			_prevInfo = arenaInfo;
			_prevChoices = choices.Choices.ToArray();
			_prevChoicesVersion = choices.Version;
			_prevPackages = choices.Packages;
			_prevIsUnderground = arenaInfo.IsUnderground;
			_prevArenaSessionState = arenaInfo.SessionState;
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

		private void RedraftCardPicked(ArenaInfo arenaInfo)
		{
			var pick = arenaInfo.RedraftDeck.Cards.FirstOrDefault(x => !_prevInfo?.RedraftDeck.Cards.Any(c => x.Id == c.Id && x.Count == c.Count) ?? false);
			if(pick != null)
			{
				OnRedraftCardPicked?.Invoke(
					this,
					new RedraftCardPickedEventArgs(
						new Card(pick.Id, 1, pick.PremiumType),
						_prevChoices!,
						arenaInfo.Deck,
						arenaInfo.RedraftDeck,
						arenaInfo.RedraftCurrentSlot - 1,
						arenaInfo.Losses,
						arenaInfo.IsUnderground
					)
				);
			}
		}

		private void RedraftLastCardPicked(ArenaInfo arenaInfo)
		{
			var pick = arenaInfo.RedraftDeck.Cards.FirstOrDefault(x => !_prevInfo?.RedraftDeck.Cards.Any(c => x.Id == c.Id && x.Count == c.Count) ?? false);
			// on the last redraft pick, all the cards are added to arenaInfo.Deck, resulting on a 35 cards deck.
			// We use prevInfo.Deck so we have the deck before the pick
			var deck = _prevInfo?.Deck ?? arenaInfo.Deck;
			if(pick != null)
			{
				OnRedraftCardPicked?.Invoke(
					this,
					new RedraftCardPickedEventArgs(
						new Card(pick.Id, 1, pick.PremiumType),
						_prevChoices!,
						deck,
						arenaInfo.RedraftDeck,
						arenaInfo.RedraftCurrentSlot - 1,
						arenaInfo.Losses,
						arenaInfo.IsUnderground
					)
				);
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

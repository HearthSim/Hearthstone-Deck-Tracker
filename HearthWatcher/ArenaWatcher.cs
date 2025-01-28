using System;
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
		private int _prevChoicesVersion = -1;
		private ArenaInfo? _prevInfo;
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

			if(_prevInfo != null && arenaInfo.CurrentSlot <= _prevSlot)
				return false;

			var choices = _arenaProvider.GetDraftChoices();
			if(choices == null || choices.Choices.Count == 0)
				return false;

			if(_prevChoicesVersion == choices.Version)
				return false;

			OnChoicesChanged?.Invoke(this,
				new ChoicesChangedEventArgs(choices.Choices.ToArray(), arenaInfo.Deck, arenaInfo.CurrentSlot));

			if(_prevSlot == 0 && arenaInfo.CurrentSlot == 1)
				HeroPicked(arenaInfo);
			else if(_prevSlot > 0)
				CardPicked(arenaInfo);
			_prevSlot = arenaInfo.CurrentSlot;
			_prevInfo = arenaInfo;
			_prevChoices = choices.Choices.ToArray();
			_prevChoicesVersion = choices.Version;
			return false;
		}

		private void HeroPicked(ArenaInfo arenaInfo)
		{
			var hero = _prevChoices?.FirstOrDefault(x => x.Id == arenaInfo.Deck.Hero);
			if(hero != null)
				OnCardPicked?.Invoke(this, new CardPickedEventArgs(hero, _prevChoices!, arenaInfo.Deck, arenaInfo.CurrentSlot - 1));
		}

		private void CardPicked(ArenaInfo arenaInfo)
		{
			var pick = arenaInfo.Deck.Cards.FirstOrDefault(x => !_prevInfo?.Deck.Cards.Any(c => x.Id == c.Id && x.Count == c.Count) ?? false);
			if(pick != null)
				OnCardPicked?.Invoke(this, new CardPickedEventArgs(new Card(pick.Id, 1, pick.PremiumType), _prevChoices!, arenaInfo.Deck, arenaInfo.CurrentSlot - 1));
		}
	}
}

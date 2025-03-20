using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls;

public partial class AnimatedCardList
{
	// Instantiating AnimatedCards at the volume we need to is pretty expensive. E.g. switching between views in
	// BattlegroundsMinions may throw away and instantiate 30+ cards.
	// So instead of constantly creating new instances we put previously used ones in a pool to be re-used later.
	// This allows the component to remain fully set up and all that needs to happen is for it to be loaded again
	// and be populated with a new viewmodel.
	private static readonly Pool<AnimatedCard> _animatedCardPool = new(200);

	public ObservableCollection<AnimatedCard> AnimatedCards { get; } = new();

	public AnimatedCardListViewModel ViewModel { get; } = new();

	public AnimatedCardList()
	{
		InitializeComponent();
	}

	public bool ShowTier7InspirationButton { get; set; }

	private Func<Hearthstone.Card, IEnumerable<Hearthstone.Card>, HighlightColor>? _shouldHighlightCard;
	public Func<Hearthstone.Card, IEnumerable<Hearthstone.Card>, HighlightColor>? ShouldHighlightCard
	{
		get => _shouldHighlightCard;
		set
		{
			if(_shouldHighlightCard == value)
				return;
			_shouldHighlightCard = value;
			UpdateHighlights();
		}
	}

	private Task? _activeUpdate;
	private object? _pendingUpdate;
	public async Task Update(List<Hearthstone.Card> cards, bool reset)
	{
		// Running multiple animations at the same time can cause weird visual effects at best, and exceptions at worst.
		if(_activeUpdate != null)
		{
			var thisUpdate = new object();
			_pendingUpdate = thisUpdate;
			await _activeUpdate;
			if(_pendingUpdate != thisUpdate)
			{
				// Was called again with a different update. Discard this one.
				return;
			}
		}
		_activeUpdate = DoUpdate(cards, reset);
		await _activeUpdate;
		_activeUpdate = null;
	}

	private async Task DoUpdate(List<Hearthstone.Card> cards, bool reset)
	{
		try
		{
			if(reset)
			{
				foreach(var card in AnimatedCards)
					_animatedCardPool.Return(card);
				AnimatedCards.Clear();
			}

			var newCards = new List<Hearthstone.Card>();
			foreach(var card in cards)
			{
				var existing = AnimatedCards.FirstOrDefault(x => x.Card != null && AreEqualForList(x.Card, card));
				if(existing?.Card == null)
					newCards.Add(card);
				else if(existing.Card.Count != card.Count || existing.Card.HighlightInHand != card.HighlightInHand)
				{
					var highlight = existing.Card.Count != card.Count;
					existing.Card.Count = card.Count;
					existing.Card.HighlightInHand = card.HighlightInHand;
					existing.Update(highlight).Forget();
				}
				else if(existing.Card.IsCreated != card.IsCreated)
					existing.Update(false).Forget();
				else if(existing.Card.ExtraInfo?.CardNameSuffix != card.ExtraInfo?.CardNameSuffix)
				{
					existing.Card.ExtraInfo = card.ExtraInfo?.Clone() as ICardExtraInfo;
					existing.Update(true).Forget();
				}
			}

			var toUpdate = new List<AnimatedCard>();
			foreach(var animatedCard in AnimatedCards)
			{
				if(!cards.Any(x => animatedCard.Card != null && AreEqualForList(x, animatedCard.Card)))
					toUpdate.Add(animatedCard);
			}

			var toRemove = new List<Tuple<AnimatedCard, bool>>();
			foreach(var card in toUpdate)
			{
				if(card.Card == null)
					continue;
				var newCard = newCards.FirstOrDefault(x => x.Id == card.Card.Id);
				toRemove.Add(new Tuple<AnimatedCard, bool>(card, newCard == null));
				if(newCard != null)
				{
					var animatedCard = GetAnimatedCard(newCard);
					AnimatedCards.Insert(AnimatedCards.IndexOf(card), animatedCard);
					animatedCard.Update(true).Forget(); // Not causing size change, order does not matter
					newCards.Remove(newCard);
				}
			}

			await Task.WhenAll(toRemove.Select(card => RemoveCard(card.Item1, card.Item2)).ToArray());
			var fadeIns = new List<Task>();
			var loadedTasks = new List<Task>();
			foreach(var newCard in newCards)
			{
				var animatedCard = GetAnimatedCard(newCard);
				// Animation needs to be started before card is added to the UI, so that
				// CardListHelper has the correct initial size for auto sizing purposes.
				// (Otherwise the card will be initialized with Scale.Y=1 instead of 0)
				fadeIns.Add(animatedCard.FadeIn(!reset));
				AnimatedCards.Insert(cards.IndexOf(newCard), animatedCard);

				if(!animatedCard.IsLoaded)
				{
					var tcs = new TaskCompletionSource<object?>();
					void OnCardLoaded(object sender, RoutedEventArgs e)
					{
						animatedCard.Loaded -= OnCardLoaded;
						tcs.TrySetResult(null);
					}
					animatedCard.Loaded += OnCardLoaded;
					loadedTasks.Add(tcs.Task);
				}
			}

			// Wait for all cards to be loaded. The delay fallback is mostly here to ensure we
			// don't hit any weird race conditions that cause this to permanently block.
			await Task.WhenAny(Task.WhenAll(loadedTasks), Task.Delay(100));

			if(newCards.Count > 0)
			{
				// Defer until all new cards are added/removed so that we have the full list of current cards
				UpdateHighlights();
			}

			// When reset=true, and initial set of cards is loaded, there may not be an actual resize event
			// after the cards have loaded. To ensure CardListHelper is able to handle the initial render
			// correctly we manually emit a size change event here.
			CardListSizeChanged?.Invoke(this);

			await Task.WhenAll(fadeIns);
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
	}

	private AnimatedCard GetAnimatedCard(Hearthstone.Card card)
	{
		var animatedCard = _animatedCardPool.GetOrCreate();
		animatedCard.Update(card, ShowTier7InspirationButton && card.IsBaconMinion);
		animatedCard.MaxHeight = ViewModel.MaxHeightCard;
		animatedCard.SetBinding(MaxHeightProperty, new Binding("MaxHeightCard") { Source = ViewModel });
		return animatedCard;
	}

	private async Task RemoveCard(AnimatedCard card, bool fadeOut)
	{
		if(fadeOut && card.Card != null)
			await card.FadeOut(card.Card.Count > 0);
		_animatedCardPool.Return(card);
		AnimatedCards.Remove(card);
	}

	private void UpdateHighlights()
	{
		if(ShouldHighlightCard == null)
		{
			foreach(var animatedCard in AnimatedCards)
			{
				if(animatedCard.CardTileViewModel == null)
					continue;
				animatedCard.CardTileViewModel.Highlight = HighlightColor.None;
			}
		}
		else
		{
			var cards = AnimatedCards.Where(ac => ac.Card?.Count > 0).Select(ac => ac.Card!).ToList();
			foreach (var animatedCard in AnimatedCards)
			{
				if(animatedCard.Card == null || animatedCard.CardTileViewModel == null)
					continue;
				if(animatedCard.Card.Count <= 0 || animatedCard.Card.Jousted)
				{
					animatedCard.CardTileViewModel.Highlight = HighlightColor.None;
					continue;
				}
				animatedCard.CardTileViewModel.Highlight = ShouldHighlightCard.Invoke(animatedCard.Card, cards);
			}
		}
	}

	private bool AreEqualForList(Hearthstone.Card c1, Hearthstone.Card c2)
	{
		return c1.Id == c2.Id && c1.Jousted == c2.Jousted && c1.IsCreated == c2.IsCreated
		       && (!Config.Instance.HighlightDiscarded || c1.WasDiscarded == c2.WasDiscarded)
		       && c1.DeckListIndex == c2.DeckListIndex && Equals(c1.ExtraInfo, c2.ExtraInfo);
	}


	private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		const double epsilon = 0.01;
		if(sender is not ScrollViewer scrollViewer)
			return;
		ViewModel.IsScrolledToTop = scrollViewer.VerticalOffset < epsilon;
		ViewModel.IsScrolledToBottom = scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset < epsilon;
	}

	private void AnimatedCardList_OnUnloaded(object sender, RoutedEventArgs e)
	{
		foreach(var card in AnimatedCards)
			_animatedCardPool.Return(card);
		AnimatedCards.Clear();
	}

	public event Action<AnimatedCardList>? CardListSizeChanged;
	private void ItemsControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		CardListSizeChanged?.Invoke(this);
	}
}

public class AnimatedCardListViewModel : ViewModel
{
	public double MaxHeightCard
	{
		get => GetProp(34.0);
		set
		{ SetProp(value); }
	}

	public bool IsScrolledToTop
	{
		get => GetProp(true);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(IsScrollable));
		}
	}

	public bool IsScrolledToBottom
	{
		get => GetProp(true);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(IsScrollable));
		}
	}

	public bool IsScrollable => !IsScrolledToTop || !IsScrolledToBottom;
}

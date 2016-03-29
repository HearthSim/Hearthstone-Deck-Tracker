using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class AnimatedCardList
	{
		private readonly ObservableCollection<AnimatedCard> _animatedCards = new ObservableCollection<AnimatedCard>();

		public AnimatedCardList()
		{
			InitializeComponent();
		}

		public void Update(List<Hearthstone.Card> cards, bool player, bool reset)
		{
			try
			{
				if(reset)
				{
					_animatedCards.Clear();
					ItemsControl.Items.Clear();
				}
				foreach(var card in cards)
				{
					var existing = _animatedCards.FirstOrDefault(x => AreEqualForList(x.Card, card));
					if(existing == null)
					{
						var newCard = new AnimatedCard(card);
						_animatedCards.Insert(cards.IndexOf(card), newCard);
						ItemsControl.Items.Insert(cards.IndexOf(card), newCard);
						newCard.FadeIn(!reset).Forget();
					}
					else if(existing.Card.Count != card.Count || existing.Card.HighlightInHand != card.HighlightInHand)
					{
						var highlight = existing.Card.Count != card.Count;
						existing.Card.Count = card.Count;
						existing.Card.HighlightInHand = card.HighlightInHand;
						existing.Update(highlight).Forget();
					}
					else if(existing.Card.IsCreated != card.IsCreated)
						existing.Update(false).Forget();
				}
				foreach(var card in _animatedCards)
				{
					if(!cards.Any(x => AreEqualForList(x, card.Card)))
						RemoveCard(card);
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private async void RemoveCard(AnimatedCard card)
		{
			await card.FadeOut(card.Card.Count > 0);
			_animatedCards.Remove(card);
			ItemsControl.Items.Remove(card);
		}

		private bool AreEqualForList(Hearthstone.Card c1, Hearthstone.Card c2)
		{
			return c1.Id == c2.Id && c1.Jousted == c2.Jousted && c1.IsCreated == c2.IsCreated
				   && (!Config.Instance.HighlightDiscarded || c1.WasDiscarded == c2.WasDiscarded);
		}
	}
}

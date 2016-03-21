using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Extensions;

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
			}
			foreach(var card in _animatedCards.Select(x => x.Card).ToList())
			{
				if(!cards.Any(x =>  AreEqualForList(x, card)))
					RemoveCard(card, player);
			}
		}
		
		private async void RemoveCard(Hearthstone.Card card, bool player)
		{
			var existing = _animatedCards.FirstOrDefault(x => AreEqualForList(x.Card, card));
			if(existing == null)
				return;
			if(Config.Instance.RemoveCardsFromDeck || !player || DeckList.Instance.ActiveDeck == null)
			{
				await existing.FadeOut(existing.Card.Count > 0);
				_animatedCards.Remove(existing);
				ItemsControl.Items.Remove(existing);
			}
			else if(existing.Card.Count > 0)
			{
				await existing.Update(true);
				existing.Card.Count = 0;
			}
		}

		private bool AreEqualForList(Hearthstone.Card c1, Hearthstone.Card c2)
		{
			return c1.Id == c2.Id && c1.Jousted == c2.Jousted && c1.IsCreated == c2.IsCreated
				   && (!Config.Instance.HighlightDiscarded || c1.WasDiscarded == c2.WasDiscarded);
		}
	}
}

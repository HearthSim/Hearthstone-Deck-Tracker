using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class AnimatedCardList
	{
		public ObservableCollection<AnimatedCard> AnimatedCards { get; } = new();

		public AnimatedCardList()
		{
			InitializeComponent();
		}

		public bool ShowTier7InspirationButton { get; set; }

		public void Update(List<Hearthstone.Card> cards, bool reset)
		{
			UpdateAsync(cards, reset).Forget();
		}

		public async Task UpdateAsync(List<Hearthstone.Card> cards, bool reset)
		{
			try
			{
				if(reset)
					AnimatedCards.Clear();

				var newCards = new List<Hearthstone.Card>();
				foreach(var card in cards)
				{
					var existing = AnimatedCards.FirstOrDefault(x => AreEqualForList(x.Card, card));
					if(existing == null)
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
				foreach(var aCard in AnimatedCards)
				{
					if(!cards.Any(x => AreEqualForList(x, aCard.Card)))
						toUpdate.Add(aCard);
				}
				var toRemove = new List<Tuple<AnimatedCard, bool>>();
				foreach(var card in toUpdate)
				{
					var newCard = newCards.FirstOrDefault(x => x.Id == card.Card.Id);
					toRemove.Add(new Tuple<AnimatedCard, bool>(card, newCard == null));
					if(newCard != null)
					{
						var newAnimated = new AnimatedCard(newCard, ShowTier7InspirationButton && newCard.IsBaconMinion);
						AnimatedCards.Insert(AnimatedCards.IndexOf(card), newAnimated);
						newAnimated.Update(true).Forget();
						newCards.Remove(newCard);
					}
				}
				await Task.WhenAll(toRemove.Select(card => RemoveCard(card.Item1, card.Item2)).ToArray());
				foreach(var card in newCards)
				{
					var newCard = new AnimatedCard(card, ShowTier7InspirationButton && card.IsBaconMinion);
					AnimatedCards.Insert(cards.IndexOf(card), newCard);
					newCard.FadeIn(!reset).Forget();
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private async Task RemoveCard(AnimatedCard card, bool fadeOut)
		{
			if(fadeOut)
				await card.FadeOut(card.Card.Count > 0);
			AnimatedCards.Remove(card);
		}

		private bool AreEqualForList(Hearthstone.Card c1, Hearthstone.Card c2)
		{
			return c1.Id == c2.Id && c1.Jousted == c2.Jousted && c1.IsCreated == c2.IsCreated
			       && (!Config.Instance.HighlightDiscarded || c1.WasDiscarded == c2.WasDiscarded)
			       && c1.DeckListIndex == c2.DeckListIndex && Equals(c1.ExtraInfo, c2.ExtraInfo);
		}
	}
}

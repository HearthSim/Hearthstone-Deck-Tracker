#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Player : INotifyPropertyChanged
	{
		public const int DeckSize = 30;
		private readonly IGame _game;

		public Player(IGame game, bool isLocalPlayer)
		{
			_game = game;
			IsLocalPlayer = isLocalPlayer;
		}

		public string Name { get; set; }
		public string Class { get; set; }
		public int Id { get; set; }
		public bool GoingFirst { get; set; }
		public int Fatigue { get; set; }
		public bool IsLocalPlayer { get; }
		public int SpellsPlayedCount { get; private set; }

		public bool HasCoin => Hand.Any(e => e.CardId == HearthDb.CardIds.NonCollectible.Neutral.TheCoin);
		public int HandCount => Hand.Count();
		public int DeckCount => Deck.Count();

		public IEnumerable<Entity> PlayerEntities => _game.Entities.Values.Where(x => !x.Info.HasOutstandingTagChanges && x.IsControlledBy(Id));
		public IEnumerable<Entity> RevealedEntities => _game.Entities.Values.Where(x => !x.Info.HasOutstandingTagChanges && (x.IsControlledBy(Id) || x.Info.OriginalController == Id)).Where(x => x.HasCardId);
		public IEnumerable<Entity> Hand => PlayerEntities.Where(x => x.IsInHand);
		public IEnumerable<Entity> Board => PlayerEntities.Where(x => x.IsInPlay);
		public IEnumerable<Entity> Deck => PlayerEntities.Where(x => x.IsInDeck);
		public IEnumerable<Entity> Graveyard => PlayerEntities.Where(x => x.IsInGraveyard);
		public IEnumerable<Entity> Secrets => PlayerEntities.Where(x => x.IsInSecret && x.IsSecret);
		public IEnumerable<Entity> Quests => PlayerEntities.Where(x => x.IsInSecret && x.IsQuest);
		public IEnumerable<Entity> SetAside => PlayerEntities.Where(x => x.IsInSetAside);

		public List<PredictedCard> InDeckPrecitions { get; } = new List<PredictedCard>();

		private DeckState GetDeckState()
		{
			var createdCardsInDeck =
				Deck.Where(x => x.HasCardId && (x.Info.Created || x.Info.Stolen) && !x.Info.Hidden)
					.GroupBy(ce => new {ce.CardId, Created = (ce.Info.Created || ce.Info.Stolen), ce.Info.Discarded})
					.Select(g =>
					{
						var card = Database.GetCardFromId(g.Key.CardId);
						card.Count = g.Count();
						card.IsCreated = g.Key.Created;
						card.HighlightInHand = Hand.Any(ce => ce.CardId == g.Key.CardId);
						return card;
					});
			var originalCardsInDeck = DeckList.Instance.ActiveDeckVersion.Cards
				.Where(x => x.Count > 0)
				.Select(x => Enumerable.Repeat(x.Id, x.Count))
				.SelectMany(x => x).ToList();
			var revealedNotInDeck = RevealedEntities.Where(x => (!x.Info.Created || x.Info.OriginalEntityWasCreated == false)
																&& x.IsPlayableCard
																&& (!x.IsInDeck || x.Info.Stolen)
																&& x.Info.OriginalController == Id
																&& !(x.Info.Hidden && (x.IsInDeck || x.IsInHand))).ToList();
			var removedFromDeck = new List<string>();
			foreach(var e in revealedNotInDeck)
			{
				originalCardsInDeck.Remove(e.CardId);
				if(!e.Info.Stolen || e.Info.OriginalController == Id)
					removedFromDeck.Add(e.CardId);
			}
			return new DeckState(createdCardsInDeck.Concat(originalCardsInDeck.GroupBy(x => x).Select(x =>
			{
				var card = Database.GetCardFromId(x.Key);
				card.Count = x.Count();
				if(Hand.Any(e => e.CardId == x.Key))
					card.HighlightInHand = true;
				return card;
			})), removedFromDeck.GroupBy(x => x).Select(c =>
			{
				var card = Database.GetCardFromId(c.Key);
				card.Count = 0;
				if(Hand.Any(e => e.CardId == c.Key))
					card.HighlightInHand = true;
				return card;
			}));
		}

		public IEnumerable<Card> PredictedCardsInDeck => InDeckPrecitions.Select(x =>
		{
			var card = Database.GetCardFromId(x.CardId);
			card.Jousted = true;
			return card;
		});

		public IEnumerable<Card> KnownCardsInDeck
			=> Deck.Where(x => x.HasCardId).GroupBy(ce => new {ce.CardId, Created = (ce.Info.Created || ce.Info.Stolen)}).Select(g =>
			{
				var card = Database.GetCardFromId(g.Key.CardId);
				card.Count = g.Count();
				card.IsCreated = g.Key.Created;
				card.Jousted = true;
				return card;
			}).ToList();

		public IEnumerable<Card> RevealedCards
			=> RevealedEntities.Where(x => !string.IsNullOrEmpty(x?.CardId) && (!x.Info.Created || x.Info.OriginalEntityWasCreated == false) && x.IsPlayableCard
									   && ((!x.IsInDeck && (!x.Info.Stolen || x.Info.OriginalController == Id)) || (x.Info.Stolen && x.Info.OriginalController == Id)))
								.GroupBy(x => new {x.CardId, Stolen = x.Info.Stolen && x.Info.OriginalController != Id})
								.Select(x =>
								{
									var card = Database.GetCardFromId(x.Key.CardId);
									card.Count = x.Count();
									card.IsCreated = x.Key.Stolen;
									card.HighlightInHand = x.Any(c => c.IsInHand && c.IsControlledBy(Id));
									return card;
								});

		public IEnumerable<Card> CreatedCardsInHand => Hand.Where(x => !string.IsNullOrEmpty(x?.CardId) && (x.Info.Created || x.Info.Stolen)).GroupBy(x => x.CardId).Select(x =>
		{
			var card = Database.GetCardFromId(x.Key);
			card.Count = x.Count();
			card.IsCreated = true;
			card.HighlightInHand = true;
			return card;
		});

		public IEnumerable<Card> GetHighlightedCardsInHand(List<Card> cardsInDeck)
			=> DeckList.Instance.ActiveDeckVersion.Cards.Where(c => cardsInDeck.All(c2 => c2.Id != c.Id) && Hand.Any(ce => c.Id == ce.CardId))
						.Select(c =>
						{
							var card = (Card)c.Clone();
							card.Count = 0;
							card.HighlightInHand = true;
							return card;
						});

		public List<Card> PlayerCardList => GetPlayerCardList(Config.Instance.RemoveCardsFromDeck, Config.Instance.HighlightCardsInHand, Config.Instance.ShowPlayerGet);

		internal List<Card> GetPlayerCardList(bool removeNotInDeck, bool highlightCardsInHand, bool includeCreatedInHand)
		{
			var createdInHand = includeCreatedInHand ? CreatedCardsInHand : new List<Card>();
			if(DeckList.Instance.ActiveDeck == null)
				return RevealedCards.Concat(createdInHand).Concat(KnownCardsInDeck).Concat(PredictedCardsInDeck).ToSortedCardList();
			var deckState = GetDeckState();
			var inDeck = deckState.RemainingInDeck.ToList();
			var notInDeck = deckState.RemovedFromDeck.Where(x => inDeck.All(c => x.Id != c.Id)).ToList();
			if(!removeNotInDeck)
				return inDeck.Concat(notInDeck).Concat(createdInHand).ToSortedCardList();
			if(highlightCardsInHand)
				return inDeck.Concat(GetHighlightedCardsInHand(inDeck)).Concat(createdInHand).ToSortedCardList();
			return inDeck.Concat(createdInHand).ToSortedCardList();
		}

		public List<Card> OpponentCardList
			=> RevealedEntities.Where(x => !(x.Info.Hidden && (x.IsInDeck || x.IsInHand)) && (x.IsPlayableCard || !x.HasTag(GameTag.CARDTYPE))
										&& (x.GetTag(GameTag.CREATOR) == 1 || ((!x.Info.Created || (Config.Instance.OpponentIncludeCreated && (x.Info.CreatedInDeck || x.Info.CreatedInHand)))
											&& x.Info.OriginalController == Id) || x.IsInHand || x.IsInDeck) && !(x.Info.Created && x.IsInSetAside))
								.GroupBy(e => new { CardId = e.Info.WasTransformed ? e.Info.OriginalCardId : e.CardId, 
													Hidden = (e.IsInHand || e.IsInDeck) && e.IsControlledBy(Id),
													Created = e.Info.Created || (e.Info.Stolen && e.Info.OriginalController != Id),
													Discarded = e.Info.Discarded && Config.Instance.HighlightDiscarded})
								.Select(g =>
								{
									var card = Database.GetCardFromId(g.Key.CardId);
									card.Count = g.Count();
									card.Jousted = g.Key.Hidden;
									card.IsCreated = g.Key.Created;
									card.WasDiscarded = g.Key.Discarded;
									return card;
								}).Concat(InDeckPrecitions.Select(x =>
								{
									var card = Database.GetCardFromId(x.CardId);
									card.Jousted = true;
									return card;
								})).ToSortedCardList();

		public event PropertyChangedEventHandler PropertyChanged;

		public void Reset()
		{
			Name = "";
			Class = "";
			Id = -1;
			GoingFirst = false;
			Fatigue = 0;
			InDeckPrecitions.Clear();
			SpellsPlayedCount = 0;
		}

		public void Draw(Entity entity, int turn)
		{
			if(IsLocalPlayer)
			{
				UpdateKnownEntitesInDeck(entity.CardId);
				entity.Info.Hidden = false;
			}
			if(!IsLocalPlayer)
			{
				if(_game.OpponentEntity?.GetTag(GameTag.MULLIGAN_STATE) == (int)HearthDb.Enums.Mulligan.DEALING)
					entity.Info.Mulliganed = true;
				else
					entity.Info.Hidden = true;
			}
			entity.Info.Turn = turn;
			Log(entity);
		}


		private void Log(Entity entity, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "") 
			=> Log(entity.ToString(), memberName, sourceFilePath);

		private void Log(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "") 
			=> Utility.Logging.Log.Info((IsLocalPlayer ? "[Player] "  : "[Opponent] ") + msg, memberName, sourceFilePath);

		public void Play(Entity entity, int turn)
		{
			if(!IsLocalPlayer)
				UpdateKnownEntitesInDeck(entity.CardId, entity.Info.Turn);
			switch(entity.GetTag(GameTag.CARDTYPE))
			{
				case (int)CardType.TOKEN:
					entity.Info.Created = true;
					break;
				case (int)CardType.SPELL:
					SpellsPlayedCount++;
					break;
			}
			entity.Info.Hidden = false;
			entity.Info.Turn = turn;
			entity.Info.CostReduction = 0;
			Log(entity);
		}

		public void DeckToPlay(Entity entity, int turn)
		{
			UpdateKnownEntitesInDeck(entity.CardId);
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void CreateInHand(Entity entity, int turn)
		{
			entity.Info.Created = true;
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void CreateInDeck(Entity entity, int turn)
		{
			if(entity.Info.Discarded)
			{
				//Entity moved back to the deck after being revealed for tracking
				entity.Info.Discarded = false;
				entity.Info.Created = false;
			}
			else
				entity.Info.Created |= turn > 1;
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void CreateInPlay(Entity entity, int turn)
		{
			entity.Info.Created = true;
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void CreateInSecret(Entity entity, int turn)
		{
			entity.Info.Created = true;
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void RemoveFromDeck(Entity entity, int turn)
		{
			//Do not check for KnownCardIds here, this is how jousted cards get removed from the deck
			entity.Info.Turn = turn;
			entity.Info.Discarded = true;
			Log(entity);
		}

		public void Mulligan(Entity entity) => Log(entity);

		public void HandDiscard(Entity entity, int turn)
		{
			if(!IsLocalPlayer)
				UpdateKnownEntitesInDeck(entity.CardId, entity.Info.Turn);
			entity.Info.Turn = turn;
			entity.Info.Discarded = true;
			Log(entity);
		}

		public void DeckDiscard(Entity entity, int turn)
		{
			UpdateKnownEntitesInDeck(entity.CardId);
			entity.Info.Turn = turn;
			entity.Info.Discarded = true;
			Log(entity);
		}

		public void BoardToDeck(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			entity.Info.Returned = true;
			Log(entity);
		}

		public void BoardToHand(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			entity.Info.Returned = true;
			Log(entity);
		}

		public void ChameleosReveal(string cardId)
		{
			if(InDeckPrecitions.All(x => x.CardId != cardId))
				InDeckPrecitions.Add(new PredictedCard(cardId, 0));
		}

		public void JoustReveal(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			var card = InDeckPrecitions.FirstOrDefault(x => x.CardId == entity.CardId);
			if(card != null)
				card.Turn = turn;
			else
				InDeckPrecitions.Add(new PredictedCard(entity.CardId, turn));
			Log(entity);
		}

		private void UpdateKnownEntitesInDeck(string cardId, int turn = int.MaxValue)
		{
			var card = InDeckPrecitions.FirstOrDefault(x => x.CardId == cardId && turn >= x.Turn);
			if(card != null)
				InDeckPrecitions.Remove(card);
		}

		public void SecretTriggered(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void SecretPlayedFromDeck(Entity entity, int turn)
		{
			UpdateKnownEntitesInDeck(entity.CardId);
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void SecretPlayedFromHand(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			SpellsPlayedCount++;
			Log(entity);
		}

		public void QuestPlayedFromHand(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			SpellsPlayedCount++;
			Log(entity);
		}

		public void PlayToGraveyard(Entity entity, string cardId, int turn)
		{
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void RemoveFromPlay(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			Log(entity);
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void StolenByOpponent(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void StolenFromOpponent(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			Log(entity);
		}

		public void CreateInSetAside(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			Log(entity);
		}
	}
}

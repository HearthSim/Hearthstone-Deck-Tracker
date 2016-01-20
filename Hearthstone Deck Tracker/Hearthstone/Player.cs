﻿#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Player : INotifyPropertyChanged
	{
		public const int DeckSize = 30;

		private readonly Queue<string> _hightlightedCards = new Queue<string>();
		private string _name;

		public Player(bool isLocalPlayer)
		{
			IsLocalPlayer = isLocalPlayer;
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				Log(value, "Name");
			}
		}

		public string Class { get; set; }
		public int Id { get; set; }
		public bool GoingFirst { get; set; }
		public int Fatigue { get; set; }
		public bool DrawnCardsMatchDeck { get; set; }
		public bool IsLocalPlayer { get; }

		public bool HasCoin => Hand.Any(ce => ce.CardId == "GAME_005" || (ce.Entity != null && ce.Entity.CardId == "GAME_005"));
		public int HandCount => Hand.Count;
		public int DeckCount => Deck.Count;

		public List<CardEntity> RevealedCards { get; } = new List<CardEntity>();
		public List<CardEntity> Hand { get; } = new List<CardEntity>();
		public List<CardEntity> Board { get; } = new List<CardEntity>();
		public List<CardEntity> Deck { get; } = new List<CardEntity>();
		public List<CardEntity> Graveyard { get; } = new List<CardEntity>();
		public List<CardEntity> Secrets { get; } = new List<CardEntity>();
		public List<CardEntity> Removed { get; } = new List<CardEntity>();
		public List<string> DrawnCardIds { get; } = new List<string>();
		public List<string> DrawnCardIdsTotal { get; } = new List<string>();
		public List<string> CreatedInHandCardIds { get; } = new List<string>();

		public List<Card> DrawnCards
		{
			get
			{
				return DrawnCardIds.Where(x => !string.IsNullOrEmpty(x)).GroupBy(x => x).Select(g =>
				{
					var card = Database.GetCardFromId(g.Key);
					card.Count = g.Count();
					return card;
				}).Where(x => x.Name != "UNKNOWN").ToList();
			}
		}

		public List<Card> DisplayCards
		{
			//TODO: this may need some refactoring :)
			get
			{
				var createdInHand = Config.Instance.ShowPlayerGet ? CreatedInHandCardIds.GroupBy(x => x).Select(x =>
				{
					var card = Database.GetCardFromId(x.Key);
					card.Count = x.Count();
					card.IsCreated = true;
					card.HighlightInHand = Hand.Any(ce => ce.CardId == card.Id);
					return card;
				}).ToList() : new List<Card>();

				if(DeckList.Instance.ActiveDeck == null)
					return DrawnCards.Concat(createdInHand).ToSortedCardList();

				var stillInDeck =
					Deck.Where(ce => !string.IsNullOrEmpty(ce.CardId)).GroupBy(ce => new {ce.CardId, ce.CardMark, ce.Discarded}).Select(g =>
					{
						var card = Database.GetCardFromId(g.Key.CardId);
						card.Count = g.Count();
						card.IsCreated = g.Key.CardMark == CardMark.Created;
						card.HighlightDraw = _hightlightedCards.Contains(g.Key.CardId);
						card.HighlightInHand = Hand.Any(ce => ce.CardId == g.Key.CardId);
						return card;
					}).ToList();
				if(Config.Instance.RemoveCardsFromDeck)
				{
					if(Config.Instance.HighlightLastDrawn)
					{
						var drawHighlight =
							DeckList.Instance.ActiveDeck.Cards.Where(c => _hightlightedCards.Contains(c.Id) && stillInDeck.All(c2 => c2.Id != c.Id))
							        .Select(c =>
							        {
								        var card = (Card)c.Clone();
								        card.Count = 0;
								        card.HighlightDraw = true;
								        return card;
							        });
						stillInDeck = stillInDeck.Concat(drawHighlight).ToList();
					}
					if(Config.Instance.HighlightCardsInHand)
					{
						var inHand =
							DeckList.Instance.ActiveDeck.Cards.Where(c => stillInDeck.All(c2 => c2.Id != c.Id) && Hand.Any(ce => c.Id == ce.CardId))
							        .Select(c =>
							        {
								        var card = (Card)c.Clone();
								        card.Count = 0;
								        card.HighlightInHand = true;
								        if(IsLocalPlayer && card.Id == HearthDb.CardIds.Collectible.Neutral.RenoJackson
								           && Deck.Where(x => !string.IsNullOrEmpty(x.CardId)).Select(x => x.CardId).GroupBy(x => x).All(x => x.Count() <= 1))
									        card.HighlightFrame = true;
								        return card;
							        });
						;
						stillInDeck = stillInDeck.Concat(inHand).ToList();
					}
					return stillInDeck.Concat(createdInHand).ToSortedCardList();
				}
				var notInDeck = DeckList.Instance.ActiveDeckVersion.Cards.Where(c => Deck.All(ce => ce.CardId != c.Id)).Select(c =>
				{
					var card = (Card)c.Clone();
					card.Count = 0;
					card.HighlightDraw = _hightlightedCards.Contains(c.Id);
					if(Hand.Any(ce => ce.CardId == c.Id))
					{
						card.HighlightInHand = true;
						if(IsLocalPlayer && card.Id == HearthDb.CardIds.Collectible.Neutral.RenoJackson
						   && Deck.Where(x => !string.IsNullOrEmpty(x.CardId)).Select(x => x.CardId).GroupBy(x => x).All(x => x.Count() <= 1))
							card.HighlightFrame = true;
					}
					return card;
				});
				return stillInDeck.Concat(notInDeck).Concat(createdInHand).ToSortedCardList();
			}
		}

		public List<Card> DisplayRevealedCards
		{
			get
			{
				return
					RevealedCards.Where(ce => !string.IsNullOrEmpty(ce.CardId))
					             .GroupBy(
					                      ce =>
					                      new
					                      {
						                      ce.CardId,
						                      Hidden = (ce.InHand || ce.InDeck),
						                      ce.Created,
						                      Discarded = ce.Discarded && Config.Instance.HighlightDiscarded
					                      })
					             .Select(g =>
					             {
						             var card = Database.GetCardFromId(g.Key.CardId);
						             card.Count = g.Count();
						             card.Jousted = g.Key.Hidden;
						             card.IsCreated = g.Key.Created;
						             card.WasDiscarded = g.Key.Discarded;
						             return card;
					             }).ToSortedCardList();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Reset()
		{
			Name = "";
			Class = "";
			Id = -1;
			GoingFirst = false;
			Fatigue = 0;
			DrawnCardsMatchDeck = true;
			Hand.Clear();
			Board.Clear();
			Deck.Clear();
			Graveyard.Clear();
			Secrets.Clear();
			DrawnCardIds.Clear();
			DrawnCardIdsTotal.Clear();
			RevealedCards.Clear();
			CreatedInHandCardIds.Clear();
			Removed.Clear();

			for(var i = 0; i < DeckSize; i++)
				Deck.Add(new CardEntity(null));
		}

		private CardEntity GetEntityFromCollection(List<CardEntity> collection, Entity entity)
		{
			var cardEntity = collection.FirstOrDefault(ce => ce.Entity == entity)
							 ?? (collection.FirstOrDefault(ce => !string.IsNullOrEmpty(ce.CardId) && ce.CardId == entity.CardId)
							 ?? collection.FirstOrDefault(ce => string.IsNullOrEmpty(ce.CardId) && ce.Entity == null));
			cardEntity?.Update(entity);
			return cardEntity;
		}

		private CardEntity MoveCardEntity(Entity entity, List<CardEntity> @from, List<CardEntity> to, int turn)
		{
			var cardEntity = GetEntityFromCollection(from, entity);
			if(cardEntity != null)
				from.Remove(cardEntity);
			else
			{
				cardEntity = @from.FirstOrDefault(ce => string.IsNullOrEmpty(ce.CardId) && ce.Entity == null);
				if(cardEntity != null)
				{
					from.Remove(cardEntity);
					cardEntity.Update(entity);
				}
				else
					cardEntity = new CardEntity(entity) {Turn = turn};
			}
			to.Add(cardEntity);
			to.Sort(ZonePosComparison);
			cardEntity.Turn = turn;
			return cardEntity;
		}

		private int ZonePosComparison(CardEntity ce1, CardEntity ce2)
		{
			var v1 = ce1.Entity?.GetTag(GAME_TAG.ZONE_POSITION) ?? 10;
			var v2 = ce2.Entity?.GetTag(GAME_TAG.ZONE_POSITION) ?? 10;
			return v1.CompareTo(v2);
		}

		public void Draw(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Deck, Hand, turn);
			if(IsLocalPlayer)
				Highlight(entity.CardId);
			else
				ce.Reset();

			if(!string.IsNullOrEmpty(entity.CardId) && ce.CardMark != CardMark.Created && ce.CardMark != CardMark.Returned && !ce.Created)
			{
				if(IsLocalPlayer && !CardMatchesActiveDeck(entity.CardId))
					DrawnCardsMatchDeck = false;
				DrawnCardIds.Add(entity.CardId);
				DrawnCardIdsTotal.Add(entity.CardId);
			}
			Log("Draw", ce);
		}

		private static bool CardMatchesActiveDeck(string cardId) => string.IsNullOrEmpty(cardId) || DeckList.Instance.ActiveDeck == null
																	|| DeckList.Instance.ActiveDeckVersion.Cards.Any(c => c.Id == cardId);


		private void Log(string action, CardEntity ce) => Log(ce.ToString(), action);

		private void Log(string msg, string category) => Logger.WriteLine(msg, (IsLocalPlayer ? "Player " : "Opponent ") + category);

		private async void Highlight(string cardId)
		{
			_hightlightedCards.Enqueue(cardId);
			Helper.UpdatePlayerCards();
			await Task.Delay(3000);
			_hightlightedCards.Dequeue();
			Helper.UpdatePlayerCards();
		}

		public void Play(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Hand, entity.IsSecret ? Secrets : Board, turn);
			if(entity.GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.TOKEN)
			{
				ce.CardMark = CardMark.Created;
				ce.Created = true;
			}
			UpdateRevealedEntity(ce, turn);
			Log("Play", ce);
		}

		public void DeckToPlay(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Deck, Board, turn);
			UpdateRevealedEntity(ce, turn);
			if(!string.IsNullOrEmpty(entity.CardId) && ce.CardMark != CardMark.Created && ce.CardMark != CardMark.Returned)
			{
				if(IsLocalPlayer && !CardMatchesActiveDeck(entity.CardId))
					DrawnCardsMatchDeck = false;
				DrawnCardIds.Add(entity.CardId);
				DrawnCardIdsTotal.Add(entity.CardId);
			}
			Log("DeckToPlay", ce);
		}

		private void UpdateRevealedEntity(CardEntity entity, int turn, bool? discarded = null, CardMark? cardMark = null)
		{
			var revealed =
				RevealedCards.FirstOrDefault(
				                             ce =>
				                             ce.Entity == entity.Entity
				                             || (ce.CardId == entity.CardId && ce.Entity == null && ce.Turn <= entity.PrevTurn));
			if(revealed != null)
				revealed.Update(entity.Entity);
			else
			{
				revealed = new CardEntity(entity.Entity) {Turn = turn, Created = entity.Created, Discarded = entity.Discarded};
				var cardType = entity.Entity.GetTag(GAME_TAG.CARDTYPE);
				if(cardType != (int)TAG_CARDTYPE.HERO && cardType != (int)TAG_CARDTYPE.ENCHANTMENT && cardType != (int)TAG_CARDTYPE.HERO_POWER
				   && cardType != (int)TAG_CARDTYPE.PLAYER)
					RevealedCards.Add(revealed);
			}
			if(discarded.HasValue)
				revealed.Discarded = discarded.Value;
			if(cardMark.HasValue)
				revealed.CardMark = cardMark.Value;
		}

		public void CreateInHand(Entity entity, int turn)
		{
			var ce = new CardEntity(entity) {Turn = turn, CardMark = CardMark.Created, Created = true};
			if(entity != null
			   && (entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.TheCoin
			       || entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.GallywixsCoinToken))
				ce.CardMark = CardMark.Coin;
			Hand.Add(ce);
			if(IsLocalPlayer)
				CreatedInHandCardIds.Add(entity.CardId);
			Log("CreateInHand", ce);
		}

		public void CreateInDeck(Entity entity, int turn)
		{
			CardEntity ce;
			if(IsLocalPlayer)
			{
				ce = new CardEntity(entity) {Turn = turn};
				Deck.Add(ce);
				RevealedCards.Add(new CardEntity(entity) {Turn = turn});
			}
			else
			{
				Deck.Add(new CardEntity(null));
				RevealDeckCard(entity.CardId, turn);
				ce = new CardEntity(entity.CardId, null) {Turn = turn};
				RevealedCards.Add(ce);
			}
			Log("CreateInDeck", ce);
		}

		public void CreateInPlay(Entity entity, int turn)
		{
			var ce = new CardEntity(entity) {Turn = turn, Created = true};
			Board.Add(ce);
			Log("CreateInPlay", ce);
		}

		public void RemoveFromDeck(Entity entity, int turn)
		{
			var revealed = RevealedCards.FirstOrDefault(r => r.Entity == entity);
			if(revealed != null)
				RevealedCards.Remove(revealed);
			var ce = MoveCardEntity(entity, Deck, Removed, turn);
			Log("RemoveFromDeck", ce);
		}

		public void Mulligan(Entity entity)
		{
			var ce = MoveCardEntity(entity, Hand, Deck, 0);

			//new cards are drawn first
			var newCard = Hand.FirstOrDefault(x => x.Entity.GetTag(GAME_TAG.ZONE_POSITION) == entity.GetTag(GAME_TAG.ZONE_POSITION));
			if(newCard != null)
				newCard.CardMark = CardMark.Mulliganed;
			if(!string.IsNullOrEmpty(entity.CardId) && DrawnCardIds.Contains(entity.CardId))
				DrawnCardIds.Remove(entity.CardId);
			Log("Mulligan", ce);
		}

		public void HandDiscard(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Hand, Graveyard, turn);
			UpdateRevealedEntity(ce, turn, true);
			Log("HandDiscard", ce);
		}

		public void DeckDiscard(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Deck, Graveyard, turn);
			UpdateRevealedEntity(ce, turn, true);
			if(!string.IsNullOrEmpty(entity.CardId) && ce.CardMark != CardMark.Created && ce.CardMark != CardMark.Returned)
			{
				if(IsLocalPlayer && !CardMatchesActiveDeck(entity.CardId))
					DrawnCardsMatchDeck = false;
				DrawnCardIds.Add(entity.CardId);
				DrawnCardIdsTotal.Add(entity.CardId);
			}
			Log("DeckDiscard", ce);
		}

		public void BoardToDeck(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Board, Deck, turn);
			UpdateRevealedEntity(ce, turn);
			if(!string.IsNullOrEmpty(entity.CardId) && DrawnCardIds.Contains(entity.CardId))
				DrawnCardIds.Remove(entity.CardId);
			Log("BoardToDeck", ce);
		}

		public void BoardToHand(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Board, Hand, turn);
			ce.CardMark = CardMark.Returned;
			UpdateRevealedEntity(ce, turn, cardMark: CardMark.Returned);
			Log("BoardToHand", ce);
		}

		public void JoustReveal(Entity entity, int turn)
		{
			if(Deck.Where(ce => ce.InDeck).All(ce => ce.CardId != entity.CardId))
			{
				RevealDeckCard(entity.CardId, turn);
				var ce = new CardEntity(entity.CardId, null) {Turn = turn};
				RevealedCards.Add(ce);
				Log("JoustReveal", ce);
			}
		}

		public void SecretTriggered(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Secrets, Graveyard, turn);
			UpdateRevealedEntity(ce, turn);
			Log("SecretTriggered", ce);
		}

		public void RevealDeckCard(string cardId, int turn)
		{
			var cardEntity = Deck.FirstOrDefault(ce => ce.Unknown);
			if(cardEntity != null)
			{
				cardEntity.CardId = cardId;
				cardEntity.Turn = turn;
			}
		}

		public void SecretPlayedFromDeck(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Deck, Secrets, turn);
			UpdateRevealedEntity(ce, turn);
			if(!string.IsNullOrEmpty(entity.CardId))
			{
				if(IsLocalPlayer && !CardMatchesActiveDeck(entity.CardId))
					DrawnCardsMatchDeck = false;
				DrawnCardIds.Add(entity.CardId);
				DrawnCardIdsTotal.Add(entity.CardId);
			}
			Log("SecretPlayedFromDeck", ce);
		}

		public void SecretPlayedFromHand(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Hand, Secrets, turn);
			Log("SecretPlayedFromHand", ce);
		}

		public void PlayToGraveyard(Entity entity, string cardId, int turn)
		{
			var ce = MoveCardEntity(entity, Board, Graveyard, turn);
			Log("PlayToGraveyard", ce);
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void UpdateZonePos(TAG_ZONE zone, int turn)
		{
			switch(zone)
			{
				case TAG_ZONE.HAND:
					Hand.Sort(ZonePosComparison);
					if(!IsLocalPlayer && turn == 0 && Hand.Count == 5 && Hand[4].Entity.Id > 67)
					{
						Hand[4].CardMark = CardMark.Coin;
						Hand[4].Created = true;
						Deck.Add(new CardEntity(null));
						Log("Coin", Hand[4]);
					}
					break;
				case TAG_ZONE.PLAY:
					Board.Sort(ZonePosComparison);
					break;
			}
		}

		public void StolenByOpponent(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Board, Removed, turn);
			UpdateRevealedEntity(ce, turn);
			Log("StolenByOpponent", ce);
		}

		public void StolenFromOpponent(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Removed, Board, turn);
			ce.Created = true;
			UpdateRevealedEntity(ce, turn);
			Log("StolenFromOpponent", ce);
		}

		public class MoveCardResult
		{
			public bool CreatedCard { get; set; }
			public CardEntity Entity { get; set; }
		}
	}


	public class CardEntity
	{
		private int _turn;

		public CardEntity(Entity entity) : this(null, entity)
		{
		}

		public CardEntity(string cardId, Entity entity)
		{
			CardId = (string.IsNullOrEmpty(cardId) && entity != null) ? entity.CardId : cardId;
			Entity = entity;
			Turn = -1;
			CardMark = (entity != null && entity.Id > 68) ? CardMark.Created : CardMark.None;
		}

		public string CardId { get; set; }
		public Entity Entity { get; set; }

		public int Turn
		{
			get { return _turn; }
			set
			{
				PrevTurn = _turn;
				_turn = value;
			}
		}

		public int PrevTurn { get; private set; }
		public CardMark CardMark { get; set; }
		public bool Discarded { get; set; }

		public bool InHand => (Entity != null && Entity.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.HAND);
		public bool InDeck => (Entity == null || Entity.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.DECK);
		public bool Unknown => string.IsNullOrEmpty(CardId) && Entity == null;

		public bool Created { get; set; }

		public void Update(Entity entity = null)
		{
			if(entity == null)
				return;
			if(Entity == null)
				Entity = entity;
			if(string.IsNullOrEmpty(CardId))
				CardId = entity.CardId;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Entity);
			if(Entity == null)
				sb.Append("cardId=" + CardId);
			sb.Append(", turn=" + Turn);
			if(CardMark != CardMark.None)
				sb.Append(", mark=" + CardMark);
			if(Discarded)
				sb.Append(", discarded=true");
			if(Created)
				sb.Append(", created=true");
			return sb.ToString();
		}

		public void Reset()
		{
			CardMark = CardMark.None;
			Created = false;
			CardId = string.Empty;
		}
	}
}
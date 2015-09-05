using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.TextFormatting;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Player : INotifyPropertyChanged
	{
		public string Name { get;set; }
		public string Class { get; set; }
		public int Id { get; set; }
		public bool GoingFirst { get; set; }
		public int Fatigue { get; set; }
		public bool DrawnCardsMatchDeck { get; set; }
		public bool IsLocalPlayer { get; private set; }
		public bool HasCoin
		{
			get { return Hand.Any(ce => ce.CardId == "GAME_005" || (ce.Entity != null && ce.Entity.CardId == "GAME_005")); }
		}

		public int HandCount { get { return Hand.Count; } }
		public int DeckCount { get { return Deck.Count; } }
		public List<CardEntity> RevealedCards { get; private set; }

		public List<CardEntity> Hand { get; private set; }
		public List<CardEntity> Board { get; private set; }
		public List<CardEntity> Deck { get; private set; }
		public List<CardEntity> Graveyard { get; private set; }
		public List<CardEntity> Secrets { get; private set; }
		public List<Card> DrawnCards { get; private set; }
		public List<string> DrawnCardsDistinctTotalIds { get; private set; }
		public const int DeckSize = 30;

		private readonly Queue<string> _hightlightedCards = new Queue<string>(); 

		public List<Card> DisplayCards
		{
			//TODO: this may need some refactoring :)
			get
			{
				if(DeckList.Instance.ActiveDeck == null)
					return DrawnCards;

				var stillInDeck =
					Deck.Where(ce => !string.IsNullOrEmpty(ce.CardId))
					    .GroupBy(ce => new {ce.CardId, ce.CardMark, ce.Discarded})
					    .Select(
					            g =>
					            new Card()
					            {
						            Id = g.Key.CardId,
						            Count = g.Count(),
						            IsCreated = g.Key.CardMark == CardMark.Created,
						            HighlightDraw = _hightlightedCards.Contains(g.Key.CardId),
						            HighlightInHand = Hand.Any(ce => ce.CardId == g.Key.CardId)
					            })
					    .ToList();
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
								        return card;
							        });
						;
						stillInDeck = stillInDeck.Concat(inHand).ToList();
					}
					return stillInDeck.ToList();
				}
				var notInDeck = DeckList.Instance.ActiveDeckVersion.Cards.Where(c => Deck.All(ce => ce.CardId != c.Id)).Select(c =>
				{
					var card = (Card)c.Clone();
					card.Count = 0;
					card.HighlightDraw = _hightlightedCards.Contains(c.Id);
					card.HighlightInHand = Hand.Any(ce => ce.CardId == c.Id);
					return card;
				});
				return stillInDeck.Concat(notInDeck).ToList();
			}
		}

		public List<Card> DisplayRevealedCards
		{

			get
			{
				return
					RevealedCards.Where(ce => !string.IsNullOrEmpty(ce.CardId))
					             .GroupBy(ce => new {ce.CardId, Hidden = (ce.InHand || ce.InDeck), ce.CardMark, ce.Discarded})
					             .Select(g => new Card() {Id = g.Key.CardId, Count = g.Count(), Jousted = g.Key.Hidden, IsCreated = g.Key.CardMark == CardMark.Created, WasDiscarded = g.Key.Discarded })
					             .ToList();
			}
		}

		private bool _mulliganed;

		public Player(bool isLocalPlayer)
		{
			Hand = new List<CardEntity>();
			Board = new List<CardEntity>();
			Deck = new List<CardEntity>();
			Graveyard = new List<CardEntity>();
			Secrets = new List<CardEntity>();
			RevealedCards = new List<CardEntity>();
			DrawnCards = new List<Card>();
			DrawnCardsDistinctTotalIds = new List<string>();
			IsLocalPlayer = isLocalPlayer;
		}

		public void Print()
		{
			Console.WriteLine("======= " + Name + " =======");
			if(Hand.Any())
				Console.WriteLine("Hand: " + Hand.Select(x => x.CardId + " (id: " + x.Entity.Id + ", t:" + x.Turn + ", zp:" + x.Entity.GetTag(GAME_TAG.ZONE_POSITION) + ") " + x.CardMark).Aggregate((c, n) => c + " | " + n));
			if(Board.Any())
				Console.WriteLine("Board: " + Board.Select(x => x.CardId + " (" + x.Turn + ") ").Aggregate((c, n) => c + ", " + n));
			if(Deck.Any())
				Console.WriteLine("Deck: " + Deck.Select(x => x.CardId).Aggregate((c, n) => c + ", " + n));
			if(Graveyard.Any())
				Console.WriteLine("Graveyard: " + Graveyard.Select(x => x.CardId).Aggregate((c, n) => c + ", " + n));
			if(Secrets.Any())
				Console.WriteLine("Secrets: " + Secrets.Select(x => x.CardId + " (" + x.Turn + ") ").Aggregate((c, n) => c + ", " + n));
		}

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
			DrawnCards.Clear();
			DrawnCardsDistinctTotalIds.Clear();
			RevealedCards.Clear();
			_mulliganed = false;

			for(var i = 0; i < DeckSize; i++)
				Deck.Add(new CardEntity(null));

		}

		private CardEntity GetEntityFromCollection(List<CardEntity> collection, Entity entity)
		{
			var cardEntity = collection.FirstOrDefault(ce => ce.Entity == entity);
			if(cardEntity == null)
			{
				cardEntity = collection.FirstOrDefault(ce => !string.IsNullOrEmpty(ce.CardId) && ce.CardId == entity.CardId)
				             ?? collection.FirstOrDefault(ce => string.IsNullOrEmpty(ce.CardId) && ce.Entity == null);
			}
			if(cardEntity != null)
				cardEntity.Update(entity);
			return cardEntity;
		}

		private CardEntity MoveCardEntity(Entity entity, List<CardEntity> @from, List<CardEntity> to, int turn)
		{
			var cardEntity = GetEntityFromCollection(from, entity);
			if(cardEntity != null)
			{
				from.Remove(cardEntity);
			}
			else
			{
				cardEntity = @from.FirstOrDefault(ce => string.IsNullOrEmpty(ce.CardId) && ce.Entity == null);
				if(cardEntity != null)
				{
					from.Remove(cardEntity);
					cardEntity.Update(entity);
				}
				else
				{
					cardEntity = new CardEntity(entity) { Turn = turn };
				}
				DrawnCardsMatchDeck = false;
			}
			to.Add(cardEntity);
			//from.Sort(ZonePosComparison);
			to.Sort(ZonePosComparison);
			cardEntity.Turn = turn;
			//Print();
			return cardEntity;
		}

		public class MoveCardResult
		{
			public bool CreatedCard { get; set; }	
			public CardEntity Entity { get; set; }
		}

		private int ZonePosComparison(CardEntity ce1, CardEntity ce2)
		{
			var v1 = (ce1.Entity != null ? ce1.Entity.GetTag(GAME_TAG.ZONE_POSITION) : 10);
			var v2 = (ce2.Entity != null ? ce2.Entity.GetTag(GAME_TAG.ZONE_POSITION) : 10);
			return v1.CompareTo(v2);
		}

		public void Draw(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Deck, Hand, turn);
			if(IsLocalPlayer)
				Highlight(entity.CardId);
			Log("Draw", ce);
		}

		private void Log(string action, CardEntity ce)
		{
			var player = IsLocalPlayer ? "Player " : "Opponent ";
			var sb = new StringBuilder();
			sb.Append(ce.Entity);
			if(ce.Entity == null)
				sb.Append("cardId=" + ce.CardId);
			sb.Append(", turn=" + ce.Turn);
			if(ce.CardMark != CardMark.None)
				sb.Append(", mark=" + ce.CardMark);
			if(ce.Discarded)
				sb.Append(", discarded=true");
			Logger.WriteLine(sb.ToString(), player + action);
		}

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
				ce.CardMark = CardMark.Created;
			UpdateRevealedEntity(entity, turn);
			Log("Play", ce);
		}

		private void UpdateRevealedEntity(Entity entity, int turn, bool? discarded = null, CardMark? cardMark = null)
		{
			var revealed = RevealedCards.FirstOrDefault(ce => ce.Entity == entity || (ce.CardId == entity.CardId && ce.Entity == null));
			if(revealed != null)
			{
				revealed.Update(entity);
			}
			else
			{
				revealed = new CardEntity(entity) {Turn = turn};
                RevealedCards.Add(revealed);
			}
			if(discarded.HasValue)
				revealed.Discarded = discarded.Value;
			if(cardMark.HasValue)
				revealed.CardMark = cardMark.Value;
		}

		public void CreateInHand(Entity entity, int turn)
		{
			var ce = new CardEntity(entity) {Turn = turn, CardMark = CardMark.Created};
            Hand.Add(ce);
			Log("CreateInHand", ce);
		}

		public void CreateInDeck(Entity entity, int turn)
		{
			var ce = new CardEntity(entity) {Turn = turn};
            Deck.Add(ce);
			RevealedCards.Add(new CardEntity(entity) { Turn = turn });
			Log("CreateInDeck", ce);
		}

		public void CreateInPlay(Entity entity, int turn)
		{
			var ce = new CardEntity(entity) {Turn = turn};
            Board.Add(ce);
			Log("CreateInPlay", ce);
		}

		public void Mulligan(Entity entity)
		{
			var ce = MoveCardEntity(entity, Hand, Deck, 0);

			//new cards are drawn first
			var newCard = Hand.FirstOrDefault(x => x.Entity.GetTag(GAME_TAG.ZONE_POSITION) == entity.GetTag(GAME_TAG.ZONE_POSITION));
			if(newCard != null)
				newCard.CardMark = CardMark.Mulliganed;
			Log("Mulligan", ce);
		}

		public void HandDiscard(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Hand, Graveyard, turn);
			UpdateRevealedEntity(entity, turn, true);
			Log("HandDiscard", ce);
		}

		public void DeckDiscard(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Deck, Graveyard, turn);
			UpdateRevealedEntity(entity, turn, true);
			Log("DeckDiscard", ce);
		}

		public void BoardToDeck(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Board, Deck, turn);
			UpdateRevealedEntity(entity, turn);
			Log("BoardToDeck", ce);
		}

		public void BoardToHand(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Board, Hand, turn);
			ce.CardMark = CardMark.Returned;
			UpdateRevealedEntity(entity, turn, cardMark: CardMark.Returned);
			Log("BoardToHand", ce);
		}

		public void JoustReveal(Entity entity, int turn)
		{
			if(Deck.Where(ce => ce.InDeck).All(ce => ce.CardId != entity.CardId))
			{
				RevealDeckCard(entity.CardId);
				var ce = new CardEntity(entity.CardId, null) {Turn = turn};
                RevealedCards.Add(ce);
				Log("JoustReveal", ce);
			}
		}

		public void SecretTriggered(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Secrets, Graveyard, turn);
			UpdateRevealedEntity(entity, turn);
			Log("SecretTriggered", ce);
		}

		public void RevealDeckCard(string cardId)
		{
			var cardEntity = Deck.FirstOrDefault(ce => ce.Unknown);
			if(cardEntity != null)
				cardEntity.CardId = cardId;
		}

		public void SecretPlayedFromDeck(Entity entity, int turn)
		{
			var ce = MoveCardEntity(entity, Deck, Secrets, turn);
			UpdateRevealedEntity(entity, turn);
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

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public void UpdateZonePos(TAG_ZONE zone)
		{
			switch(zone)
			{
				case TAG_ZONE.HAND:
					Hand.Sort(ZonePosComparison);
					break;
				case TAG_ZONE.PLAY:
					Board.Sort(ZonePosComparison);
					break;
			}
		}
	}



	public class CardEntity
	{
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
		public int Turn { get; set; }
		public CardMark CardMark { get; set; }
		public bool Discarded { get; set; }
		public bool InHand { get { return (Entity != null && Entity.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.HAND); } }
		public bool InDeck { get { return (Entity == null || Entity.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.DECK); } }
		public bool Unknown { get { return string.IsNullOrEmpty(CardId) && Entity == null; } }

		public void Update(Entity entity = null)
		{
			if(entity == null)
				return;
			if(Entity == null)
				Entity = entity;
			if(string.IsNullOrEmpty(CardId))
				CardId = entity.CardId;

		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		public List<Card> DisplayCards
		{
			get
			{
				if(DeckList.Instance.ActiveDeck == null)
					return DrawnCards;

				var stillInDeck =
					Deck.Where(ce => !string.IsNullOrEmpty(ce.CardId))
					    .GroupBy(ce => new { ce.CardId, ce.CardMark, ce.Discarded})
					    .Select(g => new Card() {Id = g.Key.CardId, Count = g.Count(), IsStolen = g.Key.CardMark == CardMark.Stolen, WasDiscarded = g.Key.Discarded})
					    .ToList();
				if(Config.Instance.RemoveCardsFromDeck)
					return stillInDeck.ToList();
				var notInDeck = DeckList.Instance.ActiveDeckVersion.Cards.Where(c => Deck.All(ce => ce.CardId != c.Id)).Select(c =>
				{
					var card = (Card)c.Clone();
					card.Count = 0;
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
					             .Select(g => new Card() {Id = g.Key.CardId, Count = g.Count(), Jousted = g.Key.Hidden, IsStolen = g.Key.CardMark == CardMark.Stolen, WasDiscarded = g.Key.Discarded })
					             .ToList();
			}
		}

		private bool _mulliganed;

		public Player()
		{
			Hand = new List<CardEntity>();
			Board = new List<CardEntity>();
			Deck = new List<CardEntity>();
			Graveyard = new List<CardEntity>();
			Secrets = new List<CardEntity>();
			RevealedCards = new List<CardEntity>();
			DrawnCards = new List<Card>();
			DrawnCardsDistinctTotalIds = new List<string>();
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
			from.Sort(ZonePosComparison);
			to.Sort(ZonePosComparison);
			cardEntity.Turn = turn;
			Print();
			return cardEntity;
		}

		public class MoveCardResult
		{
			public bool CreatedCard { get; set; }	
			public CardEntity Entity { get; set; }
		}

		public void Draw(Entity entity, int turn)
		{
			MoveCardEntity(entity, Deck, Hand, turn);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> Draw");
		}

		private int ZonePosComparison(CardEntity ce1, CardEntity ce2)
		{
			var v1 = (ce1.Entity != null ? ce1.Entity.GetTag(GAME_TAG.ZONE_POSITION) : 10);
			var v2 = (ce2.Entity != null ? ce2.Entity.GetTag(GAME_TAG.ZONE_POSITION) : 10);
			return v1.CompareTo(v2);
		}

		public void Play(Entity entity, int turn)
		{
			MoveCardEntity(entity, Hand, entity.IsSecret ? Secrets : Board, turn);
			UpdateRevealedEntity(entity, turn);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> Play");
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
			Hand.Add(new CardEntity(entity) {Turn = turn, CardMark = CardMark.Stolen});
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> CreateInHand");
		}

		public void CreateInDeck(Entity entity, int turn)
		{
			Deck.Add(new CardEntity(entity) {Turn = turn});
			RevealedCards.Add(new CardEntity(entity) { Turn = turn });
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> CreateInDeck");
		}

		public void CreateInPlay(Entity entity, int turn)
		{
			Board.Add(new CardEntity(entity) {Turn = turn});
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> CreateInPlay");
		}

		public void Mulligan(Entity entity)
		{
			MoveCardEntity(entity, Hand, Deck, 0);

			//new cards are drawn first
			var newCard = Hand.FirstOrDefault(x => x.Entity.GetTag(GAME_TAG.ZONE_POSITION) == entity.GetTag(GAME_TAG.ZONE_POSITION));
			if(newCard != null)
				newCard.CardMark = CardMark.Mulliganed;
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> Mulligan");
		}

		public void HandDiscard(Entity entity, int turn)
		{
			MoveCardEntity(entity, Hand, Graveyard, turn);
			UpdateRevealedEntity(entity, turn, true);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> HandDiscard");
		}

		public void DeckDiscard(Entity entity, int turn)
		{
			MoveCardEntity(entity, Deck, Graveyard, turn);
			UpdateRevealedEntity(entity, turn, true);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> DeckDiscard");
		}

		public void BoardToDeck(Entity entity, int turn)
		{
			MoveCardEntity(entity, Board, Deck, turn);
			UpdateRevealedEntity(entity, turn);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> BoardToDeck");
		}

		public void BoardToHand(Entity entity, int turn)
		{
			MoveCardEntity(entity, Board, Hand, turn).CardMark = CardMark.Returned;
			UpdateRevealedEntity(entity, turn, cardMark: CardMark.Returned);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> BoardToHand");
		}

		public void JoustReveal(Entity entity, int turn)
		{
			if(Deck.Where(ce => ce.InDeck).All(ce => ce.CardId != entity.CardId))
			{
				RevealDeckCard(entity.CardId);
				RevealedCards.Add(new CardEntity(entity.CardId, null) { Turn = turn });
			}
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> JoustReveal");
		}

		public void SecretTriggered(Entity entity, int turn)
		{
			MoveCardEntity(entity, Secrets, Graveyard, turn);
			UpdateRevealedEntity(entity, turn);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> SecretTriggered");
		}

		public void RevealDeckCard(string cardId)
		{
			var cardEntity = Deck.FirstOrDefault(ce => ce.Unknown);
			if(cardEntity != null)
				cardEntity.CardId = cardId;
		}

		public void SecretPlayedFromDeck(Entity entity, int turn)
		{
			MoveCardEntity(entity, Deck, Secrets, turn);
			UpdateRevealedEntity(entity, turn);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> SecretPlayedFromDeck");
		}

		public void SecretPlayedFromHand(Entity entity, int turn)
		{
			MoveCardEntity(entity, Hand, Secrets, turn);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> SecretPlayedFromHand");
		}

		public void PlayToGraveyard(Entity entity, string cardId, int turn)
		{
			MoveCardEntity(entity, Board, Graveyard, turn);
			Logger.WriteLine(string.Format("id={0}, cardId={1}, zonePos={2}", entity.Id, entity.CardId, entity.GetTag(GAME_TAG.ZONE_POSITION)), " >>> PlayToGraveyard");
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
			CardMark = (entity != null && entity.Id > 68) ? CardMark.Stolen : CardMark.None;
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

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HearthDb.Deckstrings;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using HSReplay.Requests;
using HSReplay.Responses;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Player : INotifyPropertyChanged
	{
		private readonly IGame _game;

		public Player(IGame game, bool isLocalPlayer)
		{
			_game = game;
			IsLocalPlayer = isLocalPlayer;
		}

		public string? Name { get; set; }
		public string? Class { get; set; }
		public int Id { get; set; }
		public int Fatigue { get; set; }
		public bool IsLocalPlayer { get; }
		public int SpellsPlayedCount { get; private set; }
		public List<string> CardsPlayedThisTurn { get; private set; } = new List<string>();
		public bool IsPlayingWhizbang { get; set; }
		public int PogoHopperPlayedCount { get; private set; }
		public string? LastDiedMinionCardId { get; set; }
		public string? LastDrawnCardId { get; set; }
		public int LibramReductionCount { get; private set; }
		public HashSet<SpellSchool> PlayedSpellSchools { get; private set; } = new HashSet<SpellSchool>();
		public int AbyssalCurseCount { get; private set; }

		public bool HasCoin => Hand.Any(e => e.IsTheCoin);
		public int HandCount => Hand.Count();
		public int DeckCount => Deck.Count();

		public IEnumerable<Entity> PlayerEntities => _game.Entities.Values.Where(x => !x.Info.HasOutstandingTagChanges && x.IsControlledBy(Id));
		public IEnumerable<Entity> RevealedEntities => _game.Entities.Values
			.Where(x => !x.Info.HasOutstandingTagChanges && (x.IsControlledBy(Id) || x.Info.OriginalController == Id))
			.Where(x => x.HasCardId)
			.Where(x =>
			{
				// Souleater's Scythe causes entites to be created in the graveyard.
				// We need to not reveal this card for the opponent and only reveal
				// it for the player after mulligan.
				if(x.Info.InGraveardAtStartOfGame && x.IsInGraveyard)
				{
					if(IsLocalPlayer)
						return _game.IsMulliganDone;
					return false;
				}
				return true;
			});
		public IEnumerable<Entity> Hand => PlayerEntities.Where(x => x.IsInHand);
		public IEnumerable<Entity> Board => PlayerEntities.Where(x => x.IsInPlay);
		public IEnumerable<Entity> Deck => PlayerEntities.Where(x => x.IsInDeck);
		public IEnumerable<Entity> Graveyard => PlayerEntities.Where(x => x.IsInGraveyard);
		public IEnumerable<Entity> Secrets => PlayerEntities.Where(x => x.IsInSecret && x.IsSecret);
		public IEnumerable<Entity> Quests => PlayerEntities.Where(x => x.IsInSecret && (x.IsQuest || x.IsSideQuest));
		public IEnumerable<Entity> QuestRewards => Board.Where(x => x.IsBgsQuestReward);
		public IEnumerable<Entity> Objectives => PlayerEntities.Where(x => x.IsInSecret && x.IsObjective);
		public IEnumerable<Entity> SetAside => PlayerEntities.Where(x => x.IsInSetAside);
		public static Deck? KnownOpponentDeck = null;
		public List<PredictedCard> InDeckPredictions { get; } = new List<PredictedCard>();
		public HashSet<string> PastHeroPowers { get; } = new HashSet<string>();
	
		private DeckState GetDeckState()
		{
			var createdCardsInDeck =
				Deck.Where(x => x.HasCardId && (x.Info.Created || x.Info.Stolen) && !x.Info.Hidden)
					.GroupBy(ce => new {ce.CardId, Created = (ce.Info.Created || ce.Info.Stolen), ce.Info.Discarded})
					.Select(g =>
					{
						var card = Database.GetCardFromId(g.Key.CardId);
						if(card == null)
							return null;
						card.Count = g.Count();
						card.IsCreated = g.Key.Created;
						card.HighlightInHand = Hand.Any(ce => ce.CardId == g.Key.CardId);
						return card;
					}).WhereNotNull();

			var originalCardsInDeckIds = DeckList.Instance.ActiveDeckVersion?.Cards
				.Where(x => x.Count > 0)
				.Select(x => Enumerable.Repeat(x.Id, x.Count))
				.SelectMany(x => x).ToList();
			var revealedNotInDeck = RevealedEntities.Where(x => (!x.Info.Created || x.Info.OriginalEntityWasCreated == false)
																&& x.IsPlayableCard
																&& (!x.IsInDeck || x.Info.Stolen)
																&& x.Info.OriginalController == Id
																&& !(x.Info.Hidden && (x.IsInDeck || x.IsInHand))).ToList();

			var removedFromDeckIds = new List<string>();
			foreach(var e in revealedNotInDeck)
			{
				if(e.CardId == null)
					continue;
				originalCardsInDeckIds?.Remove(e.CardId);
				if(!e.Info.Stolen || e.Info.OriginalController == Id)
					removedFromDeckIds.Add(e.CardId);
			}
			
			Card? ToRemainingCard(IGrouping<string, string> x)
			{
				var card = Database.GetCardFromId(x.Key);
				if(card == null)
					return null;
				card.Count = x.Count();
				if(Hand.Any(e => e.CardId == x.Key))
					card.HighlightInHand = true;
				return card;
			}

			Card? ToRemovedCard(IGrouping<string, string> c)
			{
				var card = Database.GetCardFromId(c.Key);
				if(card == null)
					return null;
				card.Count = 0;
				if(Hand.Any(e => e.CardId == c.Key))
					card.HighlightInHand = true;
				return card;
			}

			var remainingInDeck = createdCardsInDeck.Concat(originalCardsInDeckIds?.GroupBy(x => x).Select(ToRemainingCard).WhereNotNull() ?? new List<Card>());
			var removedFromDeck = removedFromDeckIds.GroupBy(x => x).Select(ToRemovedCard).WhereNotNull();

			var originalSideboards = DeckList.Instance.ActiveDeckVersion?.Sideboards;
			var removedFromSideboardIds = RevealedEntities.Where(x => x.HasCardId
			                                                          && x.IsPlayableCard
			                                                          && x.Info.OriginalController == Id
			                                                          && x.Info is { OriginalZone: Zone.HAND, Hidden: false } 
			                                                          && x.GetTag(GameTag.COPIED_FROM_ENTITY_ID) > 0 
			                                                          && RevealedEntities.FirstOrDefault(
				                                                          c => c.Id == x.GetTag(GameTag.COPIED_FROM_ENTITY_ID) && c is {
					                                                          IsInSetAside: true,
					                                                          Info: { CreatedInDeck: true }
				                                                          }
			                                                          ) != null).Select(x => x.CardId!).ToList();

			var remainingInSideboard = new Dictionary<string, IEnumerable<Card>>();
			var removedFromSideboard = new Dictionary<string, IEnumerable<Card>>();
			if (originalSideboards != null)
				foreach(var sideboard in originalSideboards)
				{
					var remainingSideboardCards = new List<Card>();
					var removedSideboardCards = new List<Card>();
					foreach(var c in sideboard.Cards)
					{
						var card = Database.GetCardFromId(c.Id);
						if(card == null)
							continue;
						card.Count = c.Count - removedFromSideboardIds.Count(cardId => cardId == c.Id);
						card.IsCreated = false; // Intentionally do not set cards as created to avoid gift icon
						card.HighlightInHand = Hand.Any(ce => ce.CardId == card.Id);
						if(c.Count > 0)
							remainingSideboardCards.Add(card);
						else
							removedSideboardCards.Add(card);
					}
					remainingInSideboard[sideboard.OwnerCardId] = remainingSideboardCards;
					removedFromSideboard[sideboard.OwnerCardId] = removedSideboardCards;
				};

			return new DeckState(remainingInDeck, removedFromDeck, remainingInSideboard, removedFromSideboard);
		}

		private DeckState GetOpponentDeckState()
		{
			var createdCardsInDeck =
				RevealedEntities.Where(x => x.Info.OriginalController == Id && x.IsInDeck && x.HasCardId && (x.Info.Created || x.Info.Stolen) && !x.Info.Hidden)
					.GroupBy(ce => new { ce.CardId, Created = (ce.Info.Created || ce.Info.Stolen), ce.Info.Discarded })
					.Select(g =>
					{
						var card = Database.GetCardFromId(g.Key.CardId);
						if(card == null)
							return null;
						card.Count = g.Count();
						card.IsCreated = g.Key.Created;
						card.HighlightInHand = Hand.Any(ce => ce.CardId == g.Key.CardId);
						return card;
					}).WhereNotNull();
			var originalCardsInDeck = KnownOpponentDeck?.Cards
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
				if(e.CardId == null)
					continue;
				originalCardsInDeck?.Remove(e.CardId);
				if(!e.Info.Stolen || e.Info.OriginalController == Id)
					removedFromDeck.Add(e.CardId);
			}
			return new DeckState(createdCardsInDeck.Concat(originalCardsInDeck.GroupBy(x => x).Select(x =>
			{
				var card = Database.GetCardFromId(x.Key);
				if(card == null)
					return null;
				card.Count = x.Count();
				if(Hand.Any(e => e.CardId == x.Key))
					card.HighlightInHand = true;
				return card;
			}).WhereNotNull()), removedFromDeck.GroupBy(x => x).Select(c =>
			{
				var card = Database.GetCardFromId(c.Key);
				if(card == null)
					return null;
				card.Count = 0;
				if(Hand.Any(e => e.CardId == c.Key))
					card.HighlightInHand = true;
				return card;
			}).WhereNotNull());
		}

		public IEnumerable<Card> GetPredictedCardsInDeck(bool hidden) => InDeckPredictions.Select(x =>
		{
			var card = Database.GetCardFromId(x.CardId);
			if(card == null)
				return null;
			if (hidden)
				card.Jousted = true;
			if (x.IsCreated)
			{
				card.IsCreated = true;
				card.Count = 1;
			}
			return card;
		}).WhereNotNull();

		public IEnumerable<Card> KnownCardsInDeck
			=> Deck.Where(x => x.HasCardId).GroupBy(ce => new {ce.CardId, Created = (ce.Info.Created || ce.Info.Stolen)}).Select(g =>
			{
				var card = Database.GetCardFromId(g.Key.CardId);
				if(card == null)
					return null;
				card.Count = g.Count();
				card.IsCreated = g.Key.Created;
				card.Jousted = true;
				return card;
			}).WhereNotNull().ToList();

		public IEnumerable<Card> RevealedCards
			=> RevealedEntities.Where(x => x != null && !string.IsNullOrEmpty(x.CardId) && (!x.Info.Created || x.Info.OriginalEntityWasCreated == false) && x.IsPlayableCard
									   && ((!x.IsInDeck && (!x.Info.Stolen || x.Info.OriginalController == Id)) || (x.Info.Stolen && x.Info.OriginalController == Id)))
								.GroupBy(x => new {x.CardId, Stolen = x.Info.Stolen && x.Info.OriginalController != Id})
								.Select(x =>
								{
									var card = Database.GetCardFromId(x.Key.CardId);
									if(card == null)
										return null;
									card.Count = x.Count();
									card.IsCreated = x.Key.Stolen;
									card.HighlightInHand = x.Any(c => c.IsInHand && c.IsControlledBy(Id));
									return card;
								}).WhereNotNull();

		public IEnumerable<Card> CreatedCardsInHand => Hand.Where(x => x != null && !string.IsNullOrEmpty(x.CardId) && (x.Info.Created || x.Info.Stolen)).GroupBy(x => x.CardId).Select(x =>
		{
			var card = Database.GetCardFromId(x.Key);
			if(card == null)
				return null;
			card.Count = x.Count();
			card.IsCreated = true;
			card.HighlightInHand = true;
			return card;
		}).WhereNotNull();

		public IEnumerable<Card> GetHighlightedCardsInHand(List<Card> cardsInDeck)
			=> DeckList.Instance.ActiveDeckVersion?.Cards.Where(c => cardsInDeck.All(c2 => c2.Id != c.Id) && Hand.Any(ce => c.Id == ce.CardId))
						.Select(c =>
						{
							var card = (Card)c.Clone();
							if(card == null)
								return null;
							card.Count = 0;
							card.HighlightInHand = true;
							return card;
						}).WhereNotNull() ?? new List<Card>();

		public List<Card> PlayerCardList => GetPlayerCardList(Config.Instance.RemoveCardsFromDeck, Config.Instance.HighlightCardsInHand, Config.Instance.ShowPlayerGet);

		internal List<Card> GetPlayerCardList(bool removeNotInDeck, bool highlightCardsInHand, bool includeCreatedInHand)
		{
			var createdInHand = includeCreatedInHand ? CreatedCardsInHand : new List<Card>();
			if(DeckList.Instance.ActiveDeck == null)
				return RevealedCards.Concat(createdInHand).Concat(KnownCardsInDeck).Concat(GetPredictedCardsInDeck(true)).ToSortedCardList();

			var sorting = _game.IsMulliganDone ? CardListExtensions.CardSorting.Cost : CardListExtensions.CardSorting.MulliganWr;

			var deckState = GetDeckState();
			var inDeck = deckState.RemainingInDeck.ToList();
			var notInDeck = deckState.RemovedFromDeck.Where(x => inDeck.All(c => x.Id != c.Id)).ToList();
			var predictedInDeck = GetPredictedCardsInDeck(false).Where(x => inDeck.All(c => x.Id != c.Id)).ToList();
			if(!removeNotInDeck)
				return AnnotateCards(inDeck.Concat(predictedInDeck).Concat(notInDeck).Concat(createdInHand)).ToSortedCardList(sorting);
			if(highlightCardsInHand)
				return AnnotateCards(inDeck.Concat(predictedInDeck).Concat(GetHighlightedCardsInHand(inDeck)).Concat(createdInHand)).ToSortedCardList(sorting);

			return AnnotateCards(inDeck.Concat(predictedInDeck).Concat(createdInHand)).ToSortedCardList(sorting);
		}

		private IEnumerable<Card> AnnotateCards(IEnumerable<Card> cards)
		{
			// Override Zilliax 3000 cost
			cards = Helper.ResolveZilliax3000(cards, PlayerSideboardsDict);

			// Attach Mulligan Card Data
			if(MulliganCardStats == null)
				return cards;

			return cards.Select(card =>
			{
				var cardStats = MulliganCardStats.FirstOrDefault(x => x.DbfId == card.DbfId);
				if(cardStats == null)
					return card;

				Card newCard = (Card)card.Clone();
				newCard.CardWinrates = cardStats.OpeningHandWinrate is float openingHandWinrate ? new CardWinrates()
				{
					MulliganWinrate = openingHandWinrate,
					BaseWinrate = (float?) cardStats.BaseWinrate
				} : null;
				newCard.IsMulliganOption = Hand.Any(x => x.Card.DbfId == card.DbfId);
				return newCard;
			});
		}

		public List<Sideboard> PlayerSideboardsDict => GetPlayerSideboards(Config.Instance.RemoveCardsFromDeck);

		internal List<Sideboard> GetPlayerSideboards(bool removeNotInSideboard)
		{
			if(DeckList.Instance.ActiveDeck == null)
				return new List<Sideboard>();
			var deckState = GetDeckState();
			var sideboardsDict = new Dictionary<string, List<Card>>();
			if (deckState.RemainingInSideboards != null)
				foreach(var sideboard in deckState.RemainingInSideboards)
					sideboardsDict[sideboard.Key] = new List<Card>(sideboard.Value);

			if(deckState.RemovedFromSideboards != null && !removeNotInSideboard)
				foreach(var sideboard in deckState.RemovedFromSideboards)
					if (sideboardsDict.TryGetValue(sideboard.Key, out var currentSideboard))
						currentSideboard.AddRange(sideboard.Value);
					else
						sideboardsDict[sideboard.Key] = new List<Card>(sideboard.Value);

			var sideboards = new List<Sideboard>();
				foreach(var sideboard in sideboardsDict)
					sideboards.Add(new Sideboard(sideboard.Key, sideboard.Value.ToList()));

			return sideboards;
		}

		public List<Card> OpponentCardList
			=> GetOpponentCardList(Config.Instance.RemoveCardsFromDeck, Config.Instance.HighlightCardsInHand, Config.Instance.ShowPlayerGet);

		public List<Card> GetOpponentCardList(bool removeNotInDeck, bool highlightCardsInHand, bool includeCreatedInHand)
		{
			if(KnownOpponentDeck == null)
			{
				return RevealedEntities.Where(x => !(x.Info.GuessedCardState == GuessedCardState.None && x.Info.Hidden && (x.IsInDeck || x.IsInHand))
										&& (x.IsPlayableCard || !x.HasTag(GameTag.CARDTYPE))
										&& (x.GetTag(GameTag.CREATOR) == 1
											|| ((!x.Info.Created || (Config.Instance.OpponentIncludeCreated && (x.Info.CreatedInDeck || x.Info.CreatedInHand)))
												&& x.Info.OriginalController == Id)
											|| x.IsInHand || x.IsInDeck)
										&& !CardIds.HiddenCardidPrefixes.Any(y => x.CardId != null && x.CardId.StartsWith(y))
										&& !EntityIsRemovedFromGamePassive(x)
										&& !(x.Info.Created && x.IsInSetAside && x.Info.GuessedCardState != GuessedCardState.Guessed))
								.GroupBy(e => new {
									CardId = e.Info.WasTransformed ? e.Info.OriginalCardId : e.CardId,
									Hidden = (e.IsInHand || e.IsInDeck || (e.IsInSetAside && e.Info.GuessedCardState == GuessedCardState.Guessed)) && e.IsControlledBy(Id),
									Created = e.Info.Created || (e.Info.Stolen && e.Info.OriginalController != Id),
									Discarded = e.Info.Discarded && Config.Instance.HighlightDiscarded
								})
								.Select(g =>
								{
									if(g.Key.CardId == null)
										return null;
									var card = Database.GetCardFromId(g.Key.CardId);
									if(card == null)
										return null;
									card.Count = g.Count();
									card.Jousted = g.Key.Hidden;
									card.IsCreated = g.Key.Created;
									card.WasDiscarded = g.Key.Discarded;
									return card;
								}).WhereNotNull()
								.Concat(GetPredictedCardsInDeck(true)).ToSortedCardList();
			}
			var createdInHand = includeCreatedInHand ? CreatedCardsInHand : new List<Card>();
			var deckState = GetOpponentDeckState();
			var inDeck = deckState.RemainingInDeck.ToList();
			var notInDeck = deckState.RemovedFromDeck.Where(x => inDeck.All(c => x.Id != c.Id)).ToList();
			var predictedInDeck = GetPredictedCardsInDeck(false).Where(x => inDeck.All(c => x.Id != c.Id)).ToList();
			if(!removeNotInDeck)
				return inDeck.Concat(predictedInDeck).Concat(notInDeck).Concat(createdInHand).ToSortedCardList();
			if(highlightCardsInHand)
				return inDeck.Concat(predictedInDeck).Concat(GetHighlightedCardsInHand(inDeck)).Concat(createdInHand).ToSortedCardList();
			return inDeck.Concat(predictedInDeck).Concat(createdInHand).ToSortedCardList();
		}

		private bool EntityIsRemovedFromGamePassive(Entity entity) => entity.HasTag(GameTag.DUNGEON_PASSIVE_BUFF) && entity.GetTag(GameTag.ZONE) == (int)Zone.REMOVEDFROMGAME;

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Reset()
		{
			Name = "";
			Class = "";
			Id = -1;
			Fatigue = 0;
			InDeckPredictions.Clear();
			SpellsPlayedCount = 0;
			PogoHopperPlayedCount = 0;
			CardsPlayedThisTurn.Clear();
			LastDrawnCardId = null;
			LibramReductionCount = 0;
			PlayedSpellSchools.Clear();
			AbyssalCurseCount = 0;
			PastHeroPowers.Clear();
		}

		public void Draw(Entity entity, int turn)
		{
			if(IsLocalPlayer && entity.CardId != null)
			{
				UpdateKnownEntitiesInDeck(entity.CardId);
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
			LastDrawnCardId = entity.CardId;
			//Log(entity);
		}


		private void Log(Entity entity, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "") 
			=> Log(entity.ToString(), memberName, sourceFilePath);

		private void Log(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "") 
			=> Utility.Logging.Log.Info((IsLocalPlayer ? "[Player] "  : "[Opponent] ") + msg, memberName, sourceFilePath);

		public void Play(Entity entity, int turn)
		{
			if(!IsLocalPlayer && entity.CardId != null)
				UpdateKnownEntitiesInDeck(entity.CardId, entity.Info.Turn);
			switch(entity.GetTag(GameTag.CARDTYPE))
			{
				case (int)CardType.TOKEN:
					entity.Info.Created = true;
					break;
				case (int)CardType.MINION:
					if (entity.CardId == HearthDb.CardIds.Collectible.Rogue.PogoHopper)
					{
						PogoHopperPlayedCount++;	
					}
					break;
				case (int)CardType.SPELL:
					SpellsPlayedCount++;
					if(entity.Tags.TryGetValue(GameTag.SPELL_SCHOOL, out var spellSchoolTag))
						PlayedSpellSchools.Add((SpellSchool)spellSchoolTag);
					break;
			}
			entity.Info.Hidden = false;
			entity.Info.Turn = turn;
			entity.Info.CostReduction = 0;
			if(entity.CardId != null)
				CardsPlayedThisTurn.Add(entity.CardId);
			//Log(entity);
		}

		public void OnTurnStart()
		{
			CardsPlayedThisTurn.Clear();
		}

		public void DeckToPlay(Entity entity, int turn)
		{
			if(entity.CardId != null)
				UpdateKnownEntitiesInDeck(entity.CardId);
			entity.Info.Turn = turn;
			//Log(entity);
		}

		public void CreateInHand(Entity entity, int turn)
		{
			entity.Info.Created = true;
			entity.Info.Turn = turn;
			//Log(entity);
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
			//Log(entity);
		}

		public void CreateInPlay(Entity entity, int turn)
		{
			entity.Info.Created = true;
			entity.Info.Turn = turn;
			//Log(entity);
			if(entity.IsHeroPower)
				HeroPowerChanged(entity);
		}

		public void CreateInSecret(Entity entity, int turn)
		{
			entity.Info.Created = true;
			entity.Info.Turn = turn;
			//Log(entity);
		}

		public void RemoveFromDeck(Entity entity, int turn)
		{
			//Do not check for KnownCardIds here, this is how jousted cards get removed from the deck
			entity.Info.Turn = turn;
			entity.Info.Discarded = true;
			//Log(entity);
		}

		public void Mulligan(Entity entity)
		{
			//Log(entity);
		}

		public void HandDiscard(Entity entity, int turn)
		{
			if(!IsLocalPlayer && entity.CardId != null)
				UpdateKnownEntitiesInDeck(entity.CardId, entity.Info.Turn);
			entity.Info.Turn = turn;
			entity.Info.Discarded = true;
			//Log(entity);
		}

		public void DeckDiscard(Entity entity, int turn)
		{
			if(entity.CardId != null)
				UpdateKnownEntitiesInDeck(entity.CardId);
			entity.Info.Turn = turn;
			entity.Info.Discarded = true;
			//Log(entity);
		}

		public void HandToDeck(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			entity.Info.Returned = true;
			//Log(entity);
		}

		public void BoardToDeck(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			entity.Info.Returned = true;
			//Log(entity);
		}

		public void BoardToHand(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			entity.Info.Returned = true;
			//Log(entity);
		}

		public void PredictUniqueCardInDeck(string cardId, bool isCreated)
		{
			if(InDeckPredictions.All(x => x.CardId != cardId))
				InDeckPredictions.Add(new PredictedCard(cardId, 0, isCreated));
		}

		public void JoustReveal(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			var card = InDeckPredictions.FirstOrDefault(x => x.CardId == entity.CardId);
			if(card != null)
				card.Turn = turn;
			else if(entity.CardId != null)
				InDeckPredictions.Add(new PredictedCard(entity.CardId, turn));
			//Log(entity);
		}

		private void UpdateKnownEntitiesInDeck(string cardId, int turn = int.MaxValue)
		{
			var card = InDeckPredictions.FirstOrDefault(x => x.CardId == cardId && turn >= x.Turn);
			if(card != null)
				InDeckPredictions.Remove(card);
		}

		public void SecretTriggered(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			_game.SecretsManager.SecretTriggered(entity);
			//Log(entity);
		}

		public void SecretPlayedFromDeck(Entity entity, int turn)
		{
			if(entity.CardId != null)
				UpdateKnownEntitiesInDeck(entity.CardId);
			entity.Info.Turn = turn;
			//Log(entity);
		}

		public void SecretPlayedFromHand(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			SpellsPlayedCount++;
			if(entity.Tags.TryGetValue(GameTag.SPELL_SCHOOL, out var spellSchoolTag))
				PlayedSpellSchools.Add((SpellSchool)spellSchoolTag);
			//Log(entity);
		}

		public void QuestPlayedFromHand(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			SpellsPlayedCount++;
			//Log(entity);
		}

		public void SigilPlayedFromHand(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			SpellsPlayedCount++;
			if(entity.Tags.TryGetValue(GameTag.SPELL_SCHOOL, out var spellSchoolTag))
				PlayedSpellSchools.Add((SpellSchool)spellSchoolTag);
			//Log(entity);
		}

		public void ObjectivePlayedFromHand(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			SpellsPlayedCount++;
			if(entity.Tags.TryGetValue(GameTag.SPELL_SCHOOL, out var spellSchoolTag))
				PlayedSpellSchools.Add((SpellSchool)spellSchoolTag);
			//Log(entity);
		}

		public void PlayToGraveyard(Entity entity, string cardId, int turn)
		{
			entity.Info.Turn = turn;
			if(entity.IsMinion)
				LastDiedMinionCardId = cardId;
			//Log(entity);
		}

		public void RemoveFromPlay(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			//Log(entity);
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void StolenByOpponent(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			//Log(entity);
		}

		public void StolenFromOpponent(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			//Log(entity);
		}

		public void CreateInSetAside(Entity entity, int turn)
		{
			entity.Info.Turn = turn;
			//Log(entity);
		}

		public void UpdateLibramReduction(int change) => LibramReductionCount += change;

		public void UpdateAbyssalCurse(int value)
		{
			AbyssalCurseCount = value > 0 ? value : AbyssalCurseCount + 1;
		}

		internal void ShuffleDeck()
		{
			foreach(var card in Deck)
				card.Info.DeckIndex = 0;
		}

		public void HeroPowerChanged(Entity entity)
		{
			if(!IsLocalPlayer)
				return;
			var id = entity.Info.LatestCardId;
			if(string.IsNullOrEmpty(id))
				return;
			var added = PastHeroPowers.Add(id!);
			if(added)
			{
				//Log(entity);
			}
		}

		private IEnumerable<SingleCardStats>? _mulliganCardStats = null;
		public IEnumerable<SingleCardStats>? MulliganCardStats
		{
			get => _mulliganCardStats;
			set
			{
				_mulliganCardStats = value;
				Core.UpdatePlayerCards(true);
			}
		}
	}
}

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class Game
	{
		static Game()
		{
			Entities = new Dictionary<int, Entity>();
			CurrentGameMode = GameMode.None;
			IsInMenu = true;
			SetAsideCards = new List<string>();
			OpponentReturnedToDeck = new List<KeyValuePair<string, int>>();
			PlayerDeck = new ObservableCollection<Card>();
			PlayerDrawn = new ObservableCollection<Card>();
			OpponentCards = new ObservableCollection<Card>();
			PossibleArenaCards = new List<Card>();
			PossibleConstructedCards = new List<Card>();
			_cardDb = new Dictionary<string, Card>();
			OpponentHandAge = new int[MaxHandSize];
			OpponentHandMarks = new CardMark[MaxHandSize];
			OpponentStolenCardsInformation = new Card[MaxHandSize];
			OpponentSecrets = new OpponentSecrets();
			for(var i = 0; i < MaxHandSize; i++)
			{
				OpponentHandAge[i] = -1;
				OpponentHandMarks[i] = CardMark.None;
			}

			LoadCardDb(Helper.LanguageDict.ContainsValue(Config.Instance.SelectedLanguage) ? Config.Instance.SelectedLanguage : "enUS");
		}

		public static bool IsMulliganDone
		{
			get
			{
				var player = Entities.FirstOrDefault(x => x.Value.IsPlayer);
				var opponent = Entities.FirstOrDefault(x => x.Value.HasTag(GAME_TAG.PLAYER_ID) && !x.Value.IsPlayer);
				if(player.Value == null || opponent.Value == null)
					return false;
				return player.Value.GetTag(GAME_TAG.MULLIGAN_STATE) == (int)TAG_MULLIGAN.DONE
				       && opponent.Value.GetTag(GAME_TAG.MULLIGAN_STATE) == (int)TAG_MULLIGAN.DONE;
			}
		}

		public static List<string> HSLogLines
		{
			get { return hsLogLines; }
		}

		public static int PlayerDeckSize { get; set; }
		public static bool NoMatchingDeck { get; set; }

		public static void Reset(bool resetStats = true)
		{
			Logger.WriteLine(">>>>>>>>>>> Reset <<<<<<<<<<<", "Game");

			ReplayMaker.Reset();
			PlayerDrawn.Clear();
			Entities.Clear();
			PlayerId = -1;
			OpponentId = -1;
			SavedReplay = false;
			PlayerHandCount = 0;
			PlayerFatigueCount = 0;
			OpponentSecretCount = 0;
			OpponentCards.Clear();
			OpponentHandCount = 0;
			OpponentFatigueCount = 0;
			OpponentDeckCount = 30;
			PlayerDeckSize = 30;
			SecondToLastUsedId = null;
			OpponentHandAge = new int[MaxHandSize];
			OpponentHandMarks = new CardMark[MaxHandSize];
			OpponentStolenCardsInformation = new Card[MaxHandSize];
			OpponentSecrets.ClearSecrets();
			NoMatchingDeck = false;

			for(var i = 0; i < MaxHandSize; i++)
			{
				OpponentHandAge[i] = -1;
				OpponentHandMarks[i] = CardMark.None;
			}

			// Assuming opponent has coin, corrected if we draw it
			OpponentHandMarks[DefaultCoinPosition] = CardMark.Coin;
			OpponentHandAge[DefaultCoinPosition] = 0;
			OpponentHasCoin = true;

			SetAsideCards.Clear();
			OpponentReturnedToDeck.Clear();

			//if(CurrentGameMode == GameMode.Ranked) //otherwise switching from playing ranked to casual causes problems
			//	CurrentGameMode = GameMode.Casual;


			if(!IsInMenu && resetStats)
			{
				if(CurrentGameMode != GameMode.Spectator)
					CurrentGameMode = GameMode.None;
				CurrentGameStats = new GameStats(GameResult.None, PlayingAgainst, PlayingAs)
				{
					PlayerName = PlayerName,
					OpponentName = OpponentName,
					Region = CurrentRegion
				};
			}
			hsLogLines = new List<string>();
		}

		public static void SetPremadeDeck(Deck deck)
		{
			PlayerDeck.Clear();
			foreach(var card in deck.GetSelectedDeckVersion().Cards)
				PlayerDeck.Add((Card)card.Clone());
			IsUsingPremade = true;
		}

		private static void LogDeckChange(bool opponent, Card card, bool decrease)
		{
			var previous = decrease ? card.Count + 1 : card.Count - 1;

			Logger.WriteLine(
			                 string.Format("({0} deck) {1} count {2} -> {3}", opponent ? "opponent" : "player", card.Name, previous, card.Count),
			                 "Hearthstone");
		}

		private static bool ValidateOpponentHandCount()
		{
			if(OpponentHandCount - 1 < 0 || OpponentHandCount - 1 > 9)
			{
				Logger.WriteLine("ValidateOpponentHandCount failed! OpponentHandCount = " + OpponentHandCount, "Hearthstone");
				return false;
			}

			return true;
		}

		private static void LogOpponentHand()
		{
			var zipped = OpponentHandAge.Zip(OpponentHandMarks.Select(mark => (char)mark),
			                                 (age, mark) => string.Format("{0}{1}", (age == -1 ? " " : age.ToString()), mark));

			Logger.WriteLine("Opponent Hand after draw: " + string.Join(",", zipped), "Hearthstone");
		}

		public static void AddPlayToCurrentGame(PlayType play, int turn, string cardId)
		{
			if(CurrentGameStats == null)
				return;
			CurrentGameStats.AddPlay(play, turn, cardId);
		}

		public static bool IsActualCard(Card card)
		{
			if(card == null)
				return false;
			return (card.Type == "Minion" || card.Type == "Spell" || card.Type == "Weapon")
			       && Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 1)) && Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 2))
			       && !CardIds.InvalidCardIds.Any(id => card.Id.Contains(id));
		}

		public static void ResetArenaCards()
		{
			PossibleArenaCards.Clear();
		}

		public static void ResetConstructedCards()
		{
			PossibleConstructedCards.Clear();
		}

		public static void AddHSLogLine(string logLine)
		{
			HSLogLines.Add(logLine);
		}

		//public static readonly string[] Classes = new[] { "Druid", "Hunter", "Mage", "Priest", "Paladin", "Shaman", "Rogue", "Warlock", "Warrior" };

		#region Properties

		private const int DefaultCoinPosition = 4;
		private const int MaxHandSize = 10;
		public static bool HighlightCardsInHand { get; set; }
		public static bool HighlightDiscarded { get; set; }

		private static Dictionary<string, Card> _cardDb;

		public static ObservableCollection<Card> OpponentCards { get; set; }
		public static int OpponentHandCount { get; set; }
		public static int OpponentFatigueCount { get; set; }
		public static bool IsInMenu { get; set; }
		public static bool IsUsingPremade { get; set; }
		public static int OpponentDeckCount { get; set; }
		public static bool OpponentHasCoin { get; set; }
		public static int OpponentSecretCount { get; set; }
		public static bool IsRunning { get; set; }
		public static Region CurrentRegion { get; set; }

		private static GameMode _currentGameMode;

		public static GameMode CurrentGameMode
		{
			get { return _currentGameMode; }
			set
			{
				_currentGameMode = value;
				Logger.WriteLine("set CurrentGameMode to " + value, "Game");
			}
		}

		public static GameStats CurrentGameStats { get; set; }


		public static ObservableCollection<Card> PlayerDeck { get; set; }
		public static ObservableCollection<Card> PlayerDrawn { get; set; }
		public static int PlayerHandCount { get; set; }
		public static int PlayerFatigueCount { get; set; }
		public static string PlayingAgainst { get; set; }
		public static string PlayingAs { get; set; }
		public static string PlayerName { get; set; }
		public static string OpponentName { get; set; }

		public static List<string> SetAsideCards { get; set; }
		public static List<KeyValuePair<string, int>> OpponentReturnedToDeck { get; set; }

		public static OpponentSecrets OpponentSecrets { get; set; }

		private static readonly List<string> ValidCardSets = new List<string>
		{
			"Basic",
			"Reward",
			"Classic",
			"Promotion",
			"Curse of Naxxramas",
			"Goblins vs Gnomes",
			"Blackrock Mountain"
		};

		public static List<Card> DrawnLastGame { get; set; }

		public static int[] OpponentHandAge { get; private set; }
		public static CardMark[] OpponentHandMarks { get; private set; }
		public static Card[] OpponentStolenCardsInformation { get; private set; }
		public static List<Card> PossibleArenaCards { get; set; }
		public static List<Card> PossibleConstructedCards { get; set; }
		public static int? SecondToLastUsedId { get; set; }

		//public static List<Entity> Entities;
		public static Dictionary<int, Entity> Entities { get; set; }
		public static int PlayerId { get; set; }
		public static int OpponentId { get; set; }
		public static bool SavedReplay { get; set; }
		private static List<string> hsLogLines = new List<string>();
		//public static Dictionary<string, int> PlayerIds; 

		#endregion

		#region Player

#pragma warning disable 4014
		public static async Task<bool> PlayerDraw(string cardId)
		{
			PlayerHandCount++;

			if(string.IsNullOrEmpty(cardId))
				return true;

			var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId);
			if(drawnCard != null)
				drawnCard.Count++;
			else
			{
				drawnCard = GetCardFromId(cardId);
				PlayerDrawn.Add(drawnCard);
			}
			drawnCard.InHandCount++;
			drawnCard.JustDrawn();


			var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId && c.Count > 0 && c.IsStolen)
			               ?? PlayerDeck.FirstOrDefault(c => c.Id == cardId && c.Count > 0);
			if(deckCard == null)
				return false;

			deckCard.Count--;
			deckCard.InHandCount++;
			LogDeckChange(false, deckCard, true);
			if(deckCard.Count == 0 && Config.Instance.RemoveCardsFromDeck && !Config.Instance.HighlightCardsInHand)
			{
				//wait for just-drawn highlight to be over, then doublecheck (coule be back in deck after e.g.) -> remove
				await deckCard.JustDrawn();
				if(deckCard.Count == 0)
				{
					PlayerDeck.Remove(deckCard);
					Logger.WriteLine("Removed " + deckCard.Name + " from deck (count 0)", "Game");
				}
			}
			else
				deckCard.JustDrawn();

			return true;
		}
#pragma warning restore 4014

#pragma warning disable 4014
		public static void PlayerGet(string cardId, bool fromPlay, int turn)
		{
			PlayerHandCount++;

			if(cardId == "GAME_005" && turn == 0)
			{
				OpponentHasCoin = false;
				OpponentHandMarks[DefaultCoinPosition] = CardMark.None;
				OpponentHandAge[DefaultCoinPosition] = -1;
				OpponentStolenCardsInformation[DefaultCoinPosition] = null;
				Logger.WriteLine("Player got the coin", "Hearthstone");
			}

			var fromSetAside = SetAsideCards.Any(id => cardId == id);
			if(fromSetAside)
			{
				Logger.WriteLine("Got card from setaside: " + cardId, "Game");
				foreach(var c in SetAsideCards)
					PlayerDeckDiscard(c);
				SetAsideCards.Clear();
			}

			var card = PlayerDeck.FirstOrDefault(c => c.Id == cardId && (!c.IsStolen || fromSetAside));
			if(card != null)
			{
				card.InHandCount++;
				card.JustDrawn();
			}
			else if(Config.Instance.ShowPlayerGet && !fromPlay)
			{
				var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId && c.IsStolen);
				if(drawnCard != null)
					drawnCard.Count++;
				else
				{
					drawnCard = GetCardFromId(cardId);
					drawnCard.IsStolen = true;
					PlayerDrawn.Add(drawnCard);
				}
				drawnCard.InHandCount++;
				drawnCard.JustDrawn();

				var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId && c.IsStolen);
				if(deckCard != null)
				{
					deckCard.Count++;
					deckCard.InHandCount++;
					LogDeckChange(false, deckCard, false);
				}
				else
				{
					deckCard = GetCardFromId(cardId);
					deckCard.InHandCount++;
					deckCard.IsStolen = true;
					PlayerDeck.Add(deckCard);
					deckCard.JustDrawn();
				}
			}
		}
#pragma warning restore 4014
		public static void PlayerPlayed(string cardId)
		{
			PlayerHandCount--;
			var card = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
			if(card != null)
			{
				card.InHandCount--;
				if(Config.Instance.HighlightCardsInHand && CanRemoveCard(card))
					PlayerDeck.Remove(card);
			}

			var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId);
			if(drawnCard != null)
				drawnCard.InHandCount--;
		}

		private static bool CanRemoveCard(Card card)
		{
			if(card.IsStolen && card.InHandCount < 1)
				return true;
			if(Config.Instance.RemoveCardsFromDeck && card.Count < 1)
			{
				if((Config.Instance.HighlightCardsInHand && card.InHandCount < 1) || !Config.Instance.HighlightCardsInHand)
					return true;
			}
			return false;
		}


		public static void PlayerMulligan(string cardId)
		{
			PlayerHandCount--;

			if(string.IsNullOrEmpty(cardId))
				return;

			var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId);
			if(drawnCard != null)
			{
				drawnCard.Count--;
				if(drawnCard.Count < 1)
					PlayerDrawn.Remove(drawnCard);
			}

			var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
			if(deckCard != null)
			{
				deckCard.Count++;
				deckCard.InHandCount--;
				LogDeckChange(false, deckCard, false);
			}
			else if(Config.Instance.RemoveCardsFromDeck)
			{
				deckCard = GetCardFromId(cardId);
				PlayerDeck.Add(deckCard);
				Logger.WriteLine("Added " + deckCard.Name + " to deck (count was 0)", "Game");
			}
		}

		public static void PlayerHandDiscard(string cardId)
		{
			PlayerPlayed(cardId);
		}

		public static bool PlayerDeckDiscard(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return true;

			var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId);
			if(drawnCard == null)
				PlayerDrawn.Add(GetCardFromId(cardId));
			else
				drawnCard.Count++;

			var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId && c.Count > 0 && c.IsStolen)
			               ?? PlayerDeck.FirstOrDefault(c => c.Id == cardId && c.Count > 0);
			if(deckCard == null)
				return false;

			deckCard.Count--;
			LogDeckChange(false, deckCard, true);
			if(CanRemoveCard(deckCard))
				PlayerDeck.Remove(deckCard);

			return true;
		}

		public static void PlayerPlayToDeck(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;

			var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
			if(deckCard != null)
			{
				deckCard.Count++;
				LogDeckChange(false, deckCard, false);
			}
			else if(Config.Instance.RemoveCardsFromDeck)
			{
				deckCard = GetCardFromId(cardId);
				PlayerDeck.Add(deckCard);
				Logger.WriteLine("Added " + deckCard.Name + " to deck (count was 0)", "Game");
			}
		}


		public static void PlayerGetToDeck(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;

			PlayerDeckSize++;
			var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId && c.IsStolen);
			if(deckCard != null)
			{
				deckCard.Count++;
				LogDeckChange(false, deckCard, false);
			}
			else
			{
				deckCard = GetCardFromId(cardId);
				deckCard.IsStolen = true;
				PlayerDeck.Add(deckCard);
				Logger.WriteLine("Added " + deckCard.Name + " to deck (count was 0)", "Game");
			}
		}

		#endregion

		#region Opponent

		public static void OpponentGetToDeck(int turn)
		{
			OpponentDeckCount++;
		}

		public static void OpponentDraw(int turn)
		{
			OpponentHandCount++;
			if(turn == 0 && OpponentHandCount == 5)
				//coin draw
				return;
			OpponentDeckCount--;

			if(!ValidateOpponentHandCount())
				return;
			if(OpponentHandAge[OpponentHandCount - 1] != -1)
			{
				Logger.WriteLine(string.Format("Card {0} is already set to {1}", OpponentHandCount - 1, OpponentHandAge[OpponentHandCount - 1]),
				                 "Hearthstone");
			}
			else
			{
				Logger.WriteLine(string.Format("Set card {0} to age {1}", OpponentHandCount - 1, turn), "Hearthstone");

				OpponentHandAge[OpponentHandCount - 1] = turn;
				OpponentHandMarks[OpponentHandCount - 1] = turn == 0 ? CardMark.Kept : CardMark.None;

				LogOpponentHand();
			}
		}

		public static void OpponentPlay(string id, int from, int turn)
		{
			OpponentHandCount--;

			if(id == "GAME_005")
				OpponentHasCoin = false;
			if(!string.IsNullOrEmpty(id))
			{
				//key: cardid, value: turn when returned to deck
				var wasReturnedToDeck = OpponentReturnedToDeck.Any(p => p.Key == id && p.Value <= OpponentHandAge[from - 1]);
				var stolen = from != -1
				             && (OpponentHandMarks[from - 1] == CardMark.Stolen || OpponentHandMarks[from - 1] == CardMark.Returned
				                 || wasReturnedToDeck);
				var card = OpponentCards.FirstOrDefault(c => c.Id == id && c.IsStolen == stolen && !c.WasDiscarded);

				//card can't be marked stolen or returned, since it was returned to the deck
				if(wasReturnedToDeck && stolen
				   && !(OpponentHandMarks[from - 1] == CardMark.Stolen || OpponentHandMarks[from - 1] == CardMark.Returned))
					OpponentReturnedToDeck.Remove(OpponentReturnedToDeck.First(p => p.Key == id && p.Value <= OpponentHandAge[from - 1]));

				if(card != null)
					card.Count++;
				else
				{
					card = GetCardFromId(id);
					card.IsStolen = stolen;
					OpponentCards.Add(card);
				}

				LogDeckChange(true, card, false);

				if(card.IsStolen)
					Logger.WriteLine("Opponent played stolen card from " + from, "Game");
			}

			for(var i = from - 1; i < MaxHandSize - 1; i++)
			{
				OpponentHandAge[i] = OpponentHandAge[i + 1];
				OpponentHandMarks[i] = OpponentHandMarks[i + 1];
				OpponentStolenCardsInformation[i] = OpponentStolenCardsInformation[i + 1];
			}

			OpponentHandAge[MaxHandSize - 1] = -1;
			OpponentHandMarks[MaxHandSize - 1] = CardMark.None;
			OpponentStolenCardsInformation[MaxHandSize - 1] = null;


			LogOpponentHand();
		}

		public static void OpponentMulligan(int pos)
		{
			OpponentHandCount--;
			OpponentDeckCount++;
			OpponentHandMarks[pos - 1] = CardMark.Mulliganed;
			if(OpponentHandCount < OpponentHandAge.Count(x => x != -1))
			{
				OpponentHandAge[OpponentHandCount] = -1;
				Logger.WriteLine(string.Format("Fixed hand ages after mulligan (removed {0})", OpponentHandCount), "Hearthstone");
				LogOpponentHand();
			}
		}


		public static void OpponentBackToHand(string cardId, int turn, int id)
		{
			OpponentHandCount++;

			if(!string.IsNullOrEmpty(cardId))
			{
				var card = OpponentCards.FirstOrDefault(c => c.Id == cardId && !c.WasDiscarded);
				if(card != null)
				{
					card.Count--;
					if(card.Count < 1)
						OpponentCards.Remove(card);

					LogDeckChange(true, card, true);
				}
			}

			if(ValidateOpponentHandCount())
			{
				OpponentHandAge[OpponentHandCount - 1] = turn;
				OpponentHandMarks[OpponentHandCount - 1] = CardMark.Returned;
				if(!string.IsNullOrEmpty(cardId))
				{
					var card = GetCardFromId(cardId);
					if(card != null)
						OpponentStolenCardsInformation[OpponentHandCount - 1] = card;
				}
			}
		}

		public static void OpponentPlayToDeck(string cardId, int turn)
		{
			OpponentDeckCount++;

			if(string.IsNullOrEmpty(cardId))
				return;

			OpponentReturnedToDeck.Add(new KeyValuePair<string, int>(cardId, turn));
		}

		public static void OpponentDeckDiscard(string cardId)
		{
			OpponentDeckCount--;
			if(string.IsNullOrEmpty(cardId))
				return;

			var card = OpponentCards.FirstOrDefault(c => c.Id == cardId && c.WasDiscarded);
			if(card != null)
				card.Count++;
			else
			{
				card = GetCardFromId(cardId);
				card.WasDiscarded = true;
				OpponentCards.Add(card);
			}

			LogDeckChange(true, card, false);
		}

		public static void OpponentSecretTriggered(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			var card = OpponentCards.FirstOrDefault(c => c.Id == cardId && !c.WasDiscarded);
			if(card != null)
				card.Count++;
			else
			{
				card = GetCardFromId(cardId);
				OpponentCards.Add(card);
			}

			LogDeckChange(true, card, false);
		}

		public static void OpponentGet(int turn, int id)
		{
			OpponentHandCount++;

			if(!ValidateOpponentHandCount())
				return;

			OpponentHandAge[OpponentHandCount - 1] = turn;

			if(OpponentHandMarks[OpponentHandCount - 1] != CardMark.Coin)
			{
				OpponentHandMarks[OpponentHandCount - 1] = CardMark.Stolen;
				if(SecondToLastUsedId.HasValue)
				{
					var cardId = Entities[id].CardId;
					if(cardId == "GVG_007" && Entities[id].HasTag(GAME_TAG.DISPLAYED_CREATOR))
						//Bug with created Flame Leviathan's: #863
						return;
					if(string.IsNullOrEmpty(cardId) && Entities[id].HasTag(GAME_TAG.LAST_AFFECTED_BY))
						cardId = Entities[Entities[id].GetTag(GAME_TAG.LAST_AFFECTED_BY)].CardId;
					if(string.IsNullOrEmpty(cardId))
						cardId = Entities[SecondToLastUsedId.Value].CardId;

					var card = GetCardFromId(cardId);
					if(card != null)
						OpponentStolenCardsInformation[OpponentHandCount - 1] = card;
				}
			}


			LogOpponentHand();
		}

		#endregion

		#region Database

		/*private static void LoadCardDb(string languageTag)
		{
			try
			{
				var localizedCards = new Dictionary<string, Card>();
				if(languageTag != "enUS")
				{
					var file = string.Format("Files/cardsDB.{0}.json", languageTag);
					if(File.Exists(file))
					{
						var localized = JObject.Parse(File.ReadAllText(file));
						foreach(var cardType in localized)
						{
							if(!ValidCardSets.Any(cs => cs.Equals(cardType.Key)))
								continue;
							foreach(var card in cardType.Value)
							{
								var tmp = JsonConvert.DeserializeObject<Card>(card.ToString());
								localizedCards.Add(tmp.Id, tmp);
							}
						}
					}
					Logger.WriteLine("Done loading localized card database (" + languageTag + ")", "Hearthstone");
				}


				//load engish db (needed for importing, etc)
				const string fileEng = "Files/cardsDB.enUS.json";
				var tempDb = new Dictionary<string, Card>();
				if(File.Exists(fileEng))
				{
					var obj = JObject.Parse(File.ReadAllText(fileEng));
					foreach(var cardType in obj)
					{
						var set = ValidCardSets.FirstOrDefault(cs => cs.Equals(cardType.Key));
						if(set == null)
							continue;

						foreach(var card in cardType.Value)
						{
							var tmp = JsonConvert.DeserializeObject<Card>(card.ToString());
							if(languageTag != "enUS")
							{
								var localizedCard = localizedCards[tmp.Id];
								tmp.LocalizedName = localizedCard.Name;
								tmp.Text = localizedCard.Text;
							}
							tmp.Set = set;
							tempDb.Add(tmp.Id, tmp);
						}
					}
					Logger.WriteLine("Done loading card database (enUS)", "Game");
				}
				_cardDb = new Dictionary<string, Card>(tempDb);
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error loading db: \n" + e, "Game");
			}
		}*/

		private static void LoadCardDb(string languageTag)
		{
			try
			{
				var db = XmlManager<CardDb>.Load(string.Format("Files/cardDB.{0}.xml", "enUS"));
				_cardDb = db.Cards.Where(x => ValidCardSets.Any(set => x.CardSet == set)).ToDictionary(x => x.CardId, x => x.ToCard());
				if(languageTag != "enUS")
				{
					var localized = XmlManager<CardDb>.Load(string.Format("Files/cardDB.{0}.xml", languageTag));
					foreach(var card in localized.Cards)
					{
						Card c;
						if(_cardDb.TryGetValue(card.CardId, out c))
						{
							c.LocalizedName = card.Name;
							c.Text = card.Text;
						}
					}
				}
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error loading db: \n" + e, "Game");
			}
		}

		public static Card GetCardFromId(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return null;
			Card card;
			//_cardDb.TryGetValue(cardId, out card);
			if(_cardDb.TryGetValue(cardId, out card))
				return (Card)card.Clone();
			Logger.WriteLine("Could not find entry in db for cardId: " + cardId, "Game");
			return new Card(cardId, null, "UNKNOWN", "Minion", "UNKNOWN", 0, "UNKNOWN", 0, 1, "", 0, 0, "UNKNOWN", null, 0, "", "");
		}

		public static Card GetCardFromName(string name, bool localized = false)
		{
			var card =
				GetActualCards()
					.FirstOrDefault(c => string.Equals(localized ? c.LocalizedName : c.Name, name, StringComparison.InvariantCultureIgnoreCase));
			if(card != null)
				return (Card)card.Clone();

			//not sure with all the values here
			Logger.WriteLine("Could not get card from name: " + name, "Game");
			return new Card("UNKNOWN", null, "UNKNOWN", "Minion", name, 0, name, 0, 1, "", 0, 0, "UNKNOWN", null, 0, "", "");
		}

		public static List<Card> GetActualCards()
		{
			return (from card in _cardDb.Values
			        where card.Type == "Minion" || card.Type == "Spell" || card.Type == "Weapon"
			        where Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 1))
			        where Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 2))
			        where !CardIds.InvalidCardIds.Any(id => card.Id.Contains(id))
			        select card).ToList();
		}

		public static string GetHeroNameFromId(string id, bool returnIdIfNotFound = true)
		{
			string name;
			if(CardIds.HeroIdDict.TryGetValue(id, out name))
				return name;
			var card = GetCardFromId(id);
			if(card == null || string.IsNullOrEmpty(card.Name) || card.Name == "UNKNOWN" || card.Type != "Hero")
				return returnIdIfNotFound ? id : null;
			return card.Name;
		}

		#endregion
	}
}
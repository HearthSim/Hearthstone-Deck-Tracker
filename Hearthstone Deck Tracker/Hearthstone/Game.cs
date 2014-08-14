using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Stats;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class Game
	{
		public enum CardMark
		{
			None = ' ',
			Coin = 'C',
			Returned = 'R',
			Mulliganed = 'M',
			Stolen = 'S'
		}

		public enum GameMode
		{
			All, //for filtering @ deck stats
			Ranked,
			Casual,
			Arena,
			Friendly,
			Practice,
			None
		}

		#region Properties

		private const int DefaultCoinPosition = 4;
		private const int MaxHandSize = 10;
		public static bool HighlightCardsInHand;
		public static bool HighlightDiscarded;

		private static Dictionary<string, Card> _cardDb;

		public static ObservableCollection<Card> OpponentCards;
		public static int OpponentHandCount;
		public static bool IsInMenu;
		public static bool IsUsingPremade;
		public static int OpponentDeckCount;
		public static bool OpponentHasCoin;
		public static int OpponentSecretCount;
		public static bool IsRunning;
		public static GameMode CurrentGameMode;
		public static GameStats CurrentGameStats;


		public static ObservableCollection<Card> PlayerDeck;
		public static ObservableCollection<Card> PlayerDrawn;
		public static int PlayerHandCount;
		public static string PlayingAgainst;
		public static string PlayingAs;

		public static List<string> SetAsideCards;

		private static readonly List<string> ValidCardSets = new List<string>
			{
				"Basic",
				"Reward",
				"Expert",
				"Promotion",
				"Curse of Naxxramas"
			};

		public static List<Card> DrawnLastGame;

		public static int[] OpponentHandAge { get; private set; }
		public static CardMark[] OpponentHandMarks { get; private set; }

		#endregion

		static Game()
		{
			CurrentGameMode = GameMode.None;
			IsInMenu = true;
			SetAsideCards = new List<string>();
			PlayerDeck = new ObservableCollection<Card>();
			PlayerDrawn = new ObservableCollection<Card>();
			OpponentCards = new ObservableCollection<Card>();
			_cardDb = new Dictionary<string, Card>();
			OpponentHandAge = new int[MaxHandSize];
			OpponentHandMarks = new CardMark[MaxHandSize];
			for(var i = 0; i < MaxHandSize; i++)
			{
				OpponentHandAge[i] = -1;
				OpponentHandMarks[i] = CardMark.None;
			}

			LoadCardDb(Helper.LanguageDict.ContainsValue(Config.Instance.SelectedLanguage)
				           ? Config.Instance.SelectedLanguage
				           : "enUS");
		}

		public static void Reset(bool resetStats = true)
		{
			Logger.WriteLine(">>>>>>>>>>> Reset <<<<<<<<<<<");

			PlayerDrawn.Clear();
			PlayerHandCount = 0;
			OpponentSecretCount = 0;
			OpponentCards.Clear();
			OpponentHandCount = 0;
			OpponentDeckCount = 30;
			OpponentHandAge = new int[MaxHandSize];
			OpponentHandMarks = new CardMark[MaxHandSize];

			for(var i = 0; i < MaxHandSize; i++)
			{
				OpponentHandAge[i] = -1;
				OpponentHandMarks[i] = CardMark.None;
			}

			// Assuming opponent has coin, corrected if we draw it
			OpponentHandMarks[DefaultCoinPosition] = CardMark.Coin;
			OpponentHandAge[DefaultCoinPosition] = 0;
			OpponentHasCoin = true;
			if(!IsInMenu && resetStats)
				CurrentGameStats = new GameStats(GameResult.None, PlayingAgainst);
		}

		public static void SetPremadeDeck(Deck deck)
		{
			PlayerDeck.Clear();
			foreach(var card in deck.Cards)
				PlayerDeck.Add((Card)card.Clone());
			IsUsingPremade = true;
		}

		private static void LogDeckChange(bool opponent, Card card, bool decrease)
		{
			var previous = decrease ? card.Count + 1 : card.Count - 1;

			Logger.WriteLine(
				string.Format("({0} deck) {1} count {2} -> {3}", opponent ? "opponent" : "player", card.Name, previous, card.Count), "Hearthstone");
		}

		private static bool ValidateOpponentHandCount()
		{
			if(OpponentHandCount - 1 < 0 || OpponentHandCount - 1 > 9)
			{
				Logger.WriteLine("ValidateOpponentHandCount failed! OpponentHandCount = " + OpponentHandCount.ToString(), "Hearthstone");
				return false;
			}

			return true;
		}

		private static void LogOpponentHand()
		{
			var zipped = OpponentHandAge.Zip(OpponentHandMarks.Select(mark => (char)mark),
			                                 (age, mark) =>
			                                 string.Format("{0}{1}", (age == -1 ? " " : age.ToString()), mark));

			Logger.WriteLine("Opponent Hand after draw: " + string.Join(",", zipped), "Hearthstone");
		}

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
			drawnCard.JustDrawn();


			var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
			if(deckCard == null)
				return false;
			else
			{
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
						Logger.WriteLine("Removed " + deckCard.Name + " from deck (count 0)");
					}
				}
				else
					deckCard.JustDrawn();

				return true;
			}
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
				Logger.WriteLine("Player got the coin", "Hearthstone");
			}

			var fromSetAside = SetAsideCards.Any(id => cardId == id);
			if(fromSetAside)
			{
				Logger.WriteLine("Got card from setaside: " + cardId);
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
				if(CanRemoveCard(card))
					PlayerDeck.Remove(card);
			}
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
				Logger.WriteLine("Added " + deckCard.Name + " to deck (count was 0)");
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

			var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
			if(deckCard == null)
				return false;
			else
			{
				deckCard.Count--;
				LogDeckChange(false, deckCard, true);
				if(CanRemoveCard(deckCard))
					PlayerDeck.Remove(deckCard);

				return true;
			}
		}

		#endregion

		#region Opponent

		public static void OpponentDraw(int turn)
		{
			OpponentHandCount++;
			OpponentDeckCount--;

			if(!ValidateOpponentHandCount())
				return;
			else if(OpponentHandAge[OpponentHandCount - 1] != -1)
				Logger.WriteLine(string.Format("Card {0} is already set to {1}", OpponentHandCount - 1, OpponentHandAge[OpponentHandCount - 1]), "Hearthstone");
			else
			{
				Logger.WriteLine(string.Format("Set card {0} to age {1}", OpponentHandCount - 1, turn), "Hearthstone");

				OpponentHandAge[OpponentHandCount - 1] = turn;
				OpponentHandMarks[OpponentHandCount - 1] = CardMark.None;

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
				var stolen = from != -1 && OpponentHandMarks[from - 1] == CardMark.Stolen;
				var card = OpponentCards.FirstOrDefault(c => c.Id == id && c.IsStolen == stolen && !c.WasDiscarded);

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
					Logger.WriteLine("Opponent played stolen card from " + from);
			}

			for(var i = from - 1; i < MaxHandSize - 1; i++)
			{
				OpponentHandAge[i] = OpponentHandAge[i + 1];
				OpponentHandMarks[i] = OpponentHandMarks[i + 1];
			}

			OpponentHandAge[MaxHandSize - 1] = -1;
			OpponentHandMarks[MaxHandSize - 1] = CardMark.None;

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


		public static void OpponentBackToHand(string cardId, int turn)
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
			}
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

		public static void OpponentGet(int turn)
		{
			OpponentHandCount++;

			if(!ValidateOpponentHandCount())
				return;

			OpponentHandAge[OpponentHandCount - 1] = turn;

			if(OpponentHandMarks[OpponentHandCount - 1] != CardMark.Coin)
				OpponentHandMarks[OpponentHandCount - 1] = CardMark.Stolen;

			LogOpponentHand();
		}

		#endregion

		#region Database

		private static void LoadCardDb(string languageTag)
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
							if(!ValidCardSets.Any(cs => cs.Equals(cardType.Key))) continue;
							foreach(var card in cardType.Value)
							{
								var tmp = JsonConvert.DeserializeObject<Card>(card.ToString());
								localizedCards.Add(tmp.Id, tmp);
							}
						}
					}
				}


				//load engish db (needed for importing, etc)
				var fileEng = "Files/cardsDB.enUS.json";
				var tempDb = new Dictionary<string, Card>();
				if(File.Exists(fileEng))
				{
					var obj = JObject.Parse(File.ReadAllText(fileEng));
					foreach(var cardType in obj)
					{
						if(!ValidCardSets.Any(cs => cs.Equals(cardType.Key))) continue;
						;
						foreach(var card in cardType.Value)
						{
							var tmp = JsonConvert.DeserializeObject<Card>(card.ToString());
							if(languageTag != "enUS")
							{
								var localizedCard = localizedCards[tmp.Id];
								tmp.LocalizedName = localizedCard.Name;
								tmp.Text = localizedCard.Text;
							}
							tempDb.Add(tmp.Id, tmp);
						}
					}
				}
				_cardDb = new Dictionary<string, Card>(tempDb);

				Logger.WriteLine("Done loading card database (" + languageTag + ")", "Hearthstone");
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error loading db: \n" + e);
			}
		}

		public static Card GetCardFromId(string cardId)
		{
			if(string.IsNullOrEmpty(cardId)) return null;
			if(_cardDb.ContainsKey(cardId))
				return (Card)_cardDb[cardId].Clone();
			Logger.WriteLine("Could not find entry in db for cardId: " + cardId);
			return new Card(cardId, null, "UNKNOWN", "Minion", "UNKNOWN", 0, "UNKNOWN", 0, 1, "", 0, 0, "UNKNOWN", 0);
		}

		public static Card GetCardFromName(string name)
		{
			if(GetActualCards().Any(c => c.Name.Equals(name)))
			{
				return
					(Card)
					GetActualCards().FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Clone();
			}

			//not sure with all the values here
			Logger.WriteLine("Could not get card from name: " + name);
			return new Card("UNKNOWN", null, "UNKNOWN", "Minion", name, 0, name, 0, 1, "", 0, 0, "UNKNOWN", 0);
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

		#endregion

		public static void AddPlayToCurrentGame(PlayType play, int turn, string cardId)
		{
			if(CurrentGameStats == null) return;
			CurrentGameStats.AddPlay(play, turn, cardId);
		}

		public static bool IsActualCard(Card card)
		{
			return (card.Type == "Minion"
			        || card.Type == "Spell"
			        || card.Type == "Weapon")
			       && Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 1))
			       && Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 2))
			       && !CardIds.InvalidCardIds.Any(id => card.Id.Contains(id));
		}
	}
}
#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameV2 : IGame
	{
        private static List<string> hsLogLines = new List<string>();
        private static readonly List<string> InValidCardSets = new List<string>
        {
            "Credits",
            "Missions",
            "Debug",
            "System"
        };
        
        private static Dictionary<string, Card> _cardDb;

	    static GameV2()
	    {
            _cardDb = new Dictionary<string, Card>();
            LoadCardDb(Helper.LanguageDict.ContainsValue(Config.Instance.SelectedLanguage) ? Config.Instance.SelectedLanguage : "enUS");
        }

		public GameV2()
		{
            Entities = new Dictionary<int, Entity>();
			CurrentGameMode = GameMode.None;
			IsInMenu = true;
			SetAsideCards = new List<string>();
			OpponentReturnedToDeck = new List<KeyValuePair<string, int>>();
			PlayerDeck = new ObservableCollection<Card>();
			PlayerDrawn = new ObservableCollection<Card>();
			PlayerDrawnIdsTotal = new ObservableCollection<string>();
			OpponentCards = new ObservableCollection<Card>();
			PossibleArenaCards = new List<Card>();
			PossibleConstructedCards = new List<Card>();
			OpponentHandAge = new int[MaxHandSize];
			OpponentHandMarks = new CardMark[MaxHandSize];
			OpponentStolenCardsInformation = new Card[MaxHandSize];
			OpponentSecrets = new OpponentSecrets();
			for(var i = 0; i < MaxHandSize; i++)
			{
				OpponentHandAge[i] = -1;
				OpponentHandMarks[i] = CardMark.None;
			}

			
		}

		public bool IsMulliganDone
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

		public int PlayerDeckSize { get; set; }
		public bool NoMatchingDeck { get; set; }

		public void Reset(bool resetStats = true)
		{
			Logger.WriteLine(">>>>>>>>>>> Reset <<<<<<<<<<<", "Game");

			ReplayMaker.Reset();
			PlayerDrawn.Clear();
			PlayerDrawnIdsTotal.Clear();
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
			_playingAs = null;

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

		public void SetPremadeDeck(Deck deck)
		{
			PlayerDeck.Clear();
			foreach(var card in deck.GetSelectedDeckVersion().Cards)
				PlayerDeck.Add((Card)card.Clone());
			IsUsingPremade = true;
		}

		private void LogDeckChange(bool opponent, Card card, bool decrease)
		{
			var previous = decrease ? card.Count + 1 : card.Count - 1;

			Logger.WriteLine(
			                 string.Format("({0} deck) {1} count {2} -> {3}", opponent ? "opponent" : "player", card.Name, previous, card.Count),
			                 "Hearthstone");
		}

		private bool ValidateOpponentHandCount()
		{
			if(OpponentHandCount - 1 < 0 || OpponentHandCount - 1 > 9)
			{
				Logger.WriteLine("ValidateOpponentHandCount failed! OpponentHandCount = " + OpponentHandCount, "Hearthstone");
				return false;
			}

			return true;
		}

		private void LogOpponentHand()
		{
			var zipped = OpponentHandAge.Zip(OpponentHandMarks.Select(mark => (char)mark),
			                                 (age, mark) => string.Format("{0}{1}", (age == -1 ? " " : age.ToString()), mark));

			Logger.WriteLine("Opponent Hand after draw: " + string.Join(",", zipped), "Hearthstone");
		}

		public void AddPlayToCurrentGame(PlayType play, int turn, string cardId)
		{
			if(CurrentGameStats == null)
				return;
			CurrentGameStats.AddPlay(play, turn, cardId);
		}

		public static bool IsActualCard(Card card)
		{
            if (card == null)
                return false;
            return (card.Type == "Minion" || card.Type == "Spell" || card.Type == "Weapon")
                   && (Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 1)) || card.Id == "AT_063t")
                   && Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 2))
                   && !CardIds.InvalidCardIds.Any(id => card.Id.Contains(id));
        }

		public void ResetArenaCards()
		{
			PossibleArenaCards.Clear();
		}

		public void ResetConstructedCards()
		{
			PossibleConstructedCards.Clear();
		}

		public static void AddHSLogLine(string logLine)
		{
			HSLogLines.Add(logLine);
		}

		//public readonly string[] Classes = new[] { "Druid", "Hunter", "Mage", "Priest", "Paladin", "Shaman", "Rogue", "Warlock", "Warrior" };

		#region Properties

		private const int DefaultCoinPosition = 4;
		private const int MaxHandSize = 10;
		public bool HighlightCardsInHand { get; set; }
		public bool HighlightDiscarded { get; set; }

		public ObservableCollection<Card> OpponentCards { get; set; }
		public int OpponentHandCount { get; set; }
		public int OpponentFatigueCount { get; set; }
		public bool IsInMenu { get; set; }
		public bool IsUsingPremade { get; set; }
		public int OpponentDeckCount { get; set; }
		public bool OpponentHasCoin { get; set; }
		public int OpponentSecretCount { get; set; }
		public bool IsRunning { get; set; }
		public Region CurrentRegion { get; set; }

		private GameMode _currentGameMode;

		public GameMode CurrentGameMode
		{
			get { return _currentGameMode; }
			set
			{
				_currentGameMode = value;
				Logger.WriteLine("set CurrentGameMode to " + value, "Game");
			}
		}

		public GameStats CurrentGameStats { get; set; }


		public ObservableCollection<Card> PlayerDeck { get; set; }
		public ObservableCollection<Card> PlayerDrawn { get; set; }
		public ObservableCollection<string> PlayerDrawnIdsTotal { get; set; }
		public int PlayerHandCount { get; set; }
		public int PlayerFatigueCount { get; set; }
		public string PlayingAgainst { get; set; }

		private string _playingAs;
		public string PlayingAs
		{
			get
			{
				if(string.IsNullOrEmpty(_playingAs))
				{
					var pEntity = Entities.Values.FirstOrDefault(e => e.GetTag(GAME_TAG.PLAYER_ID) == PlayerId);
					if(pEntity != null)
					{
						var hEntityId = pEntity.GetTag(GAME_TAG.HERO_ENTITY);
						Entity hEntity;
						if(Entities.TryGetValue(hEntityId, out hEntity))
						{
							_playingAs = GetHeroNameFromId(hEntity.CardId);
						}
					}
				}
				return _playingAs;
			}
			
		}
		public string PlayerName { get; set; }
		public string OpponentName { get; set; }

		public List<string> SetAsideCards { get; set; }
		public List<KeyValuePair<string, int>> OpponentReturnedToDeck { get; set; }

		public OpponentSecrets OpponentSecrets { get; set; }
        
		public List<Card> DrawnLastGame { get; set; }

		public int[] OpponentHandAge { get; private set; }
		public CardMark[] OpponentHandMarks { get; private set; }
		public Card[] OpponentStolenCardsInformation { get; private set; }
		public List<Card> PossibleArenaCards { get; set; }
		public List<Card> PossibleConstructedCards { get; set; }
		public int? SecondToLastUsedId { get; set; }

		//public List<Entity> Entities;
		public Dictionary<int, Entity> Entities { get; set; }
		public int PlayerId { get; set; }
		public int OpponentId { get; set; }
		public bool SavedReplay { get; set; }
		//public Dictionary<string, int> PlayerIds; 

		#endregion

		#region Player

#pragma warning disable 4014
		public async Task<bool> PlayerDraw(string cardId)
		{
			if(string.IsNullOrEmpty(_playingAs))
			{
				//Make sure the value get's cached as early as possible to avoid problems
				//at the end of a game, in case the hero changes - e.g. to Jaraxxus.
				PlayingAs.GetType();
			}
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
				PlayerDrawnIdsTotal.Add(cardId);
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
		public void PlayerGet(string cardId, bool fromPlay, int turn)
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
		public void PlayerPlayed(string cardId)
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

		private bool CanRemoveCard(Card card)
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


		public void PlayerMulligan(string cardId)
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

		public void PlayerHandDiscard(string cardId)
		{
			PlayerPlayed(cardId);
		}

		public bool PlayerDeckDiscard(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return true;

			var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId);
			if(drawnCard == null)
			{
				PlayerDrawn.Add(GetCardFromId(cardId));
				PlayerDrawnIdsTotal.Add(cardId);
            }
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

		public void PlayerPlayToDeck(string cardId)
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


		public void PlayerGetToDeck(string cardId, int turn)
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

		public void OpponentGetToDeck(int turn)
		{
			OpponentDeckCount++;
		}

		public void OpponentDraw(int turn)
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

		public void OpponentJoustReveal(string cardId)
		{
			if(string.IsNullOrEmpty(cardId) || OpponentCards.Any(c => c.Id == cardId && c.Jousted))
				return;
			var card = GetCardFromId(cardId);
			card.Jousted = true;
			OpponentCards.Add(card);
		}

		public void OpponentPlay(string id, int from, int turn)
		{
			OpponentHandCount--;

			if(id == "GAME_005")
				OpponentHasCoin = false;
			if(!string.IsNullOrEmpty(id))
			{
				var jousted = OpponentCards.FirstOrDefault(c => c.Id == id && c.Jousted);
				if(jousted != null)
					OpponentCards.Remove(jousted);
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

		public void OpponentMulligan(int pos)
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


		public void OpponentBackToHand(string cardId, int turn, int id)
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

		public void OpponentPlayToDeck(string cardId, int turn)
		{
			OpponentDeckCount++;

			if(string.IsNullOrEmpty(cardId))
				return;

			OpponentReturnedToDeck.Add(new KeyValuePair<string, int>(cardId, turn));
		}

		public void OpponentDeckDiscard(string cardId)
		{
			OpponentDeckCount--;
			if(string.IsNullOrEmpty(cardId))
				return;

			var jousted = OpponentCards.FirstOrDefault(c => c.Id == cardId && c.Jousted);
			if(jousted != null)
				OpponentCards.Remove(jousted);

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

		public void OpponentSecretTriggered(string cardId)
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

		public void OpponentGet(int turn, int id)
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

		/*private void LoadCardDb(string languageTag)
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
				_cardDb = db.Cards.Where(x => InValidCardSets.All(set => x.CardSet != set)).ToDictionary(x => x.CardId, x => x.ToCard());
				if(languageTag != "enUS")
				{
					var localized = XmlManager<CardDb>.Load(string.Format("Files/cardDB.{0}.xml", languageTag));
					foreach(var card in localized.Cards)
					{
						Card c;
						if(_cardDb.TryGetValue(card.CardId, out c))
						{
							c.LocalizedName = card.Name;
							c.EnglishText = c.Text;
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
			return new Card(cardId, null, "UNKNOWN", "Minion", "UNKNOWN", 0, "UNKNOWN", 0, 1, "", "", 0, 0, "UNKNOWN", null, 0, "", "");
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
			return new Card("UNKNOWN", null, "UNKNOWN", "Minion", name, 0, name, 0, 1, "", "", 0, 0, "UNKNOWN", null, 0, "", "");
		}

		public static List<Card> GetActualCards()
		{
            return (from card in _cardDb.Values
                    where card.Type == "Minion" || card.Type == "Spell" || card.Type == "Weapon"
                    where Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 1)) || card.Id == "AT_063t"
                    where Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 2))
                    where !CardIds.InvalidCardIds.Any(id => card.Id.Contains(id))
                    select card).ToList();
        }

		public static string GetHeroNameFromId(string id, bool returnIdIfNotFound = true)
		{
			string name;
			var match = Regex.Match(id, @"(?<base>(.*_\d+)).*");
			if(match.Success)
				id = match.Groups["base"].Value;
			if(CardIds.HeroIdDict.TryGetValue(id, out name))
				return name;
			var card = GetCardFromId(id);
			if(card == null || string.IsNullOrEmpty(card.Name) || card.Name == "UNKNOWN" || card.Type != "Hero")
				return returnIdIfNotFound ? id : null;
			return card.Name;
		}

		#endregion

		public Deck TempArenaDeck;
		public readonly List<Deck> DiscardedArenaDecks = new List<Deck>();

		public void NewArenaDeck(string heroId)
		{
			TempArenaDeck = new Deck
			{
				Name = Helper.ParseDeckNameTemplate(Config.Instance.ArenaDeckNameTemplate),
				IsArenaDeck = true,
				Class = GetHeroNameFromId(heroId)
			};
			Logger.WriteLine("Created new arena deck: " + TempArenaDeck.Class);
		}

		public async void NewArenaCard(string cardId)
		{
			if(TempArenaDeck == null || string.IsNullOrEmpty(cardId))
				return;
			var existingCard = TempArenaDeck.Cards.FirstOrDefault(c => c.Id == cardId);
			if(existingCard != null)
				existingCard.Count++;
			else
				TempArenaDeck.Cards.Add((Card)GetCardFromId(cardId).Clone());
			var numCards = TempArenaDeck.Cards.Sum(c => c.Count);
			Logger.WriteLine(string.Format("Added new card to arena deck: {0} ({1}/30)", cardId, numCards));
			if(numCards == 30)
			{
				Logger.WriteLine("Found complete arena deck!");
				if(!Config.Instance.SelectedArenaImportingBehaviour.HasValue)
				{
					Logger.WriteLine("...but we are using the old importing method.");
					return;
				}
				var recentArenaDecks = DeckList.Instance.Decks.Where(d => d.IsArenaDeck).OrderByDescending(d => d.LastPlayedNewFirst).Take(15);
				if(recentArenaDecks.Any(d => d.Cards.All(c => TempArenaDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count))))
				{
					Logger.WriteLine("...but we already have that one. Discarding.");
					TempArenaDeck = null;
					return;
				}
				if(DiscardedArenaDecks.Any(d => d.Cards.All(c => TempArenaDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count))))
				{
					Logger.WriteLine("...but it was already discarded by the user. No automatic action taken.");
					return;
				}
				if(Config.Instance.SelectedArenaImportingBehaviour.Value == ArenaImportingBehaviour.AutoImportSave)
				{
					Logger.WriteLine("...auto saving new arena deck.");
					Helper.MainWindow.SetNewDeck(TempArenaDeck);
					Helper.MainWindow.SaveDeck(false, TempArenaDeck.Version);
					TempArenaDeck = null;
				}
				else if(Config.Instance.SelectedArenaImportingBehaviour.Value == ArenaImportingBehaviour.AutoAsk)
				{
                    var result =
                        await
                        Helper.MainWindow.ShowMessageAsync("New arena deck detected!", "You can change this behaviour to \"auto save&import\" or \"manual\" in [options > tracker > importing]", MessageDialogStyle.AffirmativeAndNegative,
                                                           new MetroDialogSettings { AffirmativeButtonText = "import", NegativeButtonText = "cancel" });

                    if (result == MessageDialogResult.Affirmative)
					{
						Logger.WriteLine("...saving new arena deck.");
						Helper.MainWindow.SetNewDeck(TempArenaDeck);
						Helper.MainWindow.ActivateWindow();
						TempArenaDeck = null;
					}
					else
					{
						Logger.WriteLine("...discarded by user.");
						DiscardedArenaDecks.Add(TempArenaDeck);
						TempArenaDeck = null;
					}
				}
			}
		}
	}
}
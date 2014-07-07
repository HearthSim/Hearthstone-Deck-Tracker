#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Hearthstone_Deck_Tracker
{
    public class Hearthstone
    {
        public enum CardMark
        {
            None = ' ',
            Coin = 'C',
            Returned = 'R',
            Mulliganed = 'M',
            Stolen = 'S'
        }

        private const int DefaultCoinPosition = 4;
        private const int MaxHandSize = 10;
        public static bool HighlightCardsInHand;

        private static Dictionary<string, Card> _cardDb;

        private readonly List<string> _invalidCardIds = new List<string>
            {
                "EX1_tk34",
                "EX1_tk29",
                "EX1_tk28",
                "EX1_tk11",
                "EX1_598",
                "NEW1_032",
                "NEW1_033",
                "NEW1_034",
                "NEW1_009",
                "CS2_052",
                "CS2_082",
                "CS2_051",
                "CS2_050",
                "CS2_152",
                "skele11",
                "skele21",
                "GAME",
                "DREAM",
                "NEW1_006",
            };

        public ObservableCollection<Card> EnemyCards;
        public int EnemyHandCount;
        public bool IsInMenu;
        public bool IsUsingPremade;
        public int OpponentDeckCount;
        public bool OpponentHasCoin;
        public ObservableCollection<Card> PlayerDeck;
        public ObservableCollection<Card> PlayerDrawn;
        public int PlayerHandCount;
        public string PlayingAgainst;
        public string PlayingAs;

        public Hearthstone(string languageTag)
        {
            IsInMenu = true;
            PlayerDeck = new ObservableCollection<Card>();
            PlayerDrawn = new ObservableCollection<Card>();
            EnemyCards = new ObservableCollection<Card>();
            _cardDb = new Dictionary<string, Card>();
            OpponentHandAge = new int[MaxHandSize];
            OpponentHandMarks = new CardMark[MaxHandSize];
            for (int i = 0; i < MaxHandSize; i++)
            {
                OpponentHandAge[i] = -1;
                OpponentHandMarks[i] = CardMark.None;
            }

            LoadCardDb(languageTag);
        }

        public int[] OpponentHandAge { get; private set; }
        public CardMark[] OpponentHandMarks { get; private set; }

        private void LoadCardDb(string languageTag)
        {
            try
            {
                var localizedCardNames = new Dictionary<string, string>();
                if (languageTag != "enUS")
                {
                    var file = string.Format("Files/cardsDB.{0}.json", languageTag);
                    if (File.Exists(file))
                    {
                        var localized = JObject.Parse(File.ReadAllText(file));
                        foreach (var cardType in localized)
                        {
                            if (cardType.Key != "Basic" && cardType.Key != "Expert" && cardType.Key != "Promotion" &&
                                cardType.Key != "Reward") continue;
                            foreach (var card in cardType.Value)
                            {
                                var tmp = JsonConvert.DeserializeObject<Card>(card.ToString());
                                localizedCardNames.Add(tmp.Id, tmp.Name);
                            }
                        }
                    }
                }


                //load engish db (needed for importing, etc)
                var fileEng = "Files/cardsDB.enUS.json";
                var tempDb = new Dictionary<string, Card>();
                if (File.Exists(fileEng))
                {
                    var obj = JObject.Parse(File.ReadAllText(fileEng));
                    foreach (var cardType in obj)
                    {
                        if (cardType.Key != "Basic" && cardType.Key != "Expert" && cardType.Key != "Promotion" &&
                            cardType.Key != "Reward") continue;
                        foreach (var card in cardType.Value)
                        {
                            var tmp = JsonConvert.DeserializeObject<Card>(card.ToString());
                            if (languageTag != "enUS")
                            {
                                tmp.LocalizedName = localizedCardNames[tmp.Id];
                            }
                            tempDb.Add(tmp.Id, tmp);
                        }
                    }
                }
                _cardDb = new Dictionary<string, Card>(tempDb);

                Logger.WriteLine("Done loading card database (" + languageTag + ")", "Hearthstone");
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error loading db: \n" + e);
            }
        }

        public static Card GetCardFromId(string cardId)
        {
            if (cardId == "") return new Card();
            if (_cardDb.ContainsKey(cardId))
            {
                return (Card) _cardDb[cardId].Clone();
            }
            Logger.WriteLine("Could not find entry in db for cardId: " + cardId);
            return new Card(cardId, null, "UNKNOWN", "Minion", "UNKNOWN", 0, "UNKNOWN", 0, 1);
        }

        public Card GetCardFromName(string name)
        {
            if (GetActualCards().Any(c => c.Name.Equals(name)))
            {
                return (Card) GetActualCards().FirstOrDefault(c => c.Name.ToLower() == name.ToLower()).Clone();
            }

            //not sure with all the values here
            Logger.WriteLine("Could not get card from name: " + name);
            return new Card("UNKNOWN", null, "UNKNOWN", "Minion", name, 0, name, 0, 1);
        }

        public List<Card> GetActualCards()
        {
            return (from card in _cardDb.Values
                    where card.Type == "Minion" || card.Type == "Spell" || card.Type == "Weapon"
                    where Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 1))
                    where Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 2))
                    where !_invalidCardIds.Any(id => card.Id.Contains(id))
                    select card).ToList();
        }

        public void SetPremadeDeck(Deck deck)
        {
            PlayerDeck.Clear();
            foreach (var card in deck.Cards)
            {
                PlayerDeck.Add(card);
            }
            IsUsingPremade = true;
        }

        public bool PlayerDraw(string cardId)
        {
            PlayerHandCount++;

            var card = GetCardFromId(cardId);

            if (PlayerDrawn.Contains(card))
            {
                PlayerDrawn.Remove(card);
                card.Count++;
            }
            PlayerDrawn.Add(card);

            if (PlayerDeck.Contains(card))
            {
                var deckCard = PlayerDeck.First(c => c.Equals(card));
                PlayerDeck.Remove(deckCard);
                deckCard.Count--;
                deckCard.InHandCount++;
                PlayerDeck.Add(deckCard);
                LogDeckChange(false, deckCard, true);
            }
            else
            {
                return false;
            }
            return true;
        }

        public void PlayerGet(string cardId)
        {
            if (cardId == "GAME_005")
            {
                OpponentHasCoin = false;
                OpponentHandMarks[DefaultCoinPosition] = CardMark.None;
                OpponentHandAge[DefaultCoinPosition] = -1;
                Logger.WriteLine("Player got the coin", "Hearthstone");
            }

            PlayerHandCount++;
            if (PlayerDeck.Any(c => c.Id == cardId))
            {
                var card = PlayerDeck.First(c => c.Id == cardId);
                PlayerDeck.Remove(card);
                card.InHandCount++;
                PlayerDeck.Add(card);
            }
        }

        public void PlayerPlayed(string cardId)
        {
            PlayerHandCount--;
            if (PlayerDeck.Any(c => c.Id == cardId))
            {
                var card = PlayerDeck.First(c => c.Id == cardId);
                PlayerDeck.Remove(card);
                card.InHandCount--;
                PlayerDeck.Add(card);
            }
        }

        public void Mulligan(string cardId)
        {
            PlayerHandCount--;

            Card card = GetCardFromId(cardId);

            if (PlayerDrawn.Any(c => c.Equals(card)))
            {
                var drawnCard = PlayerDrawn.First(c => c.Equals(card));
                PlayerDrawn.Remove(drawnCard);
                if (drawnCard.Count > 1)
                {
                    drawnCard.Count--;
                    PlayerDrawn.Add(drawnCard);
                }
            }
            if (PlayerDeck.Any(c => c.Equals(card)))
            {
                var deckCard = PlayerDeck.First(c => c.Equals(card));
                PlayerDeck.Remove(deckCard);
                deckCard.Count++;
                deckCard.InHandCount--;
                PlayerDeck.Add(deckCard);
                LogDeckChange(false, deckCard, false);
            }
        }

        public void OpponentMulligan(int pos)
        {
            EnemyHandCount--;
            OpponentDeckCount++;
            OpponentHandMarks[pos - 1] = CardMark.Mulliganed;
            if (EnemyHandCount < OpponentHandAge.Count(x => x != -1))
            {
                OpponentHandAge[EnemyHandCount] = -1;
                Logger.WriteLine(string.Format("Fixed hand ages after mulligan (removed {0})", EnemyHandCount), "Hearthstone");
                LogOpponentHand();
            }
        }

        public void PlayerHandDiscard(string cardId)
        {
            PlayerHandCount--;
            if (PlayerDeck.Any(c => c.Id == cardId))
            {
                var card = PlayerDeck.First(c => c.Id == cardId);
                PlayerDeck.Remove(card);
                card.InHandCount--;
                PlayerDeck.Add(card);
            }
        }

        public bool PlayerDeckDiscard(string cardId)
        {
            Card card = GetCardFromId(cardId);

            if (PlayerDrawn.Contains(card))
            {
                PlayerDrawn.Remove(card);
                card.Count++;
            }

            PlayerDrawn.Add(card);

            if (PlayerDeck.Contains(card))
            {
                var deckCard = PlayerDeck.First(c => c.Equals(card));
                PlayerDeck.Remove(deckCard);
                deckCard.Count--;
                PlayerDeck.Add(deckCard);
                LogDeckChange(false, deckCard, true);
            }
            else
            {
                return false;
            }
            return true;
        }

        public void OpponentBackToHand(string cardId, int turn)
        {
            if (EnemyCards.Any(c => c.Id == cardId))
            {
                var card = EnemyCards.First(c => c.Id == cardId);
                EnemyCards.Remove(card);
                card.Count--;
                if (card.Count > 0)
                {
                    EnemyCards.Add(card);
                }
                LogDeckChange(true, card, true);
            }

            EnemyHandCount++;

            if (!ValidateEnemyHandCount())
                return;

            OpponentHandAge[EnemyHandCount - 1] = turn;
            OpponentHandMarks[EnemyHandCount - 1] = CardMark.Returned;
        }

        public void OpponentDeckDiscard(string cardId)
        {
            OpponentDeckCount--;
            if (string.IsNullOrEmpty(cardId))
                return;

            var card = GetCardFromId(cardId);
            if (EnemyCards.Contains(card))
            {
                EnemyCards.Remove(card);
                card.Count++;
            }

            EnemyCards.Add(card);
            LogDeckChange(true, card, false);
        }

        public void OpponentSecretTriggered(string cardId)
        {
            if (cardId == "")
                return;

            Card card = GetCardFromId(cardId);
            if (EnemyCards.Contains(card))
            {
                EnemyCards.Remove(card);
                card.Count++;
            }

            EnemyCards.Add(card);

            LogDeckChange(true, card, false);
        }

        internal void OpponentGet(int turn)
        {
            EnemyHandCount++;

            if (!ValidateEnemyHandCount())
                return;

            OpponentHandAge[EnemyHandCount - 1] = turn;

            if (OpponentHandMarks[EnemyHandCount - 1] != CardMark.Coin)
                OpponentHandMarks[EnemyHandCount - 1] = CardMark.Stolen;

            LogOpponentHand();
        }

        internal void Reset()
        {
            Logger.WriteLine(">>>>>>>>>>> Reset <<<<<<<<<<<");

            PlayerDrawn.Clear();
            PlayerHandCount = 0;
            EnemyCards.Clear();
            EnemyHandCount = 0;
            OpponentDeckCount = 30;
            OpponentHandAge = new int[MaxHandSize];
            OpponentHandMarks = new CardMark[MaxHandSize];

            for (int i = 0; i < MaxHandSize; i++)
            {
                OpponentHandAge[i] = -1;
                OpponentHandMarks[i] = CardMark.None;
            }

            // Assuming opponent has coin, corrected if we draw it
            OpponentHandMarks[DefaultCoinPosition] = CardMark.Coin;
            OpponentHandAge[DefaultCoinPosition] = 0;
            OpponentHasCoin = true;
        }

        public void OpponentDraw(CardPosChangeArgs args)
        {
            EnemyHandCount++;
            OpponentDeckCount--;

            if (!ValidateEnemyHandCount())
                return;

            if (OpponentHandAge[EnemyHandCount - 1] != -1)
            {
                Logger.WriteLine(string.Format("Card {0} is already set to {1}", EnemyHandCount - 1,
                                              OpponentHandAge[EnemyHandCount - 1]), "Hearthstone");

                return;
            }

            Logger.WriteLine(string.Format("Set card {0} to age {1}", EnemyHandCount - 1, args.Turn), "Hearthstone");

            OpponentHandAge[EnemyHandCount - 1] = args.Turn;
            OpponentHandMarks[EnemyHandCount - 1] = CardMark.None;

            LogOpponentHand();
        }

        public void OpponentPlay(CardPosChangeArgs args)
        {
            EnemyHandCount--;

            if (args.Id == "GAME_005")
            {
                OpponentHasCoin = false;
            }
            if (!string.IsNullOrEmpty(args.Id))
            {
                Card card = GetCardFromId(args.Id);

                if (args.From != -1 && OpponentHandMarks[args.From - 1] == CardMark.Stolen)
                    card.IsStolen = true;

                if (EnemyCards.Any(x => x.Equals(card) && x.IsStolen == card.IsStolen))
                {
                    card = EnemyCards.First(x => x.Equals(card) && x.IsStolen == card.IsStolen);
                    EnemyCards.Remove(card);
                    card.Count++;
                }
                EnemyCards.Add(card);
                LogDeckChange(true, card, false);

                if (card.IsStolen)
                    Logger.WriteLine("Opponent played stolen card from " + args.From);
            }
            
            for (int i = args.From - 1; i < MaxHandSize - 1; i++)
            {
                OpponentHandAge[i] = OpponentHandAge[i + 1];
                OpponentHandMarks[i] = OpponentHandMarks[i + 1];
            }

            OpponentHandAge[MaxHandSize - 1] = -1;
            OpponentHandMarks[MaxHandSize - 1] = CardMark.None;

            LogOpponentHand();
        }

        private static void LogDeckChange(bool opponent, Card card, bool decrease)
        {
            int previous = decrease ? card.Count + 1 : card.Count - 1;

            Logger.WriteLine(string.Format("({0} deck) {1} count {2} -> {3}", opponent?"opponent":"player", card.Name, previous, card.Count),
                             "Hearthstone");
        }

        private bool ValidateEnemyHandCount()
        {
            if (EnemyHandCount - 1 < 0 || EnemyHandCount - 1 > 9)
            {
                Logger.WriteLine("ValidateEnemyHandCount failed! EnemyHandCount = " + EnemyHandCount.ToString(), "Hearthstone");
                return false;
            }

            return true;
        }

        private void LogOpponentHand()
        {
            var zipped = OpponentHandAge.Zip(OpponentHandMarks.Select(mark => (char) mark),
                                             (age, mark) =>
                                             string.Format("{0}{1}", (age == -1 ? " " : age.ToString()), mark));

            Logger.WriteLine("Opponent Hand after draw: " + string.Join(",", zipped), "Hearthstone");
        }
    }
}
#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
    public class Game
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

        public ObservableCollection<Card> OpponentCards;
        public int OpponentHandCount;
        public bool IsInMenu;
        public bool IsUsingPremade;
        public int OpponentDeckCount;
        public bool OpponentHasCoin;
        public ObservableCollection<Card> PlayerDeck;
        public ObservableCollection<Card> PlayerDrawn;
        public int PlayerHandCount;
        public string PlayingAgainst;
        public string PlayingAs;

        public Game(string languageTag)
        {
            IsInMenu = true;
            PlayerDeck = new ObservableCollection<Card>();
            PlayerDrawn = new ObservableCollection<Card>();
            OpponentCards = new ObservableCollection<Card>();
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
                var localizedCards = new Dictionary<string, Card>();
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
                                localizedCards.Add(tmp.Id, tmp);
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
            catch (Exception e)
            {
                Logger.WriteLine("Error loading db: \n" + e);
            }
        }

        public static Card GetCardFromId(string cardId)
        {
            if (cardId == null) return null;
            if (cardId == "") return new Card();
            if (_cardDb.ContainsKey(cardId))
            {
                return (Card) _cardDb[cardId].Clone();
            }
            Logger.WriteLine("Could not find entry in db for cardId: " + cardId);
            return new Card(cardId, null, "UNKNOWN", "Minion", "UNKNOWN", 0, "UNKNOWN", 0, 1, "", 0, 0, "UNKNOWN", 0);
        }

        public Card GetCardFromName(string name)
        {
            if (GetActualCards().Any(c => c.Name.Equals(name)))
            {
                return (Card) GetActualCards().FirstOrDefault(c => c.Name.ToLower() == name.ToLower()).Clone();
            }

            //not sure with all the values here
            Logger.WriteLine("Could not get card from name: " + name);
            return new Card("UNKNOWN", null, "UNKNOWN", "Minion", name, 0, name, 0, 1, "", 0, 0, "UNKNOWN", 0);
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
                PlayerDeck.Add((Card)card.Clone());
            }
            IsUsingPremade = true;
        }

        public bool PlayerDraw(string cardId)
        {
            PlayerHandCount++;

            if (string.IsNullOrEmpty(cardId))
                return true;

            var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId);
            if (drawnCard != null)
            {
                drawnCard.Count++;
            }
            else
            {
                PlayerDrawn.Add(GetCardFromId(cardId));
            }

            var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
            if (deckCard != null)
            {
                deckCard.Count--;
                deckCard.InHandCount++;
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
            var card = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
            if (card != null)
                card.InHandCount++;
        }

        public void PlayerPlayed(string cardId)
        {
            PlayerHandCount--;

            var card = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
            if (card != null)
                card.InHandCount--;
        }

        public void Mulligan(string cardId)
        {
            PlayerHandCount--;

            if (string.IsNullOrEmpty(cardId))
                return;

            var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId);
            if (drawnCard != null)
            {
                drawnCard.Count--;
                if (drawnCard.Count < 1)
                    PlayerDrawn.Remove(drawnCard);
            }

            var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
            if(deckCard != null)
            {
                deckCard.Count++;
                deckCard.InHandCount--;
                LogDeckChange(false, deckCard, false);
            }
        }

        public void OpponentMulligan(int pos)
        {
            OpponentHandCount--;
            OpponentDeckCount++;
            OpponentHandMarks[pos - 1] = CardMark.Mulliganed;
            if (OpponentHandCount < OpponentHandAge.Count(x => x != -1))
            {
                OpponentHandAge[OpponentHandCount] = -1;
                Logger.WriteLine(string.Format("Fixed hand ages after mulligan (removed {0})", OpponentHandCount), "Hearthstone");
                LogOpponentHand();
            }
        }

        public void PlayerHandDiscard(string cardId)
        {
            PlayerPlayed(cardId);
        }

        public bool PlayerDeckDiscard(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
                return true;

            var drawnCard = PlayerDrawn.FirstOrDefault(c => c.Id == cardId);
            if (drawnCard != null)
            {
                drawnCard.Count++;
            }
            else
            {
                PlayerDrawn.Add(GetCardFromId(cardId));
            }

            var deckCard = PlayerDeck.FirstOrDefault(c => c.Id == cardId);
            if (deckCard != null)
            {
                deckCard.Count--;
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
            OpponentHandCount++;

            if(!string.IsNullOrEmpty(cardId))
            {
                var card = OpponentCards.FirstOrDefault(c => c.Id == cardId);
                if (card != null)
                {
                    card.Count--;
                    if (card.Count < 1)
                        OpponentCards.Remove(card);

                    LogDeckChange(true, card, true);
                }
            }

            if (!ValidateOpponentHandCount())
                return;

            OpponentHandAge[OpponentHandCount - 1] = turn;
            OpponentHandMarks[OpponentHandCount - 1] = CardMark.Returned;
        }

        public void OpponentDeckDiscard(string cardId)
        {
            OpponentDeckCount--;
            if (string.IsNullOrEmpty(cardId))
                 return;

            var card = OpponentCards.FirstOrDefault(c => c.Id == cardId);
            if (card != null)
            {
                card.Count++;
            }
            else
            {
                card = GetCardFromId(cardId);
                OpponentCards.Add(card);
            }

            LogDeckChange(true, card, false);
        }

        public void OpponentSecretTriggered(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) 
                return;
            var card = OpponentCards.FirstOrDefault(c => c.Id == cardId);
            if (card != null)
            {
                card.Count++;
            }
            else
            {
                card = GetCardFromId(cardId);
                OpponentCards.Add(card);
            }

            LogDeckChange(true, card, false);
        }

        public void OpponentGet(int turn)
        {
            OpponentHandCount++;

            if (!ValidateOpponentHandCount())
                return;

            OpponentHandAge[OpponentHandCount - 1] = turn;

            if (OpponentHandMarks[OpponentHandCount - 1] != CardMark.Coin)
                OpponentHandMarks[OpponentHandCount - 1] = CardMark.Stolen;

            LogOpponentHand();
        }

        public void Reset()
        {
            Logger.WriteLine(">>>>>>>>>>> Reset <<<<<<<<<<<");

            PlayerDrawn.Clear();
            PlayerHandCount = 0;
            OpponentCards.Clear();
            OpponentHandCount = 0;
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
            OpponentHandCount++;
            OpponentDeckCount--;

            if (!ValidateOpponentHandCount())
                return;

            if (OpponentHandAge[OpponentHandCount - 1] != -1)
            {
                Logger.WriteLine(string.Format("Card {0} is already set to {1}", OpponentHandCount - 1,
                                              OpponentHandAge[OpponentHandCount - 1]), "Hearthstone");

                return;
            }

            Logger.WriteLine(string.Format("Set card {0} to age {1}", OpponentHandCount - 1, args.Turn), "Hearthstone");

            OpponentHandAge[OpponentHandCount - 1] = args.Turn;
            OpponentHandMarks[OpponentHandCount - 1] = CardMark.None;

            LogOpponentHand();
        }

        public void OpponentPlay(CardPosChangeArgs args)
        {
            OpponentHandCount--;

            if (args.Id == "GAME_005")
            {
                OpponentHasCoin = false;
            }
            if (!string.IsNullOrEmpty(args.Id))
            {
                var stolen = args.From != -1 && OpponentHandMarks[args.From - 1] == CardMark.Stolen;
                var card = OpponentCards.FirstOrDefault(c => c.Id == args.Id && c.IsStolen == stolen);

                if (card != null)
                {
                    card.Count++;
                }
                else
                {
                    card = GetCardFromId(args.Id);
                    card.IsStolen = stolen;
                    OpponentCards.Add(card);
                }

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

        private bool ValidateOpponentHandCount()
        {
            if (OpponentHandCount - 1 < 0 || OpponentHandCount - 1 > 9)
            {
                Logger.WriteLine("ValidateOpponentHandCount failed! OpponentHandCount = " + OpponentHandCount.ToString(), "Hearthstone");
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hearthstone_Deck_Tracker
{
    public class Hearthstone
    {

        //dont like this solution, cant think of better atm
        public static bool HighlightCardsInHand;


        private static Dictionary<string, Card> _cardDb;
        public ObservableCollection<Card> EnemyCards;
        public int EnemyHandCount;
        public int OpponentDeckCount;
        public bool IsInMenu;
        public ObservableCollection<Card> PlayerDeck;
        public ObservableCollection<Card> PlayerDrawn; 
        public int PlayerHandCount;
        public string PlayingAgainst;
        public string PlayingAs;
        public bool OpponentHasCoin;
        public bool IsUsingPremade;

        private const int DefaultCoinPosition = 4;
        private const int MaxHandSize = 10;

        public int[] OpponentHandAge { get; private set; }
        public char[] OpponentHandMarks { get; private set; }

        private const char CardMarkNone = ' ';
        private const char CardMarkCoin = 'C';
        private const char CardMarkReturned = 'R';
        private const char CardMarkMulliganInProgress = 'm';
        private const char CardMarkMulliganed = 'M';
        private const char CardMarkStolen = 'S';

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

        public Hearthstone(string languageTag)
        {
            IsInMenu = true;
            PlayerDeck = new ObservableCollection<Card>();
            PlayerDrawn = new ObservableCollection<Card>();
            EnemyCards = new ObservableCollection<Card>();
            _cardDb = new Dictionary<string, Card>();
            OpponentHandAge = new int[MaxHandSize];
            OpponentHandMarks = new char[MaxHandSize];
            for (int i = 0; i < MaxHandSize; i++)
            {
                OpponentHandAge[i] = -1;
                OpponentHandMarks[i] = CardMarkNone;
            }

            LoadCardDb(languageTag);
        }

        private void LoadCardDb(string languageTag)
        {
            try
            {
                var localizedCardNames = new Dictionary<string, string>();
                if (languageTag != "enUS")
                {
                    var file = string.Format("Files/cardsDB.{0}.json", languageTag);
                    if(File.Exists(file))
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
                if(File.Exists(fileEng))
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
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading db: \n" + e);
            }
        }

        public static Card GetCardFromId(string cardId)
        {
            if (cardId == "") return new Card();
            if (_cardDb.ContainsKey(cardId))
            {
                return (Card)_cardDb[cardId].Clone();
            }
            Debug.WriteLine("Could not find entry in db for cardId: " + cardId);
            return new Card(cardId, null, "UNKNOWN", "Minion", "UNKNOWN", 0, "UNKNOWN", 0, 1);
        }
        public Card GetCardFromName(string name)
        {
            if (GetActualCards().Any(c => c.Name.Equals(name)))
            {
                return (Card)GetActualCards().FirstOrDefault(c => c.Name.ToLower() == name.ToLower()).Clone();
            }

            //not sure with all the values here
            Debug.WriteLine("Could not get card from name: " + name);
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
                OpponentHandMarks[DefaultCoinPosition] = CardMarkNone;
                OpponentHandAge[DefaultCoinPosition] = -1;
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
            if (PlayerDeck.Any(c =>  c.Equals(card)))
            {
                var deckCard = PlayerDeck.First(c => c.Equals(card));
                PlayerDeck.Remove(deckCard);
                deckCard.Count++;
                deckCard.InHandCount--;
                PlayerDeck.Add(deckCard);
            }
        }

        public void OpponentMulligan(int pos)
        {
            EnemyHandCount--;
            OpponentDeckCount++;
            OpponentHandMarks[pos - 1] = CardMarkMulliganInProgress;
            if (EnemyHandCount <= OpponentHandAge.Count(x => x != -1))
            {
                //move from to mulligan pos
                OpponentHandAge[pos - 1] = OpponentHandAge[EnemyHandCount];
                OpponentHandAge[EnemyHandCount] = -1;
                Debug.WriteLine("Fixed hand ages after mulligan (moved {0} to {1})", EnemyHandCount, pos - 1);
            }

            Debug.WriteLine("EnemyMulligan");
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
            }
            else
            {
                return false;
            }
            return true;
        }

        public void OpponentBackToHand(string cardId, int turn)
        {
            Debug.WriteLine("EnemyBackToHand");

            if (EnemyCards.Any(c => c.Id == cardId))
            {
                var card = EnemyCards.First(c => c.Id == cardId);
                EnemyCards.Remove(card);
                card.Count--;
                if (card.Count > 0)
                {
                    EnemyCards.Add(card);
                }
            }

            EnemyHandCount++;

            if (!ValidateEnemyHandCount())
                return;

            OpponentHandAge[EnemyHandCount - 1] = turn;
            OpponentHandMarks[EnemyHandCount - 1] = CardMarkReturned;
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
            Debug.WriteLine("EnemyDeckDiscard");
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

            Debug.WriteLine("EnemySecretTriggered");
        }

        internal void OpponentGet(int turn)
        {
            Debug.WriteLine("EnemyGet");

            EnemyHandCount++;

            if (!ValidateEnemyHandCount())
                return;

            OpponentHandAge[EnemyHandCount - 1] = turn;

            if (OpponentHandMarks[EnemyHandCount - 1] != CardMarkCoin)
                OpponentHandMarks[EnemyHandCount - 1] = CardMarkStolen;


            Debug.WriteLine("OpponentHandAge after draw: " + string.Join(",", OpponentHandAge));
            Debug.WriteLine("OpponentHandMarks         : " + string.Join(",", OpponentHandMarks));
        }

        internal void Reset()
        {
            Debug.WriteLine(">>>>>>>>>>> Reset <<<<<<<<<<<");

            PlayerDrawn.Clear();
            PlayerHandCount = 0;
            EnemyCards.Clear();
            EnemyHandCount = 0;
            OpponentDeckCount = 30;
            OpponentHandAge = new int[MaxHandSize];
            OpponentHandMarks = new char[MaxHandSize];

            for (int i = 0; i < MaxHandSize; i++)
            {
                OpponentHandAge[i] = -1;
                OpponentHandMarks[i] = CardMarkNone;
            }

            // Assuming opponent has coin, corrected if we draw it
            OpponentHandMarks[DefaultCoinPosition] = CardMarkCoin;
            OpponentHandAge[DefaultCoinPosition] = 0;
            OpponentHasCoin = true;
        }

        public void OpponentDraw(CardPosChangeArgs args)
        {
            Debug.WriteLine("OpponentDraw");

            EnemyHandCount++;
            OpponentDeckCount--;

            if (!ValidateEnemyHandCount())
                return;

            if (OpponentHandAge[EnemyHandCount - 1] != -1)
            {
                Debug.WriteLine(string.Format("Card {0} is already set to {1}", EnemyHandCount - 1,
                    OpponentHandAge[EnemyHandCount - 1]), "OpponentDraw");

                return;
            }

            Debug.WriteLine(string.Format("Set {0} to {1}", EnemyHandCount - 1, args.Turn), "OpponentDraw");

            OpponentHandAge[EnemyHandCount - 1] = args.Turn;
            OpponentHandMarks[EnemyHandCount - 1] = CardMarkNone;

            Debug.WriteLine("OpponentHandAge after draw: " + string.Join(",", OpponentHandAge));
            Debug.WriteLine("OpponentHandMarks         : " + string.Join(",", OpponentHandMarks));
        }

        public void OpponentPlay(CardPosChangeArgs args)
        {
            EnemyHandCount--;

            if (string.IsNullOrEmpty(args.Id))
            {
                return;
            }
            
            Card card = GetCardFromId(args.Id);

            if (args.From != -1 && OpponentHandMarks[args.From - 1] == CardMarkStolen)
                card.IsStolen = true;

            if (EnemyCards.Any(x => x.Equals(card) && x.IsStolen == card.IsStolen))
            {
                EnemyCards.Remove(card);
                card.Count++;
            }
            EnemyCards.Add(card);

            if (card.IsStolen)
                Debug.WriteLine("Opponent played stolen card from " + args.From);
            else
                Debug.WriteLine("Opponent played from " + args.From);



            if (OpponentHandMarks[args.From - 1] == CardMarkMulliganInProgress)
            {
                Debug.WriteLine(string.Format("Opponent card {0} - mulliganed", args.From), "OpponentPlay");

                OpponentHandMarks[args.From - 1] = CardMarkMulliganed;
            }
            else
            {
                Debug.WriteLine(string.Format("From {0} to Play", args.From), "OpponentPlay");

                for (int i = args.From - 1; i < 9; i++)
                {
                    OpponentHandAge[i] = OpponentHandAge[i + 1];
                    OpponentHandMarks[i] = OpponentHandMarks[i + 1];
                }

                OpponentHandAge[9] = -1;
                OpponentHandMarks[9] = CardMarkNone;
            }



            Debug.WriteLine("OpponentHandAge after play: " + string.Join(",", OpponentHandAge));
            Debug.WriteLine("OpponentHandMarks         : " + string.Join(",", OpponentHandMarks));
        }

        bool ValidateEnemyHandCount()
        {
            if (EnemyHandCount - 1 < 0 || EnemyHandCount - 1 > 9)
            {
                Debug.WriteLine("ValidateEnemyHandCount failed! EnemyHandCount = " + EnemyHandCount.ToString());
                return false;
            }

            return true;
        }
    }
}

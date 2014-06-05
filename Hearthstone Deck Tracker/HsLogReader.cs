using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Hearthstone_Deck_Tracker
{
    public enum GameState
    {
        GameBegin,
        GameEnd
    };

    public enum CardMovementType
    {
        PlayerDraw,
        PlayerMulligan,
        PlayerPlay,
        PlayerDeckDiscard,
        PlayerHandDiscard,
        OpponentDraw,
        OpponentMulligan,
        OpponentPlay,
        OpponentSecretTrigger,
        OpponentDeckDiscard,
        OpponentHandDiscard,
        PlayerGet,
        OpponentPlayToHand
    };

    public enum AnalyzingState
    {
        Start,
        End
    }
    public enum Turn
    {
        Player, Opponent
    }

    public class CardMovementArgs : EventArgs
    {
        public CardMovementArgs(CardMovementType movementType, string cardId)
        {
            MovementType = movementType;
            CardId = cardId;
        }

        public CardMovementType MovementType { get; private set; }
        public string CardId { get; private set; }
    }

    public class GameStateArgs : EventArgs
    {
        public GameStateArgs(GameState state)
        {
            State = state;
        }

        public GameState State { get; private set; }
    }
    public class AnalyzingArgs : EventArgs
    {
        public AnalyzingArgs(AnalyzingState state)
        {
            State = state;
        }

        public AnalyzingState State { get; private set; }
    }
    public class TurnStartArgs : EventArgs
    {
        public TurnStartArgs(Turn turn)
        {
            Turn = turn;
        }

        public Turn Turn { get; private set; }
    }

    public class HsLogReader
    {
        public delegate void CardMovementHandler(HsLogReader sender, CardMovementArgs args);

        public delegate void GameStateHandler(HsLogReader sender, GameStateArgs args);

        public delegate void AnalyzingHandler(HsLogReader sender, AnalyzingArgs args);

        public delegate void TurnStartHandler(HsLogReader sender, TurnStartArgs args);

        private readonly string _fullOutputPath;
        private readonly int _updateDelay;
        private Thread _analyzerThread;
       

        private readonly Regex _cardMovementRegex =
            new Regex(
                @"\w*(cardId=(?<Id>(\w*))).*(player=(?<player>(\d))).*(zone\ from\ (?<from>((\w*)\s*)*))((\ )*->\ (?<to>(\w*\s*)*))*.*");
        
        private long _previousSize;
        private long _lastGameEnd;
        private long _currentOffset;

        private int _powerCount;
        private const int PowerCountTreshold = 14;

        public HsLogReader(string hsDirPath, int updateDelay)
        {
            _updateDelay = (updateDelay == 0) ? 100 : updateDelay;
            while (hsDirPath.EndsWith("\\") || hsDirPath.EndsWith("/"))
            {
                hsDirPath = hsDirPath.Remove(hsDirPath.Length - 1);
            }
            _fullOutputPath = @hsDirPath + @"\Hearthstone_Data\output_log.txt"; 
        }

        public void Start()
        {
            if (_analyzerThread != null) return;
            _analyzerThread = new Thread(ReadFile);
            _analyzerThread.Start();
        }
        public void Stop()
        {
            _analyzerThread.Abort();
            _analyzerThread = null;
        }


        public event CardMovementHandler CardMovement;
        public event GameStateHandler GameStateChange;
        public event AnalyzingHandler Analyzing;
        public event TurnStartHandler TurnStart;


        private void ReadFile()
        {
            while (true)
            {
                if (File.Exists(_fullOutputPath))
                {
                    using (var fs = new FileStream(_fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        )
                    {
                        fs.Seek(_previousSize, SeekOrigin.Begin);
                        if (fs.Length == _previousSize)
                        {
                            Thread.Sleep(_updateDelay);
                            continue;
                        }
                        _previousSize = fs.Length;

                        using (var sr = new StreamReader(fs))
                        {
                            Analyzing(this, new AnalyzingArgs(AnalyzingState.Start));
                            Analyze(sr.ReadToEnd());
                            Analyzing(this, new AnalyzingArgs(AnalyzingState.End));
                            
                        }
                    }
                }
                Thread.Sleep(_updateDelay);
            }
        }

        private void Analyze(string log)
        {
            var logLines = log.Split('\n');
            foreach (var logLine in logLines)
            {
                _currentOffset += logLine.Length;
                if (logLine.StartsWith("[Power]"))
                {
                    _powerCount++;
                }
                else if (logLine.StartsWith("[Bob] legend rank"))
                {
                    //game ended
                    GameStateChange(this, new GameStateArgs(GameState.GameEnd));
                    _lastGameEnd = _currentOffset;
                }
                else if (logLine.StartsWith("[Zone]"))
                {
                    if (!_cardMovementRegex.IsMatch(logLine)) continue;

                    var id = _cardMovementRegex.Match(logLine).Groups["Id"].Value.Trim();
                    var player = int.Parse(_cardMovementRegex.Match(logLine).Groups["player"].Value);
                    var from = _cardMovementRegex.Match(logLine).Groups["from"].Value.Trim();
                    var to = _cardMovementRegex.Match(logLine).Groups["to"].Value.Trim();

                    //coin
                    if (id == "GAME_005")
                    {
                        if (from == "FRIENDLY HAND")
                        {
                            CardMovement(this, new CardMovementArgs(CardMovementType.PlayerHandDiscard, id));
                        }
                        else if (to == "FRIENDLY HAND")
                        {
                            CardMovement(this, new CardMovementArgs(CardMovementType.PlayerDraw, id));
                        }
                        else if (from.Contains("OPPOSING"))
                        {
                            CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlay, id));
                        }
                        _powerCount = 0;
                        continue;
                    }

                    //game start/end
                    if (id.Contains("HERO"))
                    {
                        if (player == 1 && !from.Contains("PLAY"))
                        {
                            GameStateChange(this, new GameStateArgs(GameState.GameBegin));
                        }
                        _powerCount = 0;
                        continue;
                    }

                    switch (from)
                    {
                        case "FRIENDLY DECK":
                            if (to == "FRIENDLY HAND")
                            {
                                //player draw
                                CardMovement(this, new CardMovementArgs(CardMovementType.PlayerDraw, id));
                                if (_powerCount >= PowerCountTreshold)
                                {
                                    TurnStart(this, new TurnStartArgs(Turn.Player));
                                }
                            }
                            else
                            {
                                //player discard from deck
                                CardMovement(this, new CardMovementArgs(CardMovementType.PlayerDeckDiscard, id));
                            }
                            break;
                        case "FRIENDLY HAND":
                            if (to == "FRIENDLY DECK")
                            {
                                //player mulligan
                                CardMovement(this, new CardMovementArgs(CardMovementType.PlayerMulligan, id));
                            }
                            else if (to == "FRIENDLY PLAY")
                            {
                                //player played
                                CardMovement(this, new CardMovementArgs(CardMovementType.PlayerPlay, id));
                            }
                            else
                            {
                                //player discard from hand and spells
                                CardMovement(this, new CardMovementArgs(CardMovementType.PlayerHandDiscard, id));
                            }
                            break;
                        case "OPPOSING HAND":
                            if (to == "OPPOSING DECK")
                            {
                                //opponent mulligan
                                CardMovement(this, new CardMovementArgs(CardMovementType.OpponentMulligan, id));
                            }
                            else if (to == "OPPOSING PLAY")
                            {
                                //opponent played
                                CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlay, id));
                            }
                            else
                            {
                                //spell
                                if (id != "")
                                {
                                    CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlay, id));
                                }
                                else
                                {
                                    //opponent discard from hand
                                    CardMovement(this, new CardMovementArgs(CardMovementType.OpponentHandDiscard, id));
                                }
                            }
                            break;
                        case "OPPOSING DECK":
                            if (to == "OPPOSING HAND")
                            {
                                //opponent draw
                                CardMovement(this, new CardMovementArgs(CardMovementType.OpponentDraw, id));
                                if (_powerCount >= PowerCountTreshold)
                                {
                                    TurnStart(this, new TurnStartArgs(Turn.Player));
                                }
                            }
                            else
                            {
                                //opponent discard from deck
                                CardMovement(this,
                                             new CardMovementArgs(CardMovementType.OpponentDeckDiscard, id));
                            }
                            break;
                        case "OPPOSING SECRET":
                            //opponent secret triggered
                            CardMovement(this, new CardMovementArgs(CardMovementType.OpponentSecretTrigger, id));
                            break;
                        case "OPPOSING PLAY":
                            if (to == "OPPOSING HAND")
                            {
                                //card from play back to hand (sap/brew)
                                CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlayToHand, id));
                            }
                            break;
                        default:
                            if (to == "OPPOSING HAND")
                            {
                                //coin, thoughtsteal etc
                                CardMovement(this, new CardMovementArgs(CardMovementType.OpponentDraw, id));
                            }
                            else if (to == "OPPOSING GRAVEYARD" && from == "" && id != "")
                            {
                                //CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlay, id));
                            }
                            else if (to == "FRIENDLY HAND")
                            {
                                //thoughtsteal etc
                                CardMovement(this, new CardMovementArgs(CardMovementType.PlayerGet, id));
                            }
                            else if (to == "FRIENDLY GRAVEYARD" && from == "")
                            {
                                // CardMovement(this, new CardMovementArgs(CardMovementType.PlayerPlay, id));
                            }
                            break;
                    }
                    _powerCount = 0;
                }
            }
        }

        internal void Reset(bool full)
        {
            if (full)
                _previousSize = 0;
            else
            {
                _currentOffset = _lastGameEnd;
                _previousSize = _lastGameEnd;

            }
        }

    }
}
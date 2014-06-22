using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
        OpponentPlayToHand,
        OpponentGet
    };
    public enum OpponentHandMovement
    {
        Draw,
        Play
    }

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
        public GameStateArgs()
        {
            
        }

        public GameState? State { get; set; }
        public string PlayerHero { get; set; }
        public string OpponentHero { get; set; }
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
    public class CardPosChangeArgs : EventArgs
    {
        public CardPosChangeArgs(OpponentHandMovement action, int from, int turn)
        {
            Turn = turn;
            Action = action;
            From = from;
        }

        public OpponentHandMovement Action { get; private set; }
        public int Turn { get; private set; }
        public int From { get; private set; }
    }

    public class HsLogReader
    {
        public delegate void CardMovementHandler(HsLogReader sender, CardMovementArgs args);

        public delegate void GameStateHandler(HsLogReader sender, GameStateArgs args);

        public delegate void AnalyzingHandler(HsLogReader sender, AnalyzingArgs args);

        public delegate void TurnStartHandler(HsLogReader sender, TurnStartArgs args);

        public delegate void CardPosChangeHandler(HsLogReader sender, CardPosChangeArgs args);

        private readonly string _fullOutputPath;
        private readonly int _updateDelay;
        private bool _doUpdate;

        private readonly Dictionary<string, string> _heroIdDict = new Dictionary<string, string>()
            {
                {"HERO_01", "Warrior"},
                {"HERO_02", "Shaman"},
                {"HERO_03", "Rogue"},
                {"HERO_04", "Paladin"},
                {"HERO_05", "Hunter"},
                {"HERO_06", "Druid"},
                {"HERO_07", "Warlock"},
                {"HERO_08", "Mage"},
                {"HERO_09", "Priest"}
            };

        private readonly Regex _cardMovementRegex =
            new Regex(
                @"\w*(cardId=(?<Id>(\w*))).*(zone\ from\ (?<from>((\w*)\s*)*))((\ )*->\ (?<to>(\w*\s*)*))*.*");

       private readonly Regex _opponentPlayRegex = 
            new Regex(
                @"\w*(zonePos=(?<zonePos>(\d))).*(zone\ from\ OPPOSING\ HAND).*");
        


        private long _previousSize;
        private long _lastGameEnd;
        private long _currentOffset;
        
        private int _powerCount;
        private const int PowerCountTreshold = 14;

        private int _turnCount;

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
            _first = true;
            _doUpdate = true;
            ReadFileAsync();

        }
        public void Stop()
        {
            _doUpdate = false;
        }


        public event CardMovementHandler CardMovement;
        public event GameStateHandler GameStateChange;
        public event AnalyzingHandler Analyzing;
        public event TurnStartHandler TurnStart;
        public event CardPosChangeHandler CardPosChange;

        private bool _first;

        private async void ReadFileAsync()
        {
            while (_doUpdate)
            {
                if (File.Exists(_fullOutputPath))
                {
                    //find end of last game (avoids reading the full log on start)
                    if (_first)
                    {
                        using (var fs = new FileStream(_fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            _previousSize = FindLastGameEnd(fs);
                            _currentOffset = _previousSize;
                            _first = false;
                        }
                    }


                    using (var fs = new FileStream(_fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        
                        fs.Seek(_previousSize, SeekOrigin.Begin);
                        if (fs.Length == _previousSize)
                        {
                            await Task.Delay(_updateDelay);
                            continue;
                        }
                        var newLength = fs.Length;

                        using (var sr = new StreamReader(fs))
                        {
                            var newLines = sr.ReadToEnd();
                            if (!newLines.EndsWith("\n"))
                            {
                                //hearthstone log apparently does not append full lines
                                await Task.Delay(_updateDelay);
                                continue;
                            }
                            Analyzing(this, new AnalyzingArgs(AnalyzingState.Start));
                            Analyze(newLines);
                            Analyzing(this, new AnalyzingArgs(AnalyzingState.End));

                        }

                        _previousSize = newLength;
                    }
                }
                await Task.Delay(_updateDelay);
            }
        }

        private long FindLastGameEnd(FileStream fs)
        {
            using (var sr = new StreamReader(fs))
            {
                var lines = sr.ReadToEnd().Split('\n');

                long offset = 0;
                long tempOffset = 0;
                foreach (var line in lines)
                {
                    tempOffset += line.Length+1;
                    if (line.StartsWith("[Bob] legend rank"))
                    {
                        offset = tempOffset;
                    }
                }

                return offset;
            }
        }

        private bool _opposingLast;
        private void Analyze(string log)
        {
            var logLines = log.Split('\n');
            foreach (var logLine in logLines)
            {
                _currentOffset += logLine.Length+1;
                if (logLine.StartsWith("[Power]"))
                {
                    _powerCount++;
                }
                else if (logLine.StartsWith("[Bob] legend rank"))
                {
                    //game ended
                    GameStateChange(this, new GameStateArgs(GameState.GameEnd));
                    _lastGameEnd = _currentOffset;
                    _turnCount = 0;
                }
                else if (logLine.StartsWith("[Zone]"))
                {
                    
                    if (_cardMovementRegex.IsMatch(logLine))
                    {
                        _opposingLast = false;
                        Match match = _cardMovementRegex.Match(logLine);

                        var id = match.Groups["Id"].Value.Trim();
                        var from = match.Groups["from"].Value.Trim();
                        var to = match.Groups["to"].Value.Trim();

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
                            if (!from.Contains("PLAY"))
                            {
                                if (to.Contains("FRIENDLY"))
                                {
                                    GameStateChange(this,
                                                    new GameStateArgs(GameState.GameBegin)
                                                        {
                                                            PlayerHero = _heroIdDict[id]
                                                        });
                                }
                                else if (to.Contains("OPPOSING"))
                                {
                                    GameStateChange(this,
                                                    new GameStateArgs() {OpponentHero = _heroIdDict[id]});
                                }
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
                                        _turnCount++;
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
                                _opposingLast = true;
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
                                        CardMovement(this,
                                                     new CardMovementArgs(CardMovementType.OpponentHandDiscard, id));
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
                                        TurnStart(this, new TurnStartArgs(Turn.Opponent));
                                        _turnCount++;
                                    }

                                    if (CardPosChange != null)
                                        CardPosChange(this,
                                                      new CardPosChangeArgs(OpponentHandMovement.Draw, 0,
                                                                            (_turnCount + 1) / 2));
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
                                    CardMovement(this, new CardMovementArgs(CardMovementType.OpponentGet, id));
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
                    if (_opponentPlayRegex.IsMatch(logLine))
                    {
                        Match match = _opponentPlayRegex.Match(logLine);
                        var zonePos = int.Parse(match.Groups["zonePos"].Value.Trim());
                        CardPosChange(this, new CardPosChangeArgs(OpponentHandMovement.Play, zonePos, ((_turnCount+1) / 2)));
                    }

                }
            }
        }

        internal void Reset(bool full)
        {
            if (full)
            {
                _previousSize = 0;
                _first = true;
            }
            else
            {
                _currentOffset = _lastGameEnd;
                _previousSize = _lastGameEnd;
            }
            _turnCount = 0;
        }

    }
}
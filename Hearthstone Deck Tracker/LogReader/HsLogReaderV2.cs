using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Handlers;

namespace Hearthstone_Deck_Tracker.LogReader
{
    public class HsLogReaderV2 : IHsLogReader
    {
        //should be about 180,000 lines
        private const int MaxFileLength = 6000000;
        private readonly string _fullOutputPath;
        private readonly bool _ifaceUpdateNeeded;
        private readonly int _updateDelay;
        private HsGameState _gameState;
        private GameV2 _game;

        private readonly PowerGameStateHandler _powerGameStateLineHandler = new PowerGameStateHandler();
        private readonly PowerHandler _powerLineHandler = new PowerHandler();
        private readonly RachelleHandler _rachelleHandler = new RachelleHandler();
        private readonly AssetHandler _assetHandler = new AssetHandler();
        private readonly ZoneHandler _zoneHandler = new ZoneHandler();
        private readonly BobHandler _bobHandler = new BobHandler();
        private readonly ArenaHandler _arenaHandler = new ArenaHandler();

        private HsLogReaderV2() : this(Config.Instance.HearthstoneDirectory, Config.Instance.UpdateDelay, true) { }

        private HsLogReaderV2(string hsDirectory, int updateDeclay, bool interfaceUpdateNeeded)
        {
            var hsDirPath = hsDirectory;
            var updateDelay = updateDeclay;
            _ifaceUpdateNeeded = interfaceUpdateNeeded;
            _updateDelay = updateDelay == 0 ? 100 : updateDelay;
            while (hsDirPath.EndsWith("\\") || hsDirPath.EndsWith("/"))
                hsDirPath = hsDirPath.Remove(hsDirPath.Length - 1);
            _fullOutputPath = @hsDirPath + @"\Hearthstone_Data\output_log.txt";
        }

        public static HsLogReaderV2 Instance { get; private set; }

        public static void Create()
        {
            if (Instance == null)
                Instance = new HsLogReaderV2();
        }

        public static void Create(string hsDirectory, int updateDeclay, bool ifaceUpdateNeeded = true)
        {
            if (Instance == null)
                Instance = new HsLogReaderV2(hsDirectory, updateDeclay, ifaceUpdateNeeded);
        }

        public void Start(GameV2 game)
        {
            _game = game;
            _gameState = new HsGameState(game);
            _gameState.GameHandler = new GameEventHandler(game);
            _gameState.GameHandler.ResetConstructedImporting();
            _gameState.LastGameStart = DateTime.Now;
            Start(_gameState.GameHandler, game);
        }

        public void Start(IGameHandler gh, GameV2 game)
        {
            _gameState.AddToTurn = -1;
            _gameState.First = true;
            _gameState.DoUpdate = true;
            _gameState.GameHandler = gh;
            _gameState.GameEnded = false;
            ReadFileAsync();
        }

        public void Stop()
        {
            _gameState.DoUpdate = false;
        }

        private async void ReadFileAsync()
        {
            while (_gameState.DoUpdate)
            {
                if (File.Exists(_fullOutputPath) && _game.IsRunning)
                {
                    //find end of last game (avoids reading the full log on start)
                    if (_gameState.First)
                    {
                        using (var fs = new FileStream(_fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var fileOffset = 0L;
                            if (fs.Length > MaxFileLength)
                            {
                                fileOffset = fs.Length - MaxFileLength;
                                fs.Seek(fs.Length - MaxFileLength, SeekOrigin.Begin);
                            }
                            _gameState.PreviousSize = FindLastGameStart(fs) + fileOffset;
                            _gameState.CurrentOffset = _gameState.PreviousSize;
                            _gameState.First = false;
                        }
                    }

                    using (var fs = new FileStream(_fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fs.Seek(_gameState.PreviousSize, SeekOrigin.Begin);
                        if (fs.Length == _gameState.PreviousSize)
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

                            Analyze(newLines);

                            if (_ifaceUpdateNeeded)
                                Helper.UpdateEverything(_game);
                        }

                        _gameState.PreviousSize = newLength;
                    }
                }

                await Task.Delay(_updateDelay);
            }
        }

        private long FindLastGameStart(FileStream fs)
        {
            using (var sr = new StreamReader(fs))
            {
                long offset = 0, tempOffset = 0;
                var lines = sr.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (line.Contains("Begin Spectating") || line.Contains("Start Spectator"))
                    {
                        offset = tempOffset;
                        _gameState.FoundSpectatorStart = true;
                    }
                    else if (line.Contains("End Spectator"))
                        offset = tempOffset;
                    else if (line.Contains("CREATE_GAME") && line.Contains("GameState."))
                    {
                        //if (_gameState.FoundSpectatorStart)
                        //{
                        //    _gameState.FoundSpectatorStart = false;
                        //    continue;
                        //}
                        offset = tempOffset;
                        continue;
                    }
                    tempOffset += line.Length + 1;
                    if (line.Contains("[Bob] legend rank"))
                    {
                        if (_gameState.FoundSpectatorStart)
                        {
                            _gameState.FoundSpectatorStart = false;
                            continue;
                        }
                        offset = tempOffset;
                    }
                }

                return offset;
            }
        }

        private void Analyze(string log)
        {
            var logLines = log.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var logLine in logLines)
            {
                _gameState.CurrentOffset += logLine.Length + 1;

                if (logLine.StartsWith("["))
                {
                    GameV2.AddHSLogLine(logLine);
                    API.LogEvents.OnLogLine.Execute(logLine);
                }

                if (logLine.StartsWith("[Power] GameState."))
                {
                    _powerGameStateLineHandler.Handle(logLine, _gameState, _game);
                }
                else if (logLine.StartsWith("[Power]"))
                {
                    _powerLineHandler.Handle(logLine, _gameState, _game);
                }
                else if (logLine.StartsWith("[Asset]"))
                {
                    _assetHandler.Handle(logLine, _gameState, _game);
                }
                else if (logLine.StartsWith("[Bob]"))
                {
                    _bobHandler.Handle(logLine, _gameState, _game);
                }
                else if (logLine.StartsWith("[Rachelle]"))
                {
                    _rachelleHandler.Handle(logLine, _gameState, _game);
                }
                else if (logLine.StartsWith("[Zone]"))
                {
                    _zoneHandler.Handle(logLine, _gameState);
                }
                else if (logLine.StartsWith("[Arena]"))
                {
                    _arenaHandler.Handle(logLine, _gameState, _game);
                }

                if (_gameState.First)
                    break;
            }
        }

        public static int ParseTagValue(GAME_TAG tag, string rawValue)
        {
            int value;
            switch (tag)
            {
                case GAME_TAG.ZONE:
                    TAG_ZONE zone;
                    Enum.TryParse(rawValue, out zone);
                    value = (int)zone;
                    break;
                case GAME_TAG.MULLIGAN_STATE:
                    {
                        TAG_MULLIGAN state;
                        Enum.TryParse(rawValue, out state);
                        value = (int)state;
                    }
                    break;
                case GAME_TAG.PLAYSTATE:
                    {
                        TAG_PLAYSTATE state;
                        Enum.TryParse(rawValue, out state);
                        value = (int)state;
                    }
                    break;
                case GAME_TAG.CARDTYPE:
                    TAG_CARDTYPE type;
                    Enum.TryParse(rawValue, out type);
                    value = (int)type;
                    break;
                default:
                    int.TryParse(rawValue, out value);
                    break;
            }
            return value;
        }

        public void ClearLog()
        {
            if (Config.Instance.ClearLogFileAfterGame)
            {
                try
                {
                    using (var fs = new FileStream(_fullOutputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    using (var sw = new StreamWriter(fs))
                        sw.Write("");
                    Logger.WriteLine("Cleared log file", "LogReader");
                    Reset(true);
                }
                catch (Exception e)
                {
                    Logger.WriteLine("Error cleared log file: " + e, "LogReader");
                }
            }
            else
                Logger.WriteLine("Logfile was not cleared! (ClearLogFileAfterGame = false)", "LogReader");
            _gameState.GameLoaded = false;
        }

        public void Reset(bool full)
        {
            if (_gameState == null)
                return;

            if (full)
            {
                _gameState.PreviousSize = 0;
                _gameState.CurrentOffset = 0;
            }
            else
            {
                _gameState.CurrentOffset = _gameState.LastGameEnd;
                _gameState.PreviousSize = _gameState.LastGameEnd;
            }
            _gameState.First = true;
            _gameState.AddToTurn = -1;
            _gameState.GameEnded = false;
            _gameState.FoundSpectatorStart = false;
            _gameState.NextUpdatedEntityIsJoust = false;
        }

        public async Task<bool> RankedDetection(int timeoutInSeconds = 3)
        {
            Logger.WriteLine("waiting for ranked detection", "LogReader");
            _gameState.AwaitingRankedDetection = true;
            _gameState.WaitingForFirstAssetUnload = true;
            _gameState.FoundRanked = false;
            _gameState.LastAssetUnload = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            while (_gameState.WaitingForFirstAssetUnload || (DateTime.Now - _gameState.LastAssetUnload) < timeout)
            {
                await Task.Delay(100);
                if (_gameState.FoundRanked)
                    break;
            }
            return _gameState.FoundRanked;
        }

        public async void GetCurrentRegion()
        {
            try
            {
                var regex = new Regex(@"AccountListener.OnAccountLevelInfoUpdated.*currentRegion=(?<region>(\d))");
                var conLogPath = Path.Combine(Config.Instance.HearthstoneDirectory, "ConnectLog.txt");
                //while(!_gameState.GameLoaded)
                //	await Task.Delay(100);
                using (var fs = new FileStream(conLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs))
                {
                    var lines = reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var match = regex.Match(line);
                        if (match.Success)
                        {
                            Region region;
                            if (Enum.TryParse(match.Groups["region"].Value, out region))
                            {
                                _game.CurrentRegion = region;
                                Logger.WriteLine("Current region: " + region, "LogReader");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error getting region:\n" + ex, "LogReader");
            }
        }

        public int GetTurnNumber()
        {
            return _gameState.GetTurnNumber();
        }
    }
}
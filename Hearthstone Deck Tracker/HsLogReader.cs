#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public enum Turn
	{
		Player,
		Opponent
	}

	public class HsLogReader
	{
		#region Properties

		private const int PowerCountTreshold = 14;

		//should be about 180,000 lines
		private const int MaxFileLength = 6000000;

		private readonly Regex _cardMovementRegex = new Regex(@"\w*(cardId=(?<Id>(\w*))).*(zone\ from\ (?<from>((\w*)\s*)*))((\ )*->\ (?<to>(\w*\s*)*))*.*");

		private readonly string _fullOutputPath;

		private readonly Dictionary<string, string> _heroIdDict = new Dictionary<string, string>
			{
				{"HERO_01", "Warrior"},
				{"HERO_02", "Shaman"},
				{"HERO_03", "Rogue"},
				{"HERO_04", "Paladin"},
				{"HERO_05", "Hunter"},
				{"HERO_06", "Druid"},
				{"HERO_07", "Warlock"},
				{"HERO_08", "Mage"},
				{"HERO_09", "Priest"},
				{"NAX1_01", "Anub'Rekhan"},
				{"NAX1h_01", "Anub'Rekhan"},
				{"NAX2_01", "Grand Widow Faerlina"},
				{"NAX2_01H", "Grand Widow Faerlina"},
				{"NAX3_01", "Maexxna"},
				{"NAX3_01H", "Maexxna"},
				{"NAX4_01", "Noth the Plaguebringer"},
				{"NAX4_01H", "Noth the Plaguebringer"},
				{"NAX5_01", "Heigan the Unclean"},
				{"NAX5_01H", "Heigan the Unclean"},
				{"NAX6_01", "Loatheb"},
				{"NAX6_01H", "Loatheb"},
				{"NAX7_01", "Instructor Razuvious"},
				{"NAX7_01H", "Instructor Razuvious"},
				{"NAX8_01", "Gothik the Harvester"},
				{"NAX8_01H", "Gothik the Harvester"},
				{"NAX9_01", "Baron Rivendare"},
				{"NAX9_01H", "Baron Rivendare"},
				{"NAX10_01", "Patchwerk"},
				{"NAX10_01H", "Patchwerk"},
				{"NAX11_01", "Grobbulus"},
				{"NAX11_01H", "Grobbulus"},
				{"NAX12_01", "Gluth"},
				{"NAX12_01H", "Gluth"},
				{"NAX13_01", "Thaddius"},
				{"NAX13_01H", "Thaddius"},
				{"NAX14_01", "Sapphiron"},
				{"NAX14_01H", "Sapphiron"},
				{"NAX15_01", "Kel'Thuzad"},
				{"NAX15_01H", "Kel'Thuzad"}
			};

		private readonly Regex _opponentPlayRegex = new Regex(@"\w*(zonePos=(?<zonePos>(\d+))).*(zone\ from\ OPPOSING\ HAND).*");

		private readonly int _updateDelay;
		private readonly Regex _zoneRegex = new Regex(@"\w*(zone=(?<zone>(\w*)).*(zone\ from\ FRIENDLY\ DECK)\w*)");


		private long _currentOffset;
		private bool _doUpdate;
		private bool _first;
		private long _lastGameEnd;
		private bool _lastOpponentDrawIncrementedTurn;
		private bool _lastPlayerDrawIncrementedTurn;
		private int _powerCount;
		private long _previousSize;
		private int _turnCount;

		#endregion

		private readonly Regex _heroPowerRegex = new Regex(@".*(cardId=(?<Id>(\w*))).*");
		private Turn _currentPlayer;
		private bool _opponentUsedHeroPower;
		private bool _playerUsedHeroPower;

		private HsLogReader()
		{
			var hsDirPath = Config.Instance.HearthstoneDirectory;
			var updateDelay = Config.Instance.UpdateDelay;

			_updateDelay = updateDelay == 0 ? 100 : updateDelay;
			while(hsDirPath.EndsWith("\\") || hsDirPath.EndsWith("/"))
				hsDirPath = hsDirPath.Remove(hsDirPath.Length - 1);
			_fullOutputPath = @hsDirPath + @"\Hearthstone_Data\output_log.txt";
		}

		public static HsLogReader Instance { get; private set; }

		public static void Create()
		{
			Instance = new HsLogReader();
		}

		public int GetTurnNumber()
		{
			return (_turnCount + 1) / 2;
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

		private async void ReadFileAsync()
		{
			while(_doUpdate)
			{
				if(File.Exists(_fullOutputPath) && Game.IsRunning)
				{
					//find end of last game (avoids reading the full log on start)
					if(_first)
					{
						using(var fs = new FileStream(_fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
						{
							var fileOffset = 0L;
							if(fs.Length > MaxFileLength)
							{
								fileOffset = fs.Length - MaxFileLength;
								fs.Seek(fs.Length - MaxFileLength, SeekOrigin.Begin);
							}
							_previousSize = FindLastGameEnd(fs) + fileOffset;
							_currentOffset = _previousSize;
							_first = false;
						}
					}

					using(var fs = new FileStream(_fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						fs.Seek(_previousSize, SeekOrigin.Begin);
						if(fs.Length == _previousSize)
						{
							await Task.Delay(_updateDelay);
							continue;
						}
						var newLength = fs.Length;

						using(var sr = new StreamReader(fs))
						{
							var newLines = sr.ReadToEnd();
							if(!newLines.EndsWith("\n"))
							{
								//hearthstone log apparently does not append full lines
								await Task.Delay(_updateDelay);
								continue;
							}
							Analyze(newLines);
							Helper.UpdateEverything();
						}

						_previousSize = newLength;
					}
				}

				await Task.Delay(_updateDelay);
			}
		}

		private long FindLastGameEnd(FileStream fs)
		{
			using(var sr = new StreamReader(fs))
			{
				long offset = 0, tempOffset = 0;
				var lines = sr.ReadToEnd().Split('\n');

				foreach(var line in lines)
				{
					tempOffset += line.Length + 1;
					if(line.StartsWith("[Bob] legend rank"))
						offset = tempOffset;
				}

				return offset;
			}
		}

		private void Analyze(string log)
		{
			var logLines = log.Split('\n');
			foreach(var logLine in logLines)
			{
				_currentOffset += logLine.Length + 1;
				if(logLine.StartsWith("[Power]"))
				{
					_powerCount++;

					if((_currentPlayer == Turn.Player && !_playerUsedHeroPower) || _currentPlayer == Turn.Opponent && !_opponentUsedHeroPower)
					{
						if(_heroPowerRegex.IsMatch(logLine))
						{
							var id = _heroPowerRegex.Match(logLine).Groups["Id"].Value.Trim();
							if(!string.IsNullOrEmpty(id))
							{
								var heroPower = Game.GetCardFromId(id);
								if(heroPower.Type == "Hero Power")
								{
									if(_currentPlayer == Turn.Player)
									{
										GameEventHandler.HandlePlayerHeroPower(id, GetTurnNumber());
										_playerUsedHeroPower = true;
									}
									else
									{
										GameEventHandler.HandleOpponentHeroPower(id, GetTurnNumber());
										_opponentUsedHeroPower = true;
									}
								}
							}
						}
					}
				}
				else if(logLine.StartsWith("[Asset]"))
				{
					if(logLine.ToLower().Contains("victory_screen_start"))
						GameEventHandler.HandleWin();
					else if(logLine.ToLower().Contains("defeat_screen_start"))
						GameEventHandler.HandleLoss();
					else if(logLine.Contains("rank_window"))
					{
						Game.CurrentGameMode = Game.GameMode.Ranked;
						Logger.WriteLine(">>> GAME MODE: RANKED");
					}
				}
				else if(logLine.StartsWith("[Bob] legend rank"))
				{
					if(!Game.IsInMenu)
						GameEventHandler.HandleGameEnd(false);
				}
				else if(logLine.StartsWith("[Bob] ---RegisterScreenPractice---"))
				{
					Game.CurrentGameMode = Game.GameMode.Practice;
					Logger.WriteLine(">>> GAME MODE: PRACTICE");
				}
				else if(logLine.StartsWith("[Bob] ---RegisterScreenTourneys---"))
				{
					Game.CurrentGameMode = Game.GameMode.Casual;
					Logger.WriteLine(">>> GAME MODE: CASUAL (RANKED)");
				}
				else if(logLine.StartsWith("[Bob] ---RegisterScreenForge---"))
				{
					Game.CurrentGameMode = Game.GameMode.Arena;
					Logger.WriteLine(">>> GAME MODE: ARENA");
				}
				else if(logLine.StartsWith("[Bob] ---RegisterScreenFriendly---"))
				{
					Game.CurrentGameMode = Game.GameMode.Friendly;
					Logger.WriteLine(">>> GAME MODE: FRIENDLY");
				}
				else if(logLine.StartsWith("[Bob] ---RegisterScreenBox---"))
				{
					//game ended

					Game.CurrentGameMode = Game.GameMode.None;
					Logger.WriteLine(">>> GAME MODE: NONE");

					GameEventHandler.HandleGameEnd(true);
					_lastGameEnd = _currentOffset;
					_turnCount = 0;
					_lastOpponentDrawIncrementedTurn = false;
					_lastPlayerDrawIncrementedTurn = false;
					ClearLog();
				}
				else if(logLine.StartsWith("[Zone]"))
				{
					if(_cardMovementRegex.IsMatch(logLine))
					{
						var match = _cardMovementRegex.Match(logLine);

						var id = match.Groups["Id"].Value.Trim();
						var from = match.Groups["from"].Value.Trim();
						var to = match.Groups["to"].Value.Trim();

						var zonePos = -1;
						//var zone = string.Empty;

						// Only for some log lines, should be valid in every action where we need it
						if(_opponentPlayRegex.IsMatch(logLine))
						{
							var match2 = _opponentPlayRegex.Match(logLine);
							zonePos = Int32.Parse(match2.Groups["zonePos"].Value.Trim());
						}
						if(_zoneRegex.IsMatch(logLine))
						{
							//var match3 = _zoneRegex.Match(logLine);
							//zone = match3.Groups["zone"].Value.Trim();
							GameEventHandler.PlayerSetAside(id);
						}

						//game start/end
						if(id.Contains("HERO") || (id.Contains("NAX") && id.Contains("_01")))
						{
							if(!from.Contains("PLAY"))
							{
								if(to.Contains("FRIENDLY"))
									GameEventHandler.HandleGameStart(_heroIdDict[id]);
								else if(to.Contains("OPPOSING"))
								{
									string heroName;
									if(_heroIdDict.TryGetValue(id, out heroName))
										GameEventHandler.SetOpponentHero(heroName);
								}
							}
							_powerCount = 0;
							continue;
						}

						switch(from)
						{
							case "FRIENDLY DECK":
								if(to == "FRIENDLY HAND")
								{
									//player draw
									if(_powerCount >= PowerCountTreshold)
									{
										_turnCount++;
										GameEventHandler.TurnStart(Turn.Player, GetTurnNumber());
										_currentPlayer = Turn.Player;
										_playerUsedHeroPower = false;
										_lastPlayerDrawIncrementedTurn = true;
									}
									else
										_lastPlayerDrawIncrementedTurn = false;
									GameEventHandler.HandlePlayerDraw(id, GetTurnNumber());
								}
								else
									//player discard from deck
									GameEventHandler.HandlePlayerDeckDiscard(id, GetTurnNumber());
								break;
							case "FRIENDLY HAND":
								if(to == "FRIENDLY DECK")
								{
									if(_lastPlayerDrawIncrementedTurn)
										_turnCount--;
									GameEventHandler.HandlePlayerMulligan(id);
								}
								else if(to == "FRIENDLY PLAY")
									GameEventHandler.HandlePlayerPlay(id, GetTurnNumber());
								else
									//player discard from hand and spells
									GameEventHandler.HandlePlayerHandDiscard(id, GetTurnNumber());

								break;
							case "FRIENDLY PLAY":
								if(to == "FRIENDLY HAND")
									GameEventHandler.HandlePlayerBackToHand(id, GetTurnNumber());
								break;
							case "OPPOSING HAND":
								if(to == "OPPOSING DECK")
								{
									if(_lastOpponentDrawIncrementedTurn)
										_turnCount--;
									//opponent mulligan
									GameEventHandler.HandleOpponentMulligan(zonePos);
								}
								else
								{
									if(to == "OPPOSING SECRET")
										GameEventHandler.HandleOpponentSecretPlayed();
									if(to == "OPPOSING PLAY")
										GameEventHandler.HandleOpponentPlay(id, zonePos, GetTurnNumber());
									else
										GameEventHandler.HandleOpponentHandDiscard(id, zonePos, GetTurnNumber());
								}
								break;
							case "OPPOSING DECK":
								if(to == "OPPOSING HAND")
								{
									if(_powerCount >= PowerCountTreshold)
									{
										_turnCount++;
										GameEventHandler.TurnStart(Turn.Opponent, GetTurnNumber());
										_currentPlayer = Turn.Opponent;
										_opponentUsedHeroPower = false;
										_lastOpponentDrawIncrementedTurn = true;
									}
									else
										_lastOpponentDrawIncrementedTurn = false;

									//opponent draw
									GameEventHandler.HandlOpponentDraw(GetTurnNumber());
								}
								else
									//opponent discard from deck
									GameEventHandler.HandleOpponentDeckDiscard(id, GetTurnNumber());

								if(to == "OPPOSING SECRET")
									GameEventHandler.HandleOpponentSecretPlayed();
								break;
							case "OPPOSING SECRET":
								//opponent secret triggered
								GameEventHandler.HandleOpponentSecretTrigger(id, GetTurnNumber());
								break;
							case "OPPOSING PLAY":
								if(to == "OPPOSING HAND") //card from play back to hand (sap/brew)
									GameEventHandler.HandleOpponentPlayToHand(id, GetTurnNumber());
								break;
							default:
								if(to == "OPPOSING HAND")
									//coin, thoughtsteal etc
									GameEventHandler.HandleOpponentGet(GetTurnNumber());
								else if(to == "FRIENDLY HAND")
									//coin, thoughtsteal etc
									GameEventHandler.HandlePlayerGet(id, GetTurnNumber());
								else if(to == "OPPOSING GRAVEYARD" && from == "" && id != "")
								{
									//todo: not sure why those two are here
									//CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlay, id));
								}
								else if(to == "FRIENDLY GRAVEYARD" && from == "")
								{
									// CardMovement(this, new CardMovementArgs(CardMovementType.PlayerPlay, id));
								}
								break;
						}
						_powerCount = 0;
					}
				}
			}
		}

		public void ClearLog()
		{
			if(Config.Instance.ClearLogFileAfterGame)
			{
				try
				{
					using(var fs = new FileStream(_fullOutputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
					using(var sw = new StreamWriter(fs))
						sw.Write("");
					Logger.WriteLine("Cleared log file");
					Reset(true);
				}
				catch(Exception e)
				{
					Logger.WriteLine("Error cleared log file: " + e.Message);
				}
			}
		}

		internal void Reset(bool full)
		{
			if(full)
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
			_lastOpponentDrawIncrementedTurn = false;
			_lastPlayerDrawIncrementedTurn = false;
			_first = true;
		}
	}
}
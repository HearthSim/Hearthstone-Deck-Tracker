#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
		private const int MaxFileLength = 3000000;

		private readonly Regex _cardMovementRegex =
			new Regex(@"\w*(cardId=(?<Id>(\w*))).*(zone\ from\ (?<from>((\w*)\s*)*))((\ )*->\ (?<to>(\w*\s*)*))*.*");

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
				{"HERO_09", "Priest"}
			};

		private readonly Regex _opponentPlayRegex = new Regex(@"\w*(zonePos=(?<zonePos>(\d+))).*(zone\ from\ OPPOSING\ HAND).*");

		private readonly int _updateDelay;

		//should be about 90,000 lines

		private long _currentOffset;
		private bool _doUpdate;
		private bool _first;
		private long _lastGameEnd;
		private int _powerCount;
		private long _previousSize;
		private int _turnCount;

		#endregion

		private HsLogReader()
		{
			var hsDirPath = Config.Instance.HearthstoneDirectory;
			var updateDelay = Config.Instance.UpdateDelay;

			_updateDelay = updateDelay == 0 ? 100 : updateDelay;
			while (hsDirPath.EndsWith("\\") || hsDirPath.EndsWith("/"))
			{
				hsDirPath = hsDirPath.Remove(hsDirPath.Length - 1);
			}
			_fullOutputPath = @hsDirPath + @"\Hearthstone_Data\output_log.txt";
		}

		public static HsLogReader Instance { get; private set; }

		public static void Create()
		{
			Instance = new HsLogReader();
		}

		public int GetTurnNumber()
		{
			return (_turnCount)/2;
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
			while (_doUpdate)
			{
				if (File.Exists(_fullOutputPath))
				{
					//find end of last game (avoids reading the full log on start)
					if (_first)
					{
						using (var fs = new FileStream(_fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
						{
							var fileOffset = 0L;
							if (fs.Length > MaxFileLength)
							{
								fileOffset = fs.Length - MaxFileLength;
								fs.Seek(fs.Length - MaxFileLength, SeekOrigin.Begin);
							}
							_previousSize = FindLastGameEnd(fs) + fileOffset;
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
			using (var sr = new StreamReader(fs))
			{
				long offset = 0, tempOffset = 0;
				var lines = sr.ReadToEnd().Split('\n');

				foreach (var line in lines)
				{
					tempOffset += line.Length + 1;
					if (line.StartsWith("[Bob] legend rank"))
						offset = tempOffset;
				}

				return offset;
			}
		}

		private void Analyze(string log)
		{
			var logLines = log.Split('\n');
			foreach (var logLine in logLines)
			{
				_currentOffset += logLine.Length + 1;
				if (logLine.StartsWith("[Power]"))
				{
					_powerCount++;
				}
				else if (logLine.StartsWith("[Bob] legend rank"))
				{
					//game ended
					GameEventHandler.HandleGameEnd();
					_lastGameEnd = _currentOffset;
					_turnCount = 0;
				}
				else if (logLine.StartsWith("[Zone]"))
				{
					if (_cardMovementRegex.IsMatch(logLine))
					{
						Match match = _cardMovementRegex.Match(logLine);

						var id = match.Groups["Id"].Value.Trim();
						var from = match.Groups["from"].Value.Trim();
						var to = match.Groups["to"].Value.Trim();

						var zonePos = -1;

						// Only for some log lines, should be valid in every action where we need it
						if (_opponentPlayRegex.IsMatch(logLine))
						{
							Match match2 = _opponentPlayRegex.Match(logLine);
							zonePos = Int32.Parse(match2.Groups["zonePos"].Value.Trim());
						}

						//game start/end
						if (id.Contains("HERO"))
						{
							if (!from.Contains("PLAY"))
							{
								if (to.Contains("FRIENDLY"))
									GameEventHandler.HandleGameStart(_heroIdDict[id]);
								else if (to.Contains("OPPOSING"))
									GameEventHandler.SetOpponentHero(_heroIdDict[id]);
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
									if (_powerCount >= PowerCountTreshold)
									{
										_turnCount++;
										GameEventHandler.TurnStart(Turn.Player, GetTurnNumber());
									}
									GameEventHandler.HandlePlayerDraw(id);
								}
								else
									//player discard from deck
									GameEventHandler.HandlePlayerDeckDiscard(id);
								break;
							case "FRIENDLY HAND":
								if (to == "FRIENDLY DECK")
									GameEventHandler.HandlePlayerMulligan(id);
								else if (to == "FRIENDLY PLAY")
									GameEventHandler.HandlePlayerPlay(id);
								else 
									//player discard from hand and spells
									GameEventHandler.HandlePlayerHandDiscard(id);

								break;
							case "OPPOSING HAND":
								if (to == "OPPOSING DECK")
									//opponent mulligan
									GameEventHandler.HandleOpponentMulligan(zonePos);
								else
								{
									if (to == "OPPOSING SECRET")
										GameEventHandler.HandleOpponentSecretPlayed();

									GameEventHandler.HandleOpponentPlay(id, zonePos, GetTurnNumber());
								}
								break;
							case "OPPOSING DECK":
								if (to == "OPPOSING HAND")
								{
									if (_powerCount >= PowerCountTreshold)
									{
										_turnCount++;
										GameEventHandler.TurnStart(Turn.Opponent, GetTurnNumber());
									}

									//opponent draw
									GameEventHandler.HandlOpponentDraw(GetTurnNumber());
								}
								else
									//opponent discard from deck
									GameEventHandler.HandleOpponentDeckDiscard(id);
								break;
							case "OPPOSING SECRET":
								//opponent secret triggered
								GameEventHandler.HandleOpponentSecretTrigger(id);
								break;
							case "OPPOSING PLAY":
								if (to == "OPPOSING HAND") //card from play back to hand (sap/brew)
									GameEventHandler.HandleOpponentPlayToHand(id, GetTurnNumber());
								break;
							default:
								if (to == "OPPOSING HAND")
									//coin, thoughtsteal etc
									GameEventHandler.HandleOpponentGet(GetTurnNumber());
								else if (to == "FRIENDLY HAND")
									//coin, thoughtsteal etc
									GameEventHandler.HandlePlayerGet(id);
								else if (to == "OPPOSING GRAVEYARD" && from == "" && id != "")
								{
									//todo: not sure why those two are here
									//CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlay, id));
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
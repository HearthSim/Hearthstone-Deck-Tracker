#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

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
		OpponentPlay,
		OpponentSecretTrigger,
		OpponentDeckDiscard,
		OpponentHandDiscard,
		PlayerGet,
		OpponentPlayToHand,
	}

	public enum OpponentHandMovement
	{
		Draw,
		Play,
		Mulligan,
		FromPlayerDeck
	}

	public enum AnalyzingState
	{
		Start,
		End
	}

	public enum Turn
	{
		Player,
		Opponent
	}

	public class CardMovementArgs : EventArgs
	{
		public CardMovementArgs(CardMovementType movementType, string cardId, int fromZonePos = -1)
		{
			MovementType = movementType;
			CardId = cardId;
			From = fromZonePos;
		}

		public CardMovementType MovementType { get; private set; }
		public string CardId { get; private set; }
		public int From { get; private set; }
	}

	public class GameStateArgs : EventArgs
	{
		public GameState? State { get; set; }
		public string PlayerHero { get; set; }
		public string OpponentHero { get; set; }

		public GameStateArgs() { }
		public GameStateArgs(GameState state)
		{
			State = state;
		}

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
		public CardPosChangeArgs(OpponentHandMovement action, int @from, int turn, string id)
		{
			Turn = turn;
			Action = action;
			From = from;
			Id = id;
		}

		public OpponentHandMovement Action { get; private set; }
		public int Turn { get; private set; }
		public int From { get; private set; }
		public string Id { get; private set; }
	}

	public class HsLogReader
	{
		#region Delegates
		/*
		public delegate void AnalyzingHandler(HsLogReader sender, AnalyzingArgs args);

		public delegate void CardMovementHandler(HsLogReader sender, CardMovementArgs args);

		public delegate void CardPosChangeHandler(HsLogReader sender, CardPosChangeArgs args);

		public delegate void GameStateHandler(HsLogReader sender, GameStateArgs args);

		public delegate void TurnStartHandler(HsLogReader sender, TurnStartArgs args);

		public delegate void SecretPlayedHandler(HsLogReader sender);
		*/
		#endregion

		#region Events

		//public event CardMovementHandler CardMovement;
		//public event GameStateHandler GameStateChange;
		//public event AnalyzingHandler Analyzing;
		//public event TurnStartHandler TurnStart;
		//public event CardPosChangeHandler CardPosChange;
		//public event SecretPlayedHandler SecretPlayed;

		#endregion

		#region Properties

		private const int PowerCountTreshold = 14;

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
				{"HERO_09", "Priest"}
			};

		private readonly Regex _opponentPlayRegex = new Regex(@"\w*(zonePos=(?<zonePos>(\d))).*(zone\ from\ OPPOSING\ HAND).*");

		private readonly int _updateDelay;

		//should be about 90,000 lines
		private const int MaxFileLength = 3000000;

		private long _currentOffset;
		private bool _doUpdate;
		private bool _first;
		private long _lastGameEnd;
		private int _powerCount;
		private long _previousSize;
		private int _turnCount;

		#endregion




		#region Moved Events

		private void CardMovement(HsLogReader sender, CardMovementArgs args)
		{
			Logger.WriteLine(string.Format("{0} (id:{1} turn:{2} from:{3})", args.MovementType.ToString(), args.CardId, sender.GetTurnNumber(), args.From), "LogReader");

			switch (args.MovementType)
			{
				case CardMovementType.PlayerGet:
					HandlePlayerGet(args.CardId);
					break;
				case CardMovementType.PlayerDraw:
					HandlePlayerDraw(args.CardId);
					break;
				case CardMovementType.PlayerMulligan:
					HandlePlayerMulligan(args.CardId);
					break;
				case CardMovementType.PlayerHandDiscard:
					HandlePlayerHandDiscard(args.CardId);
					break;
				case CardMovementType.PlayerPlay:
					HandlePlayerPlay(args.CardId);
					break;
				case CardMovementType.PlayerDeckDiscard:
					HandlePlayerDeckDiscard(args.CardId);
					break;
				case CardMovementType.OpponentSecretTrigger:
					HandleOpponentSecretTrigger(args.CardId);
					break;
				case CardMovementType.OpponentPlay:
					//moved to CardPosChange
					break;
				case CardMovementType.OpponentHandDiscard:
					//moved to CardPosChange (included in play)
					break;
				case CardMovementType.OpponentDeckDiscard:
					HandleOpponentDeckDiscard(args.CardId);
					break;
				case CardMovementType.OpponentPlayToHand:
					HandleOpponentPlayToHand(args.CardId, sender.GetTurnNumber());
					break;
				default:
					Logger.WriteLine("Invalid card movement");
					break;
			}
		}

		private void GameStateChange(HsLogReader sender, GameStateArgs args)
		{
			if (!string.IsNullOrEmpty(args.PlayerHero))
			{
				Game.Instance.PlayingAs = args.PlayerHero;
				Logger.WriteLine("Playing as " + args.PlayerHero, "Hearthstone");

			}
			if (!string.IsNullOrEmpty(args.OpponentHero))
			{
				Game.Instance.PlayingAgainst = args.OpponentHero;
				Logger.WriteLine("Playing against " + args.OpponentHero, "Hearthstone");
			}

			if (args.State != null)
			{
				switch (args.State)
				{
					case GameState.GameBegin:
						HandleGameStart();
						break;
					case GameState.GameEnd:
						HandleGameEnd();
						break;
				}
			}
		}

		private void Analyzing(HsLogReader sender, AnalyzingArgs args)
		{
			if (args.State == AnalyzingState.Start)
			{

			}
			else if (args.State == AnalyzingState.End)
			{
				//reader done analyzing new stuff, update things
				if (Helper.MainWindow._overlay.IsVisible)
					Helper.MainWindow._overlay.Update(false);

				if (Helper.MainWindow._playerWindow.IsVisible)
					Helper.MainWindow._playerWindow.SetCardCount(Game.Instance.PlayerHandCount, 30 - Game.Instance.PlayerDrawn.Sum(card => card.Count));

				if (Helper.MainWindow._opponentWindow.IsVisible)
					Helper.MainWindow._opponentWindow.SetOpponentCardCount(Game.Instance.OpponentHandCount, Game.Instance.OpponentDeckCount, Game.Instance.OpponentHasCoin);


				if (Helper.MainWindow._showIncorrectDeckMessage && !Helper.MainWindow._showingIncorrectDeckMessage)
				{
					Helper.MainWindow._showingIncorrectDeckMessage = true;
					Helper.MainWindow.ShowIncorrectDeckMessage();
				}

			}
		}

		private void TurnStart(HsLogReader sender, TurnStartArgs args)
		{
			Logger.WriteLine(string.Format("{0}-turn ({1})", args.Turn, sender.GetTurnNumber() + 1), "LogReader");
			//doesn't really matter whose turn it is for now, just restart timer
			//maybe add timer to player/opponent windows
			TurnTimer.Instance.SetCurrentPlayer(args.Turn);
			TurnTimer.Instance.Restart();
			if (args.Turn == Turn.Player && !Game.Instance.IsInMenu)
			{
				if (Config.Instance.FlashHs)
					User32.FlashHs();

				if (Config.Instance.BringHsToForeground)
					User32.BringHsToForeground();
			}

		}

		private void CardPosChange(HsLogReader sender, CardPosChangeArgs args)
		{
			Logger.WriteLine(string.Format("Opponent{0} (id:{1} turn:{2} from:{3})", args.Action.ToString(), args.Id, args.Turn, args.From), "LogReader");
			switch (args.Action)
			{
				case OpponentHandMovement.Draw:
					Game.Instance.OpponentDraw(args);
					break;
				case OpponentHandMovement.Play:
					Game.Instance.OpponentPlay(args);
					break;
				case OpponentHandMovement.Mulligan:
					HandleOpponentMulligan(args.From);
					break;
				case OpponentHandMovement.FromPlayerDeck:
					Game.Instance.OpponentGet(args.Turn);
					break;
			}
		}

		private void SecretPlayed(HsLogReader sender)
		{
			Game.Instance.OpponentSecretCount++;
			Helper.MainWindow._overlay.ShowSecrets(Game.Instance.PlayingAgainst);
		}

		/****************************************************************************************/
		/****************************************************************************************/

		private void HandleGameStart()
		{
			//avoid new game being started when jaraxxus is played
			if (!Game.Instance.IsInMenu) return;

			Logger.WriteLine("Game start");

			if (Config.Instance.FlashHs)
				User32.FlashHs();
			if (Config.Instance.BringHsToForeground)
				User32.BringHsToForeground();

			if (Config.Instance.KeyPressOnGameStart != "None" && Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
			{
				System.Windows.Forms.SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameStart + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameStart);
			}

			var selectedDeck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			if (selectedDeck != null)
				Game.Instance.SetPremadeDeck((Hearthstone_Deck_Tracker.Hearthstone.Deck)selectedDeck.Clone());

			Game.Instance.IsInMenu = false;
			Game.Instance.Reset();

			//select deck based on hero
			if (!string.IsNullOrEmpty(Game.Instance.PlayingAs))
			{
				if (!Game.Instance.IsUsingPremade || !Config.Instance.AutoDeckDetection) return;

				if (selectedDeck == null || selectedDeck.Class != Game.Instance.PlayingAs)
				{
					var classDecks = Helper.MainWindow._deckList.DecksList.Where(d => d.Class == Game.Instance.PlayingAs).ToList();
					if (classDecks.Count == 0)
					{
						Logger.WriteLine("Found no deck to switch to", "HandleGameStart");
					}
					else if (classDecks.Count == 1)
					{
						Helper.MainWindow.DeckPickerList.SelectDeck(classDecks[0]);
						Logger.WriteLine("Found deck to switch to: " + classDecks[0].Name, "HandleGameStart");
					}
					else if (Helper.MainWindow._deckList.LastDeckClass.Any(ldc => ldc.Class == Game.Instance.PlayingAs))
					{
						var lastDeckName = Helper.MainWindow._deckList.LastDeckClass.First(ldc => ldc.Class == Game.Instance.PlayingAs).Name;
						Logger.WriteLine("Found more than 1 deck to switch to - last played: " + lastDeckName, "HandleGameStart");

						var deck = Helper.MainWindow._deckList.DecksList.FirstOrDefault(d => d.Name == lastDeckName);

						if (deck != null)
						{
							Helper.MainWindow.DeckPickerList.SelectDeck(deck);
							Helper.MainWindow.UpdateDeckList(deck);
							Helper.MainWindow.UseDeck(deck);
						}
					}
				}
			}
		}

		private void HandleGameEnd()
		{
			Logger.WriteLine("Game end");
			if (Config.Instance.KeyPressOnGameEnd != "None" && Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
			{
				System.Windows.Forms.SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameEnd + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameEnd);
			}
			TurnTimer.Instance.Stop();
			Helper.MainWindow._overlay.HideTimers();
			Helper.MainWindow._overlay.HideSecrets();
			if (Config.Instance.SavePlayedGames && !Game.Instance.IsInMenu)
			{
				Helper.MainWindow.SavePlayedCards();
			}
			if (!Config.Instance.KeepDecksVisible)
			{
				var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
				if (deck != null)
					Game.Instance.SetPremadeDeck((Hearthstone_Deck_Tracker.Hearthstone.Deck)deck.Clone());

				Game.Instance.Reset();
			}
			Game.Instance.IsInMenu = true;
		}

		public void HandleOpponentPlayToHand(string cardId, int turn)
		{
			Game.Instance.OpponentBackToHand(cardId, turn);
		}

		private void HandlePlayerGet(string cardId)
		{
			Game.Instance.PlayerGet(cardId);
		}

		private void HandlePlayerDraw(string cardId)
		{
			var correctDeck = Game.Instance.PlayerDraw(cardId);

			if (!correctDeck && Config.Instance.AutoDeckDetection && !Helper.MainWindow._showIncorrectDeckMessage &&
				!Helper.MainWindow._showingIncorrectDeckMessage && Game.Instance.IsUsingPremade)
			{
				Helper.MainWindow._showIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck");
			}
		}

		private void HandlePlayerMulligan(string cardId)
		{
			TurnTimer.Instance.MulliganDone(Turn.Player);
			Game.Instance.Mulligan(cardId);

			//without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
			Helper.MainWindow._overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow._playerWindow.ListViewPlayer.Items.Refresh();
		}

		private void HandlePlayerHandDiscard(string cardId)
		{
			Game.Instance.PlayerHandDiscard(cardId);
			Helper.MainWindow._overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow._playerWindow.ListViewPlayer.Items.Refresh();
		}

		private void HandlePlayerPlay(string cardId)
		{
			Game.Instance.PlayerPlayed(cardId);
			Helper.MainWindow._overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow._playerWindow.ListViewPlayer.Items.Refresh();
		}

		private void HandlePlayerDeckDiscard(string cardId)
		{
			var correctDeck = Game.Instance.PlayerDeckDiscard(cardId);

			//don't think this will ever detect an incorrect deck but who knows...
			if (!correctDeck && Config.Instance.AutoDeckDetection && !Helper.MainWindow._showIncorrectDeckMessage &&
				!Helper.MainWindow._showingIncorrectDeckMessage && Game.Instance.IsUsingPremade)
			{
				Helper.MainWindow._showIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck", "HandlePlayerDiscard");
			}
		}

		private void HandleOpponentSecretTrigger(string cardId)
		{
			Game.Instance.OpponentSecretTriggered(cardId);
			Game.Instance.OpponentSecretCount--;
			if (Game.Instance.OpponentSecretCount <= 0)
			{
				Helper.MainWindow._overlay.HideSecrets();
			}
		}

		private void HandleOpponentMulligan(int pos)
		{
			TurnTimer.Instance.MulliganDone(Turn.Opponent);
			Game.Instance.OpponentMulligan(pos);
		}

		private void HandleOpponentDeckDiscard(string cardId)
		{
			Game.Instance.OpponentDeckDiscard(cardId);

			//there seems to be an issue with the overlay not updating here.
			//possibly a problem with order of logs?
			Helper.MainWindow._overlay.ListViewOpponent.Items.Refresh();
			Helper.MainWindow._opponentWindow.ListViewOpponent.Items.Refresh();
		}

		#endregion



		public static HsLogReader Instance { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hsDirPath"></param>
		/// <param name="updateDelay"></param>
		public static void Create()
		{
			Instance = new HsLogReader();
		}

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

		public int GetTurnNumber()
		{
			return (_turnCount) / 2;
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
					GameStateChange(this, new GameStateArgs(GameState.GameEnd));
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
							zonePos = int.Parse(match2.Groups["zonePos"].Value.Trim());
						}

						//game start/end
						if (id.Contains("HERO"))
						{
							if (!from.Contains("PLAY"))
							{
								if (to.Contains("FRIENDLY"))
									GameStateChange(this, new GameStateArgs(GameState.GameBegin) { PlayerHero = _heroIdDict[id] });
								else if (to.Contains("OPPOSING"))
									GameStateChange(this, new GameStateArgs { OpponentHero = _heroIdDict[id] });
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
								if (to == "FRIENDLY DECK")		//player mulligan
									CardMovement(this, new CardMovementArgs(CardMovementType.PlayerMulligan, id));
								else if (to == "FRIENDLY PLAY")	//player played
									CardMovement(this, new CardMovementArgs(CardMovementType.PlayerPlay, id));
								else							//player discard from hand and spells
									CardMovement(this, new CardMovementArgs(CardMovementType.PlayerHandDiscard, id));

								break;
							case "OPPOSING HAND":
								if (to == "OPPOSING DECK")
								{
									//opponent mulligan
									CardPosChange(this, new CardPosChangeArgs(OpponentHandMovement.Mulligan, zonePos, GetTurnNumber(), id));
								}
								else
								{
									if (to == "OPPOSING SECRET")
										SecretPlayed(this);
									//if (SecretPlayed != null) SecretPlayed(this);

									CardPosChange(this, new CardPosChangeArgs(OpponentHandMovement.Play, zonePos, GetTurnNumber(), id));
								}
								break;
							case "OPPOSING DECK":
								if (to == "OPPOSING HAND")
								{
									if (_powerCount >= PowerCountTreshold)
									{
										TurnStart(this, new TurnStartArgs(Turn.Opponent));
										_turnCount++;
									}

									//opponent draw
									CardPosChange(this, new CardPosChangeArgs(OpponentHandMovement.Draw, zonePos, GetTurnNumber(), id));
								}
								else
								{
									//opponent discard from deck
									CardMovement(this, new CardMovementArgs(CardMovementType.OpponentDeckDiscard, id));
								}
								break;
							case "OPPOSING SECRET":
								//opponent secret triggered
								CardMovement(this, new CardMovementArgs(CardMovementType.OpponentSecretTrigger, id));
								break;
							case "OPPOSING PLAY":
								if (to == "OPPOSING HAND")		//card from play back to hand (sap/brew)
									CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlayToHand, id));
								break;
							default:
								if (to == "OPPOSING HAND")
								{
									//coin, thoughtsteal etc
									CardPosChange(this, new CardPosChangeArgs(OpponentHandMovement.FromPlayerDeck, zonePos, GetTurnNumber(), id));
								}
								else if (to == "OPPOSING GRAVEYARD" && from == "" && id != "")
								{
									//CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlay, id));
								}
								else if (to == "FRIENDLY HAND")
								{
									//coin, thoughtsteal etc
									CardMovement(this, new CardMovementArgs(CardMovementType.PlayerGet, id));
									if (_turnCount < 2 && id == "GAME_005")
									{
										//increment turn count once in case player goes second (check turn count to avoid this from happening when thoughsteal takes coin)
										_turnCount++;
									}
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
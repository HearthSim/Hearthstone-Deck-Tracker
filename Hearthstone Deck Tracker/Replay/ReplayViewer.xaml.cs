#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Replay.Controls;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

#endregion

namespace Hearthstone_Deck_Tracker.Replay
{
	/// <summary>
	/// Interaction logic for ReplayViewer.xaml
	/// </summary>
	public partial class ReplayViewer
	{
		private readonly List<int> _collapsedTurns;
		private readonly bool _initialized;
		private readonly List<int> _showAllTurns;
		private ReplayKeyPoint _currentGameState;
		private int _opponentController;
		private int _playerController;
		public List<ReplayKeyPoint> Replay;

		public ReplayViewer()
		{
			InitializeComponent();
			Height = Config.Instance.ReplayWindowHeight;
			Width = Config.Instance.ReplayWindowWidth;
			if(Config.Instance.ReplayWindowLeft.HasValue)
				Left = Config.Instance.ReplayWindowLeft.Value;
			if(Config.Instance.ReplayWindowTop.HasValue)
				Top = Config.Instance.ReplayWindowTop.Value;

			var titleBarCorners = new[]
			{
				new System.Drawing.Point((int)Left + 5, (int)Top + 5),
				new System.Drawing.Point((int)(Left + Width) - 5, (int)Top + 5),
				new System.Drawing.Point((int)Left + 5, (int)(Top + TitlebarHeight) - 5),
				new System.Drawing.Point((int)(Left + Width) - 5, (int)(Top + TitlebarHeight) - 5)
			};
			if(!Screen.AllScreens.Any(s => titleBarCorners.Any(c => s.WorkingArea.Contains(c))))
			{
				Top = 100;
				Left = 100;
			}
			_collapsedTurns = new List<int>();
			_showAllTurns = new List<int>();
			CheckBoxAttack.IsChecked = Config.Instance.ReplayViewerShowAttack;
			CheckBoxDeath.IsChecked = Config.Instance.ReplayViewerShowDeath;
			CheckBoxDiscard.IsChecked = Config.Instance.ReplayViewerShowDiscard;
			CheckBoxDraw.IsChecked = Config.Instance.ReplayViewerShowDraw;
			CheckBoxHeroPower.IsChecked = Config.Instance.ReplayViewerShowHeroPower;
			CheckBoxPlay.IsChecked = Config.Instance.ReplayViewerShowPlay;
			CheckBoxSecret.IsChecked = Config.Instance.ReplayViewerShowSecret;
			CheckBoxSummon.IsChecked = Config.Instance.ReplayViewerShowSummon;
			_initialized = true;
		}

		private Entity PlayerEntity
		{
			get { return _currentGameState == null ? null : _currentGameState.Data.First(x => x.IsPlayer); }
		}

		private Entity OpponentEntity
		{
			get { return _currentGameState == null ? null : _currentGameState.Data.First(x => x.HasTag(GAME_TAG.PLAYER_ID) && !x.IsPlayer); }
		}

		private IEnumerable<BoardEntity> BoardEntites
		{
			get
			{
				return new[]
				{
					OpponentBoardEntity0,
					OpponentBoardEntity1,
					OpponentBoardEntity2,
					OpponentBoardEntity3,
					OpponentBoardEntity4,
					OpponentBoardEntity5,
					OpponentBoardEntity6,
					PlayerBoardEntity0,
					PlayerBoardEntity1,
					PlayerBoardEntity2,
					PlayerBoardEntity3,
					PlayerBoardEntity4,
					PlayerBoardEntity5,
					PlayerBoardEntity6,
					PlayerBoardHeroEntity,
					OpponentBoardHeroEntity
				};
			}
		}

		public Entity OpponentCardPlayed
		{
			get
			{
				var entity = _currentGameState.Data.FirstOrDefault(e => e.Id == _currentGameState.Id);
				if(entity != null && entity.IsControlledBy(_opponentController)
				   && (_currentGameState.Type == KeyPointType.Play || _currentGameState.Type == KeyPointType.PlaySpell))
				{
					entity.SetCardCount(0);
					return entity;
				}
				return null;
			}
		}

		public Entity PlayerCardPlayed
		{
			get
			{
				var entity = _currentGameState.Data.FirstOrDefault(e => e.Id == _currentGameState.Id);
				if(entity != null && entity.IsControlledBy(_playerController)
				   && (_currentGameState.Type == KeyPointType.Play || _currentGameState.Type == KeyPointType.PlaySpell))
				{
					entity.SetCardCount(0);
					return entity;
				}
				return null;
			}
		}

		public List<ReplayKeyPoint> KeyPoints
		{
			get { return Replay; }
		}

		public BitmapImage PlayerHeroImage
		{
			get
			{
				if(!Enum.GetNames(typeof(HeroClass)).Contains(PlayerHero))
					return new BitmapImage();
				var uri = new Uri(string.Format("../Resources/{0}_small.png", PlayerHero.ToLower()), UriKind.Relative);
				return new BitmapImage(uri);
			}
		}

		public string PlayerName
		{
			get
			{
				if(_currentGameState == null)
					return string.Empty;
				return PlayerEntity.Name;
			}
		}

		public string PlayerHero
		{
			get
			{
				if(_currentGameState == null)
					return string.Empty;
				var cardId = GetHero(_playerController).CardId;
				return cardId == null ? null : GameV2.GetHeroNameFromId(cardId);
			}
		}

		public int PlayerHealth
		{
			get
			{
				if(_currentGameState == null)
					return 30;
				var hero = GetHero(_playerController);
				return hero.GetTag(GAME_TAG.HEALTH) - hero.GetTag(GAME_TAG.DAMAGE);
			}
		}

		public int PlayerArmor
		{
			get
			{
				if(_currentGameState == null)
					return 0;
				return GetHero(_playerController).GetTag(GAME_TAG.ARMOR);
			}
		}

		public Visibility PlayerArmorVisibility
		{
			get { return PlayerArmor > 0 ? Visibility.Visible : Visibility.Hidden; }
		}

		public int PlayerAttack
		{
			get
			{
				if(_currentGameState == null)
					return 0;
				return GetHero(_playerController).GetTag(GAME_TAG.ATK);
			}
		}

		public Visibility PlayerAttackVisibility
		{
			get { return PlayerAttack > 0 ? Visibility.Visible : Visibility.Hidden; }
		}

		public IEnumerable<Entity> PlayerHand
		{
			get
			{
				if(_currentGameState == null)
					return new List<Entity>();
				return _currentGameState.Data.Where(x => x.IsInZone(TAG_ZONE.HAND) && x.IsControlledBy(_playerController));
				//return hand.Select(x => Game.GetCardFromId(x.CardId).Name);
			}
		}

		public IEnumerable<Entity> PlayerBoard
		{
			get
			{
				if(_currentGameState == null)
					return new List<Entity>();
				return
					_currentGameState.Data.Where(
					                             x =>
					                             x.IsInZone(TAG_ZONE.PLAY) && x.IsControlledBy(_playerController) && x.HasTag(GAME_TAG.HEALTH)
					                             && !string.IsNullOrEmpty(x.CardId) && !x.CardId.Contains("HERO"));
			}
		}

		public BitmapImage OpponentHeroImage
		{
			get
			{
				if(!Enum.GetNames(typeof(HeroClass)).Contains(OpponentHero) && OpponentHero != "Jaraxxus")
					return new BitmapImage();
				var uri = new Uri(string.Format("../Resources/{0}_small.png", OpponentHero.ToLower()), UriKind.Relative);
				return new BitmapImage(uri);
			}
		}

		public string OpponentName
		{
			get
			{
				if(_currentGameState == null)
					return string.Empty;
				return OpponentEntity.Name;
			}
		}

		public string OpponentHero
		{
			get
			{
				if(_currentGameState == null)
					return null;
				var cardId = GetHero(_opponentController).CardId;
				return cardId == null ? null : GameV2.GetHeroNameFromId(cardId);
			}
		}

		public int OpponentHealth
		{
			get
			{
				if(_currentGameState == null)
					return 30;
				var hero = GetHero(_opponentController);
				return hero.GetTag(GAME_TAG.HEALTH) - hero.GetTag(GAME_TAG.DAMAGE);
			}
		}

		public int OpponentArmor
		{
			get
			{
				if(_currentGameState == null)
					return 0;
				return GetHero(_opponentController).GetTag(GAME_TAG.ARMOR);
			}
		}

		public Visibility OpponentArmorVisibility
		{
			get { return OpponentArmor > 0 ? Visibility.Visible : Visibility.Hidden; }
		}

		public int OpponentAttack
		{
			get
			{
				if(_currentGameState == null)
					return 0;
				return GetHero(_opponentController).GetTag(GAME_TAG.ATK);
			}
		}

		public Visibility OpponentAttackVisibility
		{
			get { return OpponentAttack > 0 ? Visibility.Visible : Visibility.Hidden; }
		}

		public IEnumerable<Entity> OpponentHand
		{
			get
			{
				if(_currentGameState == null)
					return new List<Entity>();
				return _currentGameState.Data.Where(x => x.IsInZone(TAG_ZONE.HAND) && x.IsControlledBy(_opponentController));
				//return hand.Select(x => string.IsNullOrEmpty(x.CardId) ? "[unknown]" :  Game.GetCardFromId(x.CardId).Name);
			}
		}

		public IEnumerable<Entity> OpponentBoard
		{
			get
			{
				if(_currentGameState == null)
					return new List<Entity>();
				return
					_currentGameState.Data.Where(
					                             x =>
					                             x.IsInZone(TAG_ZONE.PLAY) && x.IsControlledBy(_opponentController) && x.HasTag(GAME_TAG.HEALTH)
					                             && !string.IsNullOrEmpty(x.CardId) && !x.CardId.Contains("HERO"));
				//return board.Select(x => Game.GetCardFromId(x.CardId).Name + " (" + x.GetTag(GAME_TAG.ATK) + " - " + x.GetTag(GAME_TAG.HEALTH) + ")");
			}
		}

		public Entity OpponentBoardHero
		{
			get { return GetHero(_opponentController); }
		}

		public Entity OpponentBoard0
		{
			get { return GetEntity(OpponentBoard, 0); }
		}

		public Entity OpponentBoard1
		{
			get { return GetEntity(OpponentBoard, 1); }
		}

		public Entity OpponentBoard2
		{
			get { return GetEntity(OpponentBoard, 2); }
		}

		public Entity OpponentBoard3
		{
			get { return GetEntity(OpponentBoard, 3); }
		}

		public Entity OpponentBoard4
		{
			get { return GetEntity(OpponentBoard, 4); }
		}

		public Entity OpponentBoard5
		{
			get { return GetEntity(OpponentBoard, 5); }
		}

		public Entity OpponentBoard6
		{
			get { return GetEntity(OpponentBoard, 6); }
		}

		public Entity PlayerBoardHero
		{
			get { return GetHero(_playerController); }
		}

		public Entity PlayerBoard0
		{
			get { return GetEntity(PlayerBoard, 0); }
		}

		public Entity PlayerBoard1
		{
			get { return GetEntity(PlayerBoard, 1); }
		}

		public Entity PlayerBoard2
		{
			get { return GetEntity(PlayerBoard, 2); }
		}

		public Entity PlayerBoard3
		{
			get { return GetEntity(PlayerBoard, 3); }
		}

		public Entity PlayerBoard4
		{
			get { return GetEntity(PlayerBoard, 4); }
		}

		public Entity PlayerBoard5
		{
			get { return GetEntity(PlayerBoard, 5); }
		}

		public Entity PlayerBoard6
		{
			get { return GetEntity(PlayerBoard, 6); }
		}

		public Entity PlayerCard0
		{
			get { return GetEntity(PlayerHand, 0); }
		}

		public Entity PlayerCard1
		{
			get { return GetEntity(PlayerHand, 1); }
		}

		public Entity PlayerCard2
		{
			get { return GetEntity(PlayerHand, 2); }
		}

		public Entity PlayerCard3
		{
			get { return GetEntity(PlayerHand, 3); }
		}

		public Entity PlayerCard4
		{
			get { return GetEntity(PlayerHand, 4); }
		}

		public Entity PlayerCard5
		{
			get { return GetEntity(PlayerHand, 5); }
		}

		public Entity PlayerCard6
		{
			get { return GetEntity(PlayerHand, 6); }
		}

		public Entity PlayerCard7
		{
			get { return GetEntity(PlayerHand, 7); }
		}

		public Entity PlayerCard8
		{
			get { return GetEntity(PlayerHand, 8); }
		}

		public Entity PlayerCard9
		{
			get { return GetEntity(PlayerHand, 9); }
		}

		public Entity OpponentCard0
		{
			get { return GetEntity(OpponentHand, 0); }
		}

		public Entity OpponentCard1
		{
			get { return GetEntity(OpponentHand, 1); }
		}

		public Entity OpponentCard2
		{
			get { return GetEntity(OpponentHand, 2); }
		}

		public Entity OpponentCard3
		{
			get { return GetEntity(OpponentHand, 3); }
		}

		public Entity OpponentCard4
		{
			get { return GetEntity(OpponentHand, 4); }
		}

		public Entity OpponentCard5
		{
			get { return GetEntity(OpponentHand, 5); }
		}

		public Entity OpponentCard6
		{
			get { return GetEntity(OpponentHand, 6); }
		}

		public Entity OpponentCard7
		{
			get { return GetEntity(OpponentHand, 7); }
		}

		public Entity OpponentCard8
		{
			get { return GetEntity(OpponentHand, 8); }
		}

		public Entity OpponentCard9
		{
			get { return GetEntity(OpponentHand, 9); }
		}

		public Entity PlayerWeapon
		{
			get
			{
				if(_currentGameState == null)
					return null;
				var weaponId = PlayerEntity.GetTag(GAME_TAG.EQUIPPED_WEAPON);
				if(weaponId == 0)
					return null;
				var entity = _currentGameState.Data.FirstOrDefault(x => x.Id == weaponId);
				if(entity != null)
					entity.SetCardCount(entity.GetTag(GAME_TAG.DURABILITY) - entity.GetTag(GAME_TAG.DAMAGE));
				return entity;
			}
		}

		public Entity OpponentWeapon
		{
			get
			{
				if(_currentGameState == null)
					return null;
				var weaponId = OpponentEntity.GetTag(GAME_TAG.EQUIPPED_WEAPON);
				if(weaponId == 0)
					return null;
				var entity = _currentGameState.Data.FirstOrDefault(x => x.Id == weaponId);
				if(entity != null)
					entity.SetCardCount(entity.GetTag(GAME_TAG.DURABILITY) - entity.GetTag(GAME_TAG.DAMAGE));
				return entity;
			}
		}

		public Visibility OpponentSecretVisibility
		{
			get { return OpponentSecrets.Any() ? Visibility.Visible : Visibility.Collapsed; }
		}

		private IEnumerable<Entity> OpponentSecrets
		{
			get
			{
				return _currentGameState == null
					       ? new List<Entity>()
					       : _currentGameState.Data.Where(
					                                      x =>
					                                      x.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.SECRET && x.IsControlledBy(_opponentController));
			}
		}

		private IEnumerable<Entity> PlayerSecrets
		{
			get
			{
				return _currentGameState == null
					       ? new List<Entity>()
					       : _currentGameState.Data.Where(x => x.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.SECRET && x.IsControlledBy(_playerController));
			}
		}

		public Visibility PlayerSecretVisibility
		{
			get { return PlayerSecrets.Any() ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Entity OpponentSecret0
		{
			get { return OpponentSecrets.Any() ? OpponentSecrets.ToArray()[0] : null; }
		}

		public Entity OpponentSecret1
		{
			get { return OpponentSecrets.Count() > 1 ? OpponentSecrets.ToArray()[1] : null; }
		}

		public Entity OpponentSecret2
		{
			get { return OpponentSecrets.Count() > 2 ? OpponentSecrets.ToArray()[2] : null; }
		}

		public Entity OpponentSecret3
		{
			get { return OpponentSecrets.Count() > 3 ? OpponentSecrets.ToArray()[3] : null; }
		}

		public Entity OpponentSecret4
		{
			get { return OpponentSecrets.Count() > 4 ? OpponentSecrets.ToArray()[4] : null; }
		}

		public Entity PlayerSecret0
		{
			get { return PlayerSecrets.Any() ? PlayerSecrets.ToArray()[0] : null; }
		}

		public Entity PlayerSecret1
		{
			get { return PlayerSecrets.Count() > 1 ? PlayerSecrets.ToArray()[1] : null; }
		}

		public Entity PlayerSecret2
		{
			get { return PlayerSecrets.Count() > 2 ? PlayerSecrets.ToArray()[2] : null; }
		}

		public Entity PlayerSecret3
		{
			get { return PlayerSecrets.Count() > 3 ? PlayerSecrets.ToArray()[3] : null; }
		}

		public Entity PlayerSecret4
		{
			get { return PlayerSecrets.Count() > 4 ? PlayerSecrets.ToArray()[4] : null; }
		}

		public SolidColorBrush PlayerHealthTextColor
		{
			get
			{
				if(_currentGameState == null)
					return new SolidColorBrush(Colors.White);
				var hero =
					_currentGameState.Data.FirstOrDefault(
					                                      x =>
					                                      x.IsControlledBy(_playerController) && !string.IsNullOrEmpty(x.CardId)
					                                      && x.CardId.Contains("HERO"));
				return new SolidColorBrush((hero != null && hero.GetTag(GAME_TAG.DAMAGE) > 0) ? Colors.Red : Colors.White);
			}
		}

		public SolidColorBrush OpponentHealthTextColor
		{
			get
			{
				if(_currentGameState == null)
					return new SolidColorBrush(Colors.White);
				var hero =
					_currentGameState.Data.FirstOrDefault(
					                                      x =>
					                                      x.IsControlledBy(_opponentController) && !string.IsNullOrEmpty(x.CardId)
					                                      && x.CardId.Contains("HERO"));
				return new SolidColorBrush((hero != null && hero.GetTag(GAME_TAG.DAMAGE) > 0) ? Colors.Red : Colors.White);
			}
		}

		public string PlayerMana
		{
			get
			{
				if(_currentGameState == null)
					return "0/0";
				var total = PlayerEntity.GetTag(GAME_TAG.RESOURCES);
				var current = total - PlayerEntity.GetTag(GAME_TAG.RESOURCES_USED);
				return current + "/" + total;
			}
		}

		public string OpponentMana
		{
			get
			{
				if(_currentGameState == null)
					return "0/0";
				var total = OpponentEntity.GetTag(GAME_TAG.RESOURCES);
				var current = total - OpponentEntity.GetTag(GAME_TAG.RESOURCES_USED);
				return current + "/" + total;
			}
		}

		public void Load(List<ReplayKeyPoint> replay)
		{
			if(replay == null || replay.Count == 0)
				return;
			var selectedKeypoint = DataGridKeyPoints.SelectedItem as TurnViewItem;
			DataGridKeyPoints.Items.Clear();
			Replay = replay;
			_currentGameState = Replay.FirstOrDefault(r => r.Data.Any(x => x.HasTag(GAME_TAG.PLAYER_ID)));
			if(_currentGameState == null)
			{
				Logger.WriteLine("Error loading replay. No player entity found.");
				return;
			}
			_playerController = PlayerEntity.GetTag(GAME_TAG.CONTROLLER);
			_opponentController = OpponentEntity.GetTag(GAME_TAG.CONTROLLER);
			var currentTurn = -1;
			TurnViewItem tvi = null;
			foreach(var kp in Replay)
			{
				var entity = kp.Data.FirstOrDefault(x => x.Id == kp.Id);
				if(entity == null || (string.IsNullOrEmpty(entity.CardId) && kp.Type != KeyPointType.Victory && kp.Type != KeyPointType.Defeat))
					continue;
				if(kp.Type == KeyPointType.Summon && entity.GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.ENCHANTMENT)
					continue;
				var turn = (kp.Turn + 1) / 2;
				if(turn == 1)
				{
					if(!kp.Data.Any(x => x.HasTag(GAME_TAG.PLAYER_ID) && x.GetTag(GAME_TAG.RESOURCES) == 1))
						turn = 0;
				}
				if(turn > currentTurn)
				{
					currentTurn = turn;
					if(tvi != null && tvi.IsTurnRow && tvi.Turn.HasValue && !_collapsedTurns.Contains(tvi.Turn.Value))
						DataGridKeyPoints.Items.Remove(tvi); //remove empty turns
					tvi = new TurnViewItem {Turn = turn, IsCollapsed = _collapsedTurns.Contains(turn), ShowAll = _showAllTurns.Contains(turn)};
					DataGridKeyPoints.Items.Add(tvi);
				}
				if(!_showAllTurns.Contains(turn))
				{
					switch(kp.Type)
					{
						case KeyPointType.Attack:
							if(!Config.Instance.ReplayViewerShowAttack)
								continue;
							break;
						case KeyPointType.Death:
							if(!Config.Instance.ReplayViewerShowDeath)
								continue;
							break;
						case KeyPointType.Mulligan:
						case KeyPointType.DeckDiscard:
						case KeyPointType.HandDiscard:
							if(!Config.Instance.ReplayViewerShowDiscard)
								continue;
							break;
						case KeyPointType.Draw:
						case KeyPointType.Obtain:
						case KeyPointType.PlayToDeck:
						case KeyPointType.PlayToHand:
							if(!Config.Instance.ReplayViewerShowDraw)
								continue;
							break;
						case KeyPointType.HeroPower:
							if(!Config.Instance.ReplayViewerShowHeroPower)
								continue;
							break;
						case KeyPointType.SecretStolen:
						case KeyPointType.SecretTriggered:
							if(!Config.Instance.ReplayViewerShowSecret)
								continue;
							break;
						case KeyPointType.Play:
						case KeyPointType.PlaySpell:
						case KeyPointType.SecretPlayed:
							if(!Config.Instance.ReplayViewerShowPlay)
								continue;
							break;
						case KeyPointType.Summon:
							if(!Config.Instance.ReplayViewerShowSummon)
								continue;
							break;
					}
				}
				if(_collapsedTurns.Contains(turn))
					continue;
				tvi = new TurnViewItem();
				if(kp.Player == ActivePlayer.Player)
				{
					tvi.PlayerAction = kp.Type.ToString();
					tvi.AdditionalInfoPlayer = kp.GetAdditionalInfo();
				}
				else
				{
					tvi.OpponentAction = kp.Type.ToString();
					tvi.AdditionalInfoOpponent = kp.GetAdditionalInfo();
				}
				tvi.KeyPoint = kp;
				DataGridKeyPoints.Items.Add(tvi);
			}
			if(selectedKeypoint != null)
			{
				var newSelection = selectedKeypoint.Turn.HasValue
					                   ? DataGridKeyPoints.Items.Cast<TurnViewItem>()
					                                      .FirstOrDefault(x => x.Turn.HasValue && x.Turn.Value == selectedKeypoint.Turn.Value)
					                   : DataGridKeyPoints.Items.Cast<TurnViewItem>().FirstOrDefault(x => x.KeyPoint == selectedKeypoint.KeyPoint);
				if(newSelection != null)
				{
					DataGridKeyPoints.SelectedItem = newSelection;
					DataGridKeyPoints.ScrollIntoView(newSelection);
					var index = DataGridKeyPoints.Items.IndexOf(newSelection);
					DataGridRow dgrow = (DataGridRow)DataGridKeyPoints.ItemContainerGenerator.ContainerFromItem(DataGridKeyPoints.Items[index]);
					dgrow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
				}
			}
			DataContext = this;
		}

		public void ReloadKeypoints()
		{
			Load(Replay);
		}

		private async void Update()
		{
			DataContext = null;
			DataContext = this;

			var attackArrowVisibility = Visibility.Hidden;
			var playArrowVisibility = Visibility.Hidden;
			if(_currentGameState.Type == KeyPointType.Attack)
			{
				await Task.Delay(100);
				var attackerId = _currentGameState.Data[0].GetTag(GAME_TAG.PROPOSED_ATTACKER);
				var defenderId = _currentGameState.Data[0].GetTag(GAME_TAG.PROPOSED_DEFENDER);
				var attacker = BoardEntites.FirstOrDefault(x => x.DataContext != null && ((Entity)x.DataContext).Id == attackerId);
				var defender = BoardEntites.FirstOrDefault(x => x.DataContext != null && ((Entity)x.DataContext).Id == defenderId);
				if(attacker != null && defender != null)
				{
					var attackerTop = GetPosition(attacker).Y < GetPosition(defender).Y;
					var a = GetCenterPos(attacker, attackerTop);
					var b = GetCenterPos(defender, !attackerTop);
					AttackArrow.X1 = a.X;
					AttackArrow.Y1 = a.Y;
					AttackArrow.X2 = b.X;
					AttackArrow.Y2 = b.Y;
					attackArrowVisibility = Visibility.Visible;
				}
			}
			else if(_currentGameState.Type == KeyPointType.PlaySpell)
			{
				await Task.Delay(100);
				var entity = _currentGameState.Data.FirstOrDefault(e => e.Id == _currentGameState.Id);
				if(entity != null && entity.HasTag(GAME_TAG.CARD_TARGET))
				{
					var targetId = entity.GetTag(GAME_TAG.CARD_TARGET);
					var boardEntity = BoardEntites.FirstOrDefault(x => x.DataContext != null && ((Entity)x.DataContext).Id == targetId);
					if(boardEntity != null)
					{
						var top = entity.IsControlledBy(_opponentController);
						var cardPos = GetCenterPos(entity.IsControlledBy(_opponentController) ? OpponentCardEntityPlayed : PlayerCardEntityPlayed, top);
						var boardPos = GetCenterPos(boardEntity, !top);
						AttackArrow.X1 = cardPos.X;
						AttackArrow.Y1 = cardPos.Y;
						AttackArrow.X2 = boardPos.X;
						AttackArrow.Y2 = boardPos.Y;
						attackArrowVisibility = Visibility.Visible;
					}
				}
			}
			AttackArrow.Visibility = attackArrowVisibility;

			if(_currentGameState.Type == KeyPointType.Play)
			{
				await Task.Delay(100);
				var entity = _currentGameState.Data.FirstOrDefault(e => e.Id == _currentGameState.Id);
				if(entity != null)
				{
					var boardEntity = BoardEntites.FirstOrDefault(x => x.DataContext != null && ((Entity)x.DataContext).Id == entity.Id);
					if(boardEntity != null)
					{
						var top = entity.IsControlledBy(_opponentController);
						var cardPos = GetCenterPos(entity.IsControlledBy(_opponentController) ? OpponentCardEntityPlayed : PlayerCardEntityPlayed, top);
						var boardPos = GetCenterPos(boardEntity, !top);
						PlayArrow.X1 = cardPos.X;
						PlayArrow.Y1 = cardPos.Y;
						PlayArrow.X2 = boardPos.X;
						PlayArrow.Y2 = boardPos.Y;
						playArrowVisibility = Visibility.Visible;
					}
				}
			}
			PlayArrow.Visibility = playArrowVisibility;
		}

		private Point GetCenterPos(BoardEntity entity, bool top)
		{
			var xOffset = entity.ActualWidth / 2;
			double yOffset;
			if(entity == PlayerBoardHeroEntity)
				yOffset = -31;
			else if(entity == OpponentBoardHeroEntity)
				yOffset = -28;
			else
			{
				yOffset = entity.ActualHeight / 2;
				if(!top)
					yOffset *= -1;
			}
			var x = GetPosition(entity).X + xOffset;
			var y = GetPosition(entity).Y + yOffset;
			return new Point(x, y);
		}

		public Point GetCenterPos(CardEntity entity, bool top)
		{
			var xOffset = entity.ActualWidth / 2;
			var yOffset = entity.ActualHeight / 2;
			if(top)
				yOffset -= 12;
			else
			{
				yOffset += 12;
				yOffset *= -1;
			}
			var x = GetPosition(entity).X + xOffset;
			var y = GetPosition(entity).Y + yOffset;
			return new Point(x, y);
		}

		private Point GetPosition(Visual element)
		{
			var positionTransform = element.TransformToAncestor(this);
			var areaPosition = positionTransform.Transform(new Point(0, 0));

			return areaPosition;
		}

		private Entity GetHero(int controller)
		{
			var heroEntityId = controller == _playerController
				                   ? PlayerEntity.GetTag(GAME_TAG.HERO_ENTITY) : OpponentEntity.GetTag(GAME_TAG.HERO_ENTITY);

			return _currentGameState.Data.FirstOrDefault(x => x.Id == heroEntityId) ?? new Entity();
		}

		private Entity GetEntity(IEnumerable<Entity> zone, int index)
		{
			return zone.FirstOrDefault(x => x.HasTag(GAME_TAG.ZONE_POSITION) && x.GetTag(GAME_TAG.ZONE_POSITION) == index + 1);
		}

		private void DataGridKeyPoints_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			var selected = ((DataGrid)sender).SelectedItem as TurnViewItem;
			if(selected == null || selected.KeyPoint == null)
				return;
			_currentGameState = selected.KeyPoint;
			Update();
		}

		private void ReplayViewer_OnClosing(object sender, CancelEventArgs e)
		{
			try
			{
				Config.Instance.ReplayWindowTop = (int)Top;
				Config.Instance.ReplayWindowLeft = (int)Left;
				Config.Instance.ReplayWindowWidth = (int)Width;
				Config.Instance.ReplayWindowHeight = (int)Height;
			}
			catch(Exception ex)
			{
				Logger.WriteLine(ex.ToString(), "ReplayViewer");
			}
		}

		private void CheckBoxDraw_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowDraw = true;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxDraw_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowDraw = false;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxPlay_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowPlay = true;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxPlay_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowPlay = false;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxHeroPower_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowHeroPower = true;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxHeroPower_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowHeroPower = false;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxDeath_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowDeath = true;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxDeath_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowDeath = false;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxSecret_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowSecret = true;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxSecret_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowSecret = false;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxDiscard_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowDiscard = true;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxDiscard_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowDiscard = false;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxAttack_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowAttack = true;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxAttack_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowAttack = false;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxSummon_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowSummon = true;
			Config.Save();
			ReloadKeypoints();
		}

		private void CheckBoxSummon_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ReplayViewerShowSummon = false;
			Config.Save();
			ReloadKeypoints();
		}

		private void ButtonFilter_OnClick(object sender, RoutedEventArgs e)
		{
			ContextMenuFilter.IsOpen = true;
		}

		private void RectangleCollapseExpand_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var grid = VisualTreeHelper.GetParent((Rectangle)sender) as Grid;
			if(grid != null)
			{
				var tvi = grid.DataContext as TurnViewItem;
				if(tvi != null && tvi.Turn.HasValue)
				{
					DataGridKeyPoints.SelectedItem = tvi;
					if(_collapsedTurns.Contains(tvi.Turn.Value))
						_collapsedTurns.Remove(tvi.Turn.Value);
					else
						_collapsedTurns.Add(tvi.Turn.Value);
					ReloadKeypoints();
				}
			}
		}

		private void DataGridKeyPoints_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			bool collapse;
			if(e.Key == Key.Left)
				collapse = true;
			else if(e.Key == Key.Right)
				collapse = false;
			else
				return;
			var tvi = DataGridKeyPoints.SelectedItem as TurnViewItem;
			if(tvi != null && tvi.Turn.HasValue)
			{
				if(!collapse && _collapsedTurns.Contains(tvi.Turn.Value))
				{
					_collapsedTurns.Remove(tvi.Turn.Value);
					ReloadKeypoints();
				}
				else if(collapse && !_collapsedTurns.Contains(tvi.Turn.Value))
				{
					_collapsedTurns.Add(tvi.Turn.Value);
					ReloadKeypoints();
				}
			}
		}

		private void MenuItemShowAll_OnClick(object sender, RoutedEventArgs e)
		{
			object parent = sender;
			while(!(parent is Grid))
			{
				parent = VisualTreeHelper.GetParent((DependencyObject)parent);
				if(parent == null)
					return;
			}
			var tvi = ((Grid)parent).DataContext as TurnViewItem;
			if(tvi != null && tvi.Turn.HasValue)
			{
				if(!_showAllTurns.Contains(tvi.Turn.Value))
				{
					_showAllTurns.Add(tvi.Turn.Value);
					ReloadKeypoints();
				}
			}
		}

		private void MenuItemShowFiltered_OnClick(object sender, RoutedEventArgs e)
		{
			object parent = sender;
			while(!(parent is Grid))
			{
				parent = VisualTreeHelper.GetParent((DependencyObject)parent);
				if(parent == null)
					return;
			}
			var tvi = ((Grid)parent).DataContext as TurnViewItem;
			if(tvi != null && tvi.Turn.HasValue)
			{
				if(_showAllTurns.Contains(tvi.Turn.Value))
				{
					_showAllTurns.Remove(tvi.Turn.Value);
					ReloadKeypoints();
				}
			}
		}
	}
}
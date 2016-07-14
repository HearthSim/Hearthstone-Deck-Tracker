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
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Replay.Controls;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;
using static HearthDb.Enums.GameTag;
using static Hearthstone_Deck_Tracker.Replay.KeyPointType;
using CardEntity = Hearthstone_Deck_Tracker.Replay.Controls.CardEntity;
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
			get { return _currentGameState?.Data.First(x => x.HasTag(PLAYER_ID)); }
		}

		private Entity OpponentEntity
		{
			get { return _currentGameState?.Data.Last(x => x.HasTag(PLAYER_ID)); }
		}

		private IEnumerable<BoardEntity> BoardEntites => new[]
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

		public Entity OpponentCardPlayed
		{
			get
			{
				var entity = _currentGameState.Data.FirstOrDefault(e => e.Id == _currentGameState.Id);
				if(entity?.IsControlledBy(_opponentController) == true
				   && (_currentGameState.Type == Play || _currentGameState.Type == PlaySpell))
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
				if(entity?.IsControlledBy(_playerController) == true
				   && (_currentGameState.Type == Play || _currentGameState.Type == PlaySpell))
				{
					entity.SetCardCount(0);
					return entity;
				}
				return null;
			}
		}

		public List<ReplayKeyPoint> KeyPoints => Replay;

		public BitmapImage PlayerHeroImage
		{
			get
			{
				if(!Enum.GetNames(typeof(HeroClass)).Contains(PlayerHero))
					return new BitmapImage();
				return new BitmapImage(new Uri($"../Resources/{PlayerHero.ToLower()}_small.png", UriKind.Relative));
			}
		}

		public string PlayerName => _currentGameState == null ? string.Empty : PlayerEntity.Name;

		public string PlayerHero
		{
			get
			{
				if(_currentGameState == null)
					return string.Empty;
				var cardId = GetHero(_playerController).CardId;
				return cardId == null ? null : Database.GetHeroNameFromId(cardId);
			}
		}

		public int PlayerHealth
		{
			get
			{
				if(_currentGameState == null)
					return 30;
				var hero = GetHero(_playerController);
				return hero.GetTag(HEALTH) - hero.GetTag(DAMAGE);
			}
		}

		public int PlayerArmor => _currentGameState == null ? 0 : GetHero(_playerController).GetTag(ARMOR);

		public Visibility PlayerArmorVisibility => PlayerArmor > 0 ? Visible : Hidden;

		public int PlayerAttack => _currentGameState == null ? 0 : GetHero(_playerController).GetTag(ATK);

		public Visibility PlayerAttackVisibility => PlayerAttack > 0 ? Visible : Hidden;

		public IEnumerable<Entity> PlayerHand => _currentGameState?.Data.Where(x => x.IsInZone(Zone.HAND) && x.IsControlledBy(_playerController)) ?? new List<Entity>();

		public IEnumerable<Entity> PlayerBoard => _currentGameState?.Data.Where(
																			    x =>
																				x.IsInZone(Zone.PLAY) && x.IsControlledBy(_playerController) && x.HasTag(HEALTH)
																				&& !string.IsNullOrEmpty(x.CardId) && !x.CardId.Contains("HERO")) ?? new List<Entity>();

		public BitmapImage OpponentHeroImage
		{
			get
			{
				if(!Enum.GetNames(typeof(HeroClass)).Contains(OpponentHero) && OpponentHero != "Jaraxxus")
					return new BitmapImage();
				var uri = new Uri($"../Resources/{OpponentHero.ToLower()}_small.png", UriKind.Relative);
				return new BitmapImage(uri);
			}
		}

		public string OpponentName => _currentGameState == null ? string.Empty : OpponentEntity.Name;

		public string OpponentHero
		{
			get
			{
				if(_currentGameState == null)
					return null;
				var cardId = GetHero(_opponentController).CardId;
				return cardId == null ? null : Database.GetHeroNameFromId(cardId);
			}
		}

		public int OpponentHealth
		{
			get
			{
				if(_currentGameState == null)
					return 30;
				var hero = GetHero(_opponentController);
				return hero.GetTag(HEALTH) - hero.GetTag(DAMAGE);
			}
		}

		public int OpponentArmor => _currentGameState == null ? 0 : GetHero(_opponentController).GetTag(ARMOR);

		public Visibility OpponentArmorVisibility => OpponentArmor > 0 ? Visible : Hidden;

		public int OpponentAttack => _currentGameState == null ? 0 : GetHero(_opponentController).GetTag(ATK);

		public Visibility OpponentAttackVisibility => OpponentAttack > 0 ? Visible : Hidden;

		public IEnumerable<Entity> OpponentHand => _currentGameState?.Data.Where(x => x.IsInZone(Zone.HAND) && x.IsControlledBy(_opponentController)) ?? new List<Entity>();

		public IEnumerable<Entity> OpponentBoard => _currentGameState?.Data.Where(
																				  x =>
																				  x.IsInZone(Zone.PLAY) && x.IsControlledBy(_opponentController) && x.HasTag(HEALTH)
																				  && !string.IsNullOrEmpty(x.CardId) && !x.CardId.Contains("HERO")) ?? new List<Entity>();

		public Entity OpponentBoardHero => GetHero(_opponentController);
		public Entity OpponentBoard0 => GetEntity(OpponentBoard, 0);
		public Entity OpponentBoard1 => GetEntity(OpponentBoard, 1);
		public Entity OpponentBoard2 => GetEntity(OpponentBoard, 2);
		public Entity OpponentBoard3 => GetEntity(OpponentBoard, 3);
		public Entity OpponentBoard4 => GetEntity(OpponentBoard, 4);
		public Entity OpponentBoard5 => GetEntity(OpponentBoard, 5);
		public Entity OpponentBoard6 => GetEntity(OpponentBoard, 6);
		public Entity PlayerBoardHero => GetHero(_playerController);
		public Entity PlayerBoard0 => GetEntity(PlayerBoard, 0);
		public Entity PlayerBoard1 => GetEntity(PlayerBoard, 1);
		public Entity PlayerBoard2 => GetEntity(PlayerBoard, 2);
		public Entity PlayerBoard3 => GetEntity(PlayerBoard, 3);
		public Entity PlayerBoard4 => GetEntity(PlayerBoard, 4);
		public Entity PlayerBoard5 => GetEntity(PlayerBoard, 5);
		public Entity PlayerBoard6 => GetEntity(PlayerBoard, 6);
		public Entity PlayerCard0 => GetEntity(PlayerHand, 0);
		public Entity PlayerCard1 => GetEntity(PlayerHand, 1);
		public Entity PlayerCard2 => GetEntity(PlayerHand, 2);
		public Entity PlayerCard3 => GetEntity(PlayerHand, 3);
		public Entity PlayerCard4 => GetEntity(PlayerHand, 4);
		public Entity PlayerCard5 => GetEntity(PlayerHand, 5);
		public Entity PlayerCard6 => GetEntity(PlayerHand, 6);
		public Entity PlayerCard7 => GetEntity(PlayerHand, 7);
		public Entity PlayerCard8 => GetEntity(PlayerHand, 8);
		public Entity PlayerCard9 => GetEntity(PlayerHand, 9);
		public Entity OpponentCard0 => GetEntity(OpponentHand, 0);
		public Entity OpponentCard1 => GetEntity(OpponentHand, 1);
		public Entity OpponentCard2 => GetEntity(OpponentHand, 2);
		public Entity OpponentCard3 => GetEntity(OpponentHand, 3);
		public Entity OpponentCard4 => GetEntity(OpponentHand, 4);
		public Entity OpponentCard5 => GetEntity(OpponentHand, 5);
		public Entity OpponentCard6 => GetEntity(OpponentHand, 6);
		public Entity OpponentCard7 => GetEntity(OpponentHand, 7);
		public Entity OpponentCard8 => GetEntity(OpponentHand, 8);
		public Entity OpponentCard9 => GetEntity(OpponentHand, 9);

		public Entity PlayerWeapon
		{
			get
			{
				if(_currentGameState == null)
					return null;
				var weaponId = PlayerEntity.GetTag(WEAPON);
				if(weaponId == 0)
					return null;
				var entity = _currentGameState.Data.FirstOrDefault(x => x.Id == weaponId);
				entity?.SetCardCount(entity.GetTag(DURABILITY) - entity.GetTag(DAMAGE));
				return entity;
			}
		}

		public Entity OpponentWeapon
		{
			get
			{
				if(_currentGameState == null)
					return null;
				var weaponId = OpponentEntity.GetTag(WEAPON);
				if(weaponId == 0)
					return null;
				var entity = _currentGameState.Data.FirstOrDefault(x => x.Id == weaponId);
				entity?.SetCardCount(entity.GetTag(DURABILITY) - entity.GetTag(DAMAGE));
				return entity;
			}
		}

		public Visibility OpponentSecretVisibility => OpponentSecrets.Any() ? Visible : Collapsed;

		private IEnumerable<Entity> OpponentSecrets => _currentGameState?.Data.Where(
																					 x =>
																					 x.GetTag(ZONE) == (int)Zone.SECRET && x.IsControlledBy(_opponentController)) ?? new List<Entity>();

		private IEnumerable<Entity> PlayerSecrets => _currentGameState?.Data.Where(x => x.GetTag(ZONE) == (int)Zone.SECRET && x.IsControlledBy(_playerController)) ?? new List<Entity>();

		public Visibility PlayerSecretVisibility => PlayerSecrets.Any() ? Visible : Collapsed;

		public Entity OpponentSecret0 => OpponentSecrets.Any() ? OpponentSecrets.ToArray()[0] : null;
		public Entity OpponentSecret1 => OpponentSecrets.Count() > 1 ? OpponentSecrets.ToArray()[1] : null;
		public Entity OpponentSecret2 => OpponentSecrets.Count() > 2 ? OpponentSecrets.ToArray()[2] : null;
		public Entity OpponentSecret3 => OpponentSecrets.Count() > 3 ? OpponentSecrets.ToArray()[3] : null;
		public Entity OpponentSecret4 => OpponentSecrets.Count() > 4 ? OpponentSecrets.ToArray()[4] : null;
		public Entity PlayerSecret0 => PlayerSecrets.Any() ? PlayerSecrets.ToArray()[0] : null;
		public Entity PlayerSecret1 => PlayerSecrets.Count() > 1 ? PlayerSecrets.ToArray()[1] : null;
		public Entity PlayerSecret2 => PlayerSecrets.Count() > 2 ? PlayerSecrets.ToArray()[2] : null;
		public Entity PlayerSecret3 => PlayerSecrets.Count() > 3 ? PlayerSecrets.ToArray()[3] : null;
		public Entity PlayerSecret4 => PlayerSecrets.Count() > 4 ? PlayerSecrets.ToArray()[4] : null;

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
				return new SolidColorBrush((hero != null && hero.GetTag(DAMAGE) > 0) ? Colors.Red : Colors.White);
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
				return new SolidColorBrush((hero != null && hero.GetTag(DAMAGE) > 0) ? Colors.Red : Colors.White);
			}
		}

		public string PlayerMana
		{
			get
			{
				if(_currentGameState == null)
					return "0/0";
				var total = PlayerEntity.GetTag(RESOURCES);
				var current = total - PlayerEntity.GetTag(RESOURCES_USED);
				return current + "/" + total;
			}
		}

		public string OpponentMana
		{
			get
			{
				if(_currentGameState == null)
					return "0/0";
				var total = OpponentEntity.GetTag(RESOURCES);
				var current = total - OpponentEntity.GetTag(RESOURCES_USED);
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
			_currentGameState = Replay.FirstOrDefault(r => r.Data.Any(x => x.HasTag(PLAYER_ID)));
			if(_currentGameState == null)
			{
				Log.Error("No player entity found.");
				return;
			}
			_playerController = PlayerEntity.GetTag(CONTROLLER);
			_opponentController = OpponentEntity.GetTag(CONTROLLER);
			var currentTurn = -1;
			TurnViewItem tvi = null;
			foreach(var kp in Replay)
			{
				var entity = kp.Data.FirstOrDefault(x => x.Id == kp.Id);
				if(entity == null || (string.IsNullOrEmpty(entity.CardId) && kp.Type != Victory && kp.Type != Defeat))
					continue;
				if(kp.Type == Summon && entity.GetTag(CARDTYPE) == (int)CardType.ENCHANTMENT)
					continue;
				var turn = (kp.Turn + 1) / 2;
				if(turn == 1)
				{
					if(!kp.Data.Any(x => x.HasTag(PLAYER_ID) && x.GetTag(RESOURCES) == 1))
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
						case Attack:
							if(!Config.Instance.ReplayViewerShowAttack)
								continue;
							break;
						case Death:
							if(!Config.Instance.ReplayViewerShowDeath)
								continue;
							break;
						case KeyPointType.Mulligan:
						case DeckDiscard:
						case HandDiscard:
							if(!Config.Instance.ReplayViewerShowDiscard)
								continue;
							break;
						case Draw:
						case Obtain:
						case PlayToDeck:
						case PlayToHand:
							if(!Config.Instance.ReplayViewerShowDraw)
								continue;
							break;
						case HeroPower:
							if(!Config.Instance.ReplayViewerShowHeroPower)
								continue;
							break;
						case SecretStolen:
						case SecretTriggered:
							if(!Config.Instance.ReplayViewerShowSecret)
								continue;
							break;
						case Play:
						case PlaySpell:
						case SecretPlayed:
							if(!Config.Instance.ReplayViewerShowPlay)
								continue;
							break;
						case Summon:
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

			var attackArrowVisibility = Hidden;
			var playArrowVisibility = Hidden;
			if(_currentGameState.Type == Attack)
			{
				await Task.Delay(100);
				var attackerId = _currentGameState.Data[0].GetTag(PROPOSED_ATTACKER);
				var defenderId = _currentGameState.Data[0].GetTag(PROPOSED_DEFENDER);
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
					attackArrowVisibility = Visible;
				}
			}
			else if(_currentGameState.Type == PlaySpell)
			{
				await Task.Delay(100);
				var entity = _currentGameState.Data.FirstOrDefault(e => e.Id == _currentGameState.Id);
				if(entity != null && entity.HasTag(CARD_TARGET))
				{
					var targetId = entity.GetTag(CARD_TARGET);
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
						attackArrowVisibility = Visible;
					}
				}
			}
			AttackArrow.Visibility = attackArrowVisibility;

			if(_currentGameState.Type == Play)
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
						playArrowVisibility = Visible;
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
				                   ? PlayerEntity.GetTag(HERO_ENTITY) : OpponentEntity.GetTag(HERO_ENTITY);

			return _currentGameState.Data.FirstOrDefault(x => x.Id == heroEntityId) ?? new Entity();
		}

		private Entity GetEntity(IEnumerable<Entity> zone, int index)
			=> zone.FirstOrDefault(x => x.HasTag(ZONE_POSITION) && x.GetTag(ZONE_POSITION) == index + 1);

		private void DataGridKeyPoints_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			var selected = ((DataGrid)sender).SelectedItem as TurnViewItem;
			if(selected?.KeyPoint == null)
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
				Log.Error(ex);
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

		private void ButtonFilter_OnClick(object sender, RoutedEventArgs e) => ContextMenuFilter.IsOpen = true;

		private void RectangleCollapseExpand_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var grid = VisualTreeHelper.GetParent((Rectangle)sender) as Grid;
			var tvi = grid?.DataContext as TurnViewItem;
			if(tvi?.Turn == null)
				return;
			DataGridKeyPoints.SelectedItem = tvi;
			if(_collapsedTurns.Contains(tvi.Turn.Value))
				_collapsedTurns.Remove(tvi.Turn.Value);
			else
				_collapsedTurns.Add(tvi.Turn.Value);
			ReloadKeypoints();
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
			if(tvi?.Turn == null)
				return;
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

		private void MenuItemShowAll_OnClick(object sender, RoutedEventArgs e)
		{
			var parent = sender;
			while(!(parent is Grid))
			{
				parent = VisualTreeHelper.GetParent((DependencyObject)parent);
				if(parent == null)
					return;
			}
			var tvi = ((Grid)parent).DataContext as TurnViewItem;
			if(tvi?.Turn == null)
				return;
			if(_showAllTurns.Contains(tvi.Turn.Value))
				return;
			_showAllTurns.Add(tvi.Turn.Value);
			ReloadKeypoints();
		}

		private void MenuItemShowFiltered_OnClick(object sender, RoutedEventArgs e)
		{
			var parent = sender;
			while(!(parent is Grid))
			{
				parent = VisualTreeHelper.GetParent((DependencyObject)parent);
				if(parent == null)
					return;
			}
			var tvi = ((Grid)parent).DataContext as TurnViewItem;
			if(tvi?.Turn == null)
				return;
			if(!_showAllTurns.Contains(tvi.Turn.Value))
				return;
			_showAllTurns.Remove(tvi.Turn.Value);
			ReloadKeypoints();
		}
	}
}
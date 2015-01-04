using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
using Point = System.Windows.Point;
using TreeView = System.Windows.Controls.TreeView;

namespace Hearthstone_Deck_Tracker.Replay
{
	/// <summary>
	/// Interaction logic for ReplayViewer.xaml
	/// </summary>
	public partial class ReplayViewer
	{
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
		}

	    private List<TreeViewItem> _treeViewTurnItems;

		private Entity PlayerEntity
		{
			get
			{
				return _currentGameState == null ? null : _currentGameState.Data.First(x => x.IsPlayer);
			}
		}

		private Entity OpponentEntity
		{
			get
			{
				return _currentGameState == null
					       ? null
					       : _currentGameState.Data.First(x => x.HasTag(GAME_TAG.PLAYER_ID) && !x.IsPlayer);
			}			
		}
		private int _playerController;
		private int _opponentController;
		public void Load(List<ReplayKeyPoint> replay)
		{
		    if (replay == null || replay.Count == 0)
		        return;
			Replay = replay;
            _currentGameState = Replay[0];
            _playerController = PlayerEntity.GetTag(GAME_TAG.CONTROLLER);
            _opponentController = OpponentEntity.GetTag(GAME_TAG.CONTROLLER);
            _treeViewTurnItems = new List<TreeViewItem>();
			foreach (var kp in Replay)
			{

			    var tvItem = _treeViewTurnItems.FirstOrDefault(x => (string) x.Header == "Turn " + kp.Turn);
                if (tvItem == null)
                {
                    tvItem = new TreeViewItem() {Header = "Turn " + kp.Turn, IsExpanded = true};
                    _treeViewTurnItems.Add(tvItem);
                }
				if(!string.IsNullOrEmpty(kp.Data.First(x => x.Id == kp.Id).CardId))
					tvItem.Items.Add(kp);
			}
		    foreach (var tvi in _treeViewTurnItems)
		        TreeViewKeyPoints.Items.Add(tvi);
			DataContext = this;
		}

		private IEnumerable<BoardEntity> BoardEntites
		{
			get
			{
				return new[]
				       {
					       OpponentBoardEntity0, OpponentBoardEntity1, OpponentBoardEntity2, OpponentBoardEntity3, OpponentBoardEntity4,
					       OpponentBoardEntity5, OpponentBoardEntity6, PlayerBoardEntity0, PlayerBoardEntity1, PlayerBoardEntity2,
					       PlayerBoardEntity3, PlayerBoardEntity4, PlayerBoardEntity5, PlayerBoardEntity6, PlayerBoardHeroEntity, OpponentBoardHeroEntity
				       };
			}
		}
		private async void Update()
		{
			DataContext = null;
			DataContext = this;
			if(_currentGameState.Type == KeyPointType.Attack)
			{
				await Task.Delay(100);
				var attackerId = _currentGameState.Data[0].GetTag(GAME_TAG.PROPOSED_ATTACKER);
				var defenderId = _currentGameState.Data[0].GetTag(GAME_TAG.PROPOSED_DEFENDER);
				var attacker = BoardEntites.FirstOrDefault(x => x.DataContext != null && ((Entity)x.DataContext).Id == attackerId);
				var defender = BoardEntites.FirstOrDefault(x => x.DataContext != null && ((Entity)x.DataContext).Id == defenderId);
				if(attacker != null && defender != null)
				{
					double xOffsetAttacker = 0, xOffsetDefender = 0, yOffsetAttacker = 0, yOffsetDefender = 0;
					if(attacker == PlayerBoardHeroEntity)
						yOffsetAttacker = 31;
					else if(attacker == OpponentBoardHeroEntity)
						yOffsetAttacker = -28;
					else
					{
						xOffsetAttacker = attacker.ActualWidth / 2;
						yOffsetAttacker = attacker.ActualHeight / 2;
					}
					if(defender == PlayerBoardHeroEntity)
						yOffsetDefender = 31;
					else if(defender == OpponentBoardHeroEntity)
						yOffsetDefender = -28;
					else
					{
						xOffsetDefender = defender.ActualWidth / 2;
						yOffsetDefender = defender.ActualHeight / 2;
					}
					AttackArrow.X1 = GetPosition(attacker).X + xOffsetAttacker;
					AttackArrow.X2 = GetPosition(defender).X + xOffsetDefender;
					if(GetPosition(attacker).Y < GetPosition(defender).Y)
					{
						AttackArrow.Y1 = GetPosition(attacker).Y + yOffsetAttacker;
						AttackArrow.Y2 = GetPosition(defender).Y - yOffsetDefender;
					}
					else
					{

						AttackArrow.Y1 = GetPosition(attacker).Y - yOffsetAttacker;
						AttackArrow.Y2 = GetPosition(defender).Y + yOffsetDefender;
					}
					AttackArrow.Visibility = Visibility.Visible;
				}
				else
					AttackArrow.Visibility = Visibility.Hidden;

			}
			else
				AttackArrow.Visibility = Visibility.Hidden;
		}

		private Point GetPosition(Visual element)
		{
			var positionTransform = element.TransformToAncestor(this);
			var areaPosition = positionTransform.Transform(new Point(0, 0));
			
			return areaPosition;
		}

		public List<ReplayKeyPoint> Replay;
		private ReplayKeyPoint _currentGameState;

		private Entity GetHero(int controller)
		{
			return
				_currentGameState.Data.FirstOrDefault(
				                             x =>
				                             !string.IsNullOrEmpty(x.CardId) && x.CardId.Contains("HERO") &&
				                             x.IsControlledBy(controller)) ?? new Entity();
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
                if (_currentGameState == null)
                    return string.Empty;
                return PlayerEntity.Name;
            }
        }

        public string PlayerHero
        {
            get
            {
                if (_currentGameState == null)
                    return string.Empty;
                var cardId = GetHero(_playerController).CardId;
                return cardId == null ? null : CardIds.HeroIdDict[cardId];
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
				return GetHero(_playerController).GetTag(GAME_TAG.ARMOR); }
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
				return GetHero(_playerController).GetTag(GAME_TAG.ATK); }
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

		public IEnumerable<Entity> PlayerBoard {
			get
			{
				if(_currentGameState == null)
					return new List<Entity>();
				return _currentGameState.Data.Where(x => x.IsInZone(TAG_ZONE.PLAY) && x.IsControlledBy(_playerController) && x.HasTag(GAME_TAG.HEALTH) && !string.IsNullOrEmpty(x.CardId) && !x.CardId.Contains("HERO"));
				
			}
		}

		public BitmapImage OpponentHeroImage
		{
			get
			{
				if(!Enum.GetNames(typeof(HeroClass)).Contains(OpponentHero))
					return new BitmapImage();
				var uri = new Uri(string.Format("../Resources/{0}_small.png", OpponentHero.ToLower()), UriKind.Relative);
				return new BitmapImage(uri);
			}
		}
		public string OpponentName
        {
            get
            {
                if (_currentGameState == null)
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
				return cardId == null ? null : CardIds.HeroIdDict[cardId];
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
				return _currentGameState.Data.Where(x => x.IsInZone(TAG_ZONE.PLAY) && x.IsControlledBy(_opponentController) && x.HasTag(GAME_TAG.HEALTH) && !string.IsNullOrEmpty(x.CardId) && !x.CardId.Contains("HERO"));
				//return board.Select(x => Game.GetCardFromId(x.CardId).Name + " (" + x.GetTag(GAME_TAG.ATK) + " - " + x.GetTag(GAME_TAG.HEALTH) + ")");
			}
		}

		public Entity OpponentBoardHero
		{
			get
			{
				return _currentGameState.Data.FirstOrDefault(x => !string.IsNullOrEmpty(x.CardId) && x.CardId.Contains("HERO") && x.IsControlledBy(_opponentController));
			}
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
			get
			{
				return _currentGameState.Data.FirstOrDefault(x => !string.IsNullOrEmpty(x.CardId) && x.CardId.Contains("HERO") && x.IsControlledBy(_playerController));
			}
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
		private Entity GetEntity(IEnumerable<Entity> zone, int index)
		{
			return zone.FirstOrDefault(x => x.HasTag(GAME_TAG.ZONE_POSITION) && x.GetTag(GAME_TAG.ZONE_POSITION) == index + 1);
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
					       : _currentGameState.Data.Where(x => x.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.SECRET &&
															x.IsControlledBy(_opponentController));
			}
		}
		private IEnumerable<Entity> PlayerSecrets
		{
			get
			{
				return _currentGameState == null
						   ? new List<Entity>()
						   : _currentGameState.Data.Where(x => x.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.SECRET
															&& x.IsControlledBy(_playerController));
			}
		}

		public Visibility PlayerSecretVisibility
		{
			get { return PlayerSecrets.Any() ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Entity OpponentSecret0
		{
			get
			{
				return OpponentSecrets.Any() ? OpponentSecrets.ToArray()[0] : null;
			}
		}
		public Entity OpponentSecret1
		{
			get
			{
				return OpponentSecrets.Count() > 1 ? OpponentSecrets.ToArray()[0] : null;
			}
		}
		public Entity OpponentSecret2
		{
			get
			{
				return OpponentSecrets.Count() > 2 ? OpponentSecrets.ToArray()[0] : null;
			}
		}
		public Entity OpponentSecret3
		{
			get
			{
				return OpponentSecrets.Count() > 3 ? OpponentSecrets.ToArray()[0] : null;
			}
		}
		public Entity OpponentSecret4
		{
			get
			{
				return OpponentSecrets.Count() > 4 ? OpponentSecrets.ToArray()[0] : null;
			}
		}
		public Entity PlayerSecret0
		{
			get
			{
				return PlayerSecrets.Any() ? PlayerSecrets.ToArray()[0] : null;
			}
		}
		public Entity PlayerSecret1
		{
			get
			{
				return PlayerSecrets.Count() > 1 ? PlayerSecrets.ToArray()[1] : null;
			}
		}
		public Entity PlayerSecret2
		{
			get
			{
				return PlayerSecrets.Count() > 2 ? PlayerSecrets.ToArray()[2] : null;
			}
		}
		public Entity PlayerSecret3
		{
			get
			{
				return PlayerSecrets.Count() > 3 ? PlayerSecrets.ToArray()[3] : null;
			}
		}
		public Entity PlayerSecret4
		{
			get
			{
				return PlayerSecrets.Count() > 4 ? PlayerSecrets.ToArray()[4] : null;
			}
		}

		public SolidColorBrush PlayerHealthTextColor
		{
			get
			{
				if(_currentGameState == null)
					return new SolidColorBrush(Colors.White);
				var hero = _currentGameState.Data.FirstOrDefault(x => x.IsControlledBy(_playerController) && !string.IsNullOrEmpty(x.CardId)
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
				var hero = _currentGameState.Data.FirstOrDefault(x => x.IsControlledBy(_opponentController) && !string.IsNullOrEmpty(x.CardId)
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

		private void TreeViewKeyPoints_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	    {
	        var selected = ((TreeView) sender).SelectedItem as ReplayKeyPoint;
	        if (selected == null)
	            return;
	        _currentGameState = selected;
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
				Logger.WriteLine(ex.ToString());
			}
		}
	}
}

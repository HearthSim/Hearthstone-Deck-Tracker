using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class BattlegroundsMechanicWindow : INotifyPropertyChanged
	{
		private readonly DispatcherTimer _updateTimer = new() { Interval = TimeSpan.FromSeconds(1) };

		public BattlegroundsMechanicWindow()
		{
			InitializeComponent();
			Height = Config.Instance.BattlegroundsMechanicWindowHeight;
			Width = Config.Instance.BattlegroundsMechanicWindowWidth;
			if(Config.Instance.BattlegroundsMechanicWindowLeft.HasValue)
				Left = Config.Instance.BattlegroundsMechanicWindowLeft.Value;
			if(Config.Instance.BattlegroundsMechanicWindowTop.HasValue)
				Top = Config.Instance.BattlegroundsMechanicWindowTop.Value;
			if(!System.Windows.Forms.Screen.AllScreens.Any(s => s.WorkingArea.Contains(new System.Drawing.Point((int)Left + 5, (int)Top + 5))))
			{
				Left = 100;
				Top = 100;
			}
			DataContext = this;
			_updateTimer.Tick += (_, _) => Update();
		}

		private CardAssetViewModel? _cardAsset;
		public CardAssetViewModel? CardAsset
		{
			get => _cardAsset;
			private set
			{
				_cardAsset = value;
				OnPropertyChanged();
			}
		}

		public SolidColorBrush? BackgroundColor => Helper.BrushFromHex(Config.Instance.StreamingOverlayBackground);

		public event PropertyChangedEventHandler? PropertyChanged;

		public void UpdateBackground() => OnPropertyChanged(nameof(BackgroundColor));

		public void Update()
		{
			var current = GetCurrentMechanic();
			Title = LocUtil.Get(current?.Mechanic switch
			{
				BattlegroundsMechanic.Anomaly => "BattlegroundsMechanicWindow_Title_Anomaly",
				BattlegroundsMechanic.Deity => "BattlegroundsMechanicWindow_Title_Deity",
				_ => "BattlegroundsMechanicWindow_Title",
			});
			var card = current?.Card;
			if(card?.Id == CardAsset?.Card?.Id)
				return;
			CardAsset = card != null ? new CardAssetViewModel(card, CardAssetType.FullImage) : null;
		}

		private static (Card Card, BattlegroundsMechanic Mechanic)? GetCurrentMechanic()
		{
			var game = Core.Game;
			var gameEntity = game.GameEntity;
			if(!game.IsRunning || game.IsInMenu || gameEntity == null)
				return null;
			if(game.IsBattlegroundsMatch)
			{
				if(BattlegroundsUtils.GetBattlegroundsMechanic(gameEntity, BattlegroundsUtils.GetAvailableRaces()) is not { } mechanic)
					return null;
				var card = Database.GetCardFromDbfId(mechanic.DbfId, false);
				if(card == null)
					return null;
				card.BaconCard = true;
				return (card, mechanic.Mechanic);
			}
			var anomalyEntityId = new[] { GameTag.ANOMALY1, GameTag.ANOMALY2 }.Select(gameEntity.GetTag).FirstOrDefault(x => x > 0);
			return game.Entities.TryGetValue(anomalyEntityId, out var anomaly) && anomaly != null ? (anomaly.Card, BattlegroundsMechanic.Anomaly) : null;
		}

		private void BattlegroundsMechanicWindow_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if(IsVisible)
			{
				Update();
				_updateTimer.Start();
			}
			else
				_updateTimer.Stop();
		}

		private void BattlegroundsMechanicWindow_OnClosing(object sender, CancelEventArgs e)
		{
			if(Core.IsShuttingDown)
			{
				if(!double.IsNaN(Left))
					Config.Instance.BattlegroundsMechanicWindowLeft = (int)Left;
				if(!double.IsNaN(Top))
					Config.Instance.BattlegroundsMechanicWindowTop = (int)Top;
				if(!double.IsNaN(Height) && Height > 0)
					Config.Instance.BattlegroundsMechanicWindowHeight = (int)Height;
				if(!double.IsNaN(Width) && Width > 0)
					Config.Instance.BattlegroundsMechanicWindowWidth = (int)Width;
			}
			else
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void BattlegroundsMechanicWindow_OnActivated(object sender, EventArgs e) => Topmost = true;

		private void BattlegroundsMechanicWindow_OnDeactivated(object sender, EventArgs e)
		{
			if(!Config.Instance.WindowsTopmost)
				Topmost = false;
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class MulliganPanel : UserControl, INotifyPropertyChanged
	{
		private string? _shortId;
		private int[]? _dbfIds;
		private CardClass _opponent;
		private bool _hasCoin;
		private int _playerStarLevel;

		public MulliganPanel()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void UserControl_MouseEnter(object sender, MouseEventArgs e)
		{
			if(HasData)
				(FindResource("StoryboardHover") as Storyboard)?.Begin();
		}

		private void UserControl_MouseLeave(object sender, MouseEventArgs e)
		{
			if(HasData)
				(FindResource("StoryboardNormal") as Storyboard)?.Begin();
		}

		private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Core.Overlay.HideMulliganPanel(true);
			if(HasData)
			{
				var ids = $"mulliganIds={HttpUtility.UrlEncode(string.Join(",", _dbfIds))}";
				var opponent = $"mulliganOpponent={_opponent}";
				var playerInitiative = $"mulliganPlayerInitiative={(_hasCoin ? "COIN" : "FIRST")}";
				var playerStarLevel = $"mulliganPlayerStarLevel={_playerStarLevel}";

				var url = Helper.BuildHsReplayNetUrl($"/decks/{_shortId}", "mulligan_toast", null, new[] { ids, opponent, playerInitiative, playerStarLevel });
				Helper.TryOpenUrl(url);
				HSReplayNetClientAnalytics.TryTrackToastClick("mulligan");
			}
		}

		public Cursor CursorStyle => HasData ? Cursors.Hand : Cursors.Arrow;

		private bool _hasData;
		public bool HasData
		{
			get => _hasData;
			set
			{
				if(value != _hasData)
				{
					_hasData = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(CursorStyle));
				}
			}
		}

		public void Update(string shortId, int[] dbfIds, CardClass opponent, bool hasCoin, int playerStarLevel)
		{
			_shortId = shortId;
			_dbfIds = dbfIds;
			_opponent = opponent;
			_hasCoin = hasCoin;
			_playerStarLevel = playerStarLevel;
			HasData = !string.IsNullOrEmpty(shortId) && HsReplayDataManager.Decks.AvailableDecks.Contains(_shortId);
		}

		private readonly HashSet<string> _noDataShown = new HashSet<string>();
		public bool ShouldShow()
		{
			if(string.IsNullOrEmpty(_shortId))
				return false;
			if(HasData)
			{
				if(!Config.Instance.SeenMulliganToast)
				{
					Config.Instance.SeenMulliganToast = true;
					Config.Save();
				}
				return true;
			}
			if(!Config.Instance.SeenMulliganToast)
				return false;
			if(_noDataShown.Contains(_shortId!))
				return false;
			_noDataShown.Add(_shortId!);
			return true;
		}
	}
}

using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class MulliganPanel : UserControl, INotifyPropertyChanged
	{
		private string? _shortId;
		private int[]? _dbfIds;
		private Dictionary<string, string>? _parameters;
		private bool? _autoFilters;

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
			Core.Overlay.HideMulliganToast(true);
			if(HasData)
			{
				var fragmentParams = _parameters?.Select(kv =>
					$"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}").ToList()
					?? new List<string>();

				if(_dbfIds != null)
					fragmentParams.Add($"mulliganIds={string.Join(",", _dbfIds)}");

				if(_autoFilters == true)
					fragmentParams.Add("mulliganAutoFilter=yes");

				var url = Helper.BuildHsReplayNetUrl(
					$"/decks/{_shortId}",
					"mulligan_toast",
					null,
					fragmentParams
				);
				Helper.TryOpenUrl(url);
				HSReplayNetClientAnalytics.TryTrackToastClick(Franchise.HSConstructed, ToastAction.Toast.Mulligan);
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

		private bool _mulliganGuideOverlay;
		public bool MulliganGuideOverlay
		{
			get => _mulliganGuideOverlay;
			set
			{
				if(value != _mulliganGuideOverlay)
				{
					_mulliganGuideOverlay = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(NoDataLabel));
				}
			}
		}

		public string NoDataLabel
		{
			get => LocUtil.Get(MulliganGuideOverlay ? "Toast_Mulligan_NotOnWebsite" : "Toast_Mulligan_Unavailable");
		}

		public void Update(string shortId, int[] dbfIds, Dictionary<string, string> parameters, bool showingMulliganGuideOverlay, bool autoFilters)
		{
			_shortId = shortId;
			_dbfIds = dbfIds;
			_autoFilters = autoFilters;
			_parameters = parameters;
			HasData = !string.IsNullOrEmpty(shortId) && HsReplayDataManager.Decks.AvailableDecks.Contains(_shortId);
			MulliganGuideOverlay = showingMulliganGuideOverlay;
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

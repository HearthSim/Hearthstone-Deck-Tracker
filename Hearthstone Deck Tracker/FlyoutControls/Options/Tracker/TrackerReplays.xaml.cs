#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.HsReplay.API;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.Windows.Visibility;
using static Hearthstone_Deck_Tracker.HsReplay.Enums.AccountStatus;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerReplays.xaml
	/// </summary>
	public partial class TrackerReplays : INotifyPropertyChanged
	{
		public TrackerReplays()
		{
			InitializeComponent();
		}

		public Visibility TextClaimVisibility => Account.Status == Anonymous ? Visible : Collapsed;
		public AccountStatus AccountStatus => Account.Status;
		public string BattleTag => Account.Status == Anonymous ? string.Empty : $"({Account.BattleTag})";

		public bool UploadPublic
		{
			get { return Account.ReplaysArePublic; }
			set { UpdatePrivacySetting(value); }
		}

		private async void UpdatePrivacySetting(bool value)
		{
			if(Account.ReplaysArePublic == value)
				return;
			CheckBoxReplayPrivacy.IsEnabled = false;
			Cursor = Cursors.Wait;
			try
			{
				if(await ApiManager.UpdateReplayPrivacy(value))
					Account.ReplaysArePublic = value;
			}
			catch(Exception e)
			{
				Log.Error(e);
				ErrorManager.AddError("Could not update replay privacy setting.", e.ToString());
			}
			finally
			{
				CheckBoxReplayPrivacy.IsEnabled = true;
				OnPropertyChanged(nameof(UploadPublic));
				Cursor = Cursors.Arrow;
			}
		}

		public void Update()
		{
			OnPropertyChanged(nameof(TextClaimVisibility));
			OnPropertyChanged(nameof(AccountStatus));
			OnPropertyChanged(nameof(BattleTag));
			OnPropertyChanged(nameof(UploadPublic));
		}

		private void ButtonClaimAccount_OnClick(object sender, RoutedEventArgs e) => ApiManager.ClaimAccount().Forget();

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private async void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
		{
			ButtonRefresh.IsEnabled = false;
			try
			{
				await ApiManager.UpdateAccountStatus();
				Update();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			finally
			{
				ButtonRefresh.IsEnabled = true;
			}
		}
	}
}
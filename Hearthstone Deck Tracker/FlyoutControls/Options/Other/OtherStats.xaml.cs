#region

using System.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Other
{
	/// <summary>
	/// Interaction logic for OtherStats.xaml
	/// </summary>
	public partial class OtherStats
	{
		private bool _initialized;

		public OtherStats()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxRecordArena.IsChecked = Config.Instance.RecordArena;
			CheckboxRecordCasual.IsChecked = Config.Instance.RecordCasual;
			CheckboxRecordFriendly.IsChecked = Config.Instance.RecordFriendly;
			CheckboxRecordOther.IsChecked = Config.Instance.RecordOther;
			CheckboxRecordPractice.IsChecked = Config.Instance.RecordPractice;
			CheckboxRecordRanked.IsChecked = Config.Instance.RecordRanked;
			CheckboxDiscardGame.IsChecked = Config.Instance.DiscardGameIfIncorrectDeck;
			CheckboxRecordSpectator.IsChecked = Config.Instance.RecordSpectator;
			CheckboxDiscardZeroTurnGame.IsChecked = Config.Instance.DiscardZeroTurnGame;
			CheckboxSaveHSLogIntoReplayFile.IsChecked = Config.Instance.SaveHSLogIntoReplay;
			_initialized = true;
		}

		private void CheckboxRecordRanked_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordRanked = true;
			Config.Save();
		}

		private void CheckboxRecordRanked_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordRanked = false;
			Config.Save();
		}

		private void CheckboxRecordArena_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordArena = true;
			Config.Save();
		}

		private void CheckboxRecordArena_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordArena = false;
			Config.Save();
		}

		private void CheckboxRecordCasual_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordCasual = true;
			Config.Save();
		}

		private void CheckboxRecordCasual_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordCasual = false;
			Config.Save();
		}

		private void CheckboxRecordFriendly_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordFriendly = true;
			Config.Save();
		}

		private void CheckboxRecordFriendly_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordFriendly = false;
			Config.Save();
		}

		private void CheckboxRecordPractice_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordPractice = true;
			Config.Save();
		}

		private void CheckboxRecordPractice_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordPractice = false;
			Config.Save();
		}

		private void CheckboxRecordOther_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordOther = true;
			Config.Save();
		}

		private void CheckboxRecordOther_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordOther = false;
			Config.Save();
		}

		private void CheckboxDiscardGame_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DiscardGameIfIncorrectDeck = true;
			Config.Save();
		}

		private void CheckboxDiscardGame_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DiscardGameIfIncorrectDeck = false;
			Config.Save();
		}

		private void CheckboxDiscardZeroTurnGame_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DiscardZeroTurnGame = true;
			Config.Save();
		}

		private void CheckboxDiscardZeroTurnGame_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DiscardZeroTurnGame = false;
			Config.Save();
		}

		private void CheckboxRecordSpectator_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordSpectator = true;
			Config.Save();
		}

		private void CheckboxRecordSpectator_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordSpectator = false;
			Config.Save();
		}

		private void CheckboxSaveHSLogIntoReplayFile_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SaveHSLogIntoReplay = true;
			Config.Save();
		}

		private void CheckboxSaveHSLogIntoReplayFile_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SaveHSLogIntoReplay = false;
			Config.Save();
		}
	}
}
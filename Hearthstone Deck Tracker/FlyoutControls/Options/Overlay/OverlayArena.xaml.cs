#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;


#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for Overlay.xaml
	/// </summary>
	public partial class OverlayArena : INotifyPropertyChanged
	{
		private bool _initialized;

		public OverlayArena()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Load()
		{
			CheckboxEnableArenasmith.IsChecked = Config.Instance.EnableArenasmithOverlay;

			CheckboxShowArenasmithPreLobby.IsChecked = Config.Instance.ShowArenasmithPreLobby;
			CheckboxShowArenaHeroPicking.IsChecked = Config.Instance.ShowArenaHeroPicking;
			CheckboxShowArenasmithScore.IsChecked = Config.Instance.ShowArenasmithScore;
			CheckboxShowArenaRelatedCards.IsChecked = Config.Instance.ShowArenaRelatedCards;
			CheckboxShowArenaDeckSynergies.IsChecked = Config.Instance.ShowArenaDeckSynergies;
			CheckboxShowArenaRedraftDiscard.IsChecked = Config.Instance.ShowArenaRedraftDiscard;

			_initialized = true;
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Core.Overlay.Update(true);
		}

		private void CheckboxShowArenasmithPreLobby_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenasmithPreLobby = true;
			SaveConfig(true);
			Core.Overlay.UpdateArenaPreLobbyVisibility();
		}

		private void CheckboxShowArenasmithPreLobby_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenasmithPreLobby = false;
			SaveConfig(true);
			Core.Overlay.UpdateArenaPreLobbyVisibility();
		}

		private void CheckboxShowArenaHeroPicking_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenaHeroPicking = true;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.HeroPickVisibility = Visibility.Visible;
		}

		private void CheckboxShowArenaHeroPicking_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenaHeroPicking = false;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.HeroPickVisibility = Visibility.Collapsed;
		}

		private void CheckboxShowArenaRelatedCards_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenaRelatedCards = true;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.RelatedCardsVisibility = Visibility.Visible;
		}

		private void CheckboxShowArenaRelatedCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenaRelatedCards = false;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.RelatedCardsVisibility = Visibility.Collapsed;
		}

		private void CheckboxShowArenaDeckSynergies_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenaDeckSynergies = true;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.SynergiesVisibility = Visibility.Visible;
		}

		private void CheckboxShowArenaDeckSynergies_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenaDeckSynergies = false;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.SynergiesVisibility = Visibility.Collapsed;
		}

		private void CheckboxShowArenasmithScore_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenasmithScore = true;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.ArenasmithScoreVisibility = Visibility.Visible;
		}

		private void CheckboxShowArenasmithScore_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenasmithScore = false;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.ArenasmithScoreVisibility = Visibility.Collapsed;
		}

		private void CheckboxShowArenaRedraftDiscard_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenaRedraftDiscard = true;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.RedraftDiscardVisibility = Visibility.Visible;
		}

		private void CheckboxShowArenaRedraftDiscard_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowArenaRedraftDiscard = false;
			SaveConfig(true);
			Core.Overlay.ArenaPickHelperViewModel.RedraftDiscardVisibility = Visibility.Collapsed;
		}

		private void CheckboxEnableArenasmith_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableArenasmithOverlay = true;
			SaveConfig(true);
			Core.Overlay.UpdateArenaPreLobbyVisibility();
			Core.Overlay.UpdateArenaPickHelperVisibility();
		}

		private void CheckboxEnableArenasmith_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableArenasmithOverlay = false;
			SaveConfig(true);
			Core.Overlay.UpdateArenaPreLobbyVisibility();
			Core.Overlay.UpdateArenaPickHelperVisibility();
		}
	}
}

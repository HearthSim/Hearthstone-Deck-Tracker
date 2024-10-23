﻿#region

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for Player.xaml
	/// </summary>
	public partial class OverlayPlayer : INotifyPropertyChanged
	{
		private GameV2? _game;
		private bool _initialized;

		public OverlayPlayer()
		{
			InitializeComponent();
		}

		public double PlayerScaling
		{
			get { return Config.Instance.OverlayPlayerScaling; }
			set
			{
				if(!_initialized)
					return;
				value = Math.Round(value);
				if(value < SliderOverlayPlayerScaling.Minimum)
					value = SliderOverlayPlayerScaling.Minimum;
				else if(value > SliderOverlayPlayerScaling.Maximum)
					value = SliderOverlayPlayerScaling.Maximum;
				Config.Instance.OverlayPlayerScaling = value;
				Config.Save();
				Core.Overlay.UpdateScaling();
				if(Config.Instance.UseSameScaling && Config.Instance.OverlayOpponentScaling != value && Helper.OptionsMain != null)
					Helper.OptionsMain.OptionsOverlayOpponent.OpponentScaling = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Load(GameV2 game)
		{
			_game = game;
			CheckboxHighlightCardsInHand.IsChecked = Config.Instance.HighlightCardsInHand;
			CheckboxRemoveCards.IsChecked = Config.Instance.RemoveCardsFromDeck;
			CheckboxHighlightLastDrawn.IsChecked = Config.Instance.HighlightLastDrawn;
			CheckboxShowPlayerGet.IsChecked = Config.Instance.ShowPlayerGet;
			SliderPlayerOpacity.Value = Config.Instance.PlayerOpacity;
			SliderOverlayPlayerScaling.Value = Config.Instance.OverlayPlayerScaling;
			TextBoxScaling.Text = Config.Instance.OverlayPlayerScaling.ToString(CultureInfo.InvariantCulture);
			CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;
			CheckBoxCenterDeckVertically.IsChecked = Config.Instance.OverlayCenterPlayerStackPanel;
			CheckBoxActiveEffects.IsChecked = !Config.Instance.HidePlayerActiveEffects;
			CheckBoxCounters.IsChecked = !Config.Instance.HidePlayerCounters;
			CheckBoxRelatedCards.IsChecked = !Config.Instance.HidePlayerRelatedCards;
			CheckboxEnableWotogs.IsChecked = !Config.Instance.DisablePlayerWotogs;
			CheckBoxAttack.IsChecked = !Config.Instance.HidePlayerAttackIcon;
			ComboBoxCthun.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxCthun.SelectedItem = Config.Instance.PlayerCthunCounter;
			ComboBoxSpells.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxSpells.SelectedItem = Config.Instance.PlayerSpellsCounter;
			ComboBoxJade.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxJade.SelectedItem = Config.Instance.PlayerJadeCounter;
			ComboBoxPogoHopper.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxPogoHopper.SelectedItem = Config.Instance.PlayerPogoHopperCounter;
			ComboBoxGalakrond.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxGalakrond.SelectedItem = Config.Instance.PlayerGalakrondCounter;
			ComboBoxLibram.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxLibram.SelectedItem = Config.Instance.PlayerLibramCounter;
			ComboBoxSpellSchools.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();;
			ComboBoxSpellSchools.SelectedItem = Config.Instance.PlayerSpellSchoolsCounter;
			ComboBoxAbyssal.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxAbyssal.SelectedItem = Config.Instance.PlayerAbyssalCurseCounter;
			ComboBoxExcavateTier.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxExcavateTier.SelectedItem = Config.Instance.PlayerExcavateTierCounter;

			ElementSorterPlayer.IsPlayer = true;
			SetPanel();

			_initialized = true;
		}

		private void SetPanel()
		{
			foreach(var panel in Config.Instance.DeckPanelOrderLocalPlayer)
			{
				switch(panel)
				{
					case Enums.DeckPanel.Cards:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, !Config.Instance.HidePlayerCards,
																		  value => Config.Instance.HidePlayerCards = !value, true));
						break;
					case Enums.DeckPanel.CardsTop:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, !Config.Instance.HidePlayerCardsTop,
																		  value => Config.Instance.HidePlayerCardsTop = !value, true));
						break;
					case Enums.DeckPanel.CardsBottom:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, !Config.Instance.HidePlayerCardsBottom,
																		  value => Config.Instance.HidePlayerCardsBottom = !value, true));
						break;
					case Enums.DeckPanel.Sideboards:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, !Config.Instance.HidePlayerSideboards,
																		  value => Config.Instance.HidePlayerSideboards = !value, true));
						break;
					case Enums.DeckPanel.CardCounter:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, !Config.Instance.HidePlayerCardCount,
																		  value => Config.Instance.HidePlayerCardCount = !value, true));
						break;
					case Enums.DeckPanel.DrawChances:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, !Config.Instance.HideDrawChances,
																		  value => Config.Instance.HideDrawChances = !value, true));
						break;
					case Enums.DeckPanel.Fatigue:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, !Config.Instance.HidePlayerFatigueCount,
																		  value => Config.Instance.HidePlayerFatigueCount = !value, true));
						break;
					case Enums.DeckPanel.DeckTitle:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, Config.Instance.ShowDeckTitle,
																		  value => Config.Instance.ShowDeckTitle = value, true));
						break;
					case Enums.DeckPanel.Wins:
						ElementSorterPlayer.AddItem(new ElementSorterItem(panel, Config.Instance.ShowDeckWins,
																		  value => Config.Instance.ShowDeckWins = value, true));
						break;
				}
			}
		}

		public void ReloadUI()
		{
			ElementSorterPlayer.Clear();
			SetPanel();
		}

		private void CheckboxHighlightCardsInHand_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightCardsInHand = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightCardsInHand = false;
			SaveConfig(true);
		}

		private void SliderPlayerOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized)
				return;
			Config.Instance.PlayerOpacity = SliderPlayerOpacity.Value;
			SaveConfig(true);
		}

		private async void CheckboxRemoveCards_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RemoveCardsFromDeck = true;
			SaveConfig(false);
			if(_game != null && _game.IsUsingPremade)
			{
				await Core.Reset();
				Core.Overlay.Update(true);
			}
		}

		private async void CheckboxRemoveCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RemoveCardsFromDeck = false;
			SaveConfig(false);
			if(_game != null && _game.IsUsingPremade)
			{
				await Core.Reset();
				Core.Overlay.Update(true);
			}
		}

		private void CheckboxHighlightLastDrawn_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightLastDrawn = true;
			SaveConfig(false);
		}

		private void CheckboxHighlightLastDrawn_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightLastDrawn = false;
			SaveConfig(false);
		}

		private void CheckboxShowPlayerGet_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowPlayerGet = true;
			Config.Save();
			Core.Overlay.Update(true);
		}

		private void CheckboxShowPlayerGet_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowPlayerGet = false;
			Config.Save();
			Core.Overlay.Update(true);
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Core.Overlay.Update(true);
		}

		private void CheckboxSameScaling_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(Helper.OptionsMain != null)
				Helper.OptionsMain.OptionsOverlayOpponent.CheckboxSameScaling.IsChecked = true;
			Config.Instance.UseSameScaling = true;
			Config.Save();
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(Helper.OptionsMain != null)
				Helper.OptionsMain.OptionsOverlayOpponent.CheckboxSameScaling.IsChecked = false;
			Config.Instance.UseSameScaling = false;
			Config.Save();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void TextBoxScaling_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void CheckBoxCenterDeckVertically_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCenterPlayerStackPanel = true;
			Config.Save();
			Core.Overlay.UpdateStackPanelAlignment();
		}

		private void CheckBoxCenterDeckVertically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCenterPlayerStackPanel = false;
			Config.Save();
			Core.Overlay.UpdateStackPanelAlignment();
		}

		private void ComboBoxCthun_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PlayerCthunCounter = (DisplayMode)ComboBoxCthun.SelectedItem;
			Config.Save();
		}

		private void ComboBoxSpells_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PlayerSpellsCounter = (DisplayMode)ComboBoxSpells.SelectedItem;
			Config.Save();
		}

		private void ComboBoxJade_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.PlayerJadeCounter = (DisplayMode)ComboBoxJade.SelectedItem;
			Config.Save();
		}

		private void CheckBoxAttack_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerAttackIcon = false;
			Config.Save();
		}

		private void CheckBoxAttack_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerAttackIcon = true;
			Config.Save();
		}

		private void CheckBoxActiveEffects_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerActiveEffects = false;
			Config.Save();
		}

		private void CheckBoxActiveEffects_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerActiveEffects = true;
			Config.Save();
		}

		private void CheckBoxCounters_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerCounters = false;
			Config.Save();
		}

		private void CheckBoxCounters_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerCounters = true;
			Config.Save();
		}

		private void CheckBoxRelatedCards_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerRelatedCards = false;
			Config.Save();
		}

		private void CheckBoxRelatedCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerRelatedCards = true;
			Config.Save();
		}

		private void CheckBoxWotogs_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DisablePlayerWotogs = false;
			Config.Save();
		}

		private void CheckBoxWotogs_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DisablePlayerWotogs = true;
			Config.Save();
		}

		private void ComboBoxPogoHopper_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.PlayerPogoHopperCounter = (DisplayMode)ComboBoxPogoHopper.SelectedItem;
			Config.Save();
		}

		private void ComboBoxGalakrond_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.PlayerGalakrondCounter = (DisplayMode)ComboBoxGalakrond.SelectedItem;
			Config.Save();
		}

		private void ComboBoxLibram_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PlayerLibramCounter = (DisplayMode)ComboBoxLibram.SelectedItem;
			Config.Save();
		}

		private void ComboBoxSpellSchools_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PlayerSpellSchoolsCounter = (DisplayMode)ComboBoxSpellSchools.SelectedItem;
			Config.Save();
		}

		private void ComboBoxAbyssal_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PlayerAbyssalCurseCounter = (DisplayMode)ComboBoxAbyssal.SelectedItem;
			Config.Save();
		}

		private void ComboBoxExcavateTier_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PlayerExcavateTierCounter = (DisplayMode)ComboBoxExcavateTier.SelectedItem;
			Config.Save();
		}
	}
}

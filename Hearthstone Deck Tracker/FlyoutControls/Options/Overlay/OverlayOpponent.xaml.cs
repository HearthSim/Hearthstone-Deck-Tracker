#region

using System;
using System.ComponentModel;
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
	/// Interaction logic for Opponent.xaml
	/// </summary>
	public partial class OverlayOpponent : INotifyPropertyChanged
	{
		private bool _initialized;

		public OverlayOpponent()
		{
			InitializeComponent();
		}


		public double OpponentScaling
		{
			get { return Config.Instance.OverlayOpponentScaling; }
			set
			{
				if(!_initialized)
					return;
				value = Math.Round(value);
				if(value < SliderOverlayOpponentScaling.Minimum)
					value = SliderOverlayOpponentScaling.Minimum;
				else if(value > SliderOverlayOpponentScaling.Maximum)
					value = SliderOverlayOpponentScaling.Maximum;
				Config.Instance.OverlayOpponentScaling = value;
				Config.Save();
				Core.Overlay.UpdateScaling();
				if(Config.Instance.UseSameScaling && Config.Instance.OverlayPlayerScaling != value && Helper.OptionsMain != null)
					Helper.OptionsMain.OptionsOverlayPlayer.PlayerScaling = value;
				OnPropertyChanged();
			}
		}

		public double SecretScaling
		{
			get { return Config.Instance.SecretsPanelScaling * 100; }
			set
			{
				if(!_initialized)
					return;
				value = Math.Round(value);
				if(value < SliderOverlaySecretScaling.Minimum)
					value = SliderOverlaySecretScaling.Minimum;
				else if(value > SliderOverlaySecretScaling.Maximum)
					value = SliderOverlaySecretScaling.Maximum;
				Config.Instance.SecretsPanelScaling = value / 100;
				Config.Save();
				Core.Overlay.UpdateScaling();
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Load()
		{
			CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;
			SliderOpponentOpacity.Value = Config.Instance.OpponentOpacity;
			SliderOverlayOpponentScaling.Value = Config.Instance.OverlayOpponentScaling;
			CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;
			SliderSecretOpacity.Value = Config.Instance.SecretsOpacity;
			CheckBoxCenterDeckVertically.IsChecked = Config.Instance.OverlayCenterOpponentStackPanel;
			CheckboxIncludeCreated.IsChecked = Config.Instance.OpponentIncludeCreated;
			CheckBoxAttack.IsChecked = !Config.Instance.HideOpponentAttackIcon;
			CheckBoxActiveEffects.IsChecked = !Config.Instance.HideOpponentActiveEffects;
			CheckBoxCounters.IsChecked = !Config.Instance.HideOpponentCounters;
			CheckBoxRelatedCards.IsChecked = !Config.Instance.HideOpponentRelatedCards;
			CheckBoxMaxResourcesWidget.IsChecked = !Config.Instance.HideOpponentMaxResourcesWidget;
			CheckboxOpponentCardAge.IsChecked = !Config.Instance.HideOpponentCardAge;
			CheckboxOpponentCardMarks.IsChecked = !Config.Instance.HideOpponentCardMarks;
			CheckboxShowSecrets.IsChecked = !Config.Instance.HideSecrets;

			ElementSorterOpponent.IsPlayer = false;
			SetPanel();

			_initialized = true;
		}

		private void SetPanel()
		{
			var cfg = Config.Instance;
			var move = ElementSorterOpponent.MoveItem;
			foreach(var panel in cfg.DeckPanelOrderOpponent)
			{
				var item = panel switch
				{
					Enums.DeckPanel.Winrate => new ElementSorterItem(panel, cfg.ShowWinRateAgainst, value => cfg.ShowWinRateAgainst = value, false, move),
					Enums.DeckPanel.Cards => new ElementSorterItem(panel, !cfg.HideOpponentCards, value => cfg.HideOpponentCards = !value, false, move),
					Enums.DeckPanel.CardCounter => new ElementSorterItem(panel, !cfg.HideOpponentCardCount, value => cfg.HideOpponentCardCount = !value, false, move),
					Enums.DeckPanel.DrawChances => new ElementSorterItem(panel, !cfg.HideOpponentDrawChances, value => cfg.HideOpponentDrawChances = !value, false, move),
					_ => null
				};
				if(item != null)
					ElementSorterOpponent.AddItem(item);
			}
		}

		public void ReloadUI()
		{
			ElementSorterOpponent.Clear();
			SetPanel();
		}

		private void CheckboxHighlightDiscarded_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightDiscarded = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightDiscarded_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightDiscarded = false;
			SaveConfig(true);
		}

		private void CheckboxEnableLinkOpponentDeckInNonFriendly_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableLinkOpponentDeckInNonFriendly = true;
			SaveConfig(true);
		}

		private void CheckboxEnableLinkOpponentDeckInNonFriendly_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableLinkOpponentDeckInNonFriendly = false;
			SaveConfig(true);
		}

		private void SliderOpponentOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized)
				return;
			Config.Instance.OpponentOpacity = SliderOpponentOpacity.Value;
			SaveConfig(true);
		}


		private void SliderSecretOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized)
				return;
			Config.Instance.SecretsOpacity = SliderSecretOpacity.Value;
			SaveConfig(true);
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
				Helper.OptionsMain.OptionsOverlayPlayer.CheckboxSameScaling.IsChecked = true;
			Config.Instance.UseSameScaling = true;
			Config.Save();
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(Helper.OptionsMain != null)
				Helper.OptionsMain.OptionsOverlayPlayer.CheckboxSameScaling.IsChecked = false;
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
			Config.Instance.OverlayCenterOpponentStackPanel = true;
			Config.Save();
			Core.Overlay.UpdateStackPanelAlignment();
		}

		private void CheckBoxCenterDeckVertically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCenterOpponentStackPanel = false;
			Config.Save();
			Core.Overlay.UpdateStackPanelAlignment();
		}

		private void CheckboxIncludeCreated_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OpponentIncludeCreated = true;
			Config.Save();
		}

		private void CheckboxIncludeCreated_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OpponentIncludeCreated = false;
			Config.Save();
		}

		private void CheckBoxAttack_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentAttackIcon = false;
			Config.Save();
		}

		private void CheckBoxAttack_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentAttackIcon = true;
			Config.Save();
		}

		private void CheckBoxActiveEffects_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentActiveEffects = false;
			Config.Save();
		}

		private void CheckBoxActiveEffects_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentActiveEffects = true;
			Config.Save();
		}

		private void CheckBoxCounters_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCounters = false;
			Config.Save();
		}

		private void CheckBoxCounters_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCounters = true;
			Config.Save();
		}

		private void CheckBoxRelatedCards_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentRelatedCards = false;
			Config.Save();
		}

		private void CheckBoxRelatedCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentRelatedCards = true;
			Config.Save();
		}

		private void CheckBoxMaxResourcesWidget_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentMaxResourcesWidget = false;
			Config.Save();
		}

		private void CheckBoxMaxResourcesWidget_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentMaxResourcesWidget = true;
			Config.Save();
		}

		private void CheckboxOpponentCardAge_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCardAge = true;
			SaveConfig(true);
		}

		private void CheckboxOpponentCardAge_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCardAge = false;
			SaveConfig(true);
		}

		private void CheckboxShowOpponentCardMarks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCardMarks = true;
			SaveConfig(true);
		}

		private void CheckboxShowOpponentCardMarks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCardMarks = false;
			SaveConfig(true);
		}

		private void CheckboxShowSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideSecrets = false;
			SaveConfig(false);
			Core.Overlay.ShowSecrets(Core.Game.SecretsManager.GetSecretList());
		}

		private void CheckboxShowSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideSecrets = true;
			SaveConfig(false);
			Core.Overlay.HideSecrets();
		}
	}
}

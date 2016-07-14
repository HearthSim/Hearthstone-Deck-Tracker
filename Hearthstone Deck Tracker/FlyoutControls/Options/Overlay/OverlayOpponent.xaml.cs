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
		private GameV2 _game;
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
				if(Config.Instance.UseSameScaling && Config.Instance.OverlayPlayerScaling != value)
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

		public event PropertyChangedEventHandler PropertyChanged;

		public void Load(GameV2 game)
		{
			_game = game;
			CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;
			SliderOpponentOpacity.Value = Config.Instance.OpponentOpacity;
			SliderOverlayOpponentScaling.Value = Config.Instance.OverlayOpponentScaling;
			CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;
			SliderSecretOpacity.Value = Config.Instance.SecretsOpacity;
			CheckBoxCenterDeckVertically.IsChecked = Config.Instance.OverlayCenterOpponentStackPanel;
			CheckboxIncludeCreated.IsChecked = Config.Instance.OpponentIncludeCreated;
			CheckBoxAttack.IsChecked = !Config.Instance.HideOpponentAttackIcon;
			ComboBoxCthun.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
			ComboBoxCthun.SelectedItem = Config.Instance.OpponentCthunCounter;
			ComboBoxSpells.ItemsSource = new[] {DisplayMode.Always, DisplayMode.Never};
			ComboBoxSpells.SelectedItem = Config.Instance.OpponentSpellsCounter;

			ElementSorterOpponent.IsPlayer = false;
			foreach(var itemName in Config.Instance.PanelOrderOpponent)
			{
				switch(itemName)
				{
					case "Cards":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Cards", !Config.Instance.HideOpponentCards,
						                                                    value => Config.Instance.HideOpponentCards = !value, false));
						break;
					case "Card Counter":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Card Counter", !Config.Instance.HideOpponentCardCount,
						                                                    value => Config.Instance.HideOpponentCardCount = !value, false));
						break;
					case "Fatigue Counter":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Fatigue Counter", !Config.Instance.HideOpponentFatigueCount,
						                                                    value => Config.Instance.HideOpponentFatigueCount = !value, false));
						break;
					case "Draw Chances":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Draw Chances", !Config.Instance.HideOpponentDrawChances,
						                                                    value => Config.Instance.HideOpponentDrawChances = !value, false));
						break;
					case "Win Rate":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Win Rate", Config.Instance.ShowWinRateAgainst,
						                                                    value => Config.Instance.ShowWinRateAgainst = value, false));
						break;
				}
			}
			Core.Overlay.UpdateOpponentLayout();
			Core.Windows.OpponentWindow.UpdateOpponentLayout();
			_initialized = true;
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
			Helper.OptionsMain.OptionsOverlayPlayer.CheckboxSameScaling.IsChecked = true;
			Config.Instance.UseSameScaling = true;
			Config.Save();
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.OptionsMain.OptionsOverlayPlayer.CheckboxSameScaling.IsChecked = false;
			Config.Instance.UseSameScaling = false;
			Config.Save();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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

		private void ComboBoxCthun_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OpponentCthunCounter = (DisplayMode)ComboBoxCthun.SelectedItem;
			Config.Save();
		}

		private void ComboBoxSpells_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OpponentSpellsCounter = (DisplayMode)ComboBoxSpells.SelectedItem;
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
	}
}
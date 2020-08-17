using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.BobsBuddy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BobsBuddyPanel : UserControl, INotifyPropertyChanged
	{
		public BobsBuddyPanel()
		{
			InitializeComponent();
			ResetDisplays();
		}

		private string _winRateDisplay;
		public string WinRateDisplay
		{
			get => _winRateDisplay;
			set
			{
				_winRateDisplay = value;
				OnPropertyChanged();
			}
		}

		private string _tieRateDisplay;
		public string TieRateDisplay
		{
			get => _tieRateDisplay;
			set
			{
				_tieRateDisplay = value;
				OnPropertyChanged();
			}
		}

		private string _lossRateDisplay;
		public string LossRateDisplay
		{
			get => _lossRateDisplay;
			set
			{
				_lossRateDisplay = value;
				OnPropertyChanged();
			}
		}

		private string _playerLethalDisplay;
		public string PlayerLethalDisplay
		{
			get => _playerLethalDisplay;
			set
			{
				_playerLethalDisplay = value;
				OnPropertyChanged();
			}
		}

		private string _opponentLethalDisplay;
		public string OpponentLethalDisplay
		{
			get => _opponentLethalDisplay;
			set
			{
				_opponentLethalDisplay = value;
				OnPropertyChanged();
			}
		}

		private string _averageDamageGivenDisplay;
		public string AverageDamageGivenDisplay
		{
			get => _averageDamageGivenDisplay;
			set
			{
				_averageDamageGivenDisplay = value;
				OnPropertyChanged();
			}
		}

		private string _averageDamageTakenDisplay;
		public string AverageDamageTakenDisplay
		{
			get => _averageDamageTakenDisplay;
			set
			{
				_averageDamageTakenDisplay = value;
				OnPropertyChanged();
			}
		}

		private float _tooltipOffset;
		public float TooltipOffset
		{
			get => _tooltipOffset;
			set
			{
				_tooltipOffset = value;
				OnPropertyChanged();
			}
		}

		private BobsBuddyState _state;
		public BobsBuddyState State
		{
			get => _state;
			private set
			{
				if(_state == value)
					return;
				_state = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(StatusMessage));
			}
		}

		private BobsBuddyErrorState _errorState = BobsBuddyErrorState.None;
		public BobsBuddyErrorState ErrorState
		{
			get => _errorState;
			private set
			{
				if(_errorState == value)
					return;
				_errorState = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(StatusMessage));
				OnPropertyChanged(nameof(WarningIconVisibility));
			}
		}

		public string StatusMessage => StatusMessageConverter.GetStatusMessage(State, ErrorState, _showingResults);

		public Visibility WarningIconVisibility => ErrorState == BobsBuddyErrorState.None ? Visibility.Collapsed : Visibility.Visible;

		private Visibility _spinnerVisibility;
		public Visibility SpinnerVisibility
		{
			get => _spinnerVisibility;
			set
			{
				_spinnerVisibility = value;
				OnPropertyChanged();
			}
		}
		private Visibility _percentagesVisibility;
		public Visibility PercentagesVisibility
		{
			get => _percentagesVisibility;
			set
			{
				_percentagesVisibility = value;
				OnPropertyChanged();
			}
		}

		private double _playerLethalOpacity;
		public double PlayerLethalOpacity
		{
			get => _playerLethalOpacity;
			set
			{
				_playerLethalOpacity = value;
				OnPropertyChanged();
			}
		}

		private double _opponentLethalOpacity;
		public double OpponentLethalOpacity
		{
			get => _opponentLethalOpacity;
			set
			{
				_opponentLethalOpacity = value;
				OnPropertyChanged();
			}
		}

		private Visibility _settingsVisibility = Visibility.Collapsed;
		public Visibility SettingsVisibility
		{
			get => _settingsVisibility;
			set
			{
				_settingsVisibility = value;
				OnPropertyChanged();
			}
		}

		private Visibility _infoVisibility = Config.Instance.SeenBobsBuddyInfo ? Visibility.Collapsed : Visibility.Visible;
		public Visibility InfoVisibility
		{
			get => _infoVisibility;
			set
			{
				_infoVisibility = value;
				OnPropertyChanged();
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		internal void ShowCompletedSimulation(double winRate, double tieRate, double lossRate, double playerLethal, double opponentLethal, List<int> possibleResults)
		{
			ShowPercentagesHideSpinners();
			SetAverageDamage(possibleResults);
			WinRateDisplay = string.Format("{0:0.#%}", winRate);
			TieRateDisplay = string.Format("{0:0.#%}", tieRate);
			LossRateDisplay = string.Format("{0:0.#%}", lossRate);
			PlayerLethalDisplay = string.Format("{0:0.#%}", playerLethal);
			OpponentLethalDisplay = string.Format("{0:0.#%}", opponentLethal);

			PlayerLethalOpacity = playerLethal > 0 ? 1 : 0.3;
			OpponentLethalOpacity = opponentLethal > 0 ? 1 : 0.3;
		}

		internal void SetAverageDamage(List<int> possibleResults)
		{
			float count = possibleResults.Count;
			int lowerBound = possibleResults[(int)(.2 * count)];
			int upperBound = possibleResults[(int)(.8 * count)];
			var res = lowerBound != upperBound ? string.Format("{0}, {1}", lowerBound, upperBound) : upperBound.ToString();
			AverageDamageGivenDisplay = res;
			AverageDamageTakenDisplay = res;
			Console.WriteLine("lowerbound " + lowerBound + " upper " + upperBound);

		}

		/// <summary>
		/// called when user enters a new game of BG
		/// </summary>
		/// 
		internal void ResetDisplays() 
		{
			WinRateDisplay = "-";
			LossRateDisplay = "-";
			TieRateDisplay = "-";
			PlayerLethalOpacity = 0;
			OpponentLethalOpacity = 0;
			State = BobsBuddyState.Initial;
			ClearErrorState();
			ShowResults(false);
			ShowPercentagesHideSpinners();
			OnPropertyChanged(nameof(StatusMessage));
		}

		internal void HidePercentagesShowSpinners() 
		{
			SpinnerVisibility = Visibility.Visible;
			PercentagesVisibility = Visibility.Collapsed;
		}

		/// <summary>
		/// called when simulations are done
		/// </summary>
		internal void ShowPercentagesHideSpinners()
		{
			SpinnerVisibility = Visibility.Collapsed;
			PercentagesVisibility = Visibility.Visible;
		}

		private bool _showingResults = false;
		private void ShowResults(bool show)
		{
			if(ErrorState != BobsBuddyErrorState.None)
				show = false;

			_showingResults = show;
			OnPropertyChanged(nameof(StatusMessage));

			if(show)
				(FindResource("StoryboardExpand") as Storyboard)?.Begin();
			else
				(FindResource("StoryboardCollapse") as Storyboard)?.Begin();
		}

		internal void SetState(BobsBuddyState state)
		{
			if(State == state)
				return;
			State = state;

			if(state == BobsBuddyState.Combat)
			{
				ClearErrorState();
				ShowResults(Config.Instance.ShowBobsBuddyDuringCombat);
			}
			else if(state == BobsBuddyState.Shopping)
				ShowResults(Config.Instance.ShowBobsBuddyDuringShopping);
		}

		/// <summary>
		/// Sets the new error state of the display. Setting an error state will cause no stats to be displayed
		///	until the error is cleared.
		/// </summary>
		/// <param name="error">The new error state</param>
		internal void SetErrorState(BobsBuddyErrorState error)
		{
			ErrorState = error;
			ShowResults(false);
		}

		private void ClearErrorState()
		{
			if(ErrorState != BobsBuddyErrorState.UpdateRequired)
				ErrorState = BobsBuddyErrorState.None;
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Settings_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			e.Handled = true;
			Core.MainWindow.Options.TreeViewItemOverlayBattlegrounds.IsSelected = true;
			Core.MainWindow.FlyoutOptions.IsOpen = true;
			Core.MainWindow.ActivateWindow();
		}

		private bool InCombatPhase => State == BobsBuddyState.Combat;
		private bool InShoppingPhase => State == BobsBuddyState.Shopping;
		private bool CanMinimize
			=> InCombatPhase && !Config.Instance.ShowBobsBuddyDuringCombat
			|| InShoppingPhase && !Config.Instance.ShowBobsBuddyDuringShopping;

		private void BottomBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if(InCombatPhase || InShoppingPhase)
			{
				if(!_showingResults)
					ShowResults(true);
				else if(CanMinimize)
					ShowResults(false);
			}
		}

		public void SetTooltipOffset()
		{
			if(AverageDamageTooltip.ActualWidth != 0)
				TooltipOffset = (float)(42 - (AverageDamageTooltip.ActualWidth / 2));
		}

		private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SettingsVisibility = Visibility.Visible;
		}

		private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SettingsVisibility = Visibility.Collapsed;
		}

		private void Question_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			e.Handled = true;
			InfoVisibility = InfoVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
			UpdateSeenInfo();
		}

		private void Close_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			InfoVisibility = Visibility.Collapsed;
			UpdateSeenInfo();
		}

		private void UpdateSeenInfo()
		{
			if(!Config.Instance.SeenBobsBuddyInfo && InfoVisibility == Visibility.Collapsed)
			{
				Config.Instance.SeenBobsBuddyInfo = true;
				Config.Save();
			}
		}
	}
}

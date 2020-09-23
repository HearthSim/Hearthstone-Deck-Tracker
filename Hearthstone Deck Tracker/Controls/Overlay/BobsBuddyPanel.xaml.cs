using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.BobsBuddy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

		private List<int> _lastCombatPossibilities;

		private int _lastCombatResult = 0;

		const float SoftLabelOpacity = 0.3f;

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

		private double _playerAverageDamageOpacity;
		public double PlayerAverageDamageOpacity
		{
			get => _playerAverageDamageOpacity;
			set
			{
				_playerAverageDamageOpacity = value;
				OnPropertyChanged();
			}
		}

		private double _opponentAverageDamageOpacity;
		public double OpponentAverageDamageOpacity
		{
			get => _opponentAverageDamageOpacity;
			set
			{
				_opponentAverageDamageOpacity = value;
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

		private Visibility _averageDamageInfoVisibility = Visibility.Hidden;
		public Visibility AverageDamageInfoVisibility
		{
			get => _averageDamageInfoVisibility;
			set
			{
				_averageDamageInfoVisibility = value;
				OnPropertyChanged();
			}
		}

		private Visibility _closeAverageDamageInfoVisibility = Config.Instance.BobsBuddyAverageDamageInfoClosed ? Visibility.Collapsed : Visibility.Visible;
		public Visibility CloseAverageDamageInfoVisibility
		{
			get => _closeAverageDamageInfoVisibility;
			set
			{
				_closeAverageDamageInfoVisibility = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private bool _resultsPanelExpanded = false;

		private static List<int> _playerDamageDealtBounds;
		private static List<int> _opponentDamageDealtBounds;

		internal void ShowCompletedSimulation(double winRate, double tieRate, double lossRate, double playerLethal, double opponentLethal, List<int> possibleResults)
		{
			ShowPercentagesHideSpinners();
			_lastCombatPossibilities = possibleResults;
			SetAverageDamage(possibleResults);
			WinRateDisplay = string.Format("{0:0.#%}", winRate);
			TieRateDisplay = string.Format("{0:0.#%}", tieRate);
			LossRateDisplay = string.Format("{0:0.#%}", lossRate);
			PlayerLethalDisplay = string.Format("{0:0.#%}", playerLethal);
			OpponentLethalDisplay = string.Format("{0:0.#%}", opponentLethal);

			PlayerLethalOpacity = playerLethal > 0 ? 1 : SoftLabelOpacity;
			OpponentLethalOpacity = opponentLethal > 0 ? 1 : SoftLabelOpacity;

			PlayerAverageDamageOpacity = possibleResults.Where(x => x > 0).Any() ? 1 : SoftLabelOpacity;
			OpponentAverageDamageOpacity = possibleResults.Where(x => x < 0).Any() ? 1 : SoftLabelOpacity;
		}


		internal void SetLastOutcome(int lastOutcome)
		{
			_lastCombatResult = lastOutcome;
			CheckIfDamageOutcomeOutsideEightyPercent();
		}


		private void CheckIfDamageOutcomeOutsideEightyPercent()
		{
			if(_lastCombatResult < 0 && _opponentDamageDealtBounds != null)
			{
				if(_lastCombatResult < _opponentDamageDealtBounds[0] || _lastCombatResult > _opponentDamageDealtBounds[1])
				{
					if(!Config.Instance.BobsBuddyAverageDamageInfoClosed)
						AttemptToExpandAverageDamagePanels(true);
				}
			}
			else if(_lastCombatResult > 0 && _playerDamageDealtBounds != null)
			{
				if(_lastCombatResult < _playerDamageDealtBounds[0] || _lastCombatResult > _playerDamageDealtBounds[1])
				{
					if(!Config.Instance.BobsBuddyAverageDamageInfoClosed)
						AttemptToExpandAverageDamagePanels(true);
				}
			}
		}

		internal void SetAverageDamage(List<int> possibleResults)
		{
			var playerDamageDealtPossibilities = possibleResults.Where(x => x > 0).ToList();
			var opponentSortedDamageDealtPossibilites = possibleResults.Where(x => x < 0).Select(y => y * -1).ToList();
			opponentSortedDamageDealtPossibilites.Sort((x, y) => x.CompareTo(y));

			_playerDamageDealtBounds = GetTwentiethAndEightiethPercentileFor(playerDamageDealtPossibilities);
			_opponentDamageDealtBounds = GetTwentiethAndEightiethPercentileFor(opponentSortedDamageDealtPossibilites);

			PlayerAverageDamageOpacity = _playerDamageDealtBounds == null ? SoftLabelOpacity : 1;
			OpponentAverageDamageOpacity = _opponentDamageDealtBounds == null ? SoftLabelOpacity : 1;

			AverageDamageGivenDisplay = FormatDamageBoundsFrom(_playerDamageDealtBounds);
			AverageDamageTakenDisplay = FormatDamageBoundsFrom(_opponentDamageDealtBounds);
		}

		private List<int> GetTwentiethAndEightiethPercentileFor(List<int> possibleResults)
		{
			var count = possibleResults.Count;
			if(count == 0)
				return null;
			return new List<int>() { possibleResults[(int)Math.Floor(.2 * count)], possibleResults[(int)Math.Floor(.8 * count)] };
		}

		private string FormatDamageBoundsFrom(List<int> from) => from == null ? "0" : from[0] == from[1] ? from[0].ToString() : string.Format("{0}–{1}", from[0], from[1]);

		/// <summary>
		/// called when user enters a new game of BG
		/// </summary>
		/// 
		internal void ResetDisplays()
		{
			if(_lastCombatPossibilities != null)
				_lastCombatPossibilities.Clear();
			WinRateDisplay = "-";
			LossRateDisplay = "-";
			TieRateDisplay = "-";
			PlayerLethalDisplay = "-";
			OpponentLethalDisplay = "-";
			AverageDamageGivenDisplay = "-";
			AverageDamageTakenDisplay = "-";
			PlayerLethalOpacity = SoftLabelOpacity;
			OpponentLethalOpacity = SoftLabelOpacity;
			PlayerAverageDamageOpacity = SoftLabelOpacity;
			OpponentAverageDamageOpacity = SoftLabelOpacity;
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
		public void ShowResults(bool show)
		{
			if(ErrorState != BobsBuddyErrorState.None)
				show = false;

			_showingResults = show;
			OnPropertyChanged(nameof(StatusMessage));

			if(show)
				ExpandPanel();
			else
				CollapsePanel();
		}

		void ExpandPanel()
		{
			(FindResource("StoryboardExpand") as Storyboard)?.Begin();
			_resultsPanelExpanded = true;
			if(Config.Instance.AlwaysShowAverageDamage)
				ExpandAverageDamagePanels();
		}

		void CollapsePanel()
		{
			(FindResource("StoryboardCollapse") as Storyboard)?.Begin();
			_resultsPanelExpanded = false;
			if(Config.Instance.AlwaysShowAverageDamage)
				CollapseAverageDamagePanels();
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

		public async Task ExpandAverageDamagePanels()
		{
			(FindResource("StoryboardExpandAverageDamage") as Storyboard)?.Begin();
			await Task.Delay(200);
		}

		public void CollapseAverageDamagePanels()
		{
			(FindResource("StoryboardCollapseAverageDamage") as Storyboard)?.Begin();
		}

		private void BottomBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if(InCombatPhase || InShoppingPhase)
			{
				if(!_showingResults)
				{
					ShowResults(true);
					ExpandAverageDamagePanels();
				}
				else if(CanMinimize)
				{
					ShowResults(false);
					CollapseAverageDamagePanels();

				}
			}
		}

		public void AttemptToExpandAverageDamagePanels(bool attemptShowAverageDamageInfo)
		{
			if(State != BobsBuddyState.Initial && _resultsPanelExpanded)
			{
				UpdateSeenAverageDamage();
				ExpandAverageDamagePanels();
				if(attemptShowAverageDamageInfo && !Config.Instance.BobsBuddyAverageDamageInfoClosed)
					AverageDamageInfoVisibility = Visibility.Visible;

			}
		}

		private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SettingsVisibility = Visibility.Visible;
			AttemptToExpandAverageDamagePanels(true);
		}

		private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SettingsVisibility = Visibility.Collapsed;
			AverageDamageInfoVisibility = Visibility.Hidden;
			if(!Config.Instance.AlwaysShowAverageDamage)
				CollapseAverageDamagePanels();
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

		private void AverageDamageTakenPanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			AverageDamageInfoVisibility = Visibility.Visible;
		}

		private void CloseAverageDamageInfo_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Console.WriteLine("went to close av damage");
			Config.Instance.BobsBuddyAverageDamageInfoClosed = true;
			Config.Save();
			AverageDamageInfoVisibility = Visibility.Hidden;
			CloseAverageDamageInfoVisibility = Visibility.Collapsed;

		}

		private void UpdateSeenAverageDamage()
		{
			if(!Config.Instance.SeenBobsBuddyAverageDamage)
			{
				Config.Instance.SeenBobsBuddyAverageDamage = true;
				Config.Save();
			}
		}

		private void AverageDamageTakenPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(Config.Instance.BobsBuddyAverageDamageInfoClosed)
				AverageDamageInfoVisibility = Visibility.Hidden;
		}
	}
}

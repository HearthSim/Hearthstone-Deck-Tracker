using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.HsReplay;


namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.MinionPinning;

public partial class BattlegroundsMinionPinning : INotifyPropertyChanged
{
	private readonly BrushConverter _bc = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	public BattlegroundsMinionPinning()
	{
		InitializeComponent();
		CogBtnVisibility = Visibility.Hidden;
		UpdateQuickGuideVisibility();

		ConfigWrapper.DismissedTavernMarkerQuickGuideChanged += UpdateQuickGuideVisibility;
		ConfigWrapper.DismissedCompGuidesMarkerQuickGuideChanged += UpdateQuickGuideVisibility;

		HSReplayNetOAuth.AccountDataUpdated += () => OnPropertyChanged(nameof(IsOnTrial));
		HSReplayNetOAuth.LoggedOut += () => OnPropertyChanged(nameof(IsOnTrial));

		// Set the View reference on the ViewModel when DataContext changes
		DataContextChanged += (s, e) =>
		{
			if (DataContext is BattlegroundsMinionPinningViewModel vm)
			{
				vm.View = this;
			}
		};
	}

	private Visibility _cogBtnVisibility;
	public Visibility CogBtnVisibility
	{
		get => _cogBtnVisibility;
		set
		{
			_cogBtnVisibility = value;
			OnPropertyChanged();
		}
	}

	public Visibility QuickGuideVisibility
	{
		get => Config.Instance.DismissedTavernMarkerQuickQuickGuide ? Visibility.Collapsed : Visibility.Visible;
	}

	public Visibility CompGuidesMarkerQuickGuideVisibility
	{
		get => Config.Instance.DismissedCompGuidesMarkerQuickGuide ? Visibility.Collapsed : Visibility.Visible;
	}

	public bool IsOnTrial
	{
		get => !(HSReplayNetOAuth.AccountData?.IsTier7 ?? false);
	}

	private void UpdateQuickGuideVisibility()
	{
		OnPropertyChanged(nameof(QuickGuideVisibility));
		OnPropertyChanged(nameof(CompGuidesMarkerQuickGuideVisibility));
	}

	public void ShowGuide()
	{
		ConfigWrapper.DismissedCompGuidesMarkerQuickGuide = false;
		ConfigWrapper.DismissedTavernMarkerQuickGuide = false;
		ConfigWrapper.DismissedAutoEnablePopup = false;
	}

	public void DismissGuide()
	{
		ConfigWrapper.DismissedTavernMarkerQuickGuide = true;
	}
	public void DismissCompGuidesMarkerQuickGuide()
	{
		ConfigWrapper.DismissedCompGuidesMarkerQuickGuide = true;
	}

	private bool _isQuickCompGuideVisible;
	public bool IsQuickCompGuideVisible
	{
		get => _isQuickCompGuideVisible;
		set
		{
			_isQuickCompGuideVisible = value;
			OnPropertyChanged(nameof(IsCompGuidesMarkerPanelVisible));
			OnPropertyChanged();
		}
	}

	private bool _isAutoEnableMessageVisible;
	public bool IsAutoEnableMessageVisible
	{
		get => _isAutoEnableMessageVisible;
		set
		{
			_isAutoEnableMessageVisible = value;
			OnPropertyChanged(nameof(IsCompGuidesMarkerPanelVisible));
			OnPropertyChanged();
		}
	}

	public bool IsCompGuidesMarkerPanelVisible => IsQuickCompGuideVisible || IsAutoEnableMessageVisible;

	public bool AutoEnableTavernMarkersRecommended
	{
		get => ConfigWrapper.AutoEnableTavernMarkersRecommended;
		set
		{
			ConfigWrapper.AutoEnableTavernMarkersRecommended = value;
			OnPropertyChanged();
		}
	}

	private void Panel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		CogBtnVisibility = Visibility.Visible;
	}

	private void Panel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		CogBtnVisibility = Visibility.Hidden;
	}

	private void RecommendComp_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (!Config.Instance.DismissedCompGuidesMarkerQuickGuide)
		{
			IsQuickCompGuideVisible = true;
		}
	}

	private void RecommendComp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		IsQuickCompGuideVisible = false;
	}

	private void BtnOptions_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		BtnOptions.Background = (Brush)_bc.ConvertFromString("#22FFFFFF");
	}

	private void BtnOptions_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		BtnOptions.Background = (Brush)_bc.ConvertFromString("#00FFFFFF");
	}

	private void BtnHelp_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		BtnHelp.Background = (Brush)_bc.ConvertFromString("#22FFFFFF");
	}

	private void BtnHelp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		BtnHelp.Background = (Brush)_bc.ConvertFromString("#00FFFFFF");
	}

	private void BtnHelp_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (Config.Instance.DismissedTavernMarkerQuickQuickGuide || Config.Instance.DismissedCompGuidesMarkerQuickGuide)
			ShowGuide();
		else
			DismissGuide();
	}

	private void BtnGotIt_Click(object sender, RoutedEventArgs e)
	{
		DismissGuide();
		Core.Game.Metrics.TavernMarkersQuickGuideDismissed = true;
	}

	private void BtnCompGuidesMarkerGotIt_Click(object sender, RoutedEventArgs e)
	{
		DismissCompGuidesMarkerQuickGuide();
		IsQuickCompGuideVisible = false;
		Core.Game.Metrics.TavernMarkersCompGuidesQuickGuideDismissed = true;
	}

	// private void AutoEnablePopup_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	// {
	// 	IsAutoEnableMessageVisible = true;
	// }

	public void ShowAutoEnablePopup()
	{
		// Only show if user hasn't interacted with it before
		if (!Config.Instance.DismissedAutoEnablePopup)
		{
			IsAutoEnableMessageVisible = true;
			Core.Game.Metrics.TavernMarkersAutoEnableResponse = "no_interaction";
		}
	}

	private void BtnAutoEnableYes_Click(object sender, RoutedEventArgs e)
	{
		ConfigWrapper.AutoEnableTavernMarkersRecommended = true;
		ConfigWrapper.DismissedAutoEnablePopup = true;
		IsAutoEnableMessageVisible = false;
		Core.Game.Metrics.TavernMarkersAutoEnableResponse = "yes";
	}

	private void BtnAutoEnableNo_Click(object sender, RoutedEventArgs e)
	{
		ConfigWrapper.AutoEnableTavernMarkersRecommended = false;
		ConfigWrapper.DismissedAutoEnablePopup = true;
		IsAutoEnableMessageVisible = false;
		Core.Game.Metrics.TavernMarkersAutoEnableResponse = "no";
	}
}


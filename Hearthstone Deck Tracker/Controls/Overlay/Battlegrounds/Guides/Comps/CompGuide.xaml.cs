using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;

public partial class CompGuide : INotifyPropertyChanged
{
	public event EventHandler? BackButtonClicked;

	private void Button_OnClick(object sender, RoutedEventArgs e)
	{
		BackButtonClicked?.Invoke(this, EventArgs.Empty);
	}

	public CompGuide()
	{
		InitializeComponent();
		Loaded += CompGuide_Loaded;
		Unloaded += CompGuide_Unloaded;
	}

	private void CompGuide_Loaded(object sender, RoutedEventArgs e)
	{
		Core.Overlay.BattlegroundsMinionPinningViewModel.PinsChanged += OnPinsChanged;
		UpdatePinButtonState();
	}

	private void CompGuide_Unloaded(object sender, RoutedEventArgs e)
	{
		Core.Overlay.BattlegroundsMinionPinningViewModel.PinsChanged -= OnPinsChanged;
	}

	private void OnPinsChanged(object? sender, EventArgs e)
	{
		if(!Dispatcher.CheckAccess())
		{
			Dispatcher.BeginInvoke(new Action(() => OnPinsChanged(sender, e)));
			return;
		}

		if(Core.Overlay.BgsMinionPinningVisibility != Visibility.Visible)
			return;

		var viewModel = DataContext as BattlegroundsCompGuideViewModel;
		if(viewModel == null)
			return;

		var isPinned = viewModel.AreAllCompCardsPinned;

		if(BtnPinAllCompCards.Visibility != Visibility.Visible)
			return;

		if(!isPinned && IsMouseOver)
		{
			RunStoryBoardNonBlocking("PinButtonIn");
		}
		else
		{
			var storyboardKey = isPinned ? "PinButtonPinned" : "PinButtonUnpinned";
			RunStoryBoardNonBlocking(storyboardKey);
		}
	}

	private void UpdatePinButtonState()
	{
		if(Core.Overlay.BgsMinionPinningVisibility != Visibility.Visible)
			return;

		var viewModel = DataContext as BattlegroundsCompGuideViewModel;
		if(viewModel == null)
			return;

		var isPinned = viewModel.AreAllCompCardsPinned;

		// Use storyboard to set the initial state
		var storyboardKey = (isPinned && BtnPinAllCompCards.Visibility == Visibility.Visible) ? "PinButtonPinned" : "PinButtonHidden";
		RunStoryBoardNonBlocking(storyboardKey);
	}

	private void Grid_OnMouseEnter(object sender, MouseEventArgs e)
	{
		AnimatePinButtonIn();
	}

	private void Grid_OnMouseLeave(object sender, MouseEventArgs e)
	{
		AnimatePinButtonOut();
	}

	private void AnimatePinButtonIn()
	{
		if(Core.Overlay.BgsMinionPinningVisibility != Visibility.Visible)
			return;

		var viewModel = DataContext as BattlegroundsCompGuideViewModel;
		if(viewModel == null)
			return;

		var isPinned = viewModel.AreAllCompCardsPinned;

		if(isPinned)
		{
			// For pinned cards, show immediately without animation
			RunStoryBoardNonBlocking("PinButtonPinned");
		}
		else if(BtnPinAllCompCards.Visibility == Visibility.Visible)
		{
			// For non-pinned cards, animate in
			RunStoryBoardNonBlocking("PinButtonIn");
		}
	}

	private void AnimatePinButtonOut()
	{
		if(Core.Overlay.BgsMinionPinningVisibility != Visibility.Visible)
			return;

		if(BtnPinAllCompCards.Visibility != Visibility.Visible)
			return;

		var viewModel = DataContext as BattlegroundsCompGuideViewModel;
		if(viewModel == null)
			return;

		var isPinned = viewModel.AreAllCompCardsPinned;

		if(isPinned)
		{
			// Keep pinned button at full opacity and scale
			RunStoryBoardNonBlocking("PinButtonPinned");
		}
		else
		{
			// Animate out for non-pinned cards
			RunStoryBoardNonBlocking("PinButtonOut");
		}
	}

	private void RunStoryBoardNonBlocking(string key)
	{
		try
		{
			var sb = (Storyboard)FindResource(key);
			sb.Begin();
		}
		catch(Exception)
		{
			// ignored
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void CardTooltip_Loaded(object sender, RoutedEventArgs e)
	{
		Core.Game.Metrics.BattlegroundsCompGuidesMinionHovers++;
	}
}

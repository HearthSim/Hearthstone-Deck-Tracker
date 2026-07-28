using System;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;

public class MulliganGuideTrialsExhaustedViewModel : ViewModel
{
	public event Action? OnClose;

	public string? TrialTimeRemaining
	{
		get => GetProp<string?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ResetTimeVisibility));
		}
	}

	public Visibility ResetTimeVisibility => TrialTimeRemaining != null ? Visibility.Visible : Visibility.Collapsed;

	public ICommand CloseCommand => new Command(() => OnClose?.Invoke());

	public ICommand SubscribeNowCommand => new Command(() =>
	{
		var url = Helper.BuildHsReplayNetUrl("premium/", "constructed_trials_exhausted");
		Helper.TryOpenUrl(url);
		HSReplayNetClientAnalytics.OnClickSubscribeNowLink(
			Franchise.HSConstructed, ClickSubscribeNowAction.Button.ConstructedTrialsExhausted, 0
		);
		OnClose?.Invoke();
	});
}

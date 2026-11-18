using System;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.HsReplay.Onboarding;

public class NewUserOnboardingViewModel : ViewModel
{
	public bool IsVisible
	{
		get => GetProp(false);
		private set => SetProp(value);
	}

	public string WelcomeLabelText => ConfigManager.PreviousVersion != null ? LocUtil.Get("NewUserOnboarding_WelcomeBackTo") : LocUtil.Get("NewUserOnboarding_WelcomeTo");

	public bool ShowBackgroundMode
	{
		get => GetProp(false);
		private set
		{
			SetProp(value);
			ClosesToTray = value && BackgroundMode.CloseToTray;
			// the onboarding explains the tray, even if the user minimizes before dismissing it
			if(value)
				Config.Instance.SeenTrayNotification = true;
		}
	}

	public bool ClosesToTray
	{
		get => GetProp(false);
		private set => SetProp(value);
	}

	public bool IsBackgroundModeOnly
	{
		get => GetProp(false);
		private set => SetProp(value);
	}

	public event Action? Continue;

	public void ShowAnalyticsOnboarding()
	{
		ShowBackgroundMode = BackgroundMode.NeedsOnboarding;
		IsBackgroundModeOnly = false;
		IsVisible = true;
	}

	public void ShowBackgroundModeOnboarding()
	{
		// the analytics onboarding already covers background mode
		if(IsVisible || !BackgroundMode.NeedsOnboarding)
			return;
		ShowBackgroundMode = true;
		IsBackgroundModeOnly = true;
		IsVisible = true;
	}

	public ICommand ContinueCommand => new Command(() =>
	{
		if(!IsVisible)
			return;
		IsVisible = false;

		if(ShowBackgroundMode)
			BackgroundMode.OnboardingPending = false;
		if(!IsBackgroundModeOnly)
			Config.Instance.OnboardingSeen = true;
		Config.Save();

		if(!IsBackgroundModeOnly)
			Continue?.Invoke();

		if(ClosesToTray)
			Core.MainWindow.MinimizeToTray();
	});
}

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
		set => SetProp(value);
	}

	public string WelcomeLabelText => ConfigManager.PreviousVersion != null ? LocUtil.Get("NewUserOnboarding_WelcomeBackTo") : LocUtil.Get("NewUserOnboarding_WelcomeTo");

	public event Action? Continue;

	public ICommand ContinueCommand => new Command(() =>
	{
		if(!IsVisible)
			return;
		IsVisible = false;

		Config.Instance.OnboardingSeen = true;
		Config.Save();

		Continue?.Invoke();
	});
}

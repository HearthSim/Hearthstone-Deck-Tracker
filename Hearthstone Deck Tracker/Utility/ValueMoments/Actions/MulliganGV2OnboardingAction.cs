using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;

public class MulliganGV2OnboardingAction : VMAction
{

	public MulliganGV2OnboardingAction(Franchise franchise, Action actionName) : this(franchise, actionName, null){ }

	public MulliganGV2OnboardingAction(Franchise franchise, Action actionName, SubFranchise[]? subFranchise) : base(
		franchise, subFranchise, 10, true
	)
	{
		ActionName = actionName;
	}

	public enum Action
	{
		[JsonProperty("Showed Mulligan G-V2 Onboarding notification")]
		ShowedOnboardingNotification,
		[JsonProperty("Dismissed Mulligan G-V2 Onboarding notification")]
		DismissedOnboardingNotification,
		[JsonProperty("Clicked learn more on Mulligan G-V2 Onboarding notification")]
		LearnMoreOnboardingNotification,
		[JsonProperty("Opened Mulligan G-V2 Onboarding modal")]
		OpenedOnboardingModal,
	}

	public override string Name => "Mulligan G-V2 Onboarding Action";
	public override ActionSource Source => ActionSource.Overlay;
	public override string Type => "Mulligan G-V2 Onboarding Action";


	[JsonProperty("action_name")]
	[JsonConverter(typeof(EnumJsonConverter))]
	public Action ActionName { get ; }

}

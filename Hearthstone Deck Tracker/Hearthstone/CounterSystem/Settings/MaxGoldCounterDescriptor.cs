using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

public sealed class MaxGoldCounterDescriptor : CounterDescriptor
{
	public const string Id = "MaxGoldCounter";

	public static MaxGoldCounterDescriptor Instance { get; } = new();

	private MaxGoldCounterDescriptor() : base(Id, isBattlegroundsCounter: true)
	{
	}

	public override string DisplayName => LocUtil.Get("OptionsCounters_MaxGold");

	public override bool UsesFallbackDisplayName => false;

	public override CardAssetViewModel? CardAsset => null;

	public override string IconSource => "/Images/coin-cost.png";

	public static bool IsVisible(bool shouldShow) =>
		CounterVisibilitySettings.Instance.Get(Id, true) switch
		{
			CounterVisibility.Disabled => false,
			CounterVisibility.Enabled => true,
			_ => shouldShow,
		};
}

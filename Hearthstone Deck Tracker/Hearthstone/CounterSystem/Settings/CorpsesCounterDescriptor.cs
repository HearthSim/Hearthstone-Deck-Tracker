using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

public sealed class CorpsesCounterDescriptor : CounterDescriptor
{

	public const string Id = "CorpsesCounter";

	public static CorpsesCounterDescriptor Instance { get; } = new();

	private CorpsesCounterDescriptor() : base(Id)
	{
	}

	public override string DisplayName => LocUtil.Get("OptionsCounters_Corpses");

	public override bool UsesFallbackDisplayName => false;

	public override CardAssetViewModel? CardAsset => null;

	public override string IconSource => "/Images/corpses.png";

	public static bool IsVisible(bool isPlayer, bool shouldShow) =>
		CounterVisibilitySettings.Instance.Get(Id, isPlayer) switch
		{
			CounterVisibility.Disabled => false,
			CounterVisibility.Enabled => true,
			_ => shouldShow,
		};
}

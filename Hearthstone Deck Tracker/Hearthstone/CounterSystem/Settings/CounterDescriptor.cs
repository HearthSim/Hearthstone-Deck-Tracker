using Hearthstone_Deck_Tracker.Controls.Overlay;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

/// <summary>
/// UI metadata for one counter type, used by the options page.
/// </summary>
/// <remarks>
/// Backed by a throwaway "probe" instance rather than static metadata, because a counter's portrait
/// and name are instance properties (ImbueCounter's portrait even depends on game state). Reading
/// through to the probe also means a card-language change is picked up without rebuilding anything.
/// </remarks>
public class CounterDescriptor
{
	private readonly BaseCounter _probe;

	public CounterDescriptor(BaseCounter probe)
	{
		_probe = probe;
		CounterId = probe.CounterId;
		IsBattlegroundsCounter = probe.IsBattlegroundsCounter;
	}

	public string CounterId { get; }

	public bool IsBattlegroundsCounter { get; }

	public string DisplayName => _probe.LocalizedName;

	public bool UsesFallbackDisplayName => _probe.UsesFallbackDisplayName;

	public CardAssetViewModel CardAsset => _probe.CardAsset;
}

using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

/// <summary>
/// UI metadata for one row of the counter options page.
/// </summary>
/// <remarks>
/// Backed by a throwaway "probe" instance rather than static metadata, because a counter's portrait
/// and name are instance properties (ImbueCounter's portrait even depends on game state). Reading
/// through to the probe also means a card-language change is picked up without rebuilding anything.
///
/// Readouts that are configured like counters but drawn elsewhere in the overlay have no probe to
/// read from and describe themselves by overriding instead — see <see cref="WidgetCounters"/>.
/// </remarks>
public class CounterDescriptor
{
	private readonly BaseCounter? _probe;

	public CounterDescriptor(BaseCounter probe)
	{
		_probe = probe;
		CounterId = probe.CounterId;
		IsBattlegroundsCounter = probe.IsBattlegroundsCounter;
	}

	protected CounterDescriptor(string counterId)
	{
		CounterId = counterId;
	}

	public string CounterId { get; }

	public bool IsBattlegroundsCounter { get; }

	public virtual string DisplayName => _probe!.LocalizedName;

	public virtual bool UsesFallbackDisplayName => _probe!.UsesFallbackDisplayName;

	public virtual CardAssetViewModel? CardAsset => _probe!.CardAsset;

	public virtual string? IconSource => null;

	public virtual bool SupportsPlayer => true;

	public virtual bool SupportsOpponent => !IsBattlegroundsCounter;

	public virtual string? PlayerUnsupportedTooltip => null;

	public virtual string? OpponentUnsupportedTooltip =>
		SupportsOpponent ? null : LocUtil.Get("OptionsCounters_OpponentUnsupportedTooltip");
}

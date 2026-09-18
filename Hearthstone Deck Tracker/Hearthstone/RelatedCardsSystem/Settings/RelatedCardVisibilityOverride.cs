using System.ComponentModel;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Settings;

/// <summary>
/// One persisted entry in <see cref="Config.RelatedCardVisibilityOverrides"/>.
/// </summary>
/// <remarks>
/// Storage is sparse: an entry only exists while the opponent side is non-Auto. A card with no entry
/// resolves to Auto, which is what lets a newly added related card work without any config migration.
/// The side is kept as an attribute (rather than dropping it) so a player side can be added later
/// without changing the file format.
/// </remarks>
public class RelatedCardVisibilityOverride
{
	[XmlAttribute]
	public string CardId { get; set; } = string.Empty;

	[XmlAttribute]
	[DefaultValue(CounterVisibility.Auto)]
	public CounterVisibility Opponent { get; set; } = CounterVisibility.Auto;

	public bool IsEmpty => Opponent == CounterVisibility.Auto;
}

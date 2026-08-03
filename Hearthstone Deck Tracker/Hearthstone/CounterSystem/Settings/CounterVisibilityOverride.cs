using System.ComponentModel;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

/// <summary>
/// One persisted entry in <see cref="Config.CounterVisibilityOverrides"/>.
/// </summary>
/// <remarks>
/// Storage is sparse: an entry only exists while at least one side is non-Auto. A counter with no
/// entry resolves to Auto for both sides, which is what lets a newly added counter work without any
/// config migration.
/// </remarks>
public class CounterVisibilityOverride
{
	[XmlAttribute]
	public string CounterId { get; set; } = string.Empty;

	[XmlAttribute]
	[DefaultValue(CounterVisibility.Auto)]
	public CounterVisibility Player { get; set; } = CounterVisibility.Auto;

	[XmlAttribute]
	[DefaultValue(CounterVisibility.Auto)]
	public CounterVisibility Opponent { get; set; } = CounterVisibility.Auto;

	public CounterVisibility Get(bool isPlayer) => isPlayer ? Player : Opponent;

	public void Set(bool isPlayer, CounterVisibility value)
	{
		if(isPlayer)
			Player = value;
		else
			Opponent = value;
	}

	public bool IsEmpty => Player == CounterVisibility.Auto && Opponent == CounterVisibility.Auto;
}

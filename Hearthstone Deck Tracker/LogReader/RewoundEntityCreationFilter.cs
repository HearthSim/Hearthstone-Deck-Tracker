using HearthWatcher.LogReader;

namespace Hearthstone_Deck_Tracker.LogReader;

/// <summary>
/// Decides which lines of a rewound section of the power log still have to end up in the log
/// we upload.
///
/// A rewind undoes what the rewound play did, but it does not un-create the entities that were
/// created while it resolved: cards added to a hand and enchantments can survive into the
/// timeline the game keeps, and later lines still reference them. Dropping their FULL_ENTITY
/// blocks leaves the uploaded log with tag changes on entities that were never created, which
/// the replay parser rejects. So we keep the entity creations - and only those, because keeping
/// the rewound BLOCK_START PLAY as well would make the play count twice.
/// </summary>
public class RewoundEntityCreationFilter
{
	private const string PowerLinePrefix = "GameState.DebugPrintPower() - ";
	private const string EntityCreation = "FULL_ENTITY - Creating";
	private const string EntityTag = "tag=";

	// Indentation of the creation we are currently inside of, -1 when we are not inside one.
	private int _creationIndent = -1;

	public void Reset() => _creationIndent = -1;

	public bool KeepInPowerLog(LogLine line)
	{
		if(line.Namespace != "Power" || line.LineContent == null
			|| !line.LineContent.StartsWith(PowerLinePrefix))
			return NotACreation();

		var body = line.LineContent.Substring(PowerLinePrefix.Length);
		var content = body.TrimStart(' ');
		var indent = body.Length - content.Length;

		if(content.StartsWith(EntityCreation))
		{
			_creationIndent = indent;
			return true;
		}

		// The tags of a creation are logged as its indented children.
		if(_creationIndent >= 0 && indent > _creationIndent && content.StartsWith(EntityTag))
			return true;

		return NotACreation();
	}

	private bool NotACreation()
	{
		_creationIndent = -1;
		return false;
	}
}

namespace HearthWatcher.EventArgs;

public class DiscoverStateArgs : System.EventArgs
{
	public string CardId { get; }
	public int ZonePosition { get; }
	public int ZoneSize { get; }

	public DiscoverStateArgs(string cardId, int zonePosition, int zoneSize)
	{
		CardId = cardId;
		ZonePosition = zonePosition;
		ZoneSize = zoneSize;
	}

	public override bool Equals(object? obj) => obj is DiscoverStateArgs args
	                                            && args.CardId == CardId
	                                            && args.ZonePosition == ZonePosition
	                                            && args.ZoneSize == ZoneSize;

	public override int GetHashCode()
	{
		var hashCode = -2012095321;
		hashCode = hashCode * -1521134295 + CardId.GetHashCode();
		hashCode = hashCode * -1521134295 + ZonePosition.GetHashCode();
		hashCode = hashCode * -1521134295 + ZoneSize.GetHashCode();
		return hashCode;
	}
}

namespace Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;

public class IncindiusCounter: ICardExtraInfo
{
	public int Counter { get; set; }

	public int TurnPlayed { get; }

	public string? CardNameSuffix => Counter > 0 ? $"(+{Counter})" : null;

	public IncindiusCounter(int turnPlayed, int counter = 1)
	{
		TurnPlayed = turnPlayed;
		Counter = counter;
	}


	public bool Equals(ICardExtraInfo? other) => other is IncindiusCounter counter
	                                             && TurnPlayed == counter.TurnPlayed
	                                             && Counter == counter.Counter;

	public override bool Equals(object? other)
	{
		return Equals(other as IncindiusCounter);
	}

	public override int GetHashCode()
	{
		return TurnPlayed;
	}

	public object Clone() => new IncindiusCounter(TurnPlayed, Counter);
}

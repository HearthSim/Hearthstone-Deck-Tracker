using System;

namespace Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;

public interface ICardExtraInfo: IEquatable<ICardExtraInfo?>, ICloneable
{
	string? CardNameSuffix { get; }
}

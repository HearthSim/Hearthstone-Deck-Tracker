using System.Collections.Generic;
using System.Linq;

namespace HearthWatcher.EventArgs;

public class MulliganTooltipArgs : System.EventArgs
{
	public int ZoneSize { get; }
	public int ZonePosition { get; }
	public bool IsTooltipOnRight { get; }
	public string[] TooltipCards { get; }

	public MulliganTooltipArgs(int zoneSize, int zonePosition, bool isTooltipOnRight, string[] tooltipCards)
	{
		ZoneSize = zoneSize;
		ZonePosition = zonePosition;
		IsTooltipOnRight = isTooltipOnRight;
		TooltipCards = tooltipCards;
	}

	public override bool Equals(object obj) => obj is MulliganTooltipArgs args
		&& ZoneSize == args.ZoneSize
		&& ZonePosition == args.ZonePosition
		&& IsTooltipOnRight == args.IsTooltipOnRight
		&& TooltipCards.SequenceEqual(args.TooltipCards);

	public override int GetHashCode()
	{
		var hashCode = -2012095321;
		hashCode = hashCode * -1521134295 + ZoneSize.GetHashCode();
		hashCode = hashCode * -1521134295 + ZonePosition.GetHashCode();
		hashCode = hashCode * -1521134295 + IsTooltipOnRight.GetHashCode();
		hashCode = hashCode * -1521134295 + TooltipCards.GetHashCode();
		return hashCode;
	}
}

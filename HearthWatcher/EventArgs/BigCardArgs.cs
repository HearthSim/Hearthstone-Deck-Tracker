using System.Collections.Generic;
using System.Linq;

namespace HearthWatcher.EventArgs
{
	public class BigCardArgs : System.EventArgs
	{
		public List<float> TooltipHeights { get; }
		public List<float> EnchantmentHeights { get; }
		public string CardId { get; }
		public int ZonePosition { get; }
		public int ZoneSize { get; }
		public int Side { get; }
		public bool IsHand { get; }

		public BigCardArgs(List<float> tooltipHeights, List<float> enchantmentHeights, string cardId, int zonePosition, int zoneSize, int side, bool isHand)
		{
			TooltipHeights = tooltipHeights;
			EnchantmentHeights = enchantmentHeights;
			CardId = cardId;
			ZonePosition = zonePosition;
			ZoneSize = zoneSize;
			Side = side;
			IsHand = isHand;
		}

		public override bool Equals(object obj) => obj is BigCardArgs args
			&& args.TooltipHeights.SequenceEqual(TooltipHeights)
			&& args.EnchantmentHeights.SequenceEqual(EnchantmentHeights)
			&& args.CardId == CardId
			&& args.ZonePosition == ZonePosition
			&& args.ZoneSize == ZoneSize
			&& args.Side == Side
			&& args.IsHand == IsHand;

		public override int GetHashCode()
		{
			var hashCode = -2012095321;
			hashCode = hashCode * -1521134295 + TooltipHeights.GetHashCode();
			hashCode = hashCode * -1521134295 + EnchantmentHeights.GetHashCode();
			hashCode = hashCode * -1521134295 + CardId.GetHashCode();
			hashCode = hashCode * -1521134295 + ZonePosition.GetHashCode();
			hashCode = hashCode * -1521134295 + ZoneSize.GetHashCode();
			hashCode = hashCode * -1521134295 + Side.GetHashCode();
			hashCode = hashCode * -1521134295 + IsHand.GetHashCode();
			return hashCode;
		}
	}
}

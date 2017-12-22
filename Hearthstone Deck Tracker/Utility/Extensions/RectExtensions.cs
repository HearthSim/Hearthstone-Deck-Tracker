using System.Windows;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class RectExtensions
	{
		public static Rect Move(this Rect rect, int x, int y) => new Rect(rect.X + x, rect.Y + y, rect.Width, rect.Height);
	}
}

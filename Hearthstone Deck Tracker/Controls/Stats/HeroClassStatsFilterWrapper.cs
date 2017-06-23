using System.Windows;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Stats
{
	public class HeroClassStatsFilterWrapper
	{
		public HeroClassStatsFilterWrapper(HeroClassStatsFilter heroClass)
		{
			HeroClass = heroClass;
		}

		public HeroClassStatsFilter HeroClass { get; }

		public BitmapImage ClassImage => ImageCache.GetClassIcon(HeroClass.ToString());

		public Visibility ImageVisibility => HeroClass == HeroClassStatsFilter.All ? Visibility.Collapsed : Visibility.Visible;

		public override bool Equals(object obj)
		{
			var wrapper = obj as HeroClassStatsFilterWrapper;
			return wrapper != null && HeroClass.Equals(wrapper.HeroClass);
		}

		public override int GetHashCode() => HeroClass.GetHashCode();
	}
}

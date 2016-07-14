using System;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class VersionExtensions
	{
		public static string ToVersionString(this Version version) => $"{version?.Major}.{version?.Minor}.{version?.Build}";
	}
}
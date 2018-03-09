using System;

namespace Hearthstone_Deck_Tracker.Utility
{
	public abstract class Singleton<T> where T : class
	{
		private static readonly Lazy<T> LazyInstance = new Lazy<T>(() => (T)Activator.CreateInstance(typeof(T) , true));
		public static T Instance => LazyInstance.Value;
	}
}

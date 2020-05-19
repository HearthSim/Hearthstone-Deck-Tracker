using System.Threading.Tasks;
using BobsBuddy;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal class MinionHeroPowerTrigger
	{
		public MinionHeroPowerTrigger(Minion minion, string heroPowerId)
		{
			Minion = minion;
			HeroPowerId = heroPowerId;
			Tsc = new TaskCompletionSource<object>();
		}

		public Minion Minion { get; }
		public string HeroPowerId { get; }
		public TaskCompletionSource<object> Tsc { get; }
	}
}

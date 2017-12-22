#region

using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Interfaces
{
	public interface IHsLogReader
	{
		/// <summary>
		/// Start tracking gamelogs with default impelementaion of GameEventHandler
		/// </summary>
		void Start(GameV2 game);

		/// <summary>
		/// Start tracking gamelogs with custom impelementaion of GameEventHandler
		/// </summary>
		/// <param name="gh"> Custom Game handler implementation </param>
		void Start(IGameHandler gh, GameV2 game);

		void Stop();
		void ClearLog();
		Task<bool> RankedDetection(int timeoutInSeconds = 3);
		//void GetCurrentRegion();
		void Reset(bool full);
	}
}

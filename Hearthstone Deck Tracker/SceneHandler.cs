using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;


namespace Hearthstone_Deck_Tracker;

public class SceneHandler
{
	public static Mode? Scene { get; private set; }

	private static bool? Transitioning = null;

	public static void OnSceneUpdate(Mode prevMode, Mode mode, bool sceneLoaded, bool transitioning)
	{
		if(Transitioning is null || transitioning)
		{
			OnSceneTransitionStart(prevMode, mode);
			Transitioning = true;
		}

		if(!transitioning && sceneLoaded)
		{
			OnSceneTransitionComplete(prevMode, mode);
			Transitioning = false;
		}
	}

	private static void OnSceneTransitionStart(Mode from, Mode to)
	{
		Scene = null;

		if(from == Mode.TOURNAMENT)
		{
			Core.Overlay.UpdateMulliganGuidePreLobby();
			Watchers.DeckPickerWatcher.Stop();
			Core.Overlay.ConstructedMulliganGuidePreLobbyViewModel.InvalidateAllDecks();
		}

		if(from == Mode.BACON)
		{
			if(to != Mode.GAMEPLAY)
				Core.Overlay.ShowBattlegroundsSession(false, true);
			Core.Overlay.ShowTier7PreLobby(false, false);
			Watchers.BaconWatcher.Stop();
		}
	}

	private static void OnSceneTransitionComplete(Mode from, Mode to)
	{
		Scene = to;

		if(to == Mode.TOURNAMENT)
		{
			Watchers.DeckPickerWatcher.Run();
			Core.Overlay.UpdateMulliganGuidePreLobby();
		}

		if(to == Mode.BACON)
		{
			Core.Game.CacheBattlegroundRatingInfo();
			Core.Game.BattlegroundsSessionViewModel.Update();
			if(Config.Instance.ShowSessionRecapBetweenGames)
				Core.Overlay.ShowBattlegroundsSession(true);
			Core.Overlay.ShowTier7PreLobby(true, true);
			Watchers.BaconWatcher.Run();
		}
	}
}

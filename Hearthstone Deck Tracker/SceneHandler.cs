using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.RemoteData;


namespace Hearthstone_Deck_Tracker;

public class SceneHandler
{
	public static Mode? LastScene { get; private set; }
	public static Mode? Scene { get; private set; } // null while transitioning
	public static Mode? NextScene { get; private set; }

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
		LastScene = from;
		NextScene = to;
		Scene = null;

		if(from == Mode.TOURNAMENT)
		{
			Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
			Core.Overlay.ConstructedMulliganGuidePreLobbyViewModel.InvalidateAllDecks();
			Watchers.DeckPickerWatcher.Stop();
		}
		else if(from == Mode.BACON)
		{
			Core.Overlay.UpdateBattlegroundsSessionVisibility();
			Core.Overlay.UpdateTier7PreLobbyVisibility();
			Watchers.BaconWatcher.Stop();
		}
		else if(from == Mode.GAMEPLAY)
		{
			Core.Overlay.UpdateBattlegroundsSessionVisibility();
			Watchers.BattlegroundsTeammateBoardStateWatcher.Stop();
			Watchers.BaconWatcher.Stop();
			Watchers.BigCardWatcher.Stop();
			Watchers.ChoicesWatcher.Stop();
			Watchers.DiscoverStateWatcher.Stop();
			Watchers.MulliganTooltipWatcher.Stop();
		}
		else if(from == Mode.DRAFT)
		{
			Core.Overlay.UpdateArenaPickHelperVisibility();
			Core.Overlay.UpdateArenaPreLobbyVisibility();
			Watchers.ArenaWatcher.Stop();
			Watchers.ArenaStateWatcher.Stop();
		}
	}

	private static void OnSceneTransitionComplete(Mode from, Mode to)
	{
		NextScene = null;
		Scene = to;

		if(to == Mode.TOURNAMENT)
		{
			Watchers.DeckPickerWatcher.Run();
			Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
		}
		else if(to == Mode.BACON)
		{
			Core.Game.CacheBattlegroundsRatingInfo();
			Core.Game.BattlegroundsSessionViewModel.Update();
			Core.Overlay.UpdateBattlegroundsSessionVisibility();
			Core.Overlay.UpdateTier7PreLobbyVisibility();
			Watchers.BaconWatcher.Run();
			Remote.Config.Load();
		}
		else if(to == Mode.GAMEPLAY)
		{
			Core.Overlay.UpdateBattlegroundsSessionVisibility();
			Watchers.BigCardWatcher.Run();
			Watchers.ChoicesWatcher.Run();
			Watchers.DiscoverStateWatcher.Run();
			Watchers.BaconWatcher.Run();
			Watchers.MulliganTooltipWatcher.Run();
			Watchers.OpponentBoardStateWatcher.Run();
		}
		else if(to == Mode.DRAFT)
		{
			Core.Game.CacheArenaRating();
			Core.Overlay.UpdateArenaPickHelperVisibility();
			Core.Overlay.UpdateArenaPreLobbyVisibility();
			Watchers.ArenaWatcher.Run();
			Watchers.ArenaStateWatcher.Run();
			Remote.Config.Load();
		}

		if(from == Mode.BACON)
		{
			Core.Overlay.Tier7PreLobbyViewModel.InvalidateUserState();
		}
	}
}

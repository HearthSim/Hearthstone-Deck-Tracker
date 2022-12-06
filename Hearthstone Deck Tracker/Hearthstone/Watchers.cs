using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using HearthWatcher;
using HearthWatcher.Providers;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class Watchers
	{
		static Watchers()
		{
			ArenaWatcher.OnCompleteDeck += (sender, args) => DeckManager.AutoImportArena(Config.Instance.SelectedArenaImportingBehaviour ?? ArenaImportingBehaviour.AutoImportSave, args.Info);
			PackWatcher.NewPackEventHandler += (sender, args) => PackUploader.UploadPack(args.PackId, args.Cards);
			DungeonRunWatcher.DungeonRunMatchStarted += (newRun, set) => DeckManager.DungeonRunMatchStarted(newRun, set, false);
			DungeonRunWatcher.DungeonInfoChanged += dungeonInfo => DeckManager.UpdateDungeonRunDeck(dungeonInfo, false);
			PVPDungeonRunWatcher.PVPDungeonRunMatchStarted += (newRun, set) => DeckManager.DungeonRunMatchStarted(newRun, set, true);
			PVPDungeonRunWatcher.PVPDungeonInfoChanged += dungeonInfo => DeckManager.UpdateDungeonRunDeck(dungeonInfo, true);
			FriendlyChallengeWatcher.OnFriendlyChallenge += OnFriendlyChallenge;
			ExperienceWatcher.NewExperienceHandler += (sender, args) => Core.Overlay.ExperienceChangedAsync(args.Experience, args.ExperienceNeeded, args.Level, args.LevelChange, args.Animate).Forget();
			QueueWatcher.InQueueChanged += (sender, args) => Core.Game.QueueEvents.Handle(args);
			BaconWatcher.Change += OnBaconChange;
		}

		internal static void Stop()
		{
			ArenaWatcher.Stop();
			PackWatcher.Stop();
			DungeonRunWatcher.Stop();
			PVPDungeonRunWatcher.Stop();
			FriendlyChallengeWatcher.Stop();
			ExperienceWatcher.Stop();
			QueueWatcher.Stop();
			BaconWatcher.Stop();
		}

		internal static void OnBaconChange(object sender, HearthWatcher.EventArgs.BaconEventArgs args)
		{
			Core.Overlay.ShowBattlegroundsSession(!args.IsAnyOpen);
			Core.Overlay.ShowTier7PreLobby(!args.IsAnyOpen, false);
		}

		internal static void OnFriendlyChallenge(object sender, HearthWatcher.EventArgs.FriendlyChallengeEventArgs args)
		{
			if(args.DialogVisible)
			{
				switch(Config.Instance.ChallengeAction)
				{
					case HsActionType.Flash:
						User32.FlashHs();
						break;
					case HsActionType.Popup:
						User32.BringHsToForeground();
						break;
				}
			}
		}

		public static ArenaWatcher ArenaWatcher { get; } = new ArenaWatcher(new HearthMirrorArenaProvider());
		public static PackOpeningWatcher PackWatcher { get; } = new PackOpeningWatcher(new HearthMirrorPackProvider());
		public static DungeonRunWatcher DungeonRunWatcher { get; } = new DungeonRunWatcher(new GameDataProvider());
		public static PVPDungeonRunWatcher PVPDungeonRunWatcher { get; } = new PVPDungeonRunWatcher(new GameDataProvider());
		public static FriendlyChallengeWatcher FriendlyChallengeWatcher { get; } = new FriendlyChallengeWatcher(new HearthMirrorFriendlyChallengeProvider());
		public static ExperienceWatcher ExperienceWatcher { get; } = new ExperienceWatcher(new HearthMirrorRewardTrackProvider());
		public static QueueWatcher QueueWatcher { get; } = new QueueWatcher(new HearthMirrorQueueProvider());
		public static BaconWatcher BaconWatcher { get; } = new BaconWatcher(new HearthMirrorBaconProvider());
	}

	public class GameDataProvider : IGameDataProvider
	{
		public bool InAiMatch => Core.Game.CurrentMode == Mode.GAMEPLAY && Core.Game.MatchInfo?.GameType == (int)GameType.GT_VS_AI;
		public bool InAdventureScreen => Core.Game.CurrentMode == Mode.ADVENTURE;
		public bool InPVPDungeonRunScreen
		{
			get
			{
				return Core.Game.CurrentMode == Mode.PVP_DUNGEON_RUN;
			}
		}

		public bool InPVPDungeonRunMatch
		{
			get
			{
				return Core.Game.CurrentMode == Mode.GAMEPLAY && Core.Game.PreviousMode == Mode.PVP_DUNGEON_RUN;
			}
		}
		public string? OpponentHeroId => Core.Game.Opponent.Board.FirstOrDefault(x => x.IsHero)?.CardId;
		public int OpponentHeroHealth => Core.Game.Opponent.Board.FirstOrDefault(x => x.IsHero)?.GetTag(GameTag.HEALTH) ?? 0;
	}

	public class HearthMirrorPackProvider : IPackProvider
	{
		public List<HearthMirror.Objects.Card> GetCards() => Reflection.GetPackCards();
		public int GetPackId() => Reflection.GetLastOpenedBoosterId();
	}

	public class HearthMirrorArenaProvider : IArenaProvider
	{
		public ArenaInfo? GetArenaInfo() => DeckImporter.FromArena(false);
		public HearthMirror.Objects.Card[]? GetDraftChoices() => Reflection.GetArenaDraftChoices()?.ToArray();
	}

	public class HearthMirrorFriendlyChallengeProvider : IFriendlyChallengeProvider
	{
		public bool DialogVisible => Reflection.IsFriendlyChallengeDialogVisible();
	}

	public class HearthMirrorRewardTrackProvider : IExperienceProvider
	{
		public RewardTrackData GetRewardTrackData() => Reflection.GetRewardTrackData();
	}

	public class HearthMirrorQueueProvider : IQueueProvider
	{
		public FindGameState? FindGameState => Reflection.GetFindGameState();
	}

	public class HearthMirrorBaconProvider : IBaconProvider
	{
		public bool IsShopOpen => Reflection.IsShopOpen();
		public bool IsJournalOpen => Reflection.IsJournalOpen();
		public bool IsPopupShowing => Reflection.IsPopupShowing();
		public bool IsFriendslistOpen => Reflection.IsFriendsListVisible();
	}
}

using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Importing;
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
			DungeonRunWatcher.DungeonRunMatchStarted += DeckManager.DungeonRunMatchStarted;
			DungeonRunWatcher.DungeonInfoChanged += DeckManager.UpdateDungeonRunDeck;
			FriendlyChallengeWatcher.OnFriendlyChallenge += OnFriendlyChallenge;
		}

		internal static void Stop()
		{
			ArenaWatcher.Stop();
			PackWatcher.Stop();
			DungeonRunWatcher.Stop();
			FriendlyChallengeWatcher.Stop();
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
		public static FriendlyChallengeWatcher FriendlyChallengeWatcher { get; } = new FriendlyChallengeWatcher(new HearthMirrorFriendlyChallengeProvider());
	}

	public class GameDataProvider : IGameDataProvider
	{
		public bool InAiMatch => Core.Game.CurrentMode == Mode.GAMEPLAY && Core.Game.MatchInfo?.GameType == (int)GameType.GT_VS_AI;
		public bool InAdventureScreen => Core.Game.CurrentMode == Mode.ADVENTURE;
		public string OpponentHeroId => Core.Game.Opponent.Board.FirstOrDefault(x => x.IsHero)?.CardId;
	}

	public class HearthMirrorPackProvider : IPackProvider
	{
		public List<HearthMirror.Objects.Card> GetCards() => Reflection.GetPackCards();
		public int GetPackId() => Reflection.GetLastOpenedBoosterId();
	}

	public class HearthMirrorArenaProvider : IArenaProvider
	{
		public ArenaInfo GetArenaInfo() => DeckImporter.FromArena(false);
		public HearthMirror.Objects.Card[] GetDraftChoices() => Reflection.GetArenaDraftChoices()?.ToArray();
	}

	public class HearthMirrorFriendlyChallengeProvider : IFriendlyChallengeProvider
	{
		public bool DialogVisible => Reflection.IsFriendlyChallengeDialogVisible();
	}
}

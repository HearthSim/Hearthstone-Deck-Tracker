using System.Collections.Generic;
using System.Linq;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HearthWatcher;
using HearthWatcher.Providers;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class Watchers
	{
		static Watchers()
		{
			ArenaWatcher.OnCompleteDeck += (sender, args) => DeckManager.AutoImportArena(Config.Instance.SelectedArenaImportingBehaviour ?? ArenaImportingBehaviour.AutoImportSave, args.Info);
			PackWatcher.NewPackEventHandler += (sender, args) => Log.Debug($"New Pack! Id={args.PackId}, Cards=[{string.Join(", ", args.Cards.Select(x => x.Id + (x.Premium ? " (g)" : "")))}]");
		}
		public static ArenaWatcher ArenaWatcher { get; } = new ArenaWatcher(new HearthMirrorArenaProvider());
		public static PackOpeningWatcher PackWatcher { get; } = new PackOpeningWatcher(new HearthMirrorPackProvider());
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
}

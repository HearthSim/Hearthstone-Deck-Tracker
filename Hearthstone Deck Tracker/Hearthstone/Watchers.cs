using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Importing;
using HearthWatcher;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class Watchers
	{
		static Watchers()
		{
			ArenaWatcher.OnCompleteDeck += (sender, args) => DeckManager.AutoImportArena(Config.Instance.SelectedArenaImportingBehaviour ?? ArenaImportingBehaviour.AutoImportSave, args.Info);
		}
		public static ArenaWatcher ArenaWatcher { get; } = new ArenaWatcher(new HearthMirrorArenaProvider());
	}

	public class HearthMirrorArenaProvider : IArenaProvider
	{
		public ArenaInfo GetArenaInfo() => DeckImporter.FromArena(false);
		public HearthMirror.Objects.Card[] GetDraftChoices() => Reflection.GetArenaDraftChoices()?.ToArray();
	}
}

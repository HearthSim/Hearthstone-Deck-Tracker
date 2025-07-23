using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public class ArenaSettings
	{

		[JsonProperty("arenasmith_overlay")]
		public bool ArenasmithOverlay { get => Config.Instance.EnableArenasmithOverlay; }

		[JsonProperty("arena_prelobby_overlay")]
		public bool ArenaPrelobbyOverlay { get => Config.Instance.ShowArenasmithPreLobby; }

		[JsonProperty("arena_hero_overlay")]
		public bool ArenaHeroOverlay { get => Config.Instance.ShowArenaHeroPicking; }

		[JsonProperty("arena_scores_overlay")]
		public bool ArenasmithScoresOverlay { get => Config.Instance.ShowArenasmithScore; }

		[JsonProperty("arena_related_cards_overlay")]
		public bool ArenaRelatedCardsOverlay { get => Config.Instance.ShowArenaRelatedCards; }

		[JsonProperty("arena_synergies_overlay")]
		public bool ArenaSynergiesOverlay { get => Config.Instance.ShowArenaDeckSynergies; }

		[JsonProperty("arena_redraft_discard_overlay")]
		public bool ArenaRedraftDiscardOverlay { get => Config.Instance.ShowArenaRedraftDiscard; }

	}
}

#region

using System.Collections.Generic;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using Hearthstone_Deck_Tracker.Stats;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public interface IGame
	{
		Player Player { get; set; }
		Player Opponent { get; set; }
		Entity GameEntity { get; }
		Entity PlayerEntity { get; }
		Entity OpponentEntity { get; }
		bool IsMulliganDone { get; }
		bool IsInMenu { get; set; }
		bool IsUsingPremade { get; set; }
		bool IsRunning { get; set; }
		Region CurrentRegion { get; set; }
		GameMode CurrentGameMode { get; }
		GameStats CurrentGameStats { get; set; }
		HearthMirror.Objects.Deck CurrentSelectedDeck { get; set; }
		List<Card> DrawnLastGame { get; set; }
		Dictionary<int, Entity> Entities { get; }
		bool SavedReplay { get; set; }
		GameMetaData MetaData { get; }
		MatchInfo MatchInfo { get; }
		Mode CurrentMode { get; set; }
		Mode PreviousMode { get; set; }
		GameTime GameTime { get; }
		void Reset(bool resetStats = true);
		void StoreGameState();
		string GetStoredPlayerName(int id);
		SecretsManager SecretsManager { get; }
		int OpponentMinionCount { get; }
		int OpponentHandCount { get; }
		bool IsMinionInPlay { get; }
		int PlayerMinionCount { get; }
		GameType CurrentGameType { get; }
		Format? CurrentFormat { get; }
		int ProposedAttacker { get; set; }
		int ProposedDefender { get; set; }
		bool? IsDungeonMatch { get; }
		bool PlayerChallengeable { get; }
		bool SetupDone { get; set; }
	}
}

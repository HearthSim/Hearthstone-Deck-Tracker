using HearthDb.Enums;
using HearthMirror;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class BattlegroundsUtils
	{
		private static readonly Dictionary<Guid, HashSet<Race>> _availableRacesCache = new Dictionary<Guid, HashSet<Race>>();

		const string UntransformedArannaCardid = HearthDb.CardIds.NonCollectible.Neutral.ArannaStarseekerTavernBrawl1;
		const string TransformedArannaCardid = HearthDb.CardIds.NonCollectible.Neutral.ArannaStarseeker_ArannaUnleashedTokenTavernBrawl;

		const string UntransformedQueenAzshara = HearthDb.CardIds.NonCollectible.Neutral.QueenAzsharaBATTLEGROUNDS;
		const string TransformedQueenAzshara = HearthDb.CardIds.NonCollectible.Neutral.QueenAzshara_NagaQueenAzsharaToken;

		private static readonly Dictionary<string, string> TransformableHeroCardidTable = new Dictionary<string, string>()
		{
			{ TransformedArannaCardid, UntransformedArannaCardid },
			{ TransformedQueenAzshara, UntransformedQueenAzshara }
		};

		public static HashSet<Race>? GetAvailableRaces(Guid? gameId)
		{
			if(!gameId.HasValue)
				return AvailableRaces;
			if(!_availableRacesCache.TryGetValue(gameId.Value, out var races))
			{
				races = AvailableRaces;
				// Before initialized this contains only contains Race.INVALID
				if (races != null && races.Count > 1)
					_availableRacesCache[gameId.Value] = races;
			}
			return races;
		}

		private static HashSet<Race>? AvailableRaces
		{
			get
			{
				var races = Reflection.GetAvailableBattlegroundsRaces();
				if(races == null)
					return null;
				return new HashSet<Race>(races.Cast<Race>());
			}
		}

		public static string GetOriginalHeroId(string heroId) => TransformableHeroCardidTable.TryGetValue(heroId, out var mapped) ? mapped : heroId;
	}
}

using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using NuGet;
using System.Collections.Generic;
using System.Linq;
using static Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets;

namespace HDTTests.Hearthstone.Secrets
{
	public class MockAvailableSecrets : AvailableSecretsProvider
	{
		public Dictionary<string, HashSet<string>> ByType { get; }

		public MockAvailableSecrets()
		{
			ByType = new Dictionary<string, HashSet<string>>();

			var ftWild = new HashSet<string>();
			ftWild.AddRange(Hunter.All.Select(x => x.Ids[0]));
			ftWild.AddRange(Mage.All.Select(x => x.Ids[0]));
			ftWild.AddRange(Paladin.All.Select(x => x.Ids[0]));
			ftWild.AddRange(Rogue.All.Select(x => x.Ids[0]));
			ByType["FT_WILD"] = ftWild;

			var ftStandard = new HashSet<string>();
			ftStandard.AddRange(Hunter.All.Where(x => x.IsStandard).Select(x => x.Ids[0]));
			ftStandard.AddRange(Mage.All.Where(x => x.IsStandard).Select(x => x.Ids[0]));
			ftStandard.AddRange(Paladin.All.Where(x => x.IsStandard).Select(x => x.Ids[0]));
			ftStandard.AddRange(Rogue.All.Where(x => x.IsStandard).Select(x => x.Ids[0]));
			ByType["FT_STANDARD"] = ftStandard;

			var gtArena = new HashSet<string>
			{
				Hunter.All.First().Ids[0],
				Mage.All.First().Ids[0],
				Paladin.All.First().Ids[0],
				Rogue.All.First().Ids[0]
			};
			ByType["GT_ARENA"] = gtArena;
		}
	}
}

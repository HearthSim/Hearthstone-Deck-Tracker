using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public class RemoteArenaSettings : AvailableSecretsProvider
	{
		public Dictionary<string, HashSet<string>>? ByType => Remote.LiveSecrets?.Data?.ByType;

		public Dictionary<string, Dictionary<string, HashSet<string>>>? CreatedByTypeByCreator =>
			Remote.LiveSecrets?.Data?.CreatedByTypeByCreator;
	}
}

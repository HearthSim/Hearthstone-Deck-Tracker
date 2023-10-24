using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public interface AvailableSecretsProvider
	{
		public Dictionary<string, HashSet<string>>? ByType { get; }
	}
}

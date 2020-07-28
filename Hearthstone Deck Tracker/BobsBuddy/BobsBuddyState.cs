namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	public enum BobsBuddyState
	{
		Initial,
		Combat,
		Shopping
	};

	/// <summary>
	/// Used to pass information to the twitch overlay to decide whether or not to show the relevant stats.
	/// See the bobs buddy file in https://github.com/HearthSim/twitch-hdt-frontend/tree/master/src/viewer/overlay
	/// The values in the enums should match completley; ie, changes to one should be reflected in the other.
	/// </summary>
	public enum TwitchSimulationState
	{
		WaitingForCombat = 1,
		OpponentSecrets = 2,
		TooFewSimulations = 3,
		UpdateRequired = 4,
		UnknownCards = 5,
		InCombat = 6,
		InNonFirstShoppingPhase = 7,
	}
}

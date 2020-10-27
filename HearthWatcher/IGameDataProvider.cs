namespace HearthWatcher
{
	public interface IGameDataProvider
	{
		bool InAiMatch { get; }
		bool InAdventureScreen { get; }
		bool InPVPDungeonRunScreen { get; }
		string OpponentHeroId { get; }
		int OpponentHeroHealth { get; }
	}
}

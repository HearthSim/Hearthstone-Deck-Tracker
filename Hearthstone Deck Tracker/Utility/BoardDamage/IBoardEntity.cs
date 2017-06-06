namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public interface IBoardEntity
	{
		string Name { get; }
		int Health { get; }
		int Attack { get; }
		// number of attacks made this turn
		int AttacksThisTurn { get; }
		// ability to attack this turn (some exceptions)
		bool Exhausted { get; }
		// whether to include in damage calculation
		bool Include { get; }
		// the zone the entity is in
		string Zone { get; }
	}
}

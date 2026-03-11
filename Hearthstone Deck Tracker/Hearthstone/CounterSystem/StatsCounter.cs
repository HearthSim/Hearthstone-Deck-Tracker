namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;

public abstract class StatsCounter : BaseCounter
{
	private int _attack;
	protected int AttackCounter
	{
		get => _attack;
		set
		{
			if (_attack != value)
			{
				_attack = value;
				OnCounterChanged();
				OnPropertyChanged();
			}
		}
	}

	private int _health;
	protected int HealthCounter
	{
		get => _health;
		set
		{
			if (_health != value)
			{
				_health = value;
				OnCounterChanged();
				OnPropertyChanged();
			}
		}
	}

	public override int SortValue => AttackCounter + HealthCounter;

	public StatsCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
		AttackCounter = 0;
		HealthCounter = 0;
	}
}

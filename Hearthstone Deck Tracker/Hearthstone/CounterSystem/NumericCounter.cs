namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;

public abstract class NumericCounter : BaseCounter
{
	private int _counter;
	protected int Counter
	{
		get => _counter;
		set
		{
			if (_counter != value)
			{
				_counter = value;
				OnCounterChanged();
				OnPropertyChanged();
			}
		}
	}

	protected NumericCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
		Counter = 0;
	}

	public override string ValueToShow() => Counter.ToString();
}

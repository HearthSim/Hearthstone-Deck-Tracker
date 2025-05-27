using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

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

	protected Entity? LastEntityToCount;

	protected bool DiscountIfCantPlay(GameTag tag, int value, Entity entity)
	{
		if(LastEntityToCount is null || entity.Id != LastEntityToCount.Id
		                             || tag != GameTag.CANT_PLAY || value <= 0) return false;
		Counter--;
		return true;
	}
}

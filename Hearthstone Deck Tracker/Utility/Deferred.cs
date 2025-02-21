using System;

namespace Hearthstone_Deck_Tracker.Utility;


public class Deferred : IDisposable
{
	private readonly Action _action;

	public Deferred(Action action)
	{
		_action = action;
	}

	public void Dispose() => _action();
}

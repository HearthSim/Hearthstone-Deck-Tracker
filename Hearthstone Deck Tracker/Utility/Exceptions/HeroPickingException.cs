using System;

namespace Hearthstone_Deck_Tracker.Utility.Exceptions;

public class HeroPickingException : Exception
{
	public HeroPickingException(string message) : base(message)
	{
	}
}

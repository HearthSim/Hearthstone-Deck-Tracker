using System;

namespace Hearthstone_Deck_Tracker.Utility.Exceptions;

public class HeroPickingDisabledException : HeroPickingException
{
	public HeroPickingDisabledException(string message) : base(message)
	{
	}
}

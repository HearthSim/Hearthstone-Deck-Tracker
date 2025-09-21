using System;

namespace Hearthstone_Deck_Tracker.Utility.Exceptions;

public class DuplicatedRequestException : Exception
{
	public DuplicatedRequestException(string message) : base(message)
	{
	}
}

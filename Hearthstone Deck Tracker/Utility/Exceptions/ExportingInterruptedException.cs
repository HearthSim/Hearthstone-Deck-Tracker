using System;

namespace Hearthstone_Deck_Tracker.Utility.Exceptions
{
	public class ExportingInterruptedException : Exception
	{
		public ExportingInterruptedException(string message) : base(message)
		{
		}
	}
}

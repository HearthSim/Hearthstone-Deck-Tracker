using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.LogReader.Interfaces;

public interface IHsChoice
{
	int Id { get; }
	ChoiceType ChoiceType { get; }
	int SourceEntityId { get; }
	IEnumerable<int> OfferedEntityIds { get; }
}

public interface IHsCompletedChoice : IHsChoice
{
	IEnumerable<int> ChosenEntityIds { get;  }
}

using System.Collections.Generic;
using System.Linq;

namespace HearthWatcher.EventArgs;

public class AssembleStateArgs : System.EventArgs
{
	public bool IsShowing { get; }
	public int TargetEntityId { get; }
	public int SelectedChoiceEntityId { get; }
	public IReadOnlyList<int> ChoiceEntityIds { get; }
	public int HoveredChoiceIndex { get; }
	public int HoveredHalf { get; }

	public AssembleStateArgs(bool isShowing, int targetEntityId, int selectedChoiceEntityId,
		IReadOnlyList<int> choiceEntityIds, int hoveredChoiceIndex, int hoveredHalf)
	{
		IsShowing = isShowing;
		TargetEntityId = targetEntityId;
		SelectedChoiceEntityId = selectedChoiceEntityId;
		ChoiceEntityIds = choiceEntityIds;
		HoveredChoiceIndex = hoveredChoiceIndex;
		HoveredHalf = hoveredHalf;
	}

	public override bool Equals(object? obj) => obj is AssembleStateArgs args
	                                            && args.IsShowing == IsShowing
	                                            && args.TargetEntityId == TargetEntityId
	                                            && args.SelectedChoiceEntityId == SelectedChoiceEntityId
	                                            && args.ChoiceEntityIds.SequenceEqual(ChoiceEntityIds)
	                                            && args.HoveredChoiceIndex == HoveredChoiceIndex
	                                            && args.HoveredHalf == HoveredHalf;

	public override int GetHashCode()
	{
		var hashCode = -1108542611;
		hashCode = hashCode * -1521134295 + IsShowing.GetHashCode();
		hashCode = hashCode * -1521134295 + TargetEntityId.GetHashCode();
		hashCode = hashCode * -1521134295 + SelectedChoiceEntityId.GetHashCode();
		hashCode = hashCode * -1521134295 + HoveredChoiceIndex.GetHashCode();
		hashCode = hashCode * -1521134295 + HoveredHalf.GetHashCode();
		return hashCode;
	}
}

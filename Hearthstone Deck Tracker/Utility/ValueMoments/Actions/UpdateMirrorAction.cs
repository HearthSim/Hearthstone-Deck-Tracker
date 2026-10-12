using Hearthstone_Deck_Tracker.Utility.Updating;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;

public class UpdateMirrorAction : VMAction
{
	public UpdateMirrorAction(UpdateMirror oldValue, UpdateMirror newValue) : base(Franchise.All, null, null)
	{
		OldValue = oldValue;
		NewValue = newValue;
	}

	public override string Name => "HDT Squirrel Remote Changed";
	public override ActionSource Source { get => ActionSource.App; }
	public override string Type => "Updater Remote Change";

	[JsonProperty("old_remote")]
	public UpdateMirror OldValue { get; }

	[JsonProperty("new_remote")]
	public UpdateMirror NewValue { get; }
}

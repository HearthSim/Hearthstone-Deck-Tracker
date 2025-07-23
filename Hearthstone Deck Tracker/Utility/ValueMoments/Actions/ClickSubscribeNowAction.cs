using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;

public class ClickSubscribeNowAction : VMAction
{
	public enum Button
	{
		[JsonProperty("arena_pre_lobby")]
		ArenaPreLobby,
		[JsonProperty("battlegrounds_pre_lobby")]
		BattlegroundsPreLobby,
	}

	public ClickSubscribeNowAction(
		Franchise franchise, SubFranchise[]? subFranchise, Button button, int? trialsRemaining
	) : base(franchise, subFranchise, null)
	{
		ButtonPosition = button;
		TrialsRemaining = trialsRemaining;
	}

	public override string Name => "Click Subscribe Now HDT";
	public override ActionSource Source { get => ActionSource.Overlay; }
	public override string Type => "Click Subscribe Now";

	[JsonProperty("button_position_name")]
	[JsonConverter(typeof(EnumJsonConverter))]
	public Button ButtonPosition { get; }

	[JsonProperty("trials_remaining", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? TrialsRemaining { get; }
}

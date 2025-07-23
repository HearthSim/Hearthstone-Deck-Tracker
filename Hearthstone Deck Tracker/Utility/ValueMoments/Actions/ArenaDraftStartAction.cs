using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;

public class ArenaDraftStartAction : VMArenaAction
{

	public ArenaDraftStartAction(ArenaDraftInfo draftInfo, bool arenaOverlayVisible, bool trialsActivated, int trialsRemaining) : base(null)
	{
		PlayerId = draftInfo.PlayerId;
		DeckId = draftInfo.DeckId;
		IsUnderground = draftInfo.IsUnderground;
		OverlayVisible = arenaOverlayVisible;
		TrialsActivated = trialsActivated;
		TrialsRemaining = trialsRemaining;
	}

	public override string Name => "Arena Draft Start HDT";
	public override ActionSource Source => ActionSource.App;
	public override string Type => "Arena Draft Start Action";


	[JsonProperty("player_id")]
	public string? PlayerId { get; }

	[JsonProperty("deck_id")]
	public long DeckId { get; }

	[JsonProperty("is_underground")]
	public bool IsUnderground { get; }

	[JsonProperty("overlay_visible")]
	public bool OverlayVisible { get; }

	[JsonProperty("trials_activated")]
	public bool TrialsActivated { get; }

	[JsonProperty("trials_remaining")]
	public int TrialsRemaining { get; }

}

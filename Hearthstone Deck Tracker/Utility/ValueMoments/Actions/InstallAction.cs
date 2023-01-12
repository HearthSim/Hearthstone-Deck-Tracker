using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class InstallAction : VMAction
	{
		public InstallAction() : base(Franchise.All, null, null)
		{ }

		public override string Name => "Install HDT";
		public override ActionSource Source { get => ActionSource.App; }
		public override string Type => "First App Start";

		[JsonProperty("app_version")]
		public string AppVersion => Helper.GetCurrentVersion().ToVersionString(true);
	}
}

using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class InstallAction : VMAction
	{
		public const string Name = "Install HDT";

		public InstallAction() : base(
			Name, ActionSource.App, "First App Start", Franchise.All, null, null
		)
		{ }

		[JsonProperty("app_version")]
		public string AppVersion { get => Helper.GetCurrentVersion().ToVersionString(true); }
	}
}

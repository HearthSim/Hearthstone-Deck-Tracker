using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class SetInitialLanguageAction : VMAction
	{
		public SetInitialLanguageAction(string languageSource, string language, string systemLanguage, string gameLanguage, string systemRegionName)
			: base(Franchise.HSConstructed, null, null)
		{
			Language = language;
			LanguageSource = languageSource;
			SystemLanguage = systemLanguage;
			GameLanguage = gameLanguage;
			SystemRegionName = systemRegionName;
		}

		[JsonProperty("name")]
		public override string Name => "Set Initial Language";

		[JsonProperty("action_source")]
		public override ActionSource Source { get; }

		[JsonProperty("action_type")]
		public override string Type => "Set Initial Language";

		[JsonProperty("language")]
		public string Language { get; }

		[JsonProperty("language_source")]
		public string LanguageSource { get; }

		[JsonProperty("system_language")]
		public string SystemLanguage { get; }

		[JsonProperty("game_language")]
		public string GameLanguage { get; }

		[JsonProperty("system_region_name")]
		public string SystemRegionName { get; }
	}
}

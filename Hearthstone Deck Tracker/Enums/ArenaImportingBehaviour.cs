#region

using System.ComponentModel;

#endregion

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum ArenaImportingBehaviour
	{
		[Description("Auto import&save")]
		AutoImportSave,

		[Description("Auto ask to import")]
		AutoAsk,

		[Description("Manual")]
		Manual
	}
}
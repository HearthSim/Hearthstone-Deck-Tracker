using System;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public abstract class VMActions
	{
		public class InstallAction : VMAction
		{
			public const string Name = "Install HDT";

			public InstallAction() : base(
				Name, ActionSource.App, "First App Start", Franchise.All, null, new Dictionary<string, object>
				{
					{ "app_version", Helper.GetCurrentVersion().ToVersionString(true) },
				}					
			)
			{ }
		}

		public class FirstHSCollectionUploadAction : VMAction
		{
			public const string Name = "Upload First Hearthstone Collection";

			public FirstHSCollectionUploadAction(int collectionSize) : base(
				Name, ActionSource.App, "First Collection Upload", Franchise.HSConstructed, null, new Dictionary<string, object>
				{
					{ "collection_size", collectionSize },
				}
			)
			{ }
		}
		
		public class ToastAction : VMAction
		{
			public const string Name = "Click HDT Toast";
			public enum ToastName
			{
				[MixpanelProperty("mulligan")]
				Mulligan,
				[MixpanelProperty("constructed_collection_uploaded")]
				ConstructedCollectionUploaded,
				[MixpanelProperty("battlegrounds_hero_picker")]
				BattlegroundsHeroPicker,
				[MixpanelProperty("mercenaries_collection_uploaded")]
				MercenariesCollectionUploaded,
			}

			public ToastAction(Franchise franchise, ToastName toastName) : base(
				Name, ActionSource.Overlay, "Toast Click", franchise, null, new Dictionary<string, object>
			{
				{ "toast", toastName },
			}
			)
			{ }
		}

		public class ClickAction : VMAction
		{
			public const string Name = "Click Action HDT";

			public enum ActionName
			{
				[MixpanelProperty("screenshot: Copy to Clipboard")]
				ScreenshotCopyToClipboard,
				[MixpanelProperty("screenshot: Save To Disk")]
				ScreenshotSaveToDisk,
				[MixpanelProperty("screenshot: Upload to Imgur")]
				ScreenshotUploadToImgur,

				[MixpanelProperty("stats: Constructed")]
				StatsArena,
				[MixpanelProperty("stats: Arena")]
				StatsConstructed,
			}

			public ClickAction(Franchise franchise, ActionName actionName) : this(
				franchise, actionName, new Dictionary<string, object>())
			{ }

			public ClickAction(Franchise franchise, ActionName actionName,
				Dictionary<string, object> properties) : base(
				Name, ActionSource.MainWindow, "Click Action", franchise, 10, new Dictionary<string, object>(properties)
				{
					{ "action_name", actionName },
				},
				true
			)
			{ }
		}

		public class CopyDeckAction : VMAction
		{
			public const string Name = "Copy Deck HDT";
			public enum ActionName
			{
				[MixpanelProperty("Copy All")]
				CopyAll,
				[MixpanelProperty("Copy Code")]
				CopyCode,
				[MixpanelProperty("Copy Ids to Clipboard")]
				CopyIds,
				[MixpanelProperty("Copy Names to Clipboard")]
				CopyNames,
				[MixpanelProperty("Save as XML")]
				SaveAsXML,
			}

			public CopyDeckAction(Franchise franchise, ActionName actionName) : base(
				Name, ActionSource.MainWindow, "Copy Deck", franchise, 10, new Dictionary<string, object>
				{
					{ "action_name", actionName },
				},
				true
			)
			{ }
		}

		public class EndMatchAction : VMAction
		{
			public const string Name = "End Match Action HDT";
			
			public static EndMatchAction Create(Dictionary<HearthstoneExtraData, object> extraData) => new(Franchise.HSConstructed)
			{
				FranchiseProperties = new FranchiseProperties(extraData)
			};

			public static EndMatchAction Create(Dictionary<BattlegroundsExtraData, object> extraData) => new(Franchise.Battlegrounds)
			{
				FranchiseProperties = new FranchiseProperties(extraData)
			};

			public static EndMatchAction Create(Dictionary<MercenariesExtraData, object> extraData) => new(Franchise.Mercenaries)
			{
				FranchiseProperties = new FranchiseProperties(extraData)
			};

			private EndMatchAction(Franchise franchise) : base(
				Name, ActionSource.App, "End Match Action", franchise, 1, new Dictionary<string, object>
				{
					{ "action_name", "end_match" },
				}
			)
			{ }
		}

		public class EndSpectateMatchAction : VMAction
		{
			public const string Name = "End Spectate Match Action HDT";

			public static EndSpectateMatchAction Create(Dictionary<HearthstoneExtraData, object> extraData) => new(Franchise.HSConstructed)
			{
				FranchiseProperties = new FranchiseProperties(extraData)
			};

			public static EndSpectateMatchAction Create(Dictionary<BattlegroundsExtraData, object> extraData) => new(Franchise.Battlegrounds)
			{
				FranchiseProperties = new FranchiseProperties(extraData)
			};

			public static EndSpectateMatchAction Create(Dictionary<MercenariesExtraData, object> extraData) => new(Franchise.Mercenaries)
			{
				FranchiseProperties = new FranchiseProperties(extraData)
			};

			private EndSpectateMatchAction(Franchise franchise) : base(
				Name, ActionSource.App, "End Spectate Match Action", franchise, 1, new Dictionary<string, object>
				{
					{ "action_name", "end_match"},
				}
			)
			{ }
		}
	}
}

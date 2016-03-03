#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class ConfigManager
	{
		public static Version UpdatedVersion { get; private set; }

		public static void Run()
		{
			var configVersion = string.IsNullOrEmpty(Config.Instance.CreatedByVersion) ? null : new Version(Config.Instance.CreatedByVersion);
			var currentVersion = Helper.GetCurrentVersion();
			if(currentVersion != null)
			{
				// Assign current version to the config instance so that it will be saved when the config
				// is rewritten to disk, thereby telling us what version of the application created it
				Config.Instance.CreatedByVersion = currentVersion.ToString();
			}
			ConvertLegacyConfig(currentVersion, configVersion);

			if(Config.Instance.SelectedTags.Count == 0)
				Config.Instance.SelectedTags.Add("All");

			if(!Directory.Exists(Config.Instance.DataDir))
				Config.Instance.Reset(nameof(Config.DataDirPath));
		}

		// Logic for dealing with legacy config file semantics
		// Use difference of versions to determine what should be done
		private static void ConvertLegacyConfig(Version currentVersion, Version configVersion)
		{
			var converted = false;

			var v0_3_21 = new Version(0, 3, 21, 0);

			if(configVersion == null) // Config was created prior to version tracking being introduced (v0.3.20)
			{
				Config.Instance.ResetAll();
				Config.Instance.CreatedByVersion = currentVersion.ToString();
				converted = true;
			}
			else
			{
				if(configVersion <= v0_3_21)
				{
					// Config must be between v0.3.20 and v0.3.21 inclusive
					// It was still possible in 0.3.21 to see (-32000, -32000) window positions
					// under certain circumstances (GitHub issue #135).
					if(Config.Instance.TrackerWindowLeft == -32000)
					{
						Config.Instance.Reset(nameof(Config.TrackerWindowLeft));
						converted = true;
					}
					if(Config.Instance.TrackerWindowTop == -32000)
					{
						Config.Instance.Reset(nameof(Config.TrackerWindowTop));
						converted = true;
					}

					if(Config.Instance.PlayerWindowLeft == -32000)
					{
						Config.Instance.Reset(nameof(Config.PlayerWindowLeft));
						converted = true;
					}
					if(Config.Instance.PlayerWindowTop == -32000)
					{
						Config.Instance.Reset(nameof(Config.PlayerWindowTop));
						converted = true;
					}

					if(Config.Instance.OpponentWindowLeft == -32000)
					{
						Config.Instance.Reset(nameof(Config.OpponentWindowLeft));
						converted = true;
					}
					if(Config.Instance.OpponentWindowTop == -32000)
					{
						Config.Instance.Reset(nameof(Config.OpponentWindowTop));
						converted = true;
					}

					if(Config.Instance.TimerWindowLeft == -32000)
					{
						Config.Instance.Reset(nameof(Config.TimerWindowLeft));
						converted = true;
					}
					if(Config.Instance.TimerWindowTop == -32000)
					{
						Config.Instance.Reset(nameof(Config.TimerWindowTop));
						converted = true;
					}

					//player scaling used to be increased by a very minimal about to circumvent some problem,
					//should no longer be required. not sure is the increment is actually noticeable, but resetting can't hurt
					if(Config.Instance.OverlayOpponentScaling > 100)
					{
						Config.Instance.OverlayOpponentScaling = 100;
						converted = true;
					}
					if(Config.Instance.OverlayPlayerScaling > 100)
					{
						Config.Instance.OverlayPlayerScaling = 100;
						converted = true;
					}
				}


				if(configVersion <= new Version(0, 5, 1, 0))
				{
#pragma warning disable 612
					Config.Instance.SaveConfigInAppData = Config.Instance.SaveInAppData;
					Config.Instance.SaveDataInAppData = Config.Instance.SaveInAppData;
					converted = true;
#pragma warning restore 612
				}
				if(configVersion <= new Version(0, 6, 6, 0))
				{
					if(Config.Instance.ExportClearX == 0.86)
					{
						Config.Instance.Reset(nameof(Config.ExportClearX));
						converted = true;
					}
					if(Config.Instance.ExportClearY == 0.16)
					{
						Config.Instance.Reset(nameof(Config.ExportClearY));
						converted = true;
					}
					if(Config.Instance.ExportClearCheckYFixed == 0.2)
					{
						Config.Instance.Reset(nameof(Config.ExportClearCheckYFixed));
						converted = true;
					}
				}
				if(configVersion <= new Version(0, 7, 6, 0))
				{
					if(Config.Instance.ExportCard1X != 0.04)
					{
						Config.Instance.Reset(nameof(Config.ExportCard1X));
						converted = true;
					}
					if(Config.Instance.ExportCard2X != 0.2)
					{
						Config.Instance.Reset(nameof(Config.ExportCard2X));
						converted = true;
					}
					if(Config.Instance.ExportCardsY != 0.168)
					{
						Config.Instance.Reset(nameof(Config.ExportCardsY));
						converted = true;
					}
				}
				if(configVersion <= new Version(0, 9, 6, 0))
				{
					if(!Config.Instance.PanelOrderPlayer.Contains("Fatigue Counter"))
					{
						Config.Instance.Reset(nameof(Config.PanelOrderPlayer));
						converted = true;
					}
					if(!Config.Instance.PanelOrderOpponent.Contains("Fatigue Counter"))
					{
						Config.Instance.Reset(nameof(Config.PanelOrderOpponent));
						converted = true;
					}
				}
				if(configVersion <= new Version(0, 10, 10, 0)) //button moved up with new expansion added to the list
				{
					Config.Instance.Reset(nameof(Config.ExportAllSetsButtonY));
					converted = true;
				}
				if(configVersion <= new Version(0, 11, 1, 0))
				{
					if(Config.Instance.GoldProgressLastReset.Length < 5)
					{
						Config.Instance.GoldProgressLastReset = new[]
						{
							DateTime.MinValue,
							DateTime.MinValue,
							DateTime.MinValue,
							DateTime.MinValue,
							DateTime.MinValue
						};
						converted = true;
					}
					if(Config.Instance.GoldProgress.Length < 5)
					{
						Config.Instance.Reset(nameof(Config.GoldProgress));
						converted = true;
					}
					if(Config.Instance.GoldProgressTotal.Length < 5)
					{
						Config.Instance.Reset(nameof(Config.GoldProgressTotal));
						converted = true;
					}
				}
				if(configVersion <= new Version(0, 13, 1, 0)) //button moved up with new expansion added to the list
				{
					Config.Instance.Reset(nameof(Config.ExportAllSetsButtonY));
					converted = true;
				}
			}

			if(converted)
			{
				Log.Info("changed config values");
				Config.Save();
			}

			if(configVersion != null && currentVersion > configVersion)
				UpdatedVersion = currentVersion;
		}
	}
}
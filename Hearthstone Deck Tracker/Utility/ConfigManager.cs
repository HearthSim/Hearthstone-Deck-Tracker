#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class ConfigManager
	{
		public static Version UpdatedVersion { get; private set; }
		public static Version PreviousVersion { get; private set; }

		public static void Run()
		{
			PreviousVersion = string.IsNullOrEmpty(Config.Instance.CreatedByVersion) ? null : new Version(Config.Instance.CreatedByVersion);
			var currentVersion = Helper.GetCurrentVersion();
			if(currentVersion != null)
			{
				// Assign current version to the config instance so that it will be saved when the config
				// is rewritten to disk, thereby telling us what version of the application created it
				Config.Instance.CreatedByVersion = currentVersion.ToString();
			}
			ConvertLegacyConfig(currentVersion, PreviousVersion);

			if(Config.Instance.SelectedTags.Count == 0)
				Config.Instance.SelectedTags.Add("All");

#if(!SQUIRREL)
			if(!Directory.Exists(Config.Instance.DataDir))
				Config.Instance.Reset(nameof(Config.DataDirPath));
#endif
		}

		// Logic for dealing with legacy config file semantics
		// Use difference of versions to determine what should be done
		private static void ConvertLegacyConfig(Version currentVersion, Version configVersion)
		{
			var converted = false;

			var v0_3_21 = new Version(0, 3, 21, 0);

			if(configVersion == null) // Config was created prior to version tracking being introduced (v0.3.20)
			{
				Config.Instance.CreatedByVersion = currentVersion.ToString();
				converted = true;
			}
			else if(currentVersion > configVersion)
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


#if(!SQUIRREL)
				if(configVersion <= new Version(0, 5, 1, 0))
				{
					Config.Instance.SaveConfigInAppData = Config.Instance.SaveInAppData;
					Config.Instance.SaveDataInAppData = Config.Instance.SaveInAppData;
					converted = true;
				}
#endif
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
				if(configVersion <= new Version(0, 13, 16, 0))
				{
					MetroTheme theme;
					if(Enum.TryParse(Config.Instance.ThemeName, out theme))
					{
						Config.Instance.AppTheme = theme;
						converted = true;
					}
				}
				if(configVersion <= new Version(0, 13, 17, 0))
				{
					if(Math.Abs(Config.Instance.OpponentDeckHeight - 65) < 1 && Math.Abs(Config.Instance.OpponentDeckTop - 17) < 1)
					{
						Config.Instance.Reset(nameof(Config.OpponentDeckHeight));
						Config.Instance.Reset(nameof(Config.OpponentDeckTop));
						converted = true;
					}
					if(Math.Abs(Config.Instance.PlayerDeckHeight - 65) < 1 && Math.Abs(Config.Instance.PlayerDeckTop - 17) < 1)
					{
						Config.Instance.Reset(nameof(Config.PlayerDeckHeight));
						Config.Instance.Reset(nameof(Config.PlayerDeckTop));
						converted = true;
					}
				}
				if(configVersion <= new Version(0, 14, 7, 0))
				{
					if(File.Exists("Version.xml"))
					{
						try
						{
							File.Delete("Version.xml");
						}
						catch(Exception e)
						{
							Log.Error(e);
						}
					}
				}
				if(configVersion <= new Version(0, 14, 9, 0))
				{
					Config.Instance.ConstructedAutoImportNew = false;
					Config.Instance.ConstructedAutoUpdate = false;
					converted = true;
				}
				if(configVersion <= new Version(0, 15, 13, 0))
				{
					var targetDir = PluginManager.PluginDirectory;
					var sourceDir = PluginManager.LocalPluginDirectory;
					if(sourceDir.Exists)
					{
						if(targetDir.Exists)
						{
							try
							{
								targetDir.Delete(true);
							}
							catch(Exception ex)
							{
								Log.Error(ex);
							}
						}
						try
						{
							targetDir.Create();
							Helper.CopyFolder(sourceDir.FullName, targetDir.FullName);
						}
						catch(Exception ex)
						{
							Log.Error(ex);
						}
					}

					var bars = new[] { "classic", "dark", "frost", "minimal" };
					var overlays = new[] { "classic", "default", "frost" };
					try
					{
						foreach(var folder in new DirectoryInfo("Images\\Themes\\Bars").GetDirectories().Where(x => !bars.Contains(x.Name)))
						{
							try
							{
								Helper.CopyFolder(folder.FullName, Path.Combine(Config.AppDataPath, "Themes\\Bars", folder.Name));
								folder.Delete(true);
							}
							catch(Exception ex)
							{
								Log.Error(ex);
							}
						}

						foreach(var folder in new DirectoryInfo("Images\\Themes\\Overlay").GetDirectories().Where(x => !overlays.Contains(x.Name)))
						{
							try
							{
								Helper.CopyFolder(folder.FullName, Path.Combine(Config.AppDataPath, "Themes\\Overlay", folder.Name));
								folder.Delete(true);
							}
							catch(Exception ex)
							{
								Log.Error(ex);
							}
						}
					}
					catch(Exception ex)
					{
						Log.Error(ex);
					}
				}
				if(configVersion == new Version(0, 15, 9, 0))
					DataIssueResolver.RunDeckStatsFix = true;
			}

			if(converted)
			{
				Log.Info("changed config values");
				Config.Save();
			}

			if(configVersion != null && new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build) > new Version(configVersion.Major, configVersion.Minor, configVersion.Build))
				UpdatedVersion = currentVersion;
		}
	}
}
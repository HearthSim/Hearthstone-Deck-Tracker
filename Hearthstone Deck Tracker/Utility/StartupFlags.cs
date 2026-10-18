using System;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility;

internal static class StartupFlags
{
	public const string Autostart = "--autostart";

	public static bool IsAutostart { get; } = Environment.GetCommandLineArgs().Contains(Autostart);

	public const string BackgroundUpdate = "--background-update";

	public static bool IsBackgroundUpdate { get; } = Environment.GetCommandLineArgs().Contains(BackgroundUpdate);
}

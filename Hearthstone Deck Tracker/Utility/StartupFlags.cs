using System;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility;

internal static class StartupFlags
{
	public const string Autostart = "--autostart";

	public static bool IsAutostart { get; } = Environment.GetCommandLineArgs().Contains(Autostart);

	public const string BackgroundUpdate = "--background-update";

	public static bool IsBackgroundUpdate { get; } = Environment.GetCommandLineArgs().Contains(BackgroundUpdate);

	public const string Restart = "--restart";

	public static bool IsRestart { get; } = Environment.GetCommandLineArgs().Contains(Restart);

	// pass these on when a startup update restarts the app, so the new process continues this launch
	public static string[] Active { get; } = new[] { Autostart, BackgroundUpdate, Restart }.Where(Environment.GetCommandLineArgs().Contains).ToArray();

	public static string StartReason
	{
		get
		{
			if(IsBackgroundUpdate)
				return "background_update";
			if(IsRestart)
				return "restart";
			return IsAutostart ? "autostart" : "manual";
		}
	}
}

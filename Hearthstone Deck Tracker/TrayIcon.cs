#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Updating;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class TrayIcon
	{
		public NotifyIcon NotifyIcon { get; }

		public MenuItem? MenuItemUpdate { get; }

		private readonly Icon? _defaultIcon;

		public MenuItem MenuItemStartHearthstone { get; }

		public MenuItem MenuItemSettings { get; }

		public MenuItem MenuItemUseNoDeck { get; }

		public MenuItem MenuItemQuit { get; }

#if(DEV)
		private const string Name = "Hearthstone Deck Tracker [DEV]";
#else
		private const string Name = "Hearthstone Deck Tracker";
#endif

		public TrayIcon()
		{
#if(DEV)
			const bool includeRevision = true;
#else
			const bool includeRevision = false;
#endif
			NotifyIcon = new NotifyIcon
			{
				Visible = true,
				ContextMenu = new ContextMenu(),
				Text = Name
			};

			var iconFile = new FileInfo("Images/HearthstoneDeckTracker16.ico");
			if(iconFile.Exists)
				NotifyIcon.Icon = _defaultIcon = new Icon(iconFile.FullName, SystemInformation.SmallIconSize);
			else
				Log.Error($"Cant find tray icon at \"{iconFile.FullName}\"");

			NotifyIcon.ContextMenu.MenuItems.Add(new MenuItem($"{Name} v{Helper.GetCurrentVersion().ToVersionString(includeRevision)}") { Enabled = false });
			NotifyIcon.ContextMenu.MenuItems.Add("-");

			MenuItemSettings = new MenuItem(LocUtil.Get("TrayIcon_MenuItemSettings"), (sender, args) => GlobalCommands.ShowSettings.Execute(null));
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemSettings);

#if(INSTALLED)
			MenuItemUpdate = new MenuItem("", (sender, args) => OnUpdateClicked());
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemUpdate);
			Updater.Status.Changed += _ => UpdateUpdaterState();
			UpdateUpdaterState();
#endif

			NotifyIcon.ContextMenu.MenuItems.Add("-");

			MenuItemStartHearthstone = new MenuItem(LocUtil.Get("TrayIcon_MenuItemStartHearthstone"), (sender, args) => HearthstoneRunner.StartHearthstone().Forget());
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemStartHearthstone);
			HearthstoneRunner.StartingHearthstone += starting => MenuItemStartHearthstone.Enabled = !starting;

			MenuItemUseNoDeck = new MenuItem(LocUtil.Get("TrayIcon_MenuItemUseNoDeck"), (sender, args) => UseNoDeckContextMenu()) { Visible = false };
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemUseNoDeck);
			Core.GameIsRunningChanged += running => MenuItemUseNoDeck.Visible = running;

			NotifyIcon.ContextMenu.MenuItems.Add("-");

			MenuItemQuit = new MenuItem(LocUtil.Get("TrayIcon_MenuItemQuit"), (sender, args) =>
			{
				_ = Core.Shutdown();
			});
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemQuit);

			NotifyIcon.MouseClick += (sender, args) =>
			{
			// TODO: Find a better way to interact with the MainWindow
				if(args.Button == MouseButtons.Left)
					Core.MainWindow.ActivateWindow();
			};

			// TODO: Find a better way to interact with the MainWindow
			NotifyIcon.BalloonTipClicked += (sender1, e) => { Core.MainWindow.ActivateWindow(); };

			DeckList.Instance.ActiveDeckChanged += deck =>
			{
				MenuItemUseNoDeck.Checked = deck == null;
			};
		}

#if(INSTALLED)
		private Icon? _updateAvailableIcon;

		private static bool IsUpdateAvailable => Updater.Status.UpdaterState == UpdaterState.Available;

		private static void OnUpdateClicked()
		{
			if(IsUpdateAvailable)
				Updater.RestartApp();
			else
				Updater.CheckForUpdates(true);
		}

		public void UpdateUpdaterState()
		{
			if(MenuItemUpdate == null)
				return;
			var state = Updater.Status.UpdaterState;
			MenuItemUpdate.Text = LocUtil.Get(IsUpdateAvailable ? "TrayIcon_MenuItemInstallUpdate" : "TrayIcon_MenuItemCheckForUpdates");
			MenuItemUpdate.Enabled = state is not (UpdaterState.Checking or UpdaterState.Downloading or UpdaterState.Installing);
			NotifyIcon.Text = IsUpdateAvailable ? $"{Name} - {LocUtil.Get("TrayIcon_UpdateAvailable")}" : Name;

			if(_defaultIcon == null)
				return;
			if(IsUpdateAvailable)
				NotifyIcon.Icon = _updateAvailableIcon ??= CreateUpdateAvailableIcon(_defaultIcon);
			else
				NotifyIcon.Icon = _defaultIcon;
		}

		private static Icon CreateUpdateAvailableIcon(Icon baseIcon)
		{
			var size = baseIcon.Size;
			using var bitmap = new Bitmap(size.Width, size.Height);
			using(var g = Graphics.FromImage(bitmap))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.DrawIcon(baseIcon, new Rectangle(Point.Empty, size));

				var diameter = size.Width * 0.4375f;
				var inset = size.Width / 32f;
				using var fill = new SolidBrush(Color.FromArgb(0x11, 0x9E, 0xDA));
				g.FillEllipse(fill, size.Width - inset - diameter, size.Height - inset - diameter, diameter, diameter);
			}
			// the icon keeps its HICON for the lifetime of the app, so it's never destroyed
			return Icon.FromHandle(bitmap.GetHicon());
		}
#endif

		private void UseNoDeckContextMenu()
		{
			if(MenuItemUseNoDeck.Checked)
				DeckList.Instance.ActiveDeck = DeckList.Instance.GetLastUsedDeck();
			else
				DeckList.Instance.ActiveDeck = null;
		}

		public void ShowMessage(string text, string title = "Hearthstone Deck Tracker", int duration = 5, ToolTipIcon icon = ToolTipIcon.Info)
			=> NotifyIcon.ShowBalloonTip(duration, title, text, icon);
	}
}

#region

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class TrayIcon
	{
		public NotifyIcon NotifyIcon { get; }

		public MenuItem MenuItemShow { get; }

		public MenuItem MenuItemStartHearthstone { get; }

		public MenuItem MenuItemSettings { get; }

		public MenuItem MenuItemUseNoDeck { get; }

		public MenuItem MenuItemQuit { get; }

		public TrayIcon()
		{
			NotifyIcon = new NotifyIcon
			{
				Visible = true,
				ContextMenu = new ContextMenu(),
				Text = "Hearthstone Deck Tracker"
			};

			var iconFile = new FileInfo("Images/HearthstoneDeckTracker16.ico");
			if(iconFile.Exists)
				NotifyIcon.Icon = new Icon(iconFile.FullName);
			else
				Log.Error($"Cant find tray icon at \"{iconFile.FullName}\"");

			// TODO: Find a better way to interact with the MainWindow
			MenuItemShow = new MenuItem(LocUtil.Get("TrayIcon_MenuItemShow"), (sender, args) => Core.MainWindow.ActivateWindow());
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemShow);

			MenuItemStartHearthstone = new MenuItem(LocUtil.Get("TrayIcon_MenuItemStartHearthstone"), (sender, args) => HearthstoneRunner.StartHearthstone().Forget());
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemStartHearthstone);
			HearthstoneRunner.StartingHearthstone += starting => MenuItemStartHearthstone.Enabled = !starting;

			MenuItemSettings = new MenuItem(LocUtil.Get("TrayIcon_MenuItemSettings"), (sender, args) => GlobalCommands.ShowSettings.Execute(null));
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemSettings);

			NotifyIcon.ContextMenu.MenuItems.Add("-");

			MenuItemUseNoDeck = new MenuItem(LocUtil.Get("TrayIcon_MenuItemUseNoDeck"), (sender, args) => UseNoDeckContextMenu());
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemUseNoDeck);

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

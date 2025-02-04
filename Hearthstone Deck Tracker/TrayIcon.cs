#region

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class TrayIcon
	{
		public NotifyIcon NotifyIcon { get; }

		public MenuItem MenuItemExit { get; }

		public MenuItem MenuItemShow { get; }

		public MenuItem MenuItemAutoSelect { get; }

		public MenuItem MenuItemUseNoDeck { get; }

		public MenuItem MenuItemStartHearthstone { get; }

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

			MenuItemStartHearthstone = new MenuItem(LocUtil.Get("TrayIcon_MenuItemStartHearthstone"), (sender, args) => HearthstoneRunner.StartHearthstone().Forget());
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemStartHearthstone);
			HearthstoneRunner.StartingHearthstone += starting => MenuItemStartHearthstone.Enabled = !starting;

			MenuItemUseNoDeck = new MenuItem(LocUtil.Get("TrayIcon_MenuItemUseNoDeck"), (sender, args) => UseNoDeckContextMenu());
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemUseNoDeck);

			MenuItemAutoSelect = new MenuItem(LocUtil.Get("TrayIcon_MenuItemAutoSelect"), (sender, args) => AutoDeckDetectionContextMenu());
			MenuItemAutoSelect.Checked = ConfigWrapper.Bindable.AutoDeckDetection;
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemAutoSelect);

			MenuItemShow = new MenuItem(LocUtil.Get("TrayIcon_MenuItemShow"), (sender, args) => Core.MainWindow.ActivateWindow());
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemShow);

			MenuItemExit = new MenuItem(LocUtil.Get("TrayIcon_MenuItemExit"), (sender, args) =>
			{
				_ = Core.Shutdown();
			});
			NotifyIcon.ContextMenu.MenuItems.Add(MenuItemExit);

			NotifyIcon.MouseClick += (sender, args) =>
			{
				if(args.Button == MouseButtons.Left)
					Core.MainWindow.ActivateWindow();
			};

			NotifyIcon.BalloonTipClicked += (sender1, e) => { Core.MainWindow.ActivateWindow(); };

			DeckList.Instance.ActiveDeckChanged += deck =>
			{
				MenuItemUseNoDeck.Checked = deck == null;
			};

			ConfigWrapper.Bindable.PropertyChanged += (_, args) =>
			{
				if(args.PropertyName == nameof(ConfigWrapper.Bindable.AutoDeckDetection))
					MenuItemAutoSelect.Checked = ConfigWrapper.Bindable.AutoDeckDetection;
			};
		}

		private void AutoDeckDetectionContextMenu()
		{
			ConfigWrapper.Bindable.AutoDeckDetection = !ConfigWrapper.Bindable.AutoDeckDetection;
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

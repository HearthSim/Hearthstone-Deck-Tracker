using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hearthstone_Deck_Tracker
{
    public class TrayIcon
    {
        private NotifyIcon _notifyIcon;

        public NotifyIcon NotifyIcon
        {
            get
            {
                if(_notifyIcon == null) Initialize();
                return _notifyIcon;
            }
        }

        public void Initialize()
        {
            _notifyIcon = new NotifyIcon {
                Icon = new Icon(@"Images/HearthstoneDeckTracker16.ico"),
                Visible = true,
                ContextMenu = new ContextMenu(),
                Text =
                    "Hearthstone Deck Tracker v" + (Helper.GetCurrentVersion() ?? new Version("0.0")).ToVersionString()
            };

            var startHearthstonMenuItem = new MenuItem("Start Launcher/Hearthstone",
                (sender, args) => Helper.StartHearthstoneAsync());
            startHearthstonMenuItem.Name = "startHearthstone";
            _notifyIcon.ContextMenu.MenuItems.Add(startHearthstonMenuItem);

            var useNoDeckMenuItem = new MenuItem("Use no deck", (sender, args) => UseNoDeckContextMenu());
            useNoDeckMenuItem.Name = "useNoDeck";
            _notifyIcon.ContextMenu.MenuItems.Add(useNoDeckMenuItem);

            var autoSelectDeckMenuItem = new MenuItem("Autoselect deck",
                (sender, args) => AutoDeckDetectionContextMenu());
            autoSelectDeckMenuItem.Name = "autoSelectDeck";
            _notifyIcon.ContextMenu.MenuItems.Add(autoSelectDeckMenuItem);

            var classCardsFirstMenuItem = new MenuItem("Class cards first",
                (sender, args) => SortClassCardsFirstContextMenu());
            classCardsFirstMenuItem.Name = "classCardsFirst";
            _notifyIcon.ContextMenu.MenuItems.Add(classCardsFirstMenuItem);

            _notifyIcon.ContextMenu.MenuItems.Add("Show", (sender, args) => Core.MainWindow.ActivateWindow());
            _notifyIcon.ContextMenu.MenuItems.Add("Exit", (sender, args) => Core.MainWindow.Close());
            _notifyIcon.MouseClick += (sender, args) =>
            {
                if(args.Button == MouseButtons.Left)
                    Core.MainWindow.ActivateWindow();
            };
        }


        private void AutoDeckDetectionContextMenu()
        {
            var enable = (bool)GetContextMenuProperty("autoSelectDeck", "Checked");
            Core.MainWindow.AutoDeckDetection(!enable);
        }

        private void UseNoDeckContextMenu()
        {
            var enable = (bool)GetContextMenuProperty("useNoDeck", "Checked");
            if(enable)
                Core.MainWindow.SelectLastUsedDeck();
            else
                Core.MainWindow.SelectDeck(null, true);
        }

        private int IndexOfKeyContextMenuItem(string key)
        {
            return NotifyIcon.ContextMenu.MenuItems.IndexOfKey(key);
        }

        public void SetContextMenuProperty(string key, string property, object value)
        {
            var menuItemInd = IndexOfKeyContextMenuItem(key);
            object target = NotifyIcon.ContextMenu.MenuItems[menuItemInd];
            target.GetType().GetProperty(property).SetValue(target, value);
        }

        private object GetContextMenuProperty(string key, string property)
        {
            var menuItemInd = IndexOfKeyContextMenuItem(key);
            object target = NotifyIcon.ContextMenu.MenuItems[menuItemInd];
            return target.GetType().GetProperty(property).GetValue(target, null);
        }

        private void SortClassCardsFirstContextMenu()
        {
            var enable = (bool)GetContextMenuProperty("classCardsFirst", "Checked");
            Core.MainWindow.SortClassCardsFirst(!enable);
        }
    }
}
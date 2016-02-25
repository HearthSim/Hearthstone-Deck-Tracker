#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options
{
	/// <summary>
	/// Interaction logic for OptionsSearch.xaml
	/// </summary>
	public partial class OptionsSearch : UserControl
	{
		private List<CheckBoxWrapper> _checkBoxWrappers;

		public OptionsSearch()
		{
			InitializeComponent();
		}

		private List<CheckBoxWrapper> CheckBoxWrappers => _checkBoxWrappers ?? (_checkBoxWrappers = LoadWrappers());

		private void ButtonSearch_OnClick(object sender, RoutedEventArgs e) => UpdateSearchResult(TextBoxSearch.Text);

		private List<CheckBoxWrapper> LoadWrappers()
		{
			var options = new[]
			{
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayDeckWindows, nameof(Core.MainWindow.Options.OptionsOverlayDeckWindows)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayGeneral, nameof(Core.MainWindow.Options.OptionsOverlayGeneral)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayInteractivity, nameof(Core.MainWindow.Options.OptionsOverlayInteractivity)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayOpponent, nameof(Core.MainWindow.Options.OptionsOverlayOpponent)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayPlayer, nameof(Core.MainWindow.Options.OptionsOverlayPlayer)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerAppearance, nameof(Core.MainWindow.Options.OptionsTrackerAppearance)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerBackups, nameof(Core.MainWindow.Options.OptionsTrackerBackups)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerExporting, nameof(Core.MainWindow.Options.OptionsTrackerExporting)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerGeneral, nameof(Core.MainWindow.Options.OptionsTrackerGeneral)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerHotKeys, nameof(Core.MainWindow.Options.OptionsTrackerHotKeys)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerImporting, nameof(Core.MainWindow.Options.OptionsTrackerImporting)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerNotifications, nameof(Core.MainWindow.Options.OptionsTrackerNotifications)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerPlugins, nameof(Core.MainWindow.Options.OptionsTrackerPlugins)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerSettings, nameof(Core.MainWindow.Options.OptionsTrackerSettings)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerStats, nameof(Core.MainWindow.Options.OptionsTrackerStats))
			};
			var checkBoxWrappers = new List<CheckBoxWrapper>();
			foreach(var option in options)
			{
				var checkBoxes = Helper.FindLogicalChildren<CheckBox>(option.UserControl);
				checkBoxWrappers.AddRange(checkBoxes.Select(cb => new CheckBoxWrapper {MenuItem = option, CheckBox = cb}));
			}
			return checkBoxWrappers;
		}

		private void UpdateSearchResult(string text)
		{
			ListBoxSearchResult.Items.Clear();
			if(string.IsNullOrEmpty(text))
				return;
			foreach(var wrapper in CheckBoxWrappers.Where(x => x.CheckBox.Content.ToString().ToUpperInvariant().Contains(text.ToUpperInvariant())
															|| (x.CheckBox.Name.ToUpperInvariant().Replace("CHECKBOX", "").Contains(text.ToUpperInvariant()))))
				ListBoxSearchResult.Items.Add(wrapper);
		}

		private void ListBoxSearchResult_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var selected = (sender as ListBox)?.SelectedItem as CheckBoxWrapper;
			if(selected == null)
				return;
			var tvis = Helper.FindLogicalChildren<TreeViewItem>(Core.MainWindow.Options.TreeViewOptions);
			var target = tvis.FirstOrDefault(x => x.Name.Contains(selected.MenuItem.Name.Substring(7)));
			if(target != null)
			{
				if(selected.CheckBox.Visibility == Visibility.Collapsed)
					AdvancedOptions.Instance.Show = true;
				target.IsSelected = true;
			}
		}

		public class CheckBoxWrapper
		{
			public CheckBox CheckBox { get; set; }
			public UserControlWrapper MenuItem { get; set; }
			public override string ToString() => $"{(CheckBox.Visibility == Visibility.Collapsed ? "[Adv.] " : "")}{MenuItem.Name.Substring(7).Insert(7, " > ")}: {CheckBox.Content}";
		}

		public class UserControlWrapper
		{
			public UserControlWrapper(UserControl userControl, string name)
			{
				UserControl = userControl;
				Name = name;
			}

			public UserControl UserControl { get; set; }
			public string Name { get; set; }
		}
	}
}
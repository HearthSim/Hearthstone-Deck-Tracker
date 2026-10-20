#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker.Windows;
using WPFLocalizeExtension.Engine;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options
{
	/// <summary>
	/// Interaction logic for OptionsSearch.xaml
	/// </summary>
	public partial class OptionsSearch : UserControl, IOptionsPageWithOwnScrolling
	{
		private List<IOptionWrapper>? _optionWrappers;

		public OptionsSearch()
		{
			InitializeComponent();
			LocalizeDictionary.Instance.PropertyChanged += LocalizeDictionary_OnPropertyChanged;
		}

		private void LocalizeDictionary_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName != nameof(LocalizeDictionary.Culture))
				return;
			// wait for the localized headers and checkbox texts to update first
			Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => UpdateSearchResult(TextBoxSearch.Text)));
		}

		private void TextBoxSearchLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			((TextBox) sender).Focus();
		}

		private List<IOptionWrapper> OptionWrappers => _optionWrappers ??= LoadWrappers();

		public void Reset()
		{
			TextBoxSearch.Text = string.Empty;
			ListBoxSearchResult.Items.Clear();
		}

		private void ButtonSearch_OnClick(object sender, RoutedEventArgs e) => UpdateSearchResult(TextBoxSearch.Text);

		private List<IOptionWrapper> LoadWrappers()
		{
			var optionWrappers = new List<IOptionWrapper>();
			if(this.ParentMainWindow() is not { } window)
				return optionWrappers;
			var optionsMenuItems = window.Options.Pages.Select(x => new UserControlWrapper(x.Page, x.TreeViewItem));
			foreach(var optionsMenuItem in optionsMenuItems)
			{
				var option = Helper.FindLogicalDescendants<DependencyObject>(IsSearchableOption, optionsMenuItem.UserControl);
				optionWrappers.AddRange(option.Select(cb => WrapOption(optionsMenuItem, cb)));
			}
			return optionWrappers;
		}

		private bool IsSearchableOption(DependencyObject depObj)
		{
			var basicTypes = new List<Type>{typeof(CheckBox)};
			if(basicTypes.Contains(depObj.GetType()))
				return true;

			if(depObj is DockPanel dockPanel)
			{
				var elements = dockPanel.Children.Cast<UIElement>();
				return Enumerable.Any<UIElement>(elements, child => basicTypes.Contains(child.GetType()));
			}
			return false;
		}

		private IOptionWrapper WrapOption(UserControlWrapper menuItem, DependencyObject depObj)
		{
			if(depObj is DockPanel dockPanel)
			{
				foreach(var child in dockPanel.Children)
				{
					if(child is CheckBox)
						return WrapSimpleOption(menuItem, child as CheckBox);
				}
			}
			return WrapSimpleOption(menuItem, depObj as ContentControl);
		}

		private IOptionWrapper WrapSimpleOption(UserControlWrapper menuItem, ContentControl? control)
		{
			if(control == null)
				throw new ArgumentNullException(nameof(control));
			if(control is CheckBox checkBox)
				return new CheckBoxWrapper(menuItem, checkBox);
			throw new ArgumentException("Argument must be a wrappable option type.", nameof(control));
		}

		private void UpdateSearchResult(string text)
		{
			ListBoxSearchResult.Items.Clear();
			if(string.IsNullOrEmpty(text))
				return;
			foreach(var wrapper in OptionWrappers.Where(x => x.Matches(text)))
				ListBoxSearchResult.Items.Add(wrapper);
		}

		private void ListBoxSearchResult_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if((sender as ListBox)?.SelectedItem is not IOptionWrapper selected)
				return;
			if(selected.IsAdvanced)
				AdvancedOptions.Instance.Show = true;
			selected.MenuItem.TreeViewItem.IsSelected = true;
		}

		public interface IOptionWrapper
		{
			UserControlWrapper MenuItem { get; }
			bool IsAdvanced { get; }

			// Returns true if the option matches the search query.
			bool Matches(string query);

			// Returns the user-friendly string describing the option.
			string ToString();
		}

		public abstract class OptionWrapper<T> : IOptionWrapper where T : Control
		{
			protected OptionWrapper(UserControlWrapper menuItem, T control)
			{
				MenuItem = menuItem;
				Control = control;
				IsAdvanced = IsBoundToAdvancedOptions(menuItem.TreeViewItem) || IsWithinAdvancedOption(control, menuItem.UserControl);
			}

			public UserControlWrapper MenuItem { get; }
			public bool IsAdvanced { get; }
			public T Control { get; }

			private static bool IsWithinAdvancedOption(DependencyObject? element, UserControl page)
			{
				for(; element != null && element != page; element = LogicalTreeHelper.GetParent(element))
				{
					if(IsBoundToAdvancedOptions(element))
						return true;
				}
				return false;
			}

			private static bool IsBoundToAdvancedOptions(DependencyObject element) =>
				BindingOperations.GetBinding(element, UIElement.VisibilityProperty)?.Source == AdvancedOptions.Instance;

			public abstract bool Matches(string query);
			public override abstract string ToString();
		}

		public class CheckBoxWrapper : OptionWrapper<CheckBox>
		{
			public CheckBoxWrapper(UserControlWrapper menuItem, CheckBox checkBox) : base(menuItem, checkBox)
			{
			}

			private string Text => GetText(Control.Content);

			public override bool Matches(string query)
			{
				return Text.ToUpperInvariant().Contains(query.ToUpperInvariant())
					|| Control.Name.ToUpperInvariant().Replace("CHECKBOX", "").Contains(query.ToUpperInvariant());
			}

			public override string ToString() => $"{(IsAdvanced ? "[Adv.] " : "")}{MenuItem.DisplayName}: {Text}";

			private static string GetText(object? content) => content switch
			{
				string text => text,
				TextBlock textBlock => textBlock.Text,
				ContentControl contentControl => GetText(contentControl.Content),
				Panel panel => string.Join(" ", panel.Children.Cast<object>().Select(GetText).Where(x => x.Length > 0)),
				_ => "",
			};
		}

		public class UserControlWrapper
		{
			public UserControlWrapper(UserControl userControl, TreeViewItem treeViewItem)
			{
				UserControl = userControl;
				TreeViewItem = treeViewItem;
			}

			public UserControl UserControl { get; }
			public TreeViewItem TreeViewItem { get; }

			public string DisplayName => TreeViewItem.Parent is TreeViewItem section
				? $"{section.Header} > {TreeViewItem.Header}"
				: $"{TreeViewItem.Header}";
		}

		private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (TextBoxSearch.Text.Length < MinQueryLength(TextBoxSearch.Text))
				return;
			UpdateSearchResult(TextBoxSearch.Text);
		}

		// a single Chinese, Japanese or Korean character is already a meaningful term
		private static readonly Regex CjkCharacter = new(@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographsExtensionA}\p{IsCJKUnifiedIdeographs}\p{IsHangulSyllables}]");

		private static int MinQueryLength(string query) => CjkCharacter.IsMatch(query) ? 1 : 3;
	}
}

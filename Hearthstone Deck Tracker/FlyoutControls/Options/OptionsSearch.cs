#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options
{
	public class OptionsSearch
	{
		private readonly (TreeViewItem TreeViewItem, UserControl Page)[] _pages;
		private List<IOptionWrapper>? _optionWrappers;

		public OptionsSearch((TreeViewItem TreeViewItem, UserControl Page)[] pages)
		{
			_pages = pages;
		}

		private List<IOptionWrapper> OptionWrappers => _optionWrappers ??= LoadWrappers();

		// a single Chinese, Japanese or Korean character is already a meaningful term
		private static readonly Regex CjkCharacter = new(@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographsExtensionA}\p{IsCJKUnifiedIdeographs}\p{IsHangulSyllables}]");

		public static bool IsValidQuery(string query) => query.Length >= (CjkCharacter.IsMatch(query) ? 1 : 3);

		// pages whose header matches come first, then pages in a matching category, then pages with the most matching options
		public (List<TreeViewItem> Pages, bool HasAdvanced) FindPages(string query)
		{
			var matchingOptions = OptionWrappers.Where(x => x.Matches(query)).ToList();
			var optionMatches = matchingOptions
				.GroupBy(x => x.MenuItem.TreeViewItem)
				.ToDictionary(x => x.Key, x => x.Count());
			var pages = _pages
				.Select(x => x.TreeViewItem)
				.Select(x => (
					TreeViewItem: x,
					HeaderMatches: HeaderContains(x, query),
					CategoryMatches: x.Parent is TreeViewItem category && HeaderContains(category, query),
					OptionMatches: optionMatches.TryGetValue(x, out var count) ? count : 0
				))
				.Where(x => x.HeaderMatches || x.CategoryMatches || x.OptionMatches > 0)
				.OrderByDescending(x => x.HeaderMatches)
				.ThenByDescending(x => x.CategoryMatches)
				.ThenByDescending(x => x.OptionMatches)
				.Select(x => x.TreeViewItem)
				.ToList();
			return (pages, matchingOptions.Any(x => x.IsAdvanced) || pages.Any(IsBoundToAdvancedOptions));
		}

		private static bool Contains(string text, string query) => text.ToUpperInvariant().Contains(query.ToUpperInvariant());

		private static bool HeaderContains(TreeViewItem item, string query) => Contains(item.Header?.ToString() ?? "", query);

		private static bool IsBoundToAdvancedOptions(DependencyObject element) =>
			BindingOperations.GetBinding(element, UIElement.VisibilityProperty)?.Source == AdvancedOptions.Instance;

		private List<IOptionWrapper> LoadWrappers()
		{
			var optionWrappers = new List<IOptionWrapper>();
			var optionsMenuItems = _pages.Select(x => new UserControlWrapper(x.Page, x.TreeViewItem));
			foreach(var optionsMenuItem in optionsMenuItems)
			{
				var option = Helper.FindLogicalDescendants<DependencyObject>(IsSearchableOption, optionsMenuItem.UserControl);
				optionWrappers.AddRange(option.Select(cb => WrapOption(optionsMenuItem, cb)));
			}
			return optionWrappers;
		}

		private static bool IsSearchableOption(DependencyObject depObj) => depObj is CheckBox or Label;

		private static IOptionWrapper WrapOption(UserControlWrapper menuItem, DependencyObject depObj) =>
			new ContentControlWrapper(menuItem, (ContentControl)depObj);

		public interface IOptionWrapper
		{
			UserControlWrapper MenuItem { get; }
			bool IsAdvanced { get; }

			// Returns true if the option matches the search query.
			bool Matches(string query);
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

			public abstract bool Matches(string query);
		}

		public class ContentControlWrapper : OptionWrapper<ContentControl>
		{
			public ContentControlWrapper(UserControlWrapper menuItem, ContentControl control) : base(menuItem, control)
			{
			}

			private string Text => GetText(Control.Content);

			public override bool Matches(string query)
			{
				return Contains(Text, query) || Contains(Control.Name.ToUpperInvariant().Replace("CHECKBOX", ""), query);
			}

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
		}
	}
}

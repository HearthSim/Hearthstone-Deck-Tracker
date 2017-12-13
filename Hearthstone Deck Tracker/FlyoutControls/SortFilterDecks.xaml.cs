#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for TagControl.xaml
	/// </summary>
	public partial class SortFilterDecks
	{
		public readonly ObservableCollection<Tag> Tags = new ObservableCollection<Tag>();
		private bool _initialized;

		public SortFilterDecks()
		{
			InitializeComponent();
			ListboxTags.ItemsSource = Tags;
		}

		private void NewTag(string tag)
		{
			if(DeckList.Instance.AllTags.Contains(tag))
				return;
			DeckList.Instance.AllTags.Add(tag);
			DeckList.Save();
			Core.MainWindow.ReloadTags();
		}

		private void DeleteTag(string tag)
		{
			if(!DeckList.Instance.AllTags.Contains(tag))
				return;
			DeckList.Instance.AllTags.Remove(tag);

			foreach(var deck in DeckList.Instance.Decks.Where(deck => deck.Tags.Contains(tag)))
				deck.Tags.Remove(tag);

			DeckList.Save();
			Core.MainWindow.ReloadTags();
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized || !Core.MainWindow.IsLoaded)
				return;

			var selectedValue = ComboboxDeckSorting.SelectedValue as string;
			if(selectedValue == null)
				return;

			if(Config.Instance.SelectedDeckSorting != selectedValue)
			{
				Config.Instance.SelectedDeckSorting = selectedValue;
				Config.Save();
			}

			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void SortFilterDecksFlyoutOnSelectedTagsChanged()
		{
			//only set tags if tags were changed in "My Decks"
			if(Name == nameof(Core.MainWindow.SortFilterDecksFlyout))
			{
				var tags = Tags.Where(tag => tag.Selected == true).Select(tag => tag.Name).ToList();
				Config.Instance.SelectedTags = tags;
				Config.Save();
				Core.MainWindow.DeckPickerList.UpdateDecks();
			}
			else if(Name == nameof(Core.MainWindow.TagControlEdit))
			{
				var tags = Tags.Where(tag => tag.Selected == true).Select(tag => tag.Name).ToList();
				var ignore = Tags.Where(tag => tag.Selected == null).Select(tag => tag.Name).ToList();
				foreach(var deck in Core.MainWindow.DeckPickerList.SelectedDecks)
				{
					var keep = deck.Tags.Intersect(ignore);
					deck.Tags = new List<string>(tags.Concat(keep));
					deck.Edited();
				}
				Core.MainWindow.DeckPickerList.UpdateDecks(false);
				DeckList.Save();
			}
		}

		private void BtnUp_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedTag = ListboxTags.SelectedItem as Tag;
			if(selectedTag == null)
				return;
			var index = Tags.IndexOf(selectedTag) + 2; //decklist.alltags includes "all" and "none", this does not
			if(index > 1)
				MoveTag(selectedTag.Name, index, index - 1);
		}

		private void BtnDown_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedTag = ListboxTags.SelectedItem as Tag;
			if(selectedTag == null)
				return;
			var index = Tags.IndexOf(selectedTag) + 2;
			if(index < Tags.Count + 1)
				MoveTag(selectedTag.Name, index, index + 1);
		}

		private void BtnTop_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedTag = ListboxTags.SelectedItem as Tag;
			if(selectedTag == null)
				return;
			var index = Tags.IndexOf(selectedTag) + 2;
			MoveTag(selectedTag.Name, index, 2);
		}

		private void BtnBottom_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedTag = ListboxTags.SelectedItem as Tag;
			if(selectedTag == null)
				return;
			var index = Tags.IndexOf(selectedTag) + 2;
			MoveTag(selectedTag.Name, index, Tags.Count + 1);
		}

		private void MoveTag(string tagName, int from, int to)
		{
			if(from < 0 || from >= DeckList.Instance.AllTags.Count)
				return;
			DeckList.Instance.AllTags.RemoveAt(from);
			DeckList.Instance.AllTags.Insert(to, tagName);
			DeckList.Save();
			Core.MainWindow.ReloadTags();
			ListboxTags.SelectedIndex = to - 2;
		}

		private void CheckBoxSortByClass_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(!Config.Instance.SortDecksByClass)
			{
				Config.Instance.SortDecksByClass = true;
				Config.Save();
			}
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void CheckBoxSortByClass_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SortDecksByClass = false;
			Config.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void CheckBoxSortByClassArena_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SortDecksByClassArena = true;
			Config.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void CheckBoxSortByClassArena_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SortDecksByClassArena = false;
			Config.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void SelectorArena_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized || !Core.MainWindow.IsLoaded)
				return;

			var selectedValue = ComboboxDeckSortingArena.SelectedValue as string;
			if(selectedValue == null)
				return;


			if(Config.Instance.SelectedDeckSortingArena != selectedValue)
			{
				Config.Instance.SelectedDeckSortingArena = selectedValue;
				Config.Save();
			}

			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		#region Tag

		public new class Tag
		{
			public Tag(string name, bool? selected = false)
			{
				Name = name;
				Selected = selected;
			}

			public string Name { get; set; }
			public bool? Selected { get; set; }

			public override bool Equals(object obj)
			{
				var other = obj as Tag;
				if(other == null)
					return false;
				return other.Name == Name;
			}

			public override int GetHashCode() => Name.GetHashCode();
		}

		#endregion

		#region Methods

		public void HideStuffToCreateNewTag()
		{
			TextboxNewTag.Visibility = Visibility.Collapsed;
			BtnAddTag.Visibility = Visibility.Collapsed;
			BtnDeleteTag.Visibility = Visibility.Collapsed;
			BtnUp.Visibility = Visibility.Collapsed;
			BtnDown.Visibility = Visibility.Collapsed;
			BtnTop.Visibility = Visibility.Collapsed;
			BtnBottom.Visibility = Visibility.Collapsed;
		}

		public void LoadTags(List<string> tags)
		{
			FixTagOrder();
			var oldTag = new List<Tag>(Tags);
			Tags.Clear();
			foreach(var tag in tags)
			{
				var old = oldTag.FirstOrDefault(t => t.Name == tag);
				Tags.Add(old != null ? new Tag(tag, old.Selected) : new Tag(tag));
			}
			_initialized = true;
		}

		private void FixTagOrder()
		{
			if(DeckList.Instance.AllTags.Count == 0)
				return;
			var changed = false;
			if(DeckList.Instance.AllTags.IndexOf("All") != 0)
			{
				MoveTag("All", DeckList.Instance.AllTags.IndexOf("All"), 0);
				changed = true;
			}
			if(DeckList.Instance.AllTags.IndexOf("None") != 1)
			{
				MoveTag("None", DeckList.Instance.AllTags.IndexOf("None"), 1);
				changed = true;
			}
			if(changed)
				DeckList.Save();
		}

		public List<string> GetTags() => Tags.Where(t => t.Selected == true).Select(t => t.Name).ToList();

		public void SetSelectedTags(IEnumerable<string> tags)
		{
			if(tags == null)
				return;
			foreach(var tag in Tags)
				tag.Selected = tags.Contains(tag.Name);
			ListboxTags.Items.Refresh();
		}

		public void SetSelectedTags(IEnumerable<Deck> decks)
		{
			if(!decks.Any())
				return;
			foreach(var tag in Tags)
			{
				if(decks.All(d => d.Tags.Contains(tag.Name)))
					tag.Selected = true;
				else if(!decks.Any(d => d.Tags.Contains(tag.Name)))
					tag.Selected = false;
				else
					tag.Selected = null;
			}
			ListboxTags.Items.Refresh();
		}

		public void AddSelectedTag(string tag)
		{
			if(Tags.All(t => t.Name != tag))
				return;
			if(Tags.First(t => t.Name == "All").Selected == true)
				return;

			Tags.First(t => t.Name == tag).Selected = true;
			SortFilterDecksFlyoutOnSelectedTagsChanged();
		}

		#endregion

		#region Events

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is CheckBox))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource == null)
				return;

			var selectedValue = (originalSource as CheckBox).Content.ToString();
			Tags.First(t => t.Name == selectedValue).Selected = true;
			if (Tags.Any(t => t.Name == "All"))
			{
				if (selectedValue == "All")
				{
					foreach (var tag in Tags.Where(tag => tag.Name != "All"))
						tag.Selected = false;
				}
				else
					Tags.First(t => t.Name == "All").Selected = false;
			}
			ListboxTags.Items.Refresh();
			SortFilterDecksFlyoutOnSelectedTagsChanged();
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is CheckBox))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource == null)
				return;
			var selectedValue = (originalSource as CheckBox).Content.ToString();
			Tags.First(t => t.Name == selectedValue).Selected = false;
			SortFilterDecksFlyoutOnSelectedTagsChanged();
		}

		private void BtnAddTag_Click(object sender, RoutedEventArgs e)
		{
			var tag = TextboxNewTag.Text;
			if(Tags.Any(t => t.Name == tag))
				return;

			Tags.Add(new Tag(tag));

			NewTag(tag);
		}

		private void BtnDeteleTag_Click(object sender, RoutedEventArgs e)
		{
			var msgbxoResult = MessageBox.Show("The tag will be deleted from all decks", "Are you sure?", MessageBoxButton.YesNo,
			                                   MessageBoxImage.Exclamation);
			if(msgbxoResult != MessageBoxResult.Yes)
				return;

			var tag = ListboxTags.SelectedItem as Tag;
			if(tag == null)
				return;
			if(Tags.All(t => t.Equals(tag)))
				return;

			Tags.Remove(Tags.First(t => t.Equals(tag)));

			DeleteTag(tag.Name);
		}

		private void OperationSwitch_OnChecked(object sender, RoutedEventArgs e)
		{
			Config.Instance.TagOperation = TagFilerOperation.And;
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void OperationSwitch_OnUnchecked(object sender, RoutedEventArgs e)
		{
			Config.Instance.TagOperation = TagFilerOperation.Or;
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		#endregion

		private void CheckBoxSortFavorites_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SortDecksFavoritesFirst = true;
			Config.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void CheckBoxSortFavorites_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SortDecksFavoritesFirst = false;
			Config.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}
	}
}

#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;

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

			//TagControlOnNewTag += TagControlOnNewTag;
			//SelectedTagsChanged += TagControlOnSelectedTagsChanged;
			//DeleteTag += TagControlOnDeleteTag;
		}

		//public MainWindow Window;

		private void TagControlOnNewTag(SortFilterDecks sender, string tag)
		{
			if(!DeckList.Instance.AllTags.Contains(tag))
			{
				DeckList.Instance.AllTags.Add(tag);
				DeckList.Save();
				Helper.MainWindow.SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);
				Helper.MainWindow.TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(t => t != "All" && t != "None").ToList());
			}
		}

		private void TagControlOnDeleteTag(SortFilterDecks sender, string tag)
		{
			if(DeckList.Instance.AllTags.Contains(tag))
			{
				DeckList.Instance.AllTags.Remove(tag);

				foreach(var deck in DeckList.Instance.Decks.Where(deck => deck.Tags.Contains(tag)))
					deck.Tags.Remove(tag);

				//if(Helper.MainWindow.NewDeck.Tags.Contains(tag))
				//	Helper.MainWindow.NewDeck.Tags.Remove(tag);

				DeckList.Save();
				Helper.MainWindow.SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);
				Helper.MainWindow.TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(t => t != "All" && t != "None").ToList());
				//Helper.MainWindow.DeckPickerList.UpdateList();
				Helper.MainWindow.DeckPickerList.UpdateDecks();
			}
		}

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//Helper.MainWindow.DeckPickerList.SortDecks();
			if(!_initialized || !Helper.MainWindow.IsLoaded)
				return;

			var selectedValue = ComboboxDeckSorting.SelectedValue as string;
			if(selectedValue == null)
				return;


			if(Config.Instance.SelectedDeckSorting != selectedValue)
			{
				Config.Instance.SelectedDeckSorting = selectedValue;
				Config.Save();
			}

			Helper.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void SortFilterDecksFlyoutOnOperationChanged(SortFilterDecks sender, TagFilerOperation operation)
		{
			Config.Instance.TagOperation = operation;
			//Helper.MainWindow.DeckPickerList.SetTagOperation(operation);
			//Helper.MainWindow.DeckPickerList.UpdateList();
			Helper.MainWindow.DeckPickerList.UpdateDecks();
		}

		private void SortFilterDecksFlyoutOnSelectedTagsChanged()
		{
			//only set tags if tags were changed in "My Decks"
			if(Name == "SortFilterDecksFlyout")
			{
				var tags = Tags.Where(tag => tag.Selected == true).Select(tag => tag.Name).ToList();
				//Helper.MainWindow.DeckPickerList.SetSelectedTags(tags);
				Config.Instance.SelectedTags = tags;
				Config.Save();
				Helper.MainWindow.DeckPickerList.UpdateDecks();
				Helper.MainWindow.StatsWindow.StatsControl.LoadOverallStats();
				Helper.MainWindow.DeckStatsFlyout.LoadOverallStats();
			}
			else if(Name == "TagControlEdit")
			{
				var tags = Tags.Where(tag => tag.Selected == true).Select(tag => tag.Name).ToList();
				var ignore = Tags.Where(tag => tag.Selected == null).Select(tag => tag.Name).ToList();
				//DeckList.Instance.ActiveDeck.Tags = new List<string>(tags);
				foreach(var deck in Helper.MainWindow.DeckPickerList.SelectedDecks)
				{
					var keep = deck.Tags.Intersect(ignore);
					deck.Tags = new List<string>(tags.Concat(keep));
					deck.Edited();
					if(HearthStatsAPI.IsLoggedIn && Config.Instance.HearthStatsAutoUploadNewDecks)
						HearthStatsManager.UpdateDeckAsync(deck);
				}
				Helper.MainWindow.DeckPickerList.UpdateDecks(false, false);
				DeckList.Save();
				Helper.MainWindow.UpdateQuickFilterItemSource();
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
			DeckList.Instance.AllTags.RemoveAt(from);
			DeckList.Instance.AllTags.Insert(to, tagName);
			DeckList.Save();
			Helper.MainWindow.ReloadTags();
			ListboxTags.SelectedIndex = to - 2;
			Helper.MainWindow.UpdateQuickFilterItemSource();
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

			public override int GetHashCode()
			{
				return Name.GetHashCode();
			}
		}

		#endregion

		#region Methods

		public void HideStuffToCreateNewTag()
		{
			TextboxNewTag.Visibility = Visibility.Hidden;
			BtnAddTag.Visibility = Visibility.Hidden;
			BtnDeleteTag.Visibility = Visibility.Hidden;
			BtnUp.Visibility = Visibility.Hidden;
			BtnDown.Visibility = Visibility.Hidden;
			BtnTop.Visibility = Visibility.Hidden;
			BtnBottom.Visibility = Visibility.Hidden;
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

		public List<string> GetTags()
		{
			return Tags.Where(t => t.Selected == true).Select(t => t.Name).ToList();
		}

		public void SetSelectedTags(List<string> tags)
		{
			if(tags == null)
				return;
			foreach(var tag in Tags)
				tag.Selected = tags.Contains(tag.Name);
			ListboxTags.Items.Refresh();
		}

		public void SetSelectedTags(List<Deck> decks)
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

			//if (SelectedTagsChanged != null)
			//{
			//var tagNames = _tags.Where(t => t.Selected).Select(t => t.Name).ToList();
			SortFilterDecksFlyoutOnSelectedTagsChanged();
			//}
		}

		#endregion

		#region Events

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is CheckBox))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var checkBox = originalSource as CheckBox;
				if(checkBox != null)
				{
					var selectedValue = checkBox.Content.ToString();

					Tags.First(t => t.Name == selectedValue).Selected = true;
					if(Tags.Any(t => t.Name == "All"))
					{
						if(selectedValue == "All")
						{
							foreach(var tag in Tags.Where(tag => tag.Name != "All"))
								tag.Selected = false;
						}
						else
							Tags.First(t => t.Name == "All").Selected = false;
					}
				}
				ListboxTags.Items.Refresh();
				//if (SelectedTagsChanged != null)
				//{
				//var tagNames = _tags.Where(tag => tag.Selected).Select(tag => tag.Name).ToList();
				SortFilterDecksFlyoutOnSelectedTagsChanged();
				//}
			}
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is CheckBox))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var checkBox = originalSource as CheckBox;
				if(checkBox != null)
				{
					var selectedValue = checkBox.Content.ToString();
					Tags.First(t => t.Name == selectedValue).Selected = false;
				}

				//if (SelectedTagsChanged != null)
				//{
				//var tagNames = _tags.Where(tag => tag.Selected).Select(tag => tag.Name).ToList();
				SortFilterDecksFlyoutOnSelectedTagsChanged();
				//}
			}
		}

		private void BtnAddTag_Click(object sender, RoutedEventArgs e)
		{
			var tag = TextboxNewTag.Text;
			if(Tags.Any(t => t.Name == tag))
				return;

			Tags.Add(new Tag(tag));

			//if (TagControlOnNewTag != null)
			TagControlOnNewTag(this, tag);
			Helper.MainWindow.UpdateQuickFilterItemSource();
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

			//if (DeleteTag != null)
			TagControlOnDeleteTag(this, tag.Name);
			Helper.MainWindow.UpdateQuickFilterItemSource();
		}

		private void OperationSwitch_OnChecked(object sender, RoutedEventArgs e)
		{
			//if (OperationChanged != null)
			SortFilterDecksFlyoutOnOperationChanged(this, TagFilerOperation.And);
		}

		private void OperationSwitch_OnUnchecked(object sender, RoutedEventArgs e)
		{
			//if (OperationChanged != null)
			SortFilterDecksFlyoutOnOperationChanged(this, TagFilerOperation.Or);
		}

		#endregion
	}
}
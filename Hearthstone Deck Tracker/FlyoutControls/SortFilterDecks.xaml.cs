#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for TagControl.xaml
	/// </summary>
	public partial class SortFilterDecks
	{
		#region Tag

		public new class Tag
		{
			public Tag(string name, bool selected = false)
			{
				Name = name;
				Selected = selected;
			}

			public string Name { get; set; }
			public bool Selected { get; set; }

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

		public readonly ObservableCollection<Tag> Tags = new ObservableCollection<Tag>();
		private bool _initialized;

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
			var oldTag = new List<Tag>(Tags);
			Tags.Clear();
			foreach(var tag in tags)
			{
				var old = oldTag.FirstOrDefault(t => t.Name == tag);
				Tags.Add(old != null ? new Tag(tag, old.Selected) : new Tag(tag));
			}
			_initialized = true;
		}

		public List<string> GetTags()
		{
			return Tags.Where(t => t.Selected).Select(t => t.Name).ToList();
		}

		public void SetSelectedTags(List<string> tags)
		{
			if(tags == null)
				return;
			foreach(var tag in Tags)
				tag.Selected = tags.Contains(tag.Name);
			ListboxTags.Items.Refresh();
		}

		public void AddSelectedTag(string tag)
		{
			if(Tags.All(t => t.Name != tag))
				return;
			if(Tags.First(t => t.Name == "All").Selected)
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
			if(!Helper.MainWindow.DeckList.AllTags.Contains(tag))
			{
				Helper.MainWindow.DeckList.AllTags.Add(tag);
				Helper.MainWindow.WriteDecks();
				Helper.MainWindow.SortFilterDecksFlyout.LoadTags(Helper.MainWindow.DeckList.AllTags);
				Helper.MainWindow.TagControlEdit.LoadTags(Helper.MainWindow.DeckList.AllTags.Where(t => t != "All" && t != "None").ToList());
			}
		}


		private void TagControlOnDeleteTag(SortFilterDecks sender, string tag)
		{
			if(Helper.MainWindow.DeckList.AllTags.Contains(tag))
			{
				Helper.MainWindow.DeckList.AllTags.Remove(tag);

				foreach(var deck in Helper.MainWindow.DeckList.DecksList.Where(deck => deck.Tags.Contains(tag)))
					deck.Tags.Remove(tag);

				//if(Helper.MainWindow.NewDeck.Tags.Contains(tag))
				//	Helper.MainWindow.NewDeck.Tags.Remove(tag);

				Helper.MainWindow.WriteDecks();
				Helper.MainWindow.SortFilterDecksFlyout.LoadTags(Helper.MainWindow.DeckList.AllTags);
				Helper.MainWindow.TagControlEdit.LoadTags(Helper.MainWindow.DeckList.AllTags.Where(t => t != "All" && t != "None").ToList());
				Helper.MainWindow.DeckPickerList.UpdateList();
			}
		}

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Helper.MainWindow.DeckPickerList.SortDecks();
			if(!_initialized)
				return;

			var selectedValue = ComboboxDeckSorting.SelectedValue as string;
			if(selectedValue == null)
				return;

			if(Config.Instance.SelectedDeckSorting != selectedValue)
			{
				Config.Instance.SelectedDeckSorting = selectedValue;
				Config.Save();
			}
		}

		private void SortFilterDecksFlyoutOnOperationChanged(SortFilterDecks sender, TagFilerOperation operation)
		{
			Config.Instance.TagOperation = operation;
			Helper.MainWindow.DeckPickerList.SetTagOperation(operation);
			Helper.MainWindow.DeckPickerList.UpdateList();
		}

		private void SortFilterDecksFlyoutOnSelectedTagsChanged()
		{
			//only set tags if tags were changed in "My Decks"
			if(Name == "SortFilterDecksFlyout")
			{
				var tags = Tags.Where(tag => tag.Selected).Select(tag => tag.Name).ToList();
				Helper.MainWindow.DeckPickerList.SetSelectedTags(tags);
				Config.Instance.SelectedTags = tags;
				Config.Save();
				Helper.MainWindow.StatsWindow.StatsControl.LoadOverallStats();
				Helper.MainWindow.DeckStatsFlyout.LoadOverallStats();
			}
			else if(Name == "TagControlEdit")
			{
				var tags = Tags.Where(tag => tag.Selected).Select(tag => tag.Name).ToList();
				Helper.MainWindow.DeckPickerList.SelectedDeck.Tags = new List<string>(tags);
				Helper.MainWindow.DeckPickerList.UpdateList();
				Helper.MainWindow.WriteDecks();
				Helper.MainWindow.UpdateQuickFilterItemSource();
			}
		}

		private void BtnUp_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedTag = ListboxTags.SelectedItem as Tag;
			if(selectedTag == null)
				return;
			var index = Tags.IndexOf(selectedTag) + 1; //decklist.alltags includes "all", this does not
			if(index > 1)
				MoveTag(selectedTag.Name, index, index - 1);
		}

		private void BtnDown_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedTag = ListboxTags.SelectedItem as Tag;
			if(selectedTag == null)
				return;
			var index = Tags.IndexOf(selectedTag) + 1;
			if(index < Tags.Count)
				MoveTag(selectedTag.Name, index, index + 1);
		}

		private void BtnTop_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedTag = ListboxTags.SelectedItem as Tag;
			if(selectedTag == null)
				return;
			var index = Tags.IndexOf(selectedTag) + 1;
			MoveTag(selectedTag.Name, index, 1);
		}

		private void BtnBottom_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedTag = ListboxTags.SelectedItem as Tag;
			if(selectedTag == null)
				return;
			var index = Tags.IndexOf(selectedTag) + 1;
			MoveTag(selectedTag.Name, index, Tags.Count);
		}

		private void MoveTag(string tagName, int from, int to)
		{
			Helper.MainWindow.DeckList.AllTags.RemoveAt(from);
			Helper.MainWindow.DeckList.AllTags.Insert(to, tagName);
			Helper.MainWindow.WriteDecks();
			Helper.MainWindow.ReloadTags();
			ListboxTags.SelectedIndex = to - 1;
			Helper.MainWindow.UpdateQuickFilterItemSource();
		}
	}
}
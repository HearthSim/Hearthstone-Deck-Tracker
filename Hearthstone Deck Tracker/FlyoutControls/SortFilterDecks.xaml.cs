using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker
{
	public enum Operation
	{
		And,
		Or
	}

	/// <summary>
	/// Interaction logic for TagControl.xaml
	/// </summary>
	public partial class SortFilterDecks
	{
		#region Tag

		private new class Tag
		{
			public string Name { get; set; }
			public bool Selected { get; set; }

			public Tag(string name, bool selected = false)
			{
				Name = name;
				Selected = selected;
			}

			public override bool Equals(object obj)
			{
				var other = obj as Tag;
				if (other == null) return false;
				return other.Name == Name;
			}

			public override int GetHashCode()
			{
				return Name.GetHashCode();
			}
		}

		#endregion

		#region Delegates/Events/Properties

		//private delegate void DeleteTagHandler(SortFilterDecks sender, string tag);

		//private delegate void NewTagHandler(SortFilterDecks sender, string tag);

		//public delegate void OperationChangedHandler(SortFilterDecks sender, Operation operation);

		//public delegate void SelectedTagsChangedHandler(SortFilterDecks sender, List<string> tags);

		private readonly ObservableCollection<Tag> _tags = new ObservableCollection<Tag>();

		//public event SelectedTagsChangedHandler SelectedTagsChanged;
		//private event NewTagHandler TagControlOnNewTag;
		//private event DeleteTagHandler DeleteTag;
		//public event OperationChangedHandler OperationChanged;

		#endregion

		#region Methods

		public void HideStuffToCreateNewTag()
		{
			TextboxNewTag.Visibility = Visibility.Hidden;
			BtnAddTag.Visibility = Visibility.Hidden;
			BtnDeleteTag.Visibility = Visibility.Hidden;
		}

		public void LoadTags(List<string> tags)
		{
			var oldTag = new List<Tag>(_tags);
			_tags.Clear();
			foreach (var tag in tags)
			{
				var old = oldTag.FirstOrDefault(t => t.Name == tag);
				if (old != null)
					_tags.Add(new Tag(tag, old.Selected));
				else
					_tags.Add(new Tag(tag, false));
			}
		}

		public List<string> GetTags()
		{
			return _tags.Where(t => t.Selected).Select(t => t.Name).ToList();
		}

		public void SetSelectedTags(List<string> tags)
		{
			if (tags == null) return;
			foreach (var tag in _tags)
			{
				tag.Selected = tags.Contains(tag.Name);
			}
			ListboxTags.Items.Refresh();
		}

		public void AddSelectedTag(string tag)
		{
			if (_tags.All(t => t.Name != tag)) return;
			if (_tags.First(t => t.Name == "All").Selected) return;

			_tags.First(t => t.Name == tag).Selected = true;

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
			while ((originalSource != null) && !(originalSource is CheckBox))
			{
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			if (originalSource != null)
			{
				var checkBox = originalSource as CheckBox;
				if (checkBox != null)
				{
					var selectedValue = checkBox.Content.ToString();

					_tags.First(t => t.Name == selectedValue).Selected = true;
					if (_tags.Any(t => t.Name == "All"))
					{
						if (selectedValue == "All")
						{
							foreach (var tag in _tags.Where(tag => tag.Name != "All"))
							{
								tag.Selected = false;
							}
						}
						else
							_tags.First(t => t.Name == "All").Selected = false;
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
			while ((originalSource != null) && !(originalSource is CheckBox))
			{
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			if (originalSource != null)
			{
				var checkBox = originalSource as CheckBox;
				if (checkBox != null)
				{
					var selectedValue = checkBox.Content.ToString();
					_tags.First(t => t.Name == selectedValue).Selected = false;
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
			if (_tags.Any(t => t.Name == tag)) return;

			_tags.Add(new Tag(tag));

			//if (TagControlOnNewTag != null)
			TagControlOnNewTag(this, tag);
		}

		private void BtnDeteleTag_Click(object sender, RoutedEventArgs e)
		{
			var msgbxoResult = MessageBox.Show("The tag will be deleted from all decks", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
			if (msgbxoResult != MessageBoxResult.Yes)
				return;

			var tag = ListboxTags.SelectedItem as Tag;
			if (tag == null) return;
			if (_tags.All(t => t.Equals(tag))) return;

			_tags.Remove(_tags.First(t => t.Equals(tag)));

			//if (DeleteTag != null)
			TagControlOnDeleteTag(this, tag.Name);
		}

		private void OperationSwitch_OnChecked(object sender, RoutedEventArgs e)
		{
			//if (OperationChanged != null)
			SortFilterDecksFlyoutOnOperationChanged(this, Operation.And);
		}

		private void OperationSwitch_OnUnchecked(object sender, RoutedEventArgs e)
		{
			//if (OperationChanged != null)
			SortFilterDecksFlyoutOnOperationChanged(this, Operation.Or);
		}

		#endregion

		public SortFilterDecks()
		{
			InitializeComponent();

			ListboxTags.ItemsSource = _tags;

			//TagControlOnNewTag += TagControlOnNewTag;
			//SelectedTagsChanged += TagControlOnSelectedTagsChanged;
			//DeleteTag += TagControlOnDeleteTag;
		}


		//public MainWindow Window;

		private void TagControlOnNewTag(SortFilterDecks sender, string tag)
		{
			if (!Helper.MainWindow.DeckList.AllTags.Contains(tag))
			{
				Helper.MainWindow.DeckList.AllTags.Add(tag);
				Helper.MainWindow.WriteDecks();
				Helper.MainWindow.SortFilterDecksFlyout.LoadTags(Helper.MainWindow.DeckList.AllTags);
				Helper.MainWindow.TagControlMyDecks.LoadTags(Helper.MainWindow.DeckList.AllTags.Where(t => t != "All").ToList());
				Helper.MainWindow.TagControlNewDeck.LoadTags(Helper.MainWindow.DeckList.AllTags.Where(t => t != "All").ToList());
			}
		}

		private void TagControlOnSelectedTagsChanged(SortFilterDecks sender, List<string> tags)
		{
			if (Helper.MainWindow.NewDeck == null) return;
			if (sender.Name == "TagControlNewDeck")
				Helper.MainWindow.BtnSaveDeck.Content = "Save*";
			else if (sender.Name == "TagControlMyDecks")
			{
				var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
				deck.Tags = tags;
				Helper.MainWindow.DeckPickerList.UpdateList();
				Helper.MainWindow.DeckPickerList.SelectDeck(deck);
			}
		}

		private void TagControlOnDeleteTag(SortFilterDecks sender, string tag)
		{
			if (Helper.MainWindow.DeckList.AllTags.Contains(tag))
			{
				Helper.MainWindow.DeckList.AllTags.Remove(tag);
				foreach (var deck in Helper.MainWindow.DeckList.DecksList)
				{
					if (deck.Tags.Contains(tag))
						deck.Tags.Remove(tag);
				}

				if (Helper.MainWindow.NewDeck.Tags.Contains(tag))
					Helper.MainWindow.NewDeck.Tags.Remove(tag);

				Helper.MainWindow.WriteDecks();
				Helper.MainWindow.SortFilterDecksFlyout.LoadTags(Helper.MainWindow.DeckList.AllTags);
				Helper.MainWindow.TagControlMyDecks.LoadTags(Helper.MainWindow.DeckList.AllTags.Where(t => t != "All").ToList());
				Helper.MainWindow.TagControlNewDeck.LoadTags(Helper.MainWindow.DeckList.AllTags.Where(t => t != "All").ToList());
				Helper.MainWindow.DeckPickerList.UpdateList();
			}
		}

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedValue = ComboboxDeckSorting.SelectedValue as string;
			if (selectedValue == null) return;

			Config.Instance.SelectedDeckSorting = selectedValue;
			Config.Save();

			Helper.MainWindow.DeckPickerList.SortDecks();
		}

		private void SortFilterDecksFlyoutOnOperationChanged(SortFilterDecks sender, Operation operation)
		{
			Config.Instance.TagOperation = operation;
			Helper.MainWindow.DeckPickerList.SetTagOperation(operation);
			Helper.MainWindow.DeckPickerList.UpdateList();
		}

		private void SortFilterDecksFlyoutOnSelectedTagsChanged()
		{
			var tags = _tags.Where(tag => tag.Selected).Select(tag => tag.Name).ToList();
			Helper.MainWindow.DeckPickerList.SetSelectedTags(tags);
			Config.Instance.SelectedTags = tags;
			Config.Save();
		}
	}
}
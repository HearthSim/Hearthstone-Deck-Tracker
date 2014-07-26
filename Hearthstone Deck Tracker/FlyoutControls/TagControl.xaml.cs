using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hearthstone_Deck_Tracker
{
	public enum Operation
	{
		And, Or
	}
	/// <summary>
	/// Interaction logic for TagControl.xaml
	/// </summary>
	public partial class TagControl : UserControl
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

		public delegate void SelectedTagsChangedHandler(TagControl sender, List<string> tags);
		public delegate void NewTagHandler(TagControl sender, string tag);
		public delegate void DeleteTagHandler(TagControl sender, string tag);

		public delegate void OperationChangedHandler(TagControl sender, Operation operation);

		public event SelectedTagsChangedHandler SelectedTagsChanged;
		public event NewTagHandler NewTag;
		public event DeleteTagHandler DeleteTag;
		public event OperationChangedHandler OperationChanged;

		private readonly ObservableCollection<Tag> _tags = new ObservableCollection<Tag>();

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
				bool isSelected = false;

				var old = oldTag.FirstOrDefault(t => t.Name == tag);
				if (old != null)
					isSelected = old.Selected;

				_tags.Add(new Tag(tag, isSelected));
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
			if (!_tags.Any(t => t.Name == tag)) return;
			if (_tags.First(t => t.Name == "All").Selected) return;

			_tags.First(t => t.Name == tag).Selected = true;

			if (SelectedTagsChanged != null)
			{
				var tagNames = _tags.Where(t => t.Selected).Select(t => t.Name).ToList();
				SelectedTagsChanged(this, tagNames);
			}
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

						/*
						foreach (var tag in _tags)
						{
							if (tag.Name != "All")
							{
								tag.Selected = false;
							}
						}
						*/
					}
					else
						_tags.First(t => t.Name == "All").Selected = false;

				}
				ListboxTags.Items.Refresh();

				if (SelectedTagsChanged != null)
				{
					var tagNames = _tags.Where(tag => tag.Selected).Select(tag => tag.Name).ToList();
					SelectedTagsChanged(this, tagNames);
				}

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
				var selectedValue = checkBox.Content.ToString();

				_tags.First(t => t.Name == selectedValue).Selected = false;

				if (SelectedTagsChanged != null)
				{
					var tagNames = _tags.Where(tag => tag.Selected).Select(tag => tag.Name).ToList();
					SelectedTagsChanged(this, tagNames);
				}

			}
		}

		private void BtnAddTag_Click(object sender, RoutedEventArgs e)
		{
			var tag = TextboxNewTag.Text;
			if (_tags.Any(t => t.Name == tag)) return;

			_tags.Add(new Tag(tag));

			if (NewTag != null)
				NewTag(this, tag);
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

			if (DeleteTag != null)
				DeleteTag(this, tag.Name);
		}

		private void OperationSwitch_OnChecked(object sender, RoutedEventArgs e)
		{
			if (OperationChanged != null)
				OperationChanged(this, Operation.And);
		}

		private void OperationSwitch_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if (OperationChanged != null)
				OperationChanged(this, Operation.Or);
		}

		#endregion

		public TagControl()
		{
			InitializeComponent();

			ListboxTags.ItemsSource = _tags;
		}
	}
}

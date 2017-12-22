#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Constructed
{
	/// <summary>
	/// Interaction logic for ConstructedDeckHighlights.xaml
	/// </summary>
	public partial class ConstructedDeckHighlights : INotifyPropertyChanged
	{
		public static readonly DependencyProperty ImageVisiblityProperty = DependencyProperty.Register("ImageVisiblity", typeof(Visibility),
																									   typeof(ConstructedDeckHighlights), new PropertyMetadata(Visibility.Visible));

		public static readonly DependencyProperty ClassProperty = DependencyProperty.Register("Class", typeof(string),
																							  typeof(ConstructedDeckHighlights), new PropertyMetadata(default(string)));

		public ConstructedDeckHighlights()
		{
			InitializeComponent();
		}

		public Visibility ImageVisiblity
		{
			get { return (Visibility)GetValue(ImageVisiblityProperty); }
			set { SetValue(ImageVisiblityProperty, value); }
		}

		public string ClassName
		{
			get
			{
				var stats = DataContext as ConstructedDeckStats;
				return stats != null ? stats.Class : Class;
			}
		}

		public string Class
		{
			get { return (string)GetValue(ClassProperty); }
			set { SetValue(ClassProperty, value); }
		}

		public BitmapImage ClassImage
		{
			get
			{
				var stats = DataContext as ConstructedDeckStats;
				return stats != null ? stats.ClassImage : ImageCache.GetClassIcon(Class);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void ConstructedDeckHighlights_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(ClassImage));
			OnPropertyChanged(nameof(ImageVisiblity));
			OnPropertyChanged(nameof(ClassName));
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

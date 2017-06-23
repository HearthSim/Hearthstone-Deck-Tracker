#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	/// <summary>
	/// Interaction logic for ArenaClassStats.xaml
	/// </summary>
	public partial class ArenaClassStats : INotifyPropertyChanged
	{
		public static readonly DependencyProperty ImageVisiblityProperty = DependencyProperty.Register("ImageVisiblity", typeof(Visibility),
		                                                                                               typeof(ArenaClassStats),
		                                                                                               new PropertyMetadata(
			                                                                                               Visibility.Visible));

		public static readonly DependencyProperty ClassProperty = DependencyProperty.Register("Class", typeof(string),
		                                                                                      typeof(ArenaClassStats),
		                                                                                      new PropertyMetadata(default(string)));

		public ArenaClassStats()
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
				var classStats = DataContext as ClassStats;
				return classStats != null ? classStats.Class : Class;
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
				var classStats = DataContext as ClassStats;
				return classStats != null ? classStats.ClassImage : ImageCache.GetClassIcon(Class);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void ArenaClassStats_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

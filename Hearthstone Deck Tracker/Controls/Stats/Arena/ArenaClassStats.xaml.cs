using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	/// <summary>
	/// Interaction logic for ArenaClassStats.xaml
	/// </summary>
	public partial class ArenaClassStats : INotifyPropertyChanged
	{
		public static readonly DependencyProperty ImageVisiblityProperty = DependencyProperty.Register("ImageVisiblity", typeof(Visibility), typeof(ArenaClassStats), new PropertyMetadata(Visibility.Visible));
		public static readonly DependencyProperty ClassProperty = DependencyProperty.Register("Class", typeof(string), typeof(ArenaClassStats), new PropertyMetadata(default(string)));

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
				if(classStats != null)
					return classStats.Class;
				return Class;
			}
		}

		public string Class
		{
			get
			{
				return (string)GetValue(ClassProperty);
			}
			set { SetValue(ClassProperty, value); }
		}

		public BitmapImage ClassImage
		{
			get
			{
				var classStats = DataContext as ClassStats;
				if(classStats != null)
					return classStats.ClassImage;
				return ImageCache.GetClassIcon(Class);
			}
		}

		private void ArenaClassStats_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			OnPropertyChanged("ClassImage");
			OnPropertyChanged("ImageVisiblity");
			OnPropertyChanged("ClassName");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

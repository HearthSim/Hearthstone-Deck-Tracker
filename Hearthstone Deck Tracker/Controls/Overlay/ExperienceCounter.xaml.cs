using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class ExperienceCounter : UserControl, INotifyPropertyChanged
	{
		private string _levelDisplay;
		public string LevelDisplay
		{
			get => _levelDisplay;
			set
			{
				_levelDisplay = value;
				OnPropertyChanged();
			}
		}

		private string _xpDisplay;
		public string XPDisplay
		{
			get => _xpDisplay;
			set
			{
				_xpDisplay = value;
				OnPropertyChanged();
			}
		}

		private Rect _xpBarRect;
		public Rect XPBarRect
		{
			get => _xpBarRect;
			set
			{
				_xpBarRect = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ExperienceCounter()
		{
			InitializeComponent();
		}

		public void ChangeRectangleFill(double newPercentageFull, bool instant)
		{
			XPBarRect = new Rect(0, 0, newPercentageFull * (FullXPBar.ActualWidth != 0 ? FullXPBar.ActualWidth : 380), 10000);
			if(instant)
				(FindResource("StoryBoardInstantAnimate") as Storyboard)?.Begin();
			else
				(FindResource("StoryBoardLevelUp") as Storyboard)?.Begin();
		}

		public void ResetRectangleFill()
		{
			XPBarRect = new Rect(0, 0, 0, 10000);
			(FindResource("StoryBoardReset") as Storyboard)?.Begin();
		}
	}
}

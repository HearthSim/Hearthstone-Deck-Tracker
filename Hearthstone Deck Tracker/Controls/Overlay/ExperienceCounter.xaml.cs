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
		private HearthMirror.Objects.RewardTrackData rewardTrackData = null;

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
			ChangeRectangleFill(0);
		}

		internal void UpdateCurrentRewards(HearthMirror.Objects.RewardTrackData newRewards)
		{
			if(newRewards != null)
			{
				if(rewardTrackData == null ||
				rewardTrackData.Xp != newRewards.Xp ||
				rewardTrackData.Level != newRewards.Level ||
				rewardTrackData.XpNeeded != newRewards.XpNeeded)
				{
					XPDisplay = string.Format($"{newRewards.Xp}/{newRewards.XpNeeded}");
					LevelDisplay = string.Format($"{newRewards.Level}");
					ChangeRectangleFill(480 * ((double)newRewards.Xp / (double)newRewards.XpNeeded));
				}
			}
			rewardTrackData = newRewards;
		}

		private void ChangeRectangleFill(double newPercentageFull)
		{
			XPBarRect = new Rect(0, 0, newPercentageFull * FullXPBar.ActualWidth, 10000);
			(FindResource("StoryBoardLevelUp") as Storyboard)?.Begin();
		}
	}
}

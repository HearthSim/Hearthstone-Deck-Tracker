using System.Windows;
using System.Windows.Media;
using MahApps.Metro;

namespace Hearthstone_Deck_Tracker.Replay
{
	public class TurnViewItem
	{
		public ReplayKeyPoint KeyPoint;
		public string PlayerAction { get; set; }
		public string OpponentAction { get; set; }
		public string AdditionalInfoPlayer { get; set; }
		public string AdditionalInfoOpponent { get; set; }
		public int? Turn { get; set; }
		public bool IsCollapsed { get; set; }
		public bool ShowAll { get; set; }

		public string TurnString
		{
			get { return Turn.HasValue ? "Turn " + Turn.Value : ""; }
		}

		public bool IsTurnRow
		{
			get { return Turn.HasValue; }
		}

		public SolidColorBrush RowBackground
		{
			get
			{
				return
					new SolidColorBrush(Turn.HasValue ? (Color)ThemeManager.DetectAppStyle().Item2.Resources["AccentColor"] : Colors.Transparent);
			}
		}

		public Visibility VisibilityShowAll
		{
			get { return !ShowAll ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility VisibilityShowFiltered
		{
			get { return ShowAll ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility VisibilityTurnRow
		{
			get { return Turn.HasValue ? Visibility.Visible : Visibility.Hidden; }
		}

		public Visibility VisibilityKeyPoint
		{
			get { return !Turn.HasValue ? Visibility.Visible : Visibility.Hidden; }
		}

		public Visibility VisibilityPlayer
		{
			get { return !string.IsNullOrEmpty(PlayerAction) ? Visibility.Visible : Visibility.Hidden; }
		}

		public Visibility VisibilityOpponent
		{
			get { return !string.IsNullOrEmpty(OpponentAction) ? Visibility.Visible : Visibility.Hidden; }
		}
	}
}
#region

using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro;

#endregion

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

		public Card Card
		{
			get { return KeyPoint == null ? null : GameV2.GetCardFromId(KeyPoint.GetCardId()); }
		}

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
				return new SolidColorBrush(Turn.HasValue ? (Color)ThemeManager.DetectAppStyle().Item2.Resources["AccentColor"] : Colors.Transparent);
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

		public string Action
		{
			get
			{
				if(KeyPoint == null)
					return "";
				switch(KeyPoint.Type)
				{
					case KeyPointType.Attack:
						return "(atk)";
					case KeyPointType.Death:
						return "(dth)";
					case KeyPointType.DeckDiscard:
					case KeyPointType.HandDiscard:
						return "(dsc)";
					case KeyPointType.Draw:
					case KeyPointType.Mulligan:
					case KeyPointType.Obtain:
					case KeyPointType.PlayToDeck:
					case KeyPointType.PlayToHand:
						return "(drw)";
					case KeyPointType.HeroPower:
						return "(hrp)";
					case KeyPointType.SecretStolen:
					case KeyPointType.SecretTriggered:
						return "(scr)";
					case KeyPointType.Play:
					case KeyPointType.PlaySpell:
					case KeyPointType.SecretPlayed:
						return "(ply)";
					case KeyPointType.Summon:
						return "(smn)";
				}
				return "";
			}
		}

		public VisualBrush ActionIcon
		{
			get
			{
				var resource = GetResourceName();
				if(string.IsNullOrEmpty(resource))
					return new VisualBrush();
				return new VisualBrush((Visual)Application.Current.FindResource(resource));
			}
		}

		private string GetResourceName()
		{
			if(KeyPoint == null)
				return "";
			switch(KeyPoint.Type)
			{
				case KeyPointType.Attack:
					return "action_attack";
				case KeyPointType.Death:
				case KeyPointType.Defeat:
					return "action_death";
				case KeyPointType.Mulligan:
				case KeyPointType.DeckDiscard:
				case KeyPointType.HandDiscard:
					return "action_discard";
				case KeyPointType.Draw:
				case KeyPointType.Obtain:
				case KeyPointType.PlayToDeck:
				case KeyPointType.PlayToHand:
				case KeyPointType.CreateToDeck:
					return "action_draw";
				case KeyPointType.HeroPower:
					return "action_play";
				case KeyPointType.SecretStolen:
				case KeyPointType.SecretTriggered:
					return "action_secret";
				case KeyPointType.Play:
				case KeyPointType.PlaySpell:
				case KeyPointType.SecretPlayed:
					return "action_play";
				case KeyPointType.Summon:
					return "action_summon";
				case KeyPointType.Victory:
					return "action_victory";
			}
			return "";
		}
	}
}
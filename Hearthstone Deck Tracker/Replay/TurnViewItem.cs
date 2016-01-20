#region

using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro;
using static System.Windows.Visibility;
using static Hearthstone_Deck_Tracker.Replay.KeyPointType;

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

		public Card Card => KeyPoint == null ? null : Database.GetCardFromId(KeyPoint.GetCardId());

		public string TurnString => Turn.HasValue ? "Turn " + Turn.Value : "";

		public bool IsTurnRow => Turn.HasValue;

		public SolidColorBrush RowBackground => new SolidColorBrush(Turn.HasValue ? (Color)ThemeManager.DetectAppStyle().Item2.Resources["AccentColor"] : Colors.Transparent);

		public Visibility VisibilityShowAll => !ShowAll ? Visible : Collapsed;

		public Visibility VisibilityShowFiltered => ShowAll ? Visible : Collapsed;

		public Visibility VisibilityTurnRow => Turn.HasValue ? Visible : Hidden;

		public Visibility VisibilityKeyPoint => !Turn.HasValue ? Visible : Hidden;

		public Visibility VisibilityPlayer => !string.IsNullOrEmpty(PlayerAction) ? Visible : Hidden;

		public Visibility VisibilityOpponent => !string.IsNullOrEmpty(OpponentAction) ? Visible : Hidden;

		public string Action
		{
			get
			{
				if(KeyPoint == null)
					return "";
				switch(KeyPoint.Type)
				{
					case Attack:
						return "(atk)";
					case Death:
						return "(dth)";
					case DeckDiscard:
					case HandDiscard:
						return "(dsc)";
					case Draw:
					case Mulligan:
					case Obtain:
					case PlayToDeck:
					case PlayToHand:
						return "(drw)";
					case HeroPower:
						return "(hrp)";
					case SecretStolen:
					case SecretTriggered:
						return "(scr)";
					case Play:
					case PlaySpell:
					case SecretPlayed:
						return "(ply)";
					case Summon:
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
				return string.IsNullOrEmpty(resource) ? new VisualBrush() : new VisualBrush((Visual)Application.Current.FindResource(resource));
			}
		}

		private string GetResourceName()
		{
			if(KeyPoint == null)
				return "";
			switch(KeyPoint.Type)
			{
				case Attack:
					return "action_attack";
				case Death:
				case Defeat:
					return "action_death";
				case Mulligan:
				case DeckDiscard:
				case HandDiscard:
					return "action_discard";
				case Draw:
				case Obtain:
				case PlayToDeck:
				case PlayToHand:
				case CreateToDeck:
					return "action_draw";
				case HeroPower:
					return "action_play";
				case SecretStolen:
				case SecretTriggered:
					return "action_secret";
				case Play:
				case PlaySpell:
				case SecretPlayed:
					return "action_play";
				case Summon:
					return "action_summon";
				case Victory:
					return "action_victory";
			}
			return "";
		}
	}
}
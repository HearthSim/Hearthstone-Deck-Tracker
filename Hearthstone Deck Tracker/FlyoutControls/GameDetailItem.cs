using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker
{
    public class GameDetailItem
    {
        public GameDetailItem(TurnStats.Play play, int turn)
        {
            Turn = turn.ToString();
            Player = play.Type.ToString().StartsWith("Player") ? "Player" : "Opponent";
            Action = play.Type.ToString().Replace("Player", string.Empty).Replace("Opponent", string.Empty);
            Card = GameV2.GetCardFromId(play.CardId);

            if (play.Type == PlayType.PlayerHandDiscard || play.Type == PlayType.OpponentHandDiscard && (Card != null && Card.Type == "Spell"))
                Action = "Play/Discard";
        }

        public GameDetailItem()
        {
        }

        public string Turn { get; set; }
        public string Player { get; set; }
        public string Action { get; set; }
        public Card Card { get; set; }
    }
}
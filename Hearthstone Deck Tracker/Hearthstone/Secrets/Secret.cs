#region

using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class Secret
	{
		public Secret(string cardId, int count)
		{
			CardId = cardId;
			Count = count;
		}

		public string CardId { get; private set; }
		public int Count { get; set; }


	    public int AdjustedCount(GameV2 game)
	    {
            return (Config.Instance.AutoGrayoutSecrets
                        && (game.CurrentGameMode == GameMode.Casual
                            || game.CurrentGameMode == GameMode.Ranked || game.CurrentGameMode == GameMode.Brawl
                            || game.CurrentGameMode == GameMode.Friendly || game.CurrentGameMode == GameMode.Practice)
                        && game.OpponentCards.Any(x => !x.IsStolen && x.Id == CardId & x.Count >= 2)) ? 0 : Count;
        }
        
	}
}
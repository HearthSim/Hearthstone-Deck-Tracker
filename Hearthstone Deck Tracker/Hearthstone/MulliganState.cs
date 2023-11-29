using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HearthDb.CardDefs;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	internal class MulliganState
	{
		public List<Entity> OfferedCards { get; private set; } = new List<Entity>();
		public List<Entity> KeptCards { get; private set; } = new List<Entity>();
		public List<Entity> FinalCardsInHand { get; private set; } = new List<Entity>();

		private GameV2 _game;

		public MulliganState(GameV2 game)
		{
			_game = game;
		}

		public void SnapshotMulligan()
		{
			OfferedCards = _game.Player.PlayerEntities.Where(x => x.IsInHand && !x.Info.Created).OrderBy(x => x.ZonePosition).ToList();
		}

		public void SnapshotMulliganChoices(Choice choice)
		{
			KeptCards = choice.ChosenEntities.ToList();
		}

		public void SnapshotOpeningHand()
		{
			FinalCardsInHand = _game.Player.PlayerEntities.Where(x => x.IsInHand && !x.Info.Created).OrderBy(x => x.ZonePosition).ToList();
		}
	}
}

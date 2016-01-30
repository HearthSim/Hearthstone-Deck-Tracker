using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameMetaData
	{
		public string ServerAddress { get; set; }
		public string ClientId { get; set; }
		public string GameId { get; set; }
		public string SpectateKey { get; set; }

		public override string ToString() 
			=> $"ServerAddress={ServerAddress}, ClientId={ClientId}, GameId={GameId}, SpectateKey={SpectateKey}";
	}
}

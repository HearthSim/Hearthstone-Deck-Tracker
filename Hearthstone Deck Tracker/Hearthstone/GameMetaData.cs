using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameMetaData
	{
		private int? _propagatedLegendRank;
		private int? _hearthstoneBuild;
		private int _legendRank;
		public string ServerAddress { get; set; }
		public string ClientId { get; set; }
		public string GameId { get; set; }
		public string SpectateKey { get; set; }
		public DateTime EnqueueTime { get; set; }

		public int LegendRank
		{
			get { return _propagatedLegendRank ?? _legendRank; }
			set { _legendRank = value; }
		}
		
		internal void PropagateLegendRank()
		{
			if(_legendRank > 0)
				_propagatedLegendRank = _legendRank;
		}

		public int? HearthstoneBuild
		{
			get
			{
				if(!_hearthstoneBuild.HasValue)
					_hearthstoneBuild = Helper.GetHearthstoneBuild();
				return _hearthstoneBuild;
				
			}
			set { _hearthstoneBuild = value; }
		}

		public override string ToString() 
			=> $"HearthstoneBuild={HearthstoneBuild}, ServerAddress={ServerAddress}, ClientId={ClientId}, GameId={GameId}, SpectateKey={SpectateKey}, LegendRank={LegendRank}, EnqueueTime={EnqueueTime}";
	}
}

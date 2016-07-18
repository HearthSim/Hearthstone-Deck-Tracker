using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HearthMirror.Objects;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameMetaData
	{
		private int? _hearthstoneBuild;
		public GameServerInfo ServerInfo;
		public DateTime EnqueueTime { get; set; }

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
			=> $"HearthstoneBuild={HearthstoneBuild}, ServerAddress={ServerInfo?.Address}, ClientId={ServerInfo?.ClientHandle}, GameId={ServerInfo?.GameHandle}, EnqueueTime={EnqueueTime}";
	}
}

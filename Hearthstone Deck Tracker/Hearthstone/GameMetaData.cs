using System;
using HearthMirror.Objects;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameMetaData
	{
		private int? _hearthstoneBuild;
		public GameServerInfo ServerInfo;
		public DateTime EnqueueTime { get; set; }
		public bool Reconnected { get; set; }

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

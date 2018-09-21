using System;
using HearthMirror.Objects;
using HSReplay;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameMetaData : ICloneable
	{
		private int? _hearthstoneBuild;
		public GameServerInfo ServerInfo;
		public DateTime EnqueueTime { get; set; }
		public bool Reconnected { get; set; }
		public UploadMetaData.TwitchVodData TwitchVodData { get; set; }

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

		public object Clone() => new GameMetaData
		{
			_hearthstoneBuild = _hearthstoneBuild,
			EnqueueTime = EnqueueTime,
			HearthstoneBuild = HearthstoneBuild,
			Reconnected = Reconnected,
			ServerInfo = ServerInfo,
			TwitchVodData = TwitchVodData
		};
	}
}

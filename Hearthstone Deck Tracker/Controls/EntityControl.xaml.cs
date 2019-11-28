using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class EntityControl : UserControl
	{
		public EntityControl(Entity entity)
		{
			InitializeComponent();
			DataContext = entity;
			Card = (Hearthstone.Card)entity.Card.Clone();
			Card.BaconCard = true;
		}

		public Entity Entity => (Entity)DataContext;

		public Dictionary<GameTag, string> RelevantTags = new Dictionary<GameTag, string>()
		{
			[GameTag.TAUNT] = LocUtil.Get("GameTag_Taunt"),
			[GameTag.DIVINE_SHIELD] = LocUtil.Get("GameTag_DivineShield"),
			[GameTag.POISONOUS] = LocUtil.Get("GameTag_Poisonous"),
			[GameTag.WINDFURY] = LocUtil.Get("GameTag_Windfury"),
			[GameTag.DEATHRATTLE] = LocUtil.Get("GameTag_Deathrattle")
		};

		public string Effects
		{ 
			get
			{
				if(Entity == null)
					return null;
				var tags = RelevantTags.Keys.Where(x => Entity.HasTag(x)).Select(x => RelevantTags[x]);
				return string.Join(", ", tags);
			}
		}

		public int Health => Entity?.GetTag(GameTag.HEALTH) ?? 0;

		public Hearthstone.Card Card { get; }
	}
}

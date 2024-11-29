using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Controls;

public partial class HeroPower
{
	public HeroPower()
	{
		InitializeComponent();
	}

	public HeroPower(Entity entity) : this()
	{
		DataContext = new HeroPowerViewModel
		{
			Card = entity.Card,
		};
	}
}

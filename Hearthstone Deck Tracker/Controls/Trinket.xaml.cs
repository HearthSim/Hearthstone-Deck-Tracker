using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Controls;

public partial class Trinket
{
	public Trinket()
	{
		InitializeComponent();
	}

	public Trinket(Entity entity) : this()
	{
		DataContext = new TrinketViewModel
		{
			Card = entity.Card,
		};
	}
}

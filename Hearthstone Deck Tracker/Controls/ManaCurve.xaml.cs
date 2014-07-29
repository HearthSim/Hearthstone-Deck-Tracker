using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for ManaCurve.xaml
	/// </summary>
	public partial class ManaCurve
	{
		private readonly ManaCostBar[] _manaCostBars;
		private Deck _deck;

		public ManaCurve()
		{
			InitializeComponent();

			_manaCostBars = new[]
				{
					ManaCostBar0,
					ManaCostBar1,
					ManaCostBar2,
					ManaCostBar3,
					ManaCostBar4,
					ManaCostBar5,
					ManaCostBar6,
					ManaCostBar7
				};
		}

		public void SetDeck(Deck deck)
		{
			_deck = deck;
			deck.Cards.CollectionChanged += (sender, args) => UpdateValues();
			UpdateValues();
		}

		public void ClearDeck()
		{
			_deck = null;
			for (var i = 0; i < 8; i++)
			{
				_manaCostBars[i].SetValues(0, 0, 0, 0);
				_manaCostBars[i].SetTooltipValues(0, 0, 0);
			}
		}

		public void UpdateValues()
		{
			if (_deck == null) return;

			var counts = new int[8];
			var weapons = new int[8];
			var spells = new int[8];
			var minions = new int[8];
			foreach (var card in _deck.Cards)
			{
				if (card.Cost >= 7)
				{
					switch (card.Type)
					{
						case "Weapon":
							weapons[7] += card.Count;
							break;
						case "Enchantment":
						case "Spell":
							spells[7] += card.Count;
							break;
						case "Minion":
							minions[7] += card.Count;
							break;
					}
					counts[7] += card.Count;
				}
				else
				{
					switch (card.Type)
					{
						case "Weapon":
							weapons[card.Cost] += card.Count;
							break;
						case "Enchantment":
						case "Spell":
							spells[card.Cost] += card.Count;
							break;
						case "Minion":
							minions[card.Cost] += card.Count;
							break;
					}
					counts[card.Cost] += card.Count;
				}
			}
			var max = 0;
			for (var i = 0; i < 8; i++)
			{
				var sum = weapons[i] + spells[i] + minions[i];
				if (sum > max)
					max = sum;
			}

			for (var i = 0; i < 8; i++)
			{
				if (max == 0)
				{
					_manaCostBars[i].SetValues(0, 0, 0, 0);
					_manaCostBars[i].SetTooltipValues(0, 0, 0);
				}
				else
				{
					_manaCostBars[i].SetValues(100d*weapons[i]/max, 100d*spells[i]/max, 100d*minions[i]/max, counts[i]);
					_manaCostBars[i].SetTooltipValues(weapons[i], spells[i], minions[i]);
				}
			}
		}
	}
}
#region

using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for ManaCurve.xaml
    /// </summary>
    public partial class ManaCurve
    {
        private readonly ManaCostBar[] _manaCostBars;
        private Deck _deck;
        private string _filter;

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
            if (deck == null)
            {
                ClearDeck();
                return;
            }
            _deck = deck;
            deck.GetSelectedDeckVersion().Cards.CollectionChanged += (sender, args) => UpdateValues();
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
            if (_deck == null)
                return;
            _filter = Config.Instance.curveFilter;
            var counts = new int[8];
            var weapons = new int[8];
            var spells = new int[8];
            var minions = new int[8];
            foreach (var card in _deck.GetSelectedDeckVersion().Cards)
            {
                int value=(int)card.GetType().GetProperty(_filter).GetValue(card, null);
                if (value >= 7)
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
                    if (!((_filter == "Health" || _filter == "Attack") && value==0))
                    {
                        switch (card.Type)
                        {
                            case "Weapon":
                                weapons[value] += card.Count;
                                break;
                            case "Enchantment":
                            case "Spell":
                                spells[value] += card.Count;
                                break;
                            case "Minion":
                                minions[value] += card.Count;
                                break;
                        }
                        counts[value] += card.Count;
                    }
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
                    _manaCostBars[i].SetValues(100d * weapons[i] / max, 100d * spells[i] / max, 100d * minions[i] / max, counts[i]);
                    _manaCostBars[i].SetTooltipValues(weapons[i], spells[i], minions[i]);
                }
            }
        }
    }
}
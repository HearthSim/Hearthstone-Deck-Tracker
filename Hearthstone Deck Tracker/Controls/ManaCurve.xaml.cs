using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for ManaCurve.xaml
    /// </summary>
    public partial class ManaCurve : UserControl
    {
        private Deck _deck;
        private readonly ManaCostBar[] _manaCostBars;
        private readonly Label[] _countLabels;
        public ManaCurve()
        {
            InitializeComponent();

            _manaCostBars = new ManaCostBar[]
                {
                    ManaCostBar0,
                    ManaCostBar1,
                    ManaCostBar2,
                    ManaCostBar3,
                    ManaCostBar4,
                    ManaCostBar5,
                    ManaCostBar6,
                    ManaCostBar7,
                };
            _countLabels = new Label[]
                {
                    CountLabel0,
                    CountLabel1,
                    CountLabel2,
                    CountLabel3,
                    CountLabel4,
                    CountLabel5,
                    CountLabel6,
                    CountLabel7,
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
            for(int i = 0; i < 8; i++)
            {
                _manaCostBars[i].SetValues(0, 0, 0);
                _countLabels[i].Content = 0;
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
            for (int i = 0; i < 8; i++)
            {
                var sum = weapons[i] + spells[i] + minions[i];
                if (sum > max)
                    max = sum;
            }
            
            for (int i = 0; i < 8; i++)
            {
                if (max == 0)
                {
                    _manaCostBars[i].SetValues(0, 0, 0);
                }
                else
                {
                    _manaCostBars[i].SetValues(100d * weapons[i] / max, 100d * spells[i] / max, 100d * minions[i] / max);
                }
                _countLabels[i].Content = counts[i];
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for ManaCurve.xaml
    /// </summary>
    public partial class ManaCurve : UserControl
    {
        private Deck _deck;
        private readonly ProgressBar[] _progressBars;
        private readonly Label[] _countLabels;
        public ManaCurve()
        {
            InitializeComponent();
            _progressBars = new ProgressBar[]
                {
                    ProgressBar0,
                    ProgressBar1,
                    ProgressBar2,
                    ProgressBar3,
                    ProgressBar4,
                    ProgressBar5,
                    ProgressBar6,
                    ProgressBar7,
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
                _progressBars[i].Value = 0;
                _countLabels[i].Content = 0;
            }
        }

        private void UpdateValues()
        {
            if (_deck == null) return;

            var counts = new int[8];
            var progressBarValues = new double[8];
            foreach (var card in _deck.Cards)
            {
                if (card.Cost >= 7)
                {
                    progressBarValues[7] += card.Count;
                    counts[7] += card.Count;
                }
                else
                {
                    progressBarValues[card.Cost] += card.Count;
                    counts[card.Cost] += card.Count;
                }
            }
            var max = progressBarValues.Max();
            for (int i = 0; i < 8; i++)
            {
                _progressBars[i].Value = 100*progressBarValues[i]/max;
                _countLabels[i].Content = counts[i];
            }
        }

    }
}

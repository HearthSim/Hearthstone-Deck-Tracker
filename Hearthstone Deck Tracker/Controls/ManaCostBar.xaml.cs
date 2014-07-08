using System.Windows;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for MultiProgressBar.xaml
    /// </summary>
    public partial class ManaCostBar
    {
        public ManaCostBar()
        {
            InitializeComponent();
        }

        public void SetValues(double first, double second, double third, int count)
        {
            WeaponsRect.Height = ActualHeight * first / 100;
            SpellsRect.Height = ActualHeight * second / 100;
            MinionsRect.Height = ActualHeight * third / 100;

            LabelCount.Content = count;

            var offset = TotalHeight - 22;
            if (offset < -4) offset = -4;
            LabelCount.Margin = new Thickness(0, 0, 0, offset);
        }
        public double TotalHeight
        {
            get { return WeaponsRect.Height + SpellsRect.Height + MinionsRect.Height; } 
        }
    }
}

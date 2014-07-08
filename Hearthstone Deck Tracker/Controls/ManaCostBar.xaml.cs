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

        public void SetValues(double first, double second, double third)
        {
            WeaponsRect.Height = ActualHeight * first / 100;
            SpellsRect.Height = ActualHeight * second / 100;
            MinionsRect.Height = ActualHeight * third / 100;
        }
        public double TotalHeight
        {
            get { return WeaponsRect.Height + SpellsRect.Height + MinionsRect.Height; } 
        }
    }
}

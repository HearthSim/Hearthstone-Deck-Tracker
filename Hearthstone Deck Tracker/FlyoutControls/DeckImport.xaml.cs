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
    /// Interaction logic for DeckImport.xaml
    /// </summary>
    public partial class DeckImport : UserControl
    {
        public event DeckOptionsButtonClickedEvent DeckOptionsButtonClicked;
        public delegate void DeckOptionsButtonClickedEvent(DeckImport sender);
        public DeckImport()
        {
            InitializeComponent();
        }

        private void BtnWeb_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnArenavalue_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnText_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnFileXml_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }



    }
}

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
    /// Interaction logic for DeckOptions.xaml
    /// </summary>
    public partial class DeckOptions : UserControl
    {
        public event DeckOptionsButtonClickedEvent DeckOptionsButtonClicked;
        public delegate void DeckOptionsButtonClickedEvent(DeckOptions sender);
        public DeckOptions()
        {
            InitializeComponent();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnScreenhot_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnNotes_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnTags_Click(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnSaveToFile_OnClick(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        private void BtnClipboard_OnClick(object sender, RoutedEventArgs e)
        {
            if (DeckOptionsButtonClicked != null)
                DeckOptionsButtonClicked(this);
        }

        public void EnableButtons(bool enable)
        {
            BtnScreenshot.IsEnabled = enable;
            BtnNotes.IsEnabled = enable;
            BtnExportHs.IsEnabled = enable;
            BtnDeleteDeck.IsEnabled = enable;
            BtnCloneDeck.IsEnabled = enable;
            BtnTags.IsEnabled = enable;
            BtnSaveToFile.IsEnabled = enable;
            BtnClipboard.IsEnabled = enable;
        }
    }
}

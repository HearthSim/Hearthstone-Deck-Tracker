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
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Controls
{
    /// <summary>
    /// Interaction logic for CardMarker.xaml
    /// </summary>
    public partial class CardMarker : UserControl
    {

        public String Text
        {
            get { return CardAge.Text; }
            set { CardAge.Text = value; }
        }

        protected CardMark _mark;
        public CardMark Mark
        {
            get { return _mark; }
            set
            {
                _mark = value;
                ImageSourceConverter c = new ImageSourceConverter();
                
                String source = "";
                switch (_mark)
                {
                    
                    case CardMark.Coin:
                        source = "/HearthstoneDeckTracker;component/Images/card-icon-coin.png";
                        break;
                    case CardMark.Kept:
                        source = "/HearthstoneDeckTracker;component/Images/card-icon-keep.png";
                        break;
                    case CardMark.Mulliganed:
                        source = "/HearthstoneDeckTracker;component/Images/card-icon-mulligan.png";
                        break;
                    case CardMark.Returned:
                        source = "/HearthstoneDeckTracker;component/Images/card-icon-returned.png";
                        break;
                    case CardMark.Created:
                        source = "/HearthstoneDeckTracker;component/Images/card-icon-created.png";
                        break;
                    default:
                        CardIcon.Visibility = Visibility.Collapsed;
                        break;
                }

                if (source != "")
                {
                    CardIcon.Source = new BitmapImage(new Uri(source, UriKind.Relative));
	                CardIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    CardIcon.Visibility = Visibility.Collapsed;
                }
            }
        }

        public CardMarker()
        {
            InitializeComponent();
        }
    }
}

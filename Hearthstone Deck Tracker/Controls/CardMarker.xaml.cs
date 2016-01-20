#region

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using static System.Windows.Visibility;
using static Hearthstone_Deck_Tracker.Enums.CardMark;

#endregion

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for CardMarker.xaml
	/// </summary>
	public partial class CardMarker : UserControl
	{
		protected CardMark _mark;

		public CardMarker()
		{
			InitializeComponent();
		}

		public string Text
		{
			get { return CardAge.Text; }
			set { CardAge.Text = value; }
		}

		public CardMark Mark
		{
			get { return _mark; }
			set
			{
				_mark = value;
				var source = "";
				switch(_mark)
				{
					case Coin:
						source = "/HearthstoneDeckTracker;component/Images/card-icon-coin.png";
						break;
					case Kept:
						source = "/HearthstoneDeckTracker;component/Images/card-icon-keep.png";
						break;
					case Mulliganed:
						source = "/HearthstoneDeckTracker;component/Images/card-icon-mulligan.png";
						break;
					case Returned:
						source = "/HearthstoneDeckTracker;component/Images/card-icon-returned.png";
						break;
					case Created:
						source = "/HearthstoneDeckTracker;component/Images/card-icon-created.png";
						break;
					default:
						CardIcon.Visibility = Collapsed;
						break;
				}

				if(source != "")
				{
					CardIcon.Source = new BitmapImage(new Uri(source, UriKind.Relative));
					CardIcon.Visibility = Visible;
				}
				else
					CardIcon.Visibility = Collapsed;
			}
		}
	}
}
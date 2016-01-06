#region

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;

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

				if(source != "")
				{
					CardIcon.Source = new BitmapImage(new Uri(source, UriKind.Relative));
					CardIcon.Visibility = Visibility.Visible;
				}
				else
					CardIcon.Visibility = Visibility.Collapsed;
			}
		}
	}
}
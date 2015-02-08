#region

using System;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for DeckPickerClassItem.xaml
	/// </summary>
	public partial class DeckPickerClassItem
	{
		private const int Small = 36;
		private const int Big = 48;

		public DeckPickerClassItem()
		{
			InitializeComponent();
		}

		public BitmapImage ClassImage
		{
			get
			{
				var heroClass = DataContext as HeroClassAll?;
				if(heroClass == null)
					return new BitmapImage();
				var uri = new Uri(string.Format("../../Resources/ClassIcons/{0}.png", ((HeroClassAll)DataContext).ToString().ToLower()),
				                  UriKind.Relative);
				return new BitmapImage(uri);
			}
		}

		public void OnSelected()
		{
			ImageIcon.Width = Small;
			ImageIcon.Height = Small;
		}

		public void OnDelselected()
		{
			ImageIcon.Width = Big;
			ImageIcon.Height = Big;
		}
	}
}
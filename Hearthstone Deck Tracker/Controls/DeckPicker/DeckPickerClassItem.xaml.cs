#region

using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	/// <summary>
	/// Interaction logic for DeckPickerClassItem.xaml
	/// </summary>
	public partial class DeckPickerClassItem
	{
		public DeckPickerClassItem()
		{
			InitializeComponent();
		}

		public static int Small
		{
			get { return 24; }
		}

		public static int Big
		{
			get { return 36; }
		}

		public BitmapImage ClassImage
		{
			get
			{
				var heroClass = DataContext as HeroClassAll?;
				if(heroClass == null)
					return new BitmapImage();
				return ImageCache.GetClassIcon(heroClass.Value);
			}
		}

		public void OnSelected()
		{
		}

		public void OnDelselected()
		{
		}
	}
}
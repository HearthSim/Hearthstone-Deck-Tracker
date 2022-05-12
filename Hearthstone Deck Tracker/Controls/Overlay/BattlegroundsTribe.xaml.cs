using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsTribe : UserControl, INotifyPropertyChanged
	{
		public BattlegroundsTribe()
		{
			InitializeComponent();
		}

		private Dictionary<Race, string> TribeImages = new Dictionary<Race, string>() {
			{Race.PET , "pet"},
			{Race.MECHANICAL , "mech"},
			{Race.MURLOC , "murloc"},
			{Race.DEMON , "demon"},
			{Race.DRAGON , "dragon"},
			{Race.PIRATE , "pirate"},
			{Race.ELEMENTAL , "elemental"},
			{Race.QUILBOAR , "quilboar"},
		};

		private Dictionary<Race, string> TribeNames = new Dictionary<Race, string>() {
			{Race.PET , "Beast"},
			{Race.MECHANICAL , "Mech"},
			{Race.MURLOC , "Murloc"},
			{Race.DEMON , "Demon"},
			{Race.DRAGON , "Dragon"},
			{Race.PIRATE , "Pirate"},
			{Race.ELEMENTAL , "Elemental"},
			{Race.QUILBOAR , "Quilboar"},
		};

		public string ImageSrc => $"/HearthstoneDeckTracker;component/Resources/TribeIcons/{TribeImages[Tribe]}.jpg";

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty TribeProperty = DependencyProperty.Register("Tribe", typeof(Race), typeof(BattlegroundsTribe));

		public Race Tribe
		{
			get {
				var tribe = (Race)GetValue(TribeProperty);
				if(TribeImages.TryGetValue(tribe, out _))
					return tribe;
				return Race.PET;
			}
			set
			{
				SetValue(TribeProperty, value);
				OnPropertyChanged();
				OnPropertyChanged(nameof(ImageSrc));
			}
		}

		public string TribeName
		{
			get
			{
				string name;
				return TribeNames.TryGetValue(Tribe, out name) ? name : TribeNames[Race.PET];
			}
		}
	}
}

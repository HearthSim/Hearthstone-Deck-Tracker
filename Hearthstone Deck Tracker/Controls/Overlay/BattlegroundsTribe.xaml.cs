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

		private Dictionary<Race, string> TribeImage = new Dictionary<Race, string>() {
			{Race.PET , "pet"},
			{Race.MECHANICAL , "mech"},
			{Race.MURLOC , "murloc"},
			{Race.DEMON , "demon"},
			{Race.DRAGON , "dragon"},
			{Race.PIRATE , "pirate"},
			{Race.ELEMENTAL , "elemental"},
			{Race.QUILBOAR , "quilboar"},
		};

		public string ImageSrc => $"/HearthstoneDeckTracker;component/Resources/TribeIcons/{TribeImage[Tribe]}.jpg";

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
				if(TribeImage.TryGetValue(tribe, out _))
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
	}
}

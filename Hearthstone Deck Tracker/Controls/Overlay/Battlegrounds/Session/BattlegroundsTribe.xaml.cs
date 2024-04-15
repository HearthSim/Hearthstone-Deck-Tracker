using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;

public partial class BattlegroundsTribe : INotifyPropertyChanged
{
	public BattlegroundsTribe()
	{
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

#region Tribe

	private Dictionary<Race, string> TribeImages = new() {
		{Race.PET , "pet"},
		{Race.MECHANICAL , "mech"},
		{Race.MURLOC , "murloc"},
		{Race.DEMON , "demon"},
		{Race.DRAGON , "dragon"},
		{Race.PIRATE , "pirate"},
		{Race.ELEMENTAL , "elemental"},
		{Race.QUILBOAR , "quilboar"},
		{Race.NAGA , "naga"},
		{Race.UNDEAD , "undead"},
	};

	public string ImageSrc
	{
		get
		{
			return $"/HearthstoneDeckTracker;component/Resources/TribeIcons/{TribeImages[this.Tribe]}.jpg";
		}
	}

	public static readonly DependencyProperty TribeProperty = DependencyProperty.Register(
			"Tribe",
		typeof(Race),
		typeof(BattlegroundsTribe),
		new FrameworkPropertyMetadata(Race.PET, (d, _) => ((BattlegroundsTribe)d).OnTribeChanged())
	);

	public Race Tribe
	{
		get {
			var tribe = (Race)GetValue(TribeProperty);
			if (TribeImages.ContainsKey(tribe))
				return tribe;
			return Race.PET;
		}
		set
		{
			SetValue(TribeProperty, value);
			OnTribeChanged();
		}
	}

	public string TribeName => HearthDbConverter.GetLocalizedRace(Tribe) ?? "";

	private void OnTribeChanged()
	{
		OnPropertyChanged(nameof(TribeName));
		OnPropertyChanged(nameof(ImageSrc));
	}
	#endregion

	#region Availability
	public enum MinionTypeAvailability
	{
		Available,
		Banned,
	}

	public static readonly DependencyProperty AvailabilityProperty = DependencyProperty.Register(
		nameof(Availability),
		typeof(MinionTypeAvailability),
		typeof(BattlegroundsTribe),
		new FrameworkPropertyMetadata(MinionTypeAvailability.Available, (d, _) =>
		{
			((BattlegroundsTribe)d).OnPropertyChanged(nameof(XVisibility));
			((BattlegroundsTribe)d).OnPropertyChanged(nameof(BorderColor));
		})
	);

	public MinionTypeAvailability Availability
	{
		get => (MinionTypeAvailability)GetValue(AvailabilityProperty);
		set
		{
			SetValue(AvailabilityProperty, value);
			OnPropertyChanged(nameof(XVisibility));
			OnPropertyChanged(nameof(BorderColor));
		}
	}

	public Visibility XVisibility => Availability == MinionTypeAvailability.Banned ? Visibility.Visible : Visibility.Collapsed;

	public string BorderColor
	{
		get
		{
			return Availability switch
			{
				MinionTypeAvailability.Banned => "#D44040",
				MinionTypeAvailability.Available => "#16d220",
				_ => "#FF000000"
			};
		}
	}
	#endregion
}

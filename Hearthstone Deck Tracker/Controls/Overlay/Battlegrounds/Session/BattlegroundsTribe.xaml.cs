using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Assets;

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
		{Race.ABERRATION , "aberration"},
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
		OnDeityChanged();
	}
	#endregion

	#region Deity

	public static readonly DependencyProperty DeityProperty = DependencyProperty.Register(
		nameof(Deity),
		typeof(Hearthstone.Card),
		typeof(BattlegroundsTribe),
		new FrameworkPropertyMetadata(null, (d, _) => ((BattlegroundsTribe)d).OnDeityChanged())
	);

	public Hearthstone.Card? Deity
	{
		get => (Hearthstone.Card?)GetValue(DeityProperty);
		set => SetValue(DeityProperty, value);
	}

	// a banned Aberration type never gets a Deity, so it keeps the generic icon
	private bool ShowsDeity => Deity != null && Tribe == Race.ABERRATION && Availability == MinionTypeAvailability.Available;

	public CardAssetViewModel? DeityAsset => ShowsDeity ? new CardAssetViewModel(Deity, CardAssetType.Portrait) : null;

	public Visibility DeityVisibility => ShowsDeity ? Visibility.Visible : Visibility.Collapsed;

	public Visibility TribeIconVisibility => ShowsDeity ? Visibility.Collapsed : Visibility.Visible;

	private void OnDeityChanged()
	{
		OnPropertyChanged(nameof(DeityAsset));
		OnPropertyChanged(nameof(DeityVisibility));
		OnPropertyChanged(nameof(TribeIconVisibility));
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
			((BattlegroundsTribe)d).OnDeityChanged();
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
			OnDeityChanged();
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

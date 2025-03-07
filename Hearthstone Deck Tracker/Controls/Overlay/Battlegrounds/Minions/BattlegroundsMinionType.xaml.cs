using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsMinionType : INotifyPropertyChanged
{
	public BattlegroundsMinionType()
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
		{Race.INVALID, "other"},
		{(Race)(-1), "spell"},
		{(Race)(-2), "buddy"}
	};

	public string ImageSrc
	{
		get
		{
			return $"/HearthstoneDeckTracker;component/Resources/TribeIcons/{TribeImages[Tribe]}.jpg";
		}
	}

	public static readonly DependencyProperty TribeProperty = DependencyProperty.Register(
			"Tribe",
		typeof(Race),
		typeof(BattlegroundsMinionType),
		new FrameworkPropertyMetadata(Race.PET, (d, _) => ((BattlegroundsMinionType)d).OnTribeChanged())
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

	private void OnTribeChanged()
	{
		OnPropertyChanged(nameof(ImageSrc));
	}
	#endregion
}

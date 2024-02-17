using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsMinionTypesBox : UserControl, INotifyPropertyChanged
{
	public BattlegroundsMinionTypesBox()
	{
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public static DependencyProperty TitleProperty = DependencyProperty.Register(
		nameof(Title),
		typeof(string),
		typeof(BattlegroundsMinionTypesBox)
	);

	public string Title
	{
		get { return (string)GetValue(TitleProperty); }
		set { SetValue(TitleProperty, value); }
	}

	public static DependencyProperty MinionTypesProperty = DependencyProperty.Register(
		nameof(MinionTypes),
		typeof(IEnumerable<Race>),
		typeof(BattlegroundsMinionTypesBox),
		new PropertyMetadata(MinionTypesChanged)
	);

	public IEnumerable<Race> MinionTypes
	{
		get { return (IEnumerable<Race>?)GetValue(MinionTypesProperty) ?? new List<Race>(); }
		set { SetValue(MinionTypesProperty, value); }
	}

	private static void MinionTypesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var box = (BattlegroundsMinionTypesBox)d;
		box.OnPropertyChanged(nameof(MinionTypesText));
	}

	public string MinionTypesText => string.Join(", ", MinionTypes.Select(HearthDbConverter.GetLocalizedRace).OrderBy(x => x));
}

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;

public partial class CompGuide : INotifyPropertyChanged
{
	public event EventHandler? BackButtonClicked;

	private void Button_OnClick(object sender, RoutedEventArgs e)
	{
		BackButtonClicked?.Invoke(this, EventArgs.Empty);
	}

	public CompGuide()
	{
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void CardTooltip_Loaded(object sender, RoutedEventArgs e)
	{
		Core.Game.Metrics.BattlegroundsCompGuidesMinionHovers++;
	}
}

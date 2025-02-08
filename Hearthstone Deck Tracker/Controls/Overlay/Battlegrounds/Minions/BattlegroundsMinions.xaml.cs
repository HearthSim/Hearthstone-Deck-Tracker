﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Commands;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsMinions : UserControl
{
	public BattlegroundsMinions()
	{
		InitializeComponent();
		DataContextChanged += BattlegroundsMinions_DataContextChanged;
	}

	void BattlegroundsMinions_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if(e.OldValue is INotifyPropertyChanged old)
			old.PropertyChanged -= BattlegroundsMinionsViewModel_PropertyChanged;
		if(e.NewValue is INotifyPropertyChanged @new)
			@new.PropertyChanged += BattlegroundsMinionsViewModel_PropertyChanged;
	}

	void BattlegroundsMinionsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if(e.PropertyName == nameof(BattlegroundsMinionsViewModel.ActiveTier))
			MinionScrollViewer.ScrollToTop();
	}

	public ICommand SetActiveTierCommand => new Command<int>(value =>
	{
		((BattlegroundsMinionsViewModel)DataContext).ActiveTier = ((BattlegroundsMinionsViewModel)DataContext).ActiveTier == value ? null : value;
		Core.Game.Metrics.IncrementBattlegroundsMinionsTiersClick();
	});

	public ICommand SetActiveMinionTypeCommand => new Command<Race>(value =>
	{
		((BattlegroundsMinionsViewModel)DataContext).ActiveMinionType = value;
		Core.Game.Metrics.IncrementBattlegroundsMinionsByMinionTypeClick();
	});

	public static readonly DependencyProperty StandAloneProperty =
		DependencyProperty.Register("IsStandAloneMode", typeof(bool), typeof(BattlegroundsMinions),
			new PropertyMetadata(false));

	public bool IsStandAloneMode
	{
		get { return (bool)GetValue(StandAloneProperty); }
		set { SetValue(StandAloneProperty, value); }
	}
}

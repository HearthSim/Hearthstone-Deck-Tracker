using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Tooltips;

public partial class PoolSummaryView : UserControl
{
	public PoolSummaryView()
	{
		InitializeComponent();
	}

	public static readonly DependencyProperty PoolStatisticsProperty = DependencyProperty.Register(
		nameof(PoolStatistics), typeof(PoolStatistics), typeof(PoolSummaryView));

	public PoolStatistics? PoolStatistics
	{
		get => (PoolStatistics?)GetValue(PoolStatisticsProperty);
		set => SetValue(PoolStatisticsProperty, value);
	}

	public static readonly DependencyProperty RelatedCardsSummaryTotalNumProperty = DependencyProperty.Register(
		nameof(RelatedCardsSummaryTotalNum), typeof(int), typeof(PoolSummaryView),
		new PropertyMetadata(0, OnRelatedCardsSummaryTotalNumChanged));

	public int RelatedCardsSummaryTotalNum
	{
		get => (int)GetValue(RelatedCardsSummaryTotalNumProperty);
		set => SetValue(RelatedCardsSummaryTotalNumProperty, value);
	}

	private static void OnRelatedCardsSummaryTotalNumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((PoolSummaryView)d).PoolOfCardsText = string.Format(LocUtil.Get("TheOutfinder_Label_PoolOfCards"), (int)e.NewValue);

	public static readonly DependencyProperty PoolOfCardsTextProperty = DependencyProperty.Register(
		nameof(PoolOfCardsText), typeof(string), typeof(PoolSummaryView), new PropertyMetadata(string.Empty));

	public string PoolOfCardsText
	{
		get => (string)GetValue(PoolOfCardsTextProperty);
		private set => SetValue(PoolOfCardsTextProperty, value);
	}

	public static readonly DependencyProperty RelatedCardsSummaryProperty = DependencyProperty.Register(
		nameof(RelatedCardsSummary), typeof(Dictionary<string, string>), typeof(PoolSummaryView));

	public Dictionary<string, string>? RelatedCardsSummary
	{
		get => (Dictionary<string, string>?)GetValue(RelatedCardsSummaryProperty);
		set => SetValue(RelatedCardsSummaryProperty, value);
	}

	public static readonly DependencyProperty HasLargePoolProperty = DependencyProperty.Register(
		nameof(HasLargePool), typeof(bool), typeof(PoolSummaryView));

	public bool HasLargePool
	{
		get => (bool)GetValue(HasLargePoolProperty);
		set => SetValue(HasLargePoolProperty, value);
	}

	public static readonly DependencyProperty KeywordsMaxHeightProperty = DependencyProperty.Register(
		nameof(KeywordsMaxHeight), typeof(double), typeof(PoolSummaryView), new PropertyMetadata(double.PositiveInfinity));

	public double KeywordsMaxHeight
	{
		get => (double)GetValue(KeywordsMaxHeightProperty);
		set => SetValue(KeywordsMaxHeightProperty, value);
	}

	public static readonly DependencyProperty RootBorderStyleProperty = DependencyProperty.Register(
		nameof(RootBorderStyle), typeof(Style), typeof(PoolSummaryView));

	public Style? RootBorderStyle
	{
		get => (Style?)GetValue(RootBorderStyleProperty);
		set => SetValue(RootBorderStyleProperty, value);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Themes;

namespace Hearthstone_Deck_Tracker.Utility;

public class CardListHelper
{
	public static readonly DependencyProperty AutoScaleCardTilesProperty = DependencyProperty.RegisterAttached(
		"AutoScaleCardTiles", typeof(bool), typeof(CardListHelper), new PropertyMetadata(false, OnAutoScaleChanged));

	public static void SetAutoScaleCardTiles(DependencyObject element, bool value)
	{
		element.SetValue(AutoScaleCardTilesProperty, value);
	}

	public static bool GetAutoScaleCardTiles(DependencyObject element)
	{
		return (bool)element.GetValue(AutoScaleCardTilesProperty);
	}

	private static void OnAutoScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
#if DEBUG
		if(d is AnimatedCardList)
			throw new ArgumentException("AutoScaleCardTiles cannot be set on the AnimatedCardList directly, but instead must be set on a parent container.");
#endif
		if(d is not FrameworkElement element)
			return;
		if(e.NewValue is true)
		{
			element.Loaded += OnLoaded;
			element.Unloaded += OnUnloaded;
			element.SizeChanged += OnSizeChanged;
			element.IsVisibleChanged += OnIsVisibleChanged;
			if(element.IsLoaded)
				OnLoaded(d, new RoutedEventArgs());
		}
		else
		{
			element.Loaded -= OnLoaded;
			element.Unloaded -= OnUnloaded;
			element.SizeChanged -= OnSizeChanged;
			element.IsVisibleChanged -= OnIsVisibleChanged;
			OnUnloaded(d, new RoutedEventArgs());
		}
	}

	private static void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if(sender is not FrameworkElement container || e.NewValue is false)
			return;
		Update(container);
	}

	private static void OnCardListSizeChanged(AnimatedCardList cardList)
	{
		var entry = _entries.FirstOrDefault(c => c.CardsLists.Contains(cardList));
		if(entry is not null)
			Update(entry.Container);
	}

	/// <summary>
	/// The way these components usually work is that AnimatedCardLists and various other
	/// things are in a StackPanel. This StackPanel is however big it is and the height of
	/// the content displayed to the user is controlled by the parent. This could be a
	/// container element, or the window itself.
	/// </summary>
	private static void OnParentSizeChanged(object sender, SizeChangedEventArgs e)
	{
		if(sender is not FrameworkElement parent)
			return;
		var entry = _entries.FirstOrDefault(c => c.Container.Parent == parent);
		if(entry is not null)
			Update(entry.Container);
	}

	/// <summary>
	/// This mostly responds to changes to components internal to the card list stack panel.
	/// E.g. Card counters being shown or hidden
	/// </summary>
	private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		if(sender is not FrameworkElement container)
			return;
		Update(container);
	}

	private static void OnLoaded(object sender, RoutedEventArgs e)
	{
		if(sender is not FrameworkElement container)
			return;
		if(_entries.Any(c => c.Container == container))
			return;
		var entry = new Entry(container, new List<AnimatedCardList>());
		_entries.Add(entry);
		AddEventHandlers(entry);
		Update(container);
	}

	private static void AddEventHandlers(Entry entry)
	{
		if(entry.Container.Parent is FrameworkElement parent)
			parent.SizeChanged += OnParentSizeChanged;
		var cardLists = Helper.FindVisualChildren<AnimatedCardList>(entry.Container).ToList();
		foreach(var cardList in cardLists)
			cardList.CardListSizeChanged += OnCardListSizeChanged;
		entry.CardsLists.AddRange(cardLists);
	}

	private static void RemoveEventHandlers(Entry entry)
	{
		if(entry.Container.Parent is FrameworkElement parent)
			parent.SizeChanged -= OnParentSizeChanged;
		foreach(var cardList in entry.CardsLists)
			cardList.CardListSizeChanged -= OnCardListSizeChanged;
		entry.CardsLists.Clear();
	}

	private static void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if(sender is not FrameworkElement element)
			return;
		var entry = _entries.FirstOrDefault(c => c.Container == element);
		if(entry is not null)
		{
			RemoveEventHandlers(entry);
			_entries.Remove(entry);
		}
	}

	private record Entry(FrameworkElement Container, List<AnimatedCardList> CardsLists);
	private static readonly List<Entry> _entries = new();

	static CardListHelper()
	{
		ThemeManager.ThemeChanged += () =>
		{
			foreach(var entry in _entries)
				Update(entry.Container);
		};
	}

	// These could potentially be made configurable in the future.
	private const int MinCardHeight = 24;
	private const int MaxCardHeight = 34;

	private static void Update(FrameworkElement container)
	{
#if DEBUG
		// This only works correctly if update is called with the correct container element.
		if(container.ReadLocalValue(AutoScaleCardTilesProperty) == DependencyProperty.UnsetValue)
			throw new ArgumentException("Must be called with the container that sets AutoScaleCardTiles property.");
#endif
		if(!container.IsVisible || !container.IsLoaded || container.ActualHeight <= 0 || container.ActualWidth <= 0 || !GetAutoScaleCardTiles(container))
			return;

		// AnimatedCardLists that are immediate children of the container. All of these will receive a max height
		// to allow them to be scrollable. (e.g. the player or opponent deck)
		var topLevelLists = new List<AnimatedCardList>();
		var topLevelCardScaleSum = 0.0; // Functionally the card count, but taking into account the scale of the cards.
		var topLevelCardMargins = 0.0;

		// AnimatedCardLists that are nested in other components within the container. The card scaling will be applied
		// to these, but they will never be scrollable. (e.g. sideboards)
		var nestedLists = new List<AnimatedCardList>();
		var nestedCardScaleSum = 0.0; // Functionally the card count, but taking into account the scale of the cards.
		var nestedCardMargins = 0.0;

		// Any elements of fixed height. E.g. card counters, headers, etc. These remain unscaled and will be excluded
		// from calculations to determine the max height of cards.
		var fixedHeight = 0.0;

		var entry = _entries.FirstOrDefault(c => c.Container == container);
		var childrenCount = VisualTreeHelper.GetChildrenCount(container);
		for(var i = 0; i < childrenCount; i++)
		{
			var child = VisualTreeHelper.GetChild(container, i) as FrameworkElement;
			if(child == null || !child.IsLoaded || child.Visibility == Visibility.Collapsed)
				continue;
			if(child is AnimatedCardList cardList)
			{
				foreach(var card in cardList.AnimatedCards)
				{
					if(card.IsLoaded)
						topLevelCardScaleSum += ((ScaleTransform)card.LayoutTransform).ScaleY;
					topLevelCardMargins += card.Margin.Top + card.Margin.Bottom;
				}
				topLevelLists.Add(cardList);
				if(entry != null && !entry.CardsLists.Contains(cardList))
				{
					// Looks like the AnimatedCardLists in this entry have changed. Update them.
					RemoveEventHandlers(entry);
					AddEventHandlers(entry);
				}
			}
			else
			{
				var nestedCardLists = Helper.FindVisualChildren<AnimatedCardList>(child).ToList();
				if(nestedCardLists.Count == 0)
					fixedHeight += GetHeight(child);
				else
				{
					var childFixedHeight = GetHeight(child);
					foreach(var nestedCardList in nestedCardLists)
					{
						foreach(var card in nestedCardList.AnimatedCards)
						{
							if(card.IsLoaded)
								nestedCardScaleSum += ((ScaleTransform)card.LayoutTransform).ScaleY;
							nestedCardMargins += card.Margin.Top + card.Margin.Bottom;
						}
						childFixedHeight -= GetHeight(nestedCardList);
						nestedLists.Add(nestedCardList);

						if(entry != null && !entry.CardsLists.Contains(nestedCardList))
						{
							// Looks like the AnimatedCardLists in this entry have changed. Update them.
							// This happens e.g. when changing filters in BattlegroundsMinions
							RemoveEventHandlers(entry);
							AddEventHandlers(entry);
						}
					}
					fixedHeight += Math.Max(0, childFixedHeight);
				}
			}
		}

		if(topLevelCardScaleSum <= 0 && nestedCardScaleSum <= 0)
			return;

		var availableHeight = GetHeight(container.Parent as FrameworkElement) - fixedHeight - topLevelCardMargins - nestedCardMargins;

		var cardHeight = Math.Max(MinCardHeight, Math.Min(MaxCardHeight, availableHeight / (topLevelCardScaleSum + nestedCardScaleSum)));
		var topLevelListMaxHeight = (availableHeight + topLevelCardMargins - nestedCardScaleSum * cardHeight) / topLevelLists.Count;

		foreach(var acl in topLevelLists)
		{
			if(topLevelListMaxHeight > 0)
				acl.MaxHeight = topLevelListMaxHeight;
			acl.ViewModel.MaxHeightCard = cardHeight;
		}

		foreach(var acl in nestedLists)
			acl.ViewModel.MaxHeightCard = cardHeight;
	}

	private static double GetHeight(FrameworkElement? e)
	{
		if(e == null || !e.IsVisible || !e.IsLoaded)
			return 0;
		return e.ActualHeight + e.Margin.Top + e.Margin.Bottom;
	}
}

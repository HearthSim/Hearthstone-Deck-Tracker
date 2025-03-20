using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;

namespace Hearthstone_Deck_Tracker.Controls.Overlay;

public partial class CountersOverlay
{
	public bool IsPlayer { get; set; }
	private readonly CounterManager _counters;

	public static int InnerMargin => 5;

	public CountersOverlay()
	{
		_counters = Core.Game.CounterManager;
		_counters.CountersChanged += CountersChanged;
		InitializeComponent();
		UpdateVisibleCounters();
	}

	private void CountersChanged(object sender, EventArgs e)
	{
		UpdateVisibleCounters();
	}

	public ObservableCollection<BaseCounter> VisibleCounters { get; } = new();

	public void UpdateVisibleCounters()
	{
		var visibleCounters = _counters.GetVisibleCounters(IsPlayer);

		foreach(var counter in VisibleCounters.ToList())
		{
			if(!visibleCounters.Contains(counter))
			{
				VisibleCounters.Remove(counter);
				OnPropertyChanged(nameof(VisibleCounters));
			}
		}

		foreach(var counter in visibleCounters)
		{
			if(!VisibleCounters.Contains(counter))
			{
				VisibleCounters.Add(counter);
				OnPropertyChanged(nameof(VisibleCounters));
			}
		}
	}

	public void ForceShowExampleCounters()
	{
		VisibleCounters.Clear();
		var exampleCounters = _counters.GetExampleCounters(IsPlayer);

		foreach(var counter in exampleCounters)
		{
			VisibleCounters.Add(counter);
		}

		OnPropertyChanged(nameof(VisibleCounters));
	}

	public void ForceHideExampleCounters()
	{
		VisibleCounters.Clear();
		UpdateVisibleCounters();
	}

	private readonly List<double> _elementWidths = new List<double>();

	private void Element_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		UpdateWidthsInOrder();
	}

	private void UpdateWidthsInOrder()
	{
		_elementWidths.Clear();

		for (int i = 0; i < CountersItemsControl.Items.Count; i++)
		{
			var container = CountersItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;

			if (container != null)
			{
				var border = FindVisualChild<Border>(container);
				if (border != null)
				{
					_elementWidths.Add(border.ActualWidth);
				}
			}
		}
	}

	private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			if (child is T tChild)
			{
				return tChild;
			}

			T? childOfChild = FindVisualChild<T>(child);
			if (childOfChild != null)
			{
				return childOfChild;
			}
		}

		return null;
	}

	public List<double> GetWidths()
	{
		return _elementWidths;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

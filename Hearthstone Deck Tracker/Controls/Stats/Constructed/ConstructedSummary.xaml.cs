#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Constructed
{
	/// <summary>
	/// Interaction logic for ConstructedSummary.xaml
	/// </summary>
	public partial class ConstructedSummary : INotifyPropertyChanged
	{
		public ConstructedSummary()
		{
			InitializeComponent();
			UpdateContent();
		}

		private readonly ConstructedMatchupTable _constructedMatchupTable = new ConstructedMatchupTable();
		private readonly ConstructedDeckDetailsTable _constructedDeckDetailsTable = new ConstructedDeckDetailsTable();

		//http://stackoverflow.com/questions/3498686/wpf-remove-scrollviewer-from-treeview
		private void ForwardScrollEvent(object sender, MouseWheelEventArgs e)
		{
			if(e.Handled)
				return;
			e.Handled = true;
			var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {RoutedEvent = MouseWheelEvent, Source = sender};
			var parent = ((Control)sender).Parent as UIElement;
			parent?.RaiseEvent(eventArg);
		}

		private void CheckBoxPercent_OnCheckedChanged(object sender, RoutedEventArgs e)
		{
			ConstructedStats.Instance.UpdateMatchups();
		}

		public string GroupBoxMatchupsHeader
			=> Config.Instance.ConstructedStatsActiveDeckOnly ? DeckList.Instance.ActiveDeck?.Name.ToUpper() : "MATCHUPS";

		internal void UpdateContent()
		{
			if(Config.Instance.ConstructedStatsActiveDeckOnly)
			{
				ContentControlMatchups.Content = _constructedDeckDetailsTable;
				ConstructedStats.Instance.UpdateConstructedStats();
			}
			else
				ContentControlMatchups.Content = _constructedMatchupTable;
			OnPropertyChanged(nameof(GroupBoxMatchupsHeader));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

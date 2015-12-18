#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Stats.Arena.Charts;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	/// <summary>
	/// Interaction logic for ArenaRuns.xaml
	/// </summary>
	public partial class ArenaRuns : INotifyPropertyChanged
	{
		private readonly bool _initialized;
		private object _chartWinsControl = new ChartWins();

		public ArenaRuns()
		{
			InitializeComponent();
			_initialized = true;
		}

		public object ChartWinsControl
		{
			get { return _chartWinsControl; }
			set
			{
				_chartWinsControl = value;
				OnPropertyChanged();
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
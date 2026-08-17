using System;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class TurnCounter : UserControl, INotifyPropertyChanged
	{
		private readonly LocalizedPropNotifier _localizedPropNotifier;
		private int _turn = 1;

		public TurnCounter()
		{
			InitializeComponent();
			_localizedPropNotifier = new LocalizedPropNotifier(GetType(), OnPropertyChanged);
		}

		[LocalizedProp]
		public string TurnText => string.Format(LocUtil.Get("Overlay_Battlegrounds_Turn_Counter"), _turn);

		internal void UpdateTurn(int turn)
		{
			_turn = Math.Max(turn, 1);
			OnPropertyChanged(nameof(TurnText));
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

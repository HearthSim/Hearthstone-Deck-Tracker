using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public sealed partial class BattlegroundsOpponentInfo : INotifyPropertyChanged
	{
		private Dictionary<int, int> _heroTriples = new();
		private Dictionary<int, int> _heroTierTurns = new();

		public BattlegroundsOpponentInfo()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public int TriplesTier1 => _heroTriples.TryGetValue(1, out var qty) ? qty : 0;
		public int TriplesTier2 => _heroTriples.TryGetValue(2, out var qty) ? qty : 0;
		public int TriplesTier3 => _heroTriples.TryGetValue(3, out var qty) ? qty : 0;
		public int TriplesTier4 => _heroTriples.TryGetValue(4, out var qty) ? qty : 0;
		public int TriplesTier5 => _heroTriples.TryGetValue(5, out var qty) ? qty : 0;
		public int TriplesTier6 => _heroTriples.TryGetValue(6, out var qty) ? qty : 0;

		public int TurnTier2 => _heroTierTurns.TryGetValue(2, out var turn) ? turn : 0;
		public int TurnTier3 => _heroTierTurns.TryGetValue(3, out var turn) ? turn : 0;
		public int TurnTier4 => _heroTierTurns.TryGetValue(4, out var turn) ? turn : 0;
		public int TurnTier5 => _heroTierTurns.TryGetValue(5, out var turn) ? turn : 0;
		public int TurnTier6 => _heroTierTurns.TryGetValue(6, out var turn) ? turn : 0;

		public void ShowNotFoughtOpponent()
		{
			BattlegroundsBoard.Children.Clear();
			NotFoughtOpponent.Visibility = Visibility.Visible;
			HeroNoMinionsOnBoard.Visibility = Visibility.Collapsed;
			TiersInfo.Visibility = Visibility.Collapsed;
		}

		public void ClearLastKnownBoard()
		{
			BattlegroundsBoard.Children.Clear();
		}

		public void Update(Entity hero, BoardSnapshot? state, int turnNumber)
		{
			BattlegroundsBoard.Children.Clear();
			NotFoughtOpponent.Visibility = Visibility.Collapsed;
			HeroNoMinionsOnBoard.Visibility = Visibility.Collapsed;
			TiersInfo.Visibility = Visibility.Visible;
			if(state == null)
			{
				BattlegroundsAge.Text = "";
				NotFoughtOpponent.Visibility = Visibility.Visible;
			} else
			{
				foreach(var e in state.Entities)
					BattlegroundsBoard.Children.Add(new BattlegroundsMinion(e));
				if(!state.Entities.Any())
					HeroNoMinionsOnBoard.Visibility = Visibility.Visible;
				var age = turnNumber - state.Turn;
				BattlegroundsAge.Text = string.Format(LocUtil.Get("Overlay_Battlegrounds_Turns"), age);
			}

			var heroTriples = Core.Game.GetBattlegroundsHeroTriplesByTier(hero.Id);
			_heroTriples = heroTriples ?? new Dictionary<int, int>();
			OnPropertyChanged(nameof(TriplesTier1));
			OnPropertyChanged(nameof(TriplesTier2));
			OnPropertyChanged(nameof(TriplesTier3));
			OnPropertyChanged(nameof(TriplesTier4));
			OnPropertyChanged(nameof(TriplesTier5));
			OnPropertyChanged(nameof(TriplesTier6));


			var heroTavernUpTurn = Core.Game.GetBattlegroundsHeroLatestTavernUpTurn(hero.Id);
			_heroTierTurns = heroTavernUpTurn ?? new Dictionary<int, int>();
			OnPropertyChanged(nameof(TurnTier2));
			OnPropertyChanged(nameof(TurnTier3));
			OnPropertyChanged(nameof(TurnTier4));
			OnPropertyChanged(nameof(TurnTier5));
			OnPropertyChanged(nameof(TurnTier6));
		}
	}
}


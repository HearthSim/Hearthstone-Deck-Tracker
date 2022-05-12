using System.Windows.Controls;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsGameView : UserControl
	{
		public BattlegroundsGameView()
		{
			InitializeComponent();
		}

		public SolidColorBrush PlacementTextBrush
		{
			get
			{
				if(DataContext == null)
					return new SolidColorBrush(Colors.White);
				BattlegroundsGameViewModel viewModel = (BattlegroundsGameViewModel)DataContext;
				return new SolidColorBrush(viewModel.Placement <= 4 ? Color.FromRgb(109, 235, 108) : Color.FromRgb(236, 105, 105));
			}
		}

		public SolidColorBrush MMRDeltaTextBrush
		{
			get
			{
				var mmrDelta = DataContext != null ? ((BattlegroundsGameViewModel)DataContext).MMRDelta : 0;
				if(mmrDelta == 0)
					return new SolidColorBrush(Colors.White);
				return new SolidColorBrush(mmrDelta > 0 ? Color.FromRgb(139, 210, 134) : Color.FromRgb(236, 105, 105));
			}
		}
	}
}

using System.Windows.Controls;
using System.Windows.Input;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;

public partial class ConstructedMulliganGuidePreLobby : UserControl
{
	public ConstructedMulliganGuidePreLobby()
	{
		InitializeComponent();
	}

	private void ViewMetaDecks_MouseUp(object sender, MouseButtonEventArgs e)
	{
		if(DataContext is ConstructedMulliganGuidePreLobbyViewModel vm)
		{
			var fragments = vm.FormatType switch
			{
				FormatType.FT_UNKNOWN => new string[] { "gameType=UNKNOWN"},
				FormatType.FT_WILD => new string[] { "gameType=RANKED_WILD" },
				FormatType.FT_STANDARD => new string[] {},
				FormatType.FT_CLASSIC => new string[] { "gameType=CLASSIC" },
				FormatType.FT_TWIST => new string[] { "gameType=TWIST" },
				_ => new string[] {}
			};

			var url = Helper.BuildHsReplayNetUrl("decks", "constructed_lobby_view_meta_decks", fragmentParams: fragments);
			Helper.TryOpenUrl(url);
		}

	}

}

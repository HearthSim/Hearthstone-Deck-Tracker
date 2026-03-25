using System;
using System.Collections.Generic;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan.V2;

public class ConstructedMulliganV2SingleCardViewModel : ViewModel
{
	public ConstructedMulliganV2SingleCardHeaderViewModel CardHeaderVM { get;  }

	public ConstructedMulliganV2SingleCardViewModel(
		int position,
		MulliganCard data,
		MulliganState? mulliganState = null
	)
	{
		CardHeaderVM = new(position, data, mulliganState);
	}
}

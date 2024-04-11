using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;

public class OverlayMessageViewModel : ViewModel
{
	public string? Text
	{
		get => GetProp<string?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public Visibility Visibility => Text == null ? Collapsed : Visible;

	public async void Error()
	{
		var errorText = LocUtil.Get("ConstructedMulliganGuide_Message_Error");
		Text = errorText;

		await Task.Delay(5000);
		// Only clear if no other text was set in the meantime
		if(Text == errorText)
		{
			Clear();
		}
	}

	public enum PlayerInitiative
	{
		FIRST,
		COIN,
	}

	public void Scope(CardClass cardClass, PlayerInitiative initiative)
	{
		var localizedCardClass = LocUtil.Get(HearthDbConverter.ConvertClass(cardClass) ?? "Unavailable");

		if(initiative == PlayerInitiative.FIRST)
		{
			Text = string.Format(LocUtil.Get("ConstructedMulliganGuide_Message_VsClass_GoingFirst"), localizedCardClass);
		}
		else
		{
			Text = string.Format(LocUtil.Get("ConstructedMulliganGuide_Message_VsClass_ExtraCard"), localizedCardClass);
		}
	}

	public void Clear()
	{
		Text = null;
	}
}

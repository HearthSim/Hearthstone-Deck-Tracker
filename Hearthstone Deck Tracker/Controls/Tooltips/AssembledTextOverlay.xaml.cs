using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Tooltips;

/// <summary>
/// The text of an assembled card, drawn over the description box of the base card's minion render.
/// Placed on top of the image, it scales with it.
/// </summary>
public partial class AssembledTextOverlay : UserControl
{
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
		nameof(Text), typeof(string), typeof(AssembledTextOverlay), new PropertyMetadata(null));

	public static readonly DependencyProperty HasRaceBannerProperty = DependencyProperty.Register(
		nameof(HasRaceBanner), typeof(bool), typeof(AssembledTextOverlay), new PropertyMetadata(false));

	public AssembledTextOverlay()
	{
		InitializeComponent();
	}

	/// <summary>
	/// The text, with bold and italic tags. Nothing is drawn when null.
	/// </summary>
	public string? Text
	{
		get => (string?)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	/// <summary>
	/// Whether the render has a minion type banner, which the text leaves showing.
	/// </summary>
	public bool HasRaceBanner
	{
		get => (bool)GetValue(HasRaceBannerProperty);
		set => SetValue(HasRaceBannerProperty, value);
	}
}

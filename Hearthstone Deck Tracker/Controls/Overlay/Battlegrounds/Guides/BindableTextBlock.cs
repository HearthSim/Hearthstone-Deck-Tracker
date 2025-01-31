using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

public static class TextBlockExtensions
{
	public static IEnumerable<Inline>? GetBindableInlines ( DependencyObject obj )
	{
		return (IEnumerable<Inline>?) obj.GetValue ( BindableInlinesProperty );
	}

	public static void SetBindableInlines ( DependencyObject obj, IEnumerable<Inline>? value )
	{
		obj.SetValue ( BindableInlinesProperty, value );
	}

	public static readonly DependencyProperty BindableInlinesProperty =
		DependencyProperty.RegisterAttached ( "BindableInlines", typeof ( IEnumerable<Inline> ), typeof ( TextBlockExtensions ), new PropertyMetadata ( null, OnBindableInlinesChanged ) );

	private static void OnBindableInlinesChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
	{
		var Target = d as TextBlock;

		if ( Target != null )
		{
			Target.Inlines.Clear ();
			if(e.NewValue != null)
				Target.Inlines.AddRange ( (System.Collections.IEnumerable) e.NewValue );
		}
	}
}

#region

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

#endregion

namespace Hearthstone_Deck_Tracker
{
	[ContentProperty("Text")]
	public class OutlinedTextBlock : FrameworkElement
	{
		public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(OutlinedTextBlock),
		                                                                                     new FrameworkPropertyMetadata(Brushes.White,
		                                                                                                                   FrameworkPropertyMetadataOptions
			                                                                                                                   .AffectsRender));

		public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush),
		                                                                                       typeof(OutlinedTextBlock),
		                                                                                       new FrameworkPropertyMetadata(Brushes.Black,
		                                                                                                                     FrameworkPropertyMetadataOptions
			                                                                                                                     .AffectsRender));

		public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(OutlinedTextBlock),
		                                                                                                       new FrameworkPropertyMetadata(
			                                                                                                       OnFormattedTextUpdated));

		public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(OutlinedTextBlock),
		                                                                                                   new FrameworkPropertyMetadata(
			                                                                                                   OnFormattedTextUpdated));

		public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(OutlinedTextBlock),
		                                                                                                         new FrameworkPropertyMetadata
			                                                                                                         (OnFormattedTextUpdated));

		public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(OutlinedTextBlock),
		                                                                                                     new FrameworkPropertyMetadata(
			                                                                                                     OnFormattedTextUpdated));

		public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(OutlinedTextBlock),
		                                                                                                       new FrameworkPropertyMetadata(
			                                                                                                       OnFormattedTextUpdated));

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
		                                                                                     typeof(OutlinedTextBlock),
		                                                                                     new FrameworkPropertyMetadata(
			                                                                                     OnFormattedTextInvalidated));

		public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment),
		                                                                                              typeof(OutlinedTextBlock),
		                                                                                              new FrameworkPropertyMetadata(
			                                                                                              OnFormattedTextUpdated));

		public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations",
		                                                                                                typeof(TextDecorationCollection),
		                                                                                                typeof(OutlinedTextBlock),
		                                                                                                new FrameworkPropertyMetadata(
			                                                                                                OnFormattedTextUpdated));

		public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register("TextTrimming", typeof(TextTrimming),
		                                                                                             typeof(OutlinedTextBlock),
		                                                                                             new FrameworkPropertyMetadata(
			                                                                                             OnFormattedTextUpdated));

		public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping),
		                                                                                             typeof(OutlinedTextBlock),
		                                                                                             new FrameworkPropertyMetadata(
			                                                                                             TextWrapping.Wrap,
			                                                                                             OnFormattedTextUpdated));

		private FormattedText _formattedText;

		public OutlinedTextBlock()
		{
			TextDecorations = new TextDecorationCollection();
		}

		public Brush Fill
		{
			get { return (Brush)GetValue(FillProperty); }
			set { SetValue(FillProperty, value); }
		}

		public FontFamily FontFamily
		{
			get { return (FontFamily)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		[TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public FontStretch FontStretch
		{
			get { return (FontStretch)GetValue(FontStretchProperty); }
			set { SetValue(FontStretchProperty, value); }
		}

		public FontStyle FontStyle
		{
			get { return (FontStyle)GetValue(FontStyleProperty); }
			set { SetValue(FontStyleProperty, value); }
		}

		public FontWeight FontWeight
		{
			get { return (FontWeight)GetValue(FontWeightProperty); }
			set { SetValue(FontWeightProperty, value); }
		}

		public Brush Stroke
		{
			get { return (Brush)GetValue(StrokeProperty); }
			set { SetValue(StrokeProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public TextAlignment TextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentProperty); }
			set { SetValue(TextAlignmentProperty, value); }
		}

		public TextDecorationCollection TextDecorations
		{
			get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
			set { SetValue(TextDecorationsProperty, value); }
		}

		public TextTrimming TextTrimming
		{
			get { return (TextTrimming)GetValue(TextTrimmingProperty); }
			set { SetValue(TextTrimmingProperty, value); }
		}

		public TextWrapping TextWrapping
		{
			get { return (TextWrapping)GetValue(TextWrappingProperty); }
			set { SetValue(TextWrappingProperty, value); }
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			EnsureFormattedText();

			if(_formattedText == null)
				return;
			var y = !double.IsNaN(ActualHeight) ? (ActualHeight - _formattedText.Height) / 2 + _formattedText.Height * 0.05: 0;
			drawingContext.DrawGeometry(Stroke, new Pen(Brushes.Black, 2.0) {LineJoin = PenLineJoin.Round}, _formattedText.BuildGeometry(new Point(0, y)));
			drawingContext.DrawGeometry(Fill, new Pen(Brushes.White, 0), _formattedText.BuildGeometry(new Point(0, y)));
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			EnsureFormattedText();

			if(_formattedText == null)
				return new Size(0, 0);
			// constrain the formatted text according to the available size
			// the Math.Min call is important - without this constraint (which seems arbitrary, but is the maximum allowable text width), things blow up when availableSize is infinite in both directions
			// the Math.Max call is to ensure we don't hit zero, which will cause MaxTextHeight to throw
			var maxWidth = Math.Min(3579139, Math.Max(0.0001d, availableSize.Width));
			var ratio = maxWidth / _formattedText.Width;
			if(ratio < 1 && (TextWrapping == TextWrapping.NoWrap || ratio > 0.8))
				_formattedText.SetFontSize((int)(FontSize * ratio));
			_formattedText.MaxTextWidth = maxWidth;
			_formattedText.MaxTextHeight = Math.Max(0.0001d, availableSize.Height);

			// return the desired size
			return new Size(_formattedText.Width, _formattedText.Height + 2);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			EnsureFormattedText();

			if(_formattedText == null)
				return new Size(0, 0);
			// update the formatted text with the final size
			_formattedText.MaxTextWidth = Math.Min(3579139, Math.Max(0.0001d, finalSize.Width));
			_formattedText.MaxTextHeight = Math.Max(0.0001d, finalSize.Height);

			return finalSize;
		}

		private static void OnFormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
			outlinedTextBlock._formattedText = null;

			outlinedTextBlock.InvalidateMeasure();
			outlinedTextBlock.InvalidateVisual();
		}

		private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
			outlinedTextBlock.UpdateFormattedText();

			outlinedTextBlock.InvalidateMeasure();
			outlinedTextBlock.InvalidateVisual();
		}

		private void EnsureFormattedText()
		{
			if(_formattedText != null || Text == null)
				return;

			_formattedText = new FormattedText(Text, CultureInfo.CurrentUICulture, FlowDirection,
			                                   new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Condensed), FontSize, Brushes.Black, 
											   null, TextFormattingMode.Ideal);

			UpdateFormattedText();
		}

		private void UpdateFormattedText()
		{
			if(_formattedText == null)
				return;

			_formattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
			_formattedText.TextAlignment = TextAlignment;
			_formattedText.Trimming = TextTrimming;

			_formattedText.SetFontSize(FontSize);
			_formattedText.SetFontStyle(FontStyle);
			_formattedText.SetFontWeight(FontWeight);
			_formattedText.SetFontFamily(FontFamily);
			_formattedText.SetFontStretch(FontStretch);
			_formattedText.SetTextDecorations(TextDecorations);
		}
	}
}

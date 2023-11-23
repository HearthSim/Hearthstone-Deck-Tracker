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
	public class MultiLineTextBlock : FrameworkElement
	{
		public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(MultiLineTextBlock),
		                                                                                     new FrameworkPropertyMetadata(Brushes.White,
		                                                                                                                   FrameworkPropertyMetadataOptions
			                                                                                                                   .AffectsRender));

		public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(MultiLineTextBlock),
		                                                                                                       new FrameworkPropertyMetadata(
			                                                                                                       OnFormattedTextUpdated));

		public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(MultiLineTextBlock),
		                                                                                                   new FrameworkPropertyMetadata(
			                                                                                                   OnFormattedTextUpdated));

		public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(MultiLineTextBlock),
		                                                                                                         new FrameworkPropertyMetadata
			                                                                                                         (OnFormattedTextUpdated));

		public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(MultiLineTextBlock),
		                                                                                                     new FrameworkPropertyMetadata(
			                                                                                                     OnFormattedTextUpdated));

		public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(MultiLineTextBlock),
		                                                                                                       new FrameworkPropertyMetadata(
			                                                                                                       OnFormattedTextUpdated));

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
		                                                                                     typeof(MultiLineTextBlock),
		                                                                                     new FrameworkPropertyMetadata(
			                                                                                     OnFormattedTextInvalidated));

		public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment),
		                                                                                              typeof(MultiLineTextBlock),
		                                                                                              new FrameworkPropertyMetadata(
			                                                                                              OnFormattedTextUpdated));

		public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations",
		                                                                                                typeof(TextDecorationCollection),
		                                                                                                typeof(MultiLineTextBlock),
		                                                                                                new FrameworkPropertyMetadata(
			                                                                                                OnFormattedTextUpdated));

		public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register("TextTrimming", typeof(TextTrimming),
		                                                                                             typeof(MultiLineTextBlock),
		                                                                                             new FrameworkPropertyMetadata(
			                                                                                             OnFormattedTextUpdated));

		public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping),
		                                                                                             typeof(MultiLineTextBlock),
		                                                                                             new FrameworkPropertyMetadata(
			                                                                                             TextWrapping.Wrap,
			                                                                                             OnFormattedTextUpdated));

		private FormattedText? _formattedText;

		public MultiLineTextBlock()
		{
			TextDecorations = new TextDecorationCollection();
		}

		public Brush Foreground
		{
			get { return (Brush)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
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

		public int MaxLines
		{
			get => 2;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			EnsureFormattedText();

			if(_formattedText == null)
				return;
			var y = !double.IsNaN(ActualHeight) ? (ActualHeight - _formattedText.Height) / 2 + _formattedText.Height * 0.05 + (_formattedText.Height > FontSize * 1.4 ? -2 : 0) : 0;
			drawingContext.DrawGeometry(Foreground, new Pen(Brushes.White, 0), _formattedText.BuildGeometry(new Point(0, y)));
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			EnsureFormattedText();

			if(_formattedText == null)
				return new Size(0, 0);

			// This code's goal is to find and SetFontSize on FormattedText, so that the text fits, using line breaks if necessary.
			// It's all fairly ugly, but the easiest solution for now

			var maxWidth = Math.Min(3579139, Math.Max(0.0001d, availableSize.Width));
			var singleLineRatio = maxWidth / _formattedText.Width; // if we need this later

			_formattedText.SetFontSize((int)FontSize);
			_formattedText.MaxTextWidth = maxWidth;
			_formattedText.MaxTextHeight = int.MaxValue;

			var fontSize = FontSize;

			while((_formattedText.Height / (fontSize * 1.3)) > MaxLines && fontSize > 5)
				_formattedText.SetFontSize(fontSize--);

			if((int)fontSize == (int)FontSize && singleLineRatio < 1)
				_formattedText.SetFontSize((int)(fontSize * singleLineRatio));

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
			var multiLineTextBlock = (MultiLineTextBlock)dependencyObject;
			multiLineTextBlock._formattedText = null;

			multiLineTextBlock.InvalidateMeasure();
			multiLineTextBlock.InvalidateVisual();
		}

		private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var multiLineTextBlock = (MultiLineTextBlock)dependencyObject;
			multiLineTextBlock.UpdateFormattedText();

			multiLineTextBlock.InvalidateMeasure();
			multiLineTextBlock.InvalidateVisual();
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

			_formattedText.MaxLineCount = int.MaxValue;
			_formattedText.TextAlignment = TextAlignment;

			_formattedText.SetFontSize(FontSize);
			_formattedText.SetFontStyle(FontStyle);
			_formattedText.SetFontWeight(FontWeight);
			_formattedText.SetFontFamily(FontFamily);
			_formattedText.SetFontStretch(FontStretch);
			_formattedText.SetTextDecorations(TextDecorations);
		}
	}
}

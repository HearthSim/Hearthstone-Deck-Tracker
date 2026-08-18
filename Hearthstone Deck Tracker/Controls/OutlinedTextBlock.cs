#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
		public static readonly DependencyProperty StrokeWidthProperty = DependencyProperty.Register("StrokeWidth", typeof(double),
		                                                                                       typeof(OutlinedTextBlock),
		                                                                                       new FrameworkPropertyMetadata(2.0,
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

		private FormattedText? _formattedText;

		public OutlinedTextBlock()
		{
			TextDecorations = new TextDecorationCollection();
			VerticalAlignment = VerticalAlignment.Center;
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

		public double StrokeWidth
		{
			get { return (double)GetValue(StrokeWidthProperty); }
			set { SetValue(StrokeWidthProperty, value); }
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

		private readonly record struct GeometryCacheKey(
			string Text, string FontFamily, int FontSize, FontWeight FontWeight, FontStretch FontStretch,
			TextAlignment Alignment, FlowDirection FlowDirection, int MaxWidthTenths, int OriginYTenths);

		// Building glyph geometry is expensive and the same strings (card names, costs, counts)
		// are rendered over and over across tile instances, so cache globally and frozen.
		private static readonly Dictionary<GeometryCacheKey, Geometry> _geometryCache = new();
		private const int GeometryCacheMaxEntries = 2048;

		private static readonly Dictionary<double, Pen> _strokePens = new();
		private static readonly Pen _fillPen = CreateFillPen();

		private static Pen CreateFillPen()
		{
			var pen = new Pen(Brushes.White, 0);
			pen.Freeze();
			return pen;
		}

		private static Pen GetStrokePen(double width)
		{
			lock(_strokePens)
			{
				if(!_strokePens.TryGetValue(width, out var pen))
				{
					_strokePens[width] = pen = new Pen(Brushes.Black, width) { LineJoin = PenLineJoin.Round };
					pen.Freeze();
				}
				return pen;
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			EnsureFormattedText();

			if(_formattedText == null)
				return;

			double originY;
			int maxWidthTenths;
			if(!double.IsNaN(Width) || !double.IsNaN(Height) || TextAlignment != TextAlignment.Left)
			{
				var center = (ActualHeight - _formattedText.Height) / 2;
				originY = center + _formattedText.Height * 0.05;
				maxWidthTenths = (int)Math.Round(_formattedText.MaxTextWidth * 10);
			}
			else
			{
				// layout of unconstrained left-aligned text does not depend on the max width
				originY = _formattedText.Height * 0.05;
				maxWidthTenths = 0;
			}

			var key = new GeometryCacheKey(_formattedText.Text, FontFamily.Source, _fontSize, FontWeight, FontStretch,
			                               TextAlignment, FlowDirection, maxWidthTenths, (int)Math.Round(originY * 10));
			Geometry geometry;
			lock(_geometryCache)
			{
				if(!_geometryCache.TryGetValue(key, out var cached))
				{
					if(_geometryCache.Count >= GeometryCacheMaxEntries)
						_geometryCache.Clear();
					cached = _formattedText.BuildGeometry(new Point(0, originY));
					if(cached.CanFreeze)
						cached.Freeze();
					_geometryCache[key] = cached;
				}
				geometry = cached;
			}

			drawingContext.DrawGeometry(Stroke, GetStrokePen(StrokeWidth), geometry);
			drawingContext.DrawGeometry(Fill, _fillPen, geometry);
		}

		private const int MaxAllowedTextSize = 3579139;

		private int _fontSize;

		private record SizeCache(int FontSize, double Width, double Height);
		private readonly List<SizeCache> _measureCache = new();
		protected override Size MeasureOverride(Size availableSize)
		{
			EnsureFormattedText();

			if(_formattedText == null || string.IsNullOrEmpty(_formattedText.Text) || availableSize.Width <= 0 || availableSize.Height <= 0)
				return new Size(0, 0);

			// 1. Reset max size so that we get the real _formattedText.Width without any wrapping/clipping (not sure which happens)
			_formattedText.MaxTextWidth = MaxAllowedTextSize;
			_formattedText.MaxTextHeight = MaxAllowedTextSize;

			var measured = _measureCache.FirstOrDefault(x => x.Width <= availableSize.Width && x.Height <= availableSize.Height);
			if(measured == null)
			{
				var fontSize = (_measureCache.LastOrDefault()?.FontSize - 1) ?? (int)FontSize;
				// 2. Decrease font size until text fits in available size
				for(; fontSize > 1; fontSize--)
				{
					_formattedText.SetFontSize(fontSize);
					_measureCache.Add(new SizeCache(fontSize, _formattedText.Width, _formattedText.Height));
					if(_formattedText.Width <= availableSize.Width && _formattedText.Height <= availableSize.Height)
						break;
				}
				measured = _measureCache.Last();
			}
			else
				_formattedText.SetFontSize(measured.FontSize);
			_fontSize = measured.FontSize;

			// 3. Set the actual max size so that our text has the correct dimensions
			_formattedText.MaxTextWidth = Math.Min(MaxAllowedTextSize, Math.Max(1, availableSize.Width));
			_formattedText.MaxTextHeight = Math.Min(MaxAllowedTextSize, Math.Max(1, availableSize.Height));

			return new Size(Math.Ceiling(measured.Width), Math.Ceiling(measured.Height));
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			EnsureFormattedText();

			if(_formattedText == null || finalSize.Width <= 0 || finalSize.Height <= 0)
				return new Size(0, 0);

			_formattedText.MaxTextWidth = Math.Min(MaxAllowedTextSize, Math.Max(1, finalSize.Width));
			_formattedText.MaxTextHeight = Math.Min(MaxAllowedTextSize, Math.Max(1, finalSize.Height));

			return finalSize;
		}

		private static void OnFormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
			outlinedTextBlock._formattedText = null;

			outlinedTextBlock.InvalidateMeasure();
			outlinedTextBlock.InvalidateVisual();
			outlinedTextBlock._measureCache.Clear();
			outlinedTextBlock._fontSize = (int)outlinedTextBlock.FontSize;
		}

		private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
			outlinedTextBlock.UpdateFormattedText();

			outlinedTextBlock.InvalidateMeasure();
			outlinedTextBlock.InvalidateVisual();
			outlinedTextBlock._measureCache.Clear();
			outlinedTextBlock._fontSize = (int)outlinedTextBlock.FontSize;
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

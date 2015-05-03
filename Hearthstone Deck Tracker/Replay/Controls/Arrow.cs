#region

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

#endregion

namespace Hearthstone_Deck_Tracker.Replay.Controls
{
	//source: http://www.codeproject.com/Articles/23116/WPF-Arrow-and-Custom-Shapes
	public sealed class Arrow : Shape
	{
		#region Overrides

		protected override Geometry DefiningGeometry
		{
			get
			{
				// Create a StreamGeometry for describing the shape
				var geometry = new StreamGeometry();
				geometry.FillRule = FillRule.EvenOdd;

				using(var context = geometry.Open())
					InternalDrawArrowGeometry(context);

				// Freeze the geometry for performance benefits
				geometry.Freeze();

				return geometry;
			}
		}

		#endregion

		#region Privates

		private void InternalDrawArrowGeometry(StreamGeometryContext context)
		{
			var theta = Math.Atan2(Y1 - Y2, X1 - X2);
			var sint = Math.Sin(theta);
			var cost = Math.Cos(theta);

			var pt1 = new Point(X1, Y1);
			var pt2 = new Point(X2, Y2);

			var pt3 = new Point(X2 + (HeadWidth * cost - HeadHeight * sint), Y2 + (HeadWidth * sint + HeadHeight * cost));

			var pt4 = new Point(X2 + (HeadWidth * cost + HeadHeight * sint), Y2 - (HeadHeight * cost - HeadWidth * sint));

			context.BeginFigure(pt1, true, false);
			context.LineTo(pt2, true, true);
			context.LineTo(pt3, true, true);
			context.LineTo(pt2, true, true);
			context.LineTo(pt4, true, true);
		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(Arrow),
		                                                                                   new FrameworkPropertyMetadata(0.0,
		                                                                                                                 FrameworkPropertyMetadataOptions
			                                                                                                                 .AffectsRender
		                                                                                                                 | FrameworkPropertyMetadataOptions
			                                                                                                                   .AffectsMeasure));

		public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(Arrow),
		                                                                                   new FrameworkPropertyMetadata(0.0,
		                                                                                                                 FrameworkPropertyMetadataOptions
			                                                                                                                 .AffectsRender
		                                                                                                                 | FrameworkPropertyMetadataOptions
			                                                                                                                   .AffectsMeasure));

		public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(Arrow),
		                                                                                   new FrameworkPropertyMetadata(0.0,
		                                                                                                                 FrameworkPropertyMetadataOptions
			                                                                                                                 .AffectsRender
		                                                                                                                 | FrameworkPropertyMetadataOptions
			                                                                                                                   .AffectsMeasure));

		public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(Arrow),
		                                                                                   new FrameworkPropertyMetadata(0.0,
		                                                                                                                 FrameworkPropertyMetadataOptions
			                                                                                                                 .AffectsRender
		                                                                                                                 | FrameworkPropertyMetadataOptions
			                                                                                                                   .AffectsMeasure));

		public static readonly DependencyProperty HeadWidthProperty = DependencyProperty.Register("HeadWidth", typeof(double), typeof(Arrow),
		                                                                                          new FrameworkPropertyMetadata(0.0,
		                                                                                                                        FrameworkPropertyMetadataOptions
			                                                                                                                        .AffectsRender
		                                                                                                                        | FrameworkPropertyMetadataOptions
			                                                                                                                          .AffectsMeasure));

		public static readonly DependencyProperty HeadHeightProperty = DependencyProperty.Register("HeadHeight", typeof(double),
		                                                                                           typeof(Arrow),
		                                                                                           new FrameworkPropertyMetadata(0.0,
		                                                                                                                         FrameworkPropertyMetadataOptions
			                                                                                                                         .AffectsRender
		                                                                                                                         | FrameworkPropertyMetadataOptions
			                                                                                                                           .AffectsMeasure));

		#endregion

		#region CLR Properties

		[TypeConverter(typeof(LengthConverter))]
		public double X1
		{
			get { return (double)GetValue(X1Property); }
			set { SetValue(X1Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double Y1
		{
			get { return (double)GetValue(Y1Property); }
			set { SetValue(Y1Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double X2
		{
			get { return (double)GetValue(X2Property); }
			set { SetValue(X2Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double Y2
		{
			get { return (double)GetValue(Y2Property); }
			set { SetValue(Y2Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double HeadWidth
		{
			get { return (double)GetValue(HeadWidthProperty); }
			set { SetValue(HeadWidthProperty, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double HeadHeight
		{
			get { return (double)GetValue(HeadHeightProperty); }
			set { SetValue(HeadHeightProperty, value); }
		}

		#endregion
	}
}
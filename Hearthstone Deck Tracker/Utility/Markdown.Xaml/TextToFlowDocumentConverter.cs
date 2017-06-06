#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Markdown.Xaml
{
	public class TextToFlowDocumentConverter : DependencyObject, IValueConverter
	{
		// Using a DependencyProperty as the backing store for Markdown.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register("Markdown", typeof(Markdown),
		                                                                                         typeof(TextToFlowDocumentConverter),
		                                                                                         new PropertyMetadata(null));

		private readonly Lazy<Markdown> mMarkdown = new Lazy<Markdown>(() => new Markdown());

		public Markdown Markdown
		{
			get { return (Markdown)GetValue(MarkdownProperty); }
			set { SetValue(MarkdownProperty, value); }
		}

		/// <summary>
		/// Converts a value. 
		/// </summary>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
				return null;

			var text = (string)value;

			var engine = Markdown ?? mMarkdown.Value;

			return engine.Transform(text);
		}

		/// <summary>
		/// Converts a value. 
		/// </summary>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

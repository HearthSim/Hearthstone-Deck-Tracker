using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Hearthstone_Deck_Tracker.Utility
{
	internal class AttachedFormattedString
	{
		public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
			"FormattedText", typeof (string), typeof (AttachedFormattedString),
			new FrameworkPropertyMetadata(PropertyChangedCallback));

		private static void PropertyChangedCallback(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			if (!(dependencyObject is TextBlock)) return;
			var textBlock = (TextBlock) dependencyObject;
			textBlock.Inlines.Clear();
			textBlock.Inlines.AddRange(GetInlines(GetFormattedText(dependencyObject)));
		}

		public static void SetFormattedText(DependencyObject dependencyObject, string value)
		{
			dependencyObject.SetValue(FormattedTextProperty, value);
		}

		private static IEnumerable<Inline> GetInlines(string value)
		{
			if(value == null)
				 yield break;
			//<w> is a wrapper so it gets read as an XElement
			var element = XElement.Parse("<w>" + Regex.Replace(value, @"\<(\w+?)\>", x => x.Captures[0].Value.ToLower()) + "</w>");
			//descendantnodes is the only method that includes the XTexts, but it also includes the XTexts of the children XElements.
			//so we gotta make sure the parent is the element we created to know these are the values we are after.
			foreach (var n in element.DescendantNodes().Where(x => x.Parent.Name == "w"))
			{
				if (n is XText text)
				{
					yield return new Run(text.Value);
					continue;
				}
				var x = (XElement) n;
				if (x.Name == "b")
				{
					foreach(var xText in x.DescendantNodes().OfType<XText>())
						yield return new Run(xText.Value) {FontWeight = FontWeights.Bold};
				}
				else if (x.Name == "i")
				{
					foreach(var xText in x.DescendantNodes().OfType<XText>())
						yield return new Run(xText.Value) { FontStyle = FontStyles.Italic };
				}
			}
		}

		public static string GetFormattedText(DependencyObject dependencyObject)
		{
			return (string) dependencyObject.GetValue(FormattedTextProperty);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;
using WPFLocalizeExtension.Engine;

namespace Hearthstone_Deck_Tracker.Utility
{
	internal class AttachedFormattedString
	{
		public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
			"FormattedText", typeof (string), typeof (AttachedFormattedString),
			new FrameworkPropertyMetadata(PropertyChangedCallback));

		// registered under another name so XAML goes through GetLinks (which creates the collection) instead of reading the property directly
		public static readonly DependencyProperty LinksProperty = DependencyProperty.RegisterAttached(
			"LinksInternal", typeof(FreezableCollection<FormattedLink>), typeof(AttachedFormattedString));

		public static readonly DependencyProperty LocKeyProperty = DependencyProperty.RegisterAttached(
			"LocKey", typeof(string), typeof(AttachedFormattedString),
			new FrameworkPropertyMetadata(LocKeyChangedCallback));


		private static List<WeakReference<DependencyObject>>? _locKeyTargets;

		private static void PropertyChangedCallback(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			Render(dependencyObject);
		}

		private static void Render(DependencyObject dependencyObject)
		{
			var inlines = dependencyObject switch
			{
				TextBlock textBlock => textBlock.Inlines,
				Span span => span.Inlines,
				_ => null,
			};
			if(inlines == null)
				return;
			inlines.Clear();
			var links = dependencyObject.GetValue(LinksProperty) as FreezableCollection<FormattedLink>;
			inlines.AddRange(GetInlines(GetFormattedText(dependencyObject), links));
		}

		public static void SetFormattedText(DependencyObject dependencyObject, string value)
		{
			dependencyObject.SetValue(FormattedTextProperty, value);
		}

		public static FreezableCollection<FormattedLink> GetLinks(DependencyObject dependencyObject)
		{
			if(dependencyObject.GetValue(LinksProperty) is FreezableCollection<FormattedLink> links)
				return links;
			links = new FreezableCollection<FormattedLink>();
			links.Changed += (sender, args) => Render(dependencyObject);
			dependencyObject.SetValue(LinksProperty, links);
			return links;
		}

		public static string? GetLocKey(DependencyObject dependencyObject) => (string?)dependencyObject.GetValue(LocKeyProperty);

		public static void SetLocKey(DependencyObject dependencyObject, string value) => dependencyObject.SetValue(LocKeyProperty, value);

		private static void LocKeyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			if(args.OldValue == null)
				TrackLocKey(dependencyObject);
			UpdateLocKey(dependencyObject);
		}

		private static void UpdateLocKey(DependencyObject dependencyObject)
		{
			var key = GetLocKey(dependencyObject);
			if(key != null)
				SetFormattedText(dependencyObject, Localize(key));
		}

		// {lex:Loc} only hands us the translated value, so we track the key ourselves to be able to fall back to English
		private static void TrackLocKey(DependencyObject dependencyObject)
		{
			if(_locKeyTargets == null)
			{
				_locKeyTargets = new List<WeakReference<DependencyObject>>();
				LocalizeDictionary.Instance.PropertyChanged += (sender, args) =>
				{
					if(args.PropertyName != nameof(LocalizeDictionary.Culture))
						return;
					_locKeyTargets.RemoveAll(x => !x.TryGetTarget(out _));
					foreach(var weak in _locKeyTargets.ToList())
					{
						if(weak.TryGetTarget(out var target))
							UpdateLocKey(target);
					}
				};
			}
			_locKeyTargets.Add(new WeakReference<DependencyObject>(dependencyObject));
		}

		/// <summary>
		/// Returns the translation of the given key with the (escaped) arguments formatted into it, or the English string
		/// if the translation is malformed or does not contain the same links.
		/// </summary>
		public static string Localize(string key, params object?[] args)
		{
			var values = args.Select(x => (object)Escape(x?.ToString())).ToArray();
			var english = Format(LocUtil.GetEnglish(key), values) ?? "";
			var translated = Format(LocUtil.Get(key), values);
			if(translated == english)
				return english;
			var linkIndices = GetLinkIndices(translated);
			if(linkIndices != null && linkIndices.SetEquals(GetLinkIndices(english) ?? new HashSet<int>()))
				return translated!;
			Log.Warn($"Falling back to English for malformed translation of {key}");
			return english;
		}

		public static string Escape(string? text) => SecurityElement.Escape(text) ?? "";

		private static string? Format(string template, object[] values)
		{
			if(values.Length == 0)
				return template;
			try
			{
				return string.Format(template, values);
			}
			catch(FormatException)
			{
				return null;
			}
		}

		// null when the text is malformed or uses markup we can't render
		internal static HashSet<int>? GetLinkIndices(string? text)
		{
			var root = text != null ? Parse(text) : null;
			if(root == null)
				return null;
			var elements = root.Descendants().ToList();
			if(elements.Any(x => GetLinkIndex(x) != null ? x.Parent != root : x.Name != "b" && x.Name != "i"))
				return null;
			return new HashSet<int>(elements.Select(GetLinkIndex).OfType<int>());
		}

		private static int? GetLinkIndex(XElement element)
		{
			var name = element.Name.LocalName;
			return name.StartsWith("_") && int.TryParse(name.Substring(1), out var index) ? index : null;
		}

		private static XElement? Parse(string text)
		{
			var normalized = Regex.Replace(text, @"\<(\w+?)\>", x => x.Captures[0].Value.ToLower());
			// xml names can't start with a digit, so links <0>...</0> are parsed as <_0>...</_0>
			normalized = Regex.Replace(normalized, @"<(/?)(\d+)>", "<$1_$2>");
			try
			{
				//<w> is a wrapper so it gets read as an XElement
				return XElement.Parse("<w>" + normalized + "</w>");
			}
			catch(Exception)
			{
				return null;
			}
		}

		private static Inline CreateLink(XElement element, int index, IList<FormattedLink>? links)
		{
			var run = new Run(string.Concat(element.DescendantNodes().OfType<XText>().Select(x => x.Value)));
			if(links == null || index >= links.Count)
				return run;
			var link = links[index];
			var hyperlink = new Hyperlink(run);
			if(link.Command != null)
			{
				hyperlink.Command = link.Command;
				hyperlink.CommandParameter = link.CommandParameter;
			}
			else if(link.Url != null)
				hyperlink.Click += (sender, args) => Helper.TryOpenUrl(link.Url);
			return hyperlink;
		}

		private static IEnumerable<Inline> GetInlines(string? value, IList<FormattedLink>? links)
		{
			if(value == null)
				 yield break;

			var element = Parse(value);
			if(element == null)
			{
				Log.Error($"Could not parse formatted text: {value}");
				yield break;
			}
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
				else if (GetLinkIndex(x) is { } index)
					yield return CreateLink(x, index, links);
			}
		}

		public static string? GetFormattedText(DependencyObject dependencyObject)
		{
			return (string?) dependencyObject.GetValue(FormattedTextProperty);
		}
	}

	/// <summary>
	/// Target of a &lt;0&gt;, &lt;1&gt;, ... link in <see cref="AttachedFormattedString"/>, by its position in Links.
	/// A Command takes precedence over a Url.
	/// </summary>
	internal class FormattedLink : Freezable
	{
		public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
			nameof(Url), typeof(string), typeof(FormattedLink));

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			nameof(Command), typeof(ICommand), typeof(FormattedLink));

		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
			nameof(CommandParameter), typeof(object), typeof(FormattedLink));

		public string? Url
		{
			get => (string?)GetValue(UrlProperty);
			set => SetValue(UrlProperty, value);
		}

		public ICommand? Command
		{
			get => (ICommand?)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public object? CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		protected override Freezable CreateInstanceCore() => new FormattedLink();
	}
}

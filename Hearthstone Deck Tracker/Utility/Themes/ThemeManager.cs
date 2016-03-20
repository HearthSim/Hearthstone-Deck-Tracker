using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public static class ThemeManager
	{
		private const string ThemeDir = @"Images\Themes\Bars";
		private const string ThemeRegex = @"[a-zA-Z]+";
		private static Theme _barTheme;

		public static List<Theme> Themes = new List<Theme>();

		public static Theme CurrentTheme
		{
			get
			{
				return _barTheme;
			}
			set
			{
				_barTheme = value;
			}
		}

		public static void Run()
		{
			var dirs = Directory.GetDirectories(ThemeDir);
			foreach(var d in dirs)
			{
				var di = new DirectoryInfo(d);
				if(Regex.IsMatch(di.Name, ThemeRegex))
				{
					Themes.Add(
						new Theme(
							di.Name,
							di.FullName,
							GetBuilderType(di.Name)));
				}
				else
				{
					Logging.Log.Warn($"Invalid theme directory name {di.Name}", "ThemeManager");
				}
			}
			_barTheme = FindTheme(Config.Instance.CardBarTheme);
		}

		public static Theme FindTheme(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
				return null;
			return Themes.FirstOrDefault(x =>
				x.Name.ToLowerInvariant() == name.ToLowerInvariant());
		}

		public static void SetTheme(string theme)
		{
			var t = Themes.FirstOrDefault(
				x => x.Name.ToLowerInvariant() == theme.ToLowerInvariant());
			if(t != null)
				_barTheme = t;
		}

		public static CardBarImageBuilder GetBarImageBuilder(Card card)
		{
			Type buildType = _barTheme.BuildType;
			if(buildType == null)
				buildType = typeof(DefaultBarImageBuilder);

			return (CardBarImageBuilder)Activator.CreateInstance(buildType, card, _barTheme.Directory);
		}

		private static Type GetBuilderType(string name)
		{
			string className = null;
			if(!string.IsNullOrWhiteSpace(name))
			{
				className = name[0].ToString().ToUpperInvariant();
				if(name.Length > 1)
					className += name.ToLowerInvariant().Substring(1);
				className += "BarImageBuilder";
			}

			Type buildType = null;
			try
			{
				buildType = Type.GetType("Hearthstone_Deck_Tracker.Utility.Themes." + className);
			}
			catch(Exception)
			{
				Logging.Log.Warn($"Theme builder {className} not found, using default.", "ThemeManager");
			}
			return buildType;
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class ThemeManager
	{
		private static string CustomThemeDir => Path.Combine(Config.AppDataPath, @"Themes\Bars");
		private const string ThemeDir = @"Images\Themes\Bars";
		private const string ThemeRegex = @"[a-zA-Z]+";

		public static List<Theme> Themes = new List<Theme>();

		public static Theme? CurrentTheme { get; private set; }

		public static void Run()
		{
			LoadThemes(CustomThemeDir);
			LoadThemes(ThemeDir);
			CurrentTheme = FindTheme(Config.Instance.CardBarTheme) ?? Themes.FirstOrDefault();
		}

		private static void LoadThemes(string dir)
		{
			var dirInfo = new DirectoryInfo(dir);
			if(!dirInfo.Exists)
				return;
			foreach(var di in dirInfo.GetDirectories())
			{
				if(Regex.IsMatch(di.Name, ThemeRegex))
				{
					var type = GetBuilderType(di.Name);
					if(type != null)
					{
						Logging.Log.Info($"Found theme: {di.Name}");
						Themes.Add(new Theme(di.Name, di.FullName, type));
					}
				}
				else
				{
					Logging.Log.Warn($"Invalid theme directory name {di.Name}");
				}
			}
		}

		public static Theme? FindTheme(string name)
			=> string.IsNullOrWhiteSpace(name) ? null : Themes.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());

		public static void SetTheme(string theme)
		{
			var t = Themes.FirstOrDefault(x => x.Name.ToLowerInvariant() == theme.ToLowerInvariant());
			if(t == null)
				return;
			CurrentTheme = t;
			UpdateCards();
		}

		public static void UpdateCards()
		{
			Core.UpdatePlayerCards(true);
			Core.UpdateOpponentCards(true);
			Core.Overlay.PlayerDeck.ForEach(c => c.UpdateHighlight());
			Core.Overlay.OpponentDeck.ForEach(c => c.UpdateHighlight());
			Core.Windows.PlayerWindow.PlayerDeck.ForEach(c => c.UpdateHighlight());
			Core.Windows.OpponentWindow.OpponentDeck.ForEach(c => c.UpdateHighlight());
			// TODO: Find a better way to interact with the MainWindow
			foreach(var card in Core.MainWindow.ListViewDeck.Items.Cast<Card>())
				card.Update();
			Core.Windows.PlayerWindow.UpdateCardFrames();
			Core.Windows.OpponentWindow.UpdateCardFrames();
			Core.Overlay.UpdateCardFrames();
		}

		public static CardBarImageBuilder? GetBarImageBuilder(Card card)
		{
			if(CurrentTheme == null)
				return null;
			var buildType = CurrentTheme.BuildType ?? typeof(DefaultBarImageBuilder);
			return (CardBarImageBuilder)Activator.CreateInstance(buildType, card, CurrentTheme.Directory);
		}

		private static Type? GetBuilderType(string name)
		{
			string? className = null;
			if(!string.IsNullOrWhiteSpace(name))
			{
				className = name[0].ToString().ToUpperInvariant();
				if(name.Length > 1)
					className += name.ToLowerInvariant().Substring(1);
				className += "BarImageBuilder";
			}

			Type? buildType = null;
			try
			{
				buildType = Type.GetType("Hearthstone_Deck_Tracker.Utility.Themes." + className);
			}
			catch(Exception)
			{
				Logging.Log.Warn($"Theme builder {className} not found, using default.");
			}
			return buildType;
		}
	}
}

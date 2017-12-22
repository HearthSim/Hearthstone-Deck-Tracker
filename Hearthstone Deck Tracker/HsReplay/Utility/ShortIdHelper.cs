using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.HsReplay.Utility
{
	public static class ShortIdHelper
	{
		private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		private static readonly int AlphabetLength = Alphabet.Length;

		public static string GetShortId(Deck deck)
		{
			if(deck == null || deck.Cards.Count == 0)
				return string.Empty;
			try
			{
				var ids = deck.Cards.SelectMany(c => Enumerable.Repeat(c.Id.ToString(), c.Count));
				var idString = string.Join(",", ids.OrderBy(x => x, new Utf8StringComperer()));
				var bytes = Encoding.UTF8.GetBytes(idString);
				var hash = MD5.Create().ComputeHash(bytes);
				var hex = BitConverter.ToString(hash).Replace("-", string.Empty);
				return IntToString(BigInteger.Parse("00" + hex, NumberStyles.HexNumber));
			}
			catch(Exception e)
			{
				Log.Error(e);
				return string.Empty;
			}
		}

		private static string IntToString(BigInteger number)
		{
			var sb = new StringBuilder();
			while(number > 0)
			{
				var mod = number % AlphabetLength;
				sb.Append(Alphabet[(int)mod]);
				number = number / AlphabetLength;
			}
			return sb.ToString();
		}
	}

	public class Utf8StringComperer : IComparer<string>
	{
		private const string Chars = "_0123456789aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ";

		public int Compare(string x, string y)
		{
			if(x == y)
				return 0;
			var val = x.Zip(y, Utf8Compare).FirstOrDefault(v => v != 0);
			if(val != 0)
				return val;
			return x.Length - y.Length;
		}

		private int Utf8Compare(char x, char y) => Chars.IndexOf(x) - Chars.IndexOf(y);
	}
}

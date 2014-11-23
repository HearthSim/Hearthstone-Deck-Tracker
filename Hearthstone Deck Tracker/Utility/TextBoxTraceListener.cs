using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class TextBoxTraceListener : TraceListener
	{
		private readonly TextBox _textBox;

		public TextBoxTraceListener(TextBox textBox)
		{
			_textBox = textBox;
		}

		public override void Write(string message)
		{
			_textBox.Text += message;
			_textBox.ScrollToEnd();
		}

		public override void WriteLine(string message)
		{
			_textBox.Text += message + Environment.NewLine;
			_textBox.ScrollToEnd();
		}

		public override bool Equals(object obj)
		{
			var tbtl = obj as TextBoxTraceListener;
			return tbtl != null && _textBox.Equals(tbtl._textBox);
		}

		public override int GetHashCode()
		{
			return _textBox.GetHashCode();
		}
	}

}
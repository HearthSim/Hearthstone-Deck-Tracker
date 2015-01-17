#region

using System.Diagnostics;
using System.Windows.Navigation;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for Help.xaml
	/// </summary>
	public partial class Help
	{
		public Help()
		{
			InitializeComponent();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
		}
	}
}
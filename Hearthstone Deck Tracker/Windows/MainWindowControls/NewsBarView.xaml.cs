using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	/// <summary>
	/// Interaction logic for NewsBarView.xaml
	/// </summary>
	public partial class NewsBarView : UserControl
	{
		public NewsBarView()
		{
			InitializeComponent();
		}

		public TextBlock NewsContent
		{
			get => ((NewsBarViewModel)DataContext).NewsContent;
			set => ((NewsBarViewModel)DataContext).NewsContent = value;
		}

		public string IndexContent
		{
			get => ((NewsBarViewModel)DataContext).IndexContent;
			set => ((NewsBarViewModel)DataContext).IndexContent = value;
		}
	}
}

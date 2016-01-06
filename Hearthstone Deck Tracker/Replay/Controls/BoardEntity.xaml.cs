#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.Replay.Controls
{
	/// <summary>
	/// Interaction logic for IBoardEntity.xaml
	/// </summary>
	public partial class BoardEntity : UserControl
	{
		public BoardEntity()
		{
			InitializeComponent();
		}

		public Visibility EntityVisibility => DataContext == null ? Visibility.Collapsed : Visibility.Visible;

		private void BoardEntity_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var binding = GetBindingExpression(VisibilityProperty);
			binding?.UpdateTarget();
		}
	}
}
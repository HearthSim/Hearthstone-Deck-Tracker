#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.Replay.Controls
{
	/// <summary>
	/// Interaction logic for BoardEntity.xaml
	/// </summary>
	public partial class BoardEntity : UserControl
	{
		public BoardEntity()
		{
			InitializeComponent();
		}

		public Visibility EntityVisibility
		{
			get { return DataContext == null ? Visibility.Collapsed : Visibility.Visible; }
		}

		private void BoardEntity_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var binding = GetBindingExpression(VisibilityProperty);
			if(binding != null)
				binding.UpdateTarget();
		}
	}
}
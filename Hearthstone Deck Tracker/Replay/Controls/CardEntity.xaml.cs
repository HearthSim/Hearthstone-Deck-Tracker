#region

using System.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.Replay.Controls
{
	/// <summary>
	/// Interaction logic for CardEntity.xaml
	/// </summary>
	public partial class CardEntity
	{
		public CardEntity()
		{
			InitializeComponent();
		}

		public Visibility EntityVisibility => DataContext == null ? Visibility.Collapsed : Visibility.Visible;

		private void CardEntity_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var binding = GetBindingExpression(VisibilityProperty);
			binding?.UpdateTarget();
		}
	}
}
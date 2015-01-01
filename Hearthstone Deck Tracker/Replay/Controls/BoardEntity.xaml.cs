using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

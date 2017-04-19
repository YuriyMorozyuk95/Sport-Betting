using System.Windows.Controls;
using BaseObjects;

namespace DefaultViews.Views
{
    /// <summary>
    /// Interaction logic for CategoriesView.xaml
    /// </summary>
    public partial class CategoriesView
    {
        public CategoriesView()
        {
            InitializeComponent();
            
            //this.CloseViewModelOnUnloaded = false;
        }

        private void Frame_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Frame F = sender as Frame;
            F.Navigate(new TournamentsView());
        }
    }
}

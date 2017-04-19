using System.Windows.Controls;
using SportBetting.WPF.Prism.ModalWindows.ViewModels;

namespace SportBetting.WPF.Prism.ModalWindows.Views
{

    public partial class TestInputWindow 
    {
        public TestInputWindow()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((TestInputViewModel) this.DataContext).Text = e.AddedItems[0] as string;
        }
    }
}
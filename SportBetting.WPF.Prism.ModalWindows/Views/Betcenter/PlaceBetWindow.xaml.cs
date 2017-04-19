namespace SportBetting.WPF.Prism.ModalWindows.Views.Betcenter
{
    using Catel.Windows;
    using ViewModels;

	public partial class PlaceBetWindow : DataWindow
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceBetWindow"/> class.
        /// </summary>
        public PlaceBetWindow(PlaceBetViewModel viewModel)
            : base(viewModel, DataWindowMode.Custom, null, DataWindowDefaultButton.None, true, InfoBarMessageControlGenerationMode.None)

		{
			InitializeComponent();
		}
	}
}

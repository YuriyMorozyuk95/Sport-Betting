using System;
using BaseObjects;
using BaseObjects.ViewModels;
using SportRadar.Common.Logs;

namespace SportBetting.WPF.Prism.Modules.UserManagement.ViewModels
{
    /// <summary>
    /// Categories view model.
    /// </summary>
    public class NewPinViewModel : BaseViewModel
    {

        #region Constructors
        private static ILog Log = LogFactory.CreateLog(typeof(NewPinViewModel));

        public NewPinViewModel()
        {
            BackCommand = new Command(OnBackCommand);
            Log.Debug(String.Format("{0}.{1}", "Enabling scanner", "PaymentViewModel"));
        }



        #endregion

        #region Properties


        #endregion

        #region Commands

        public Command BackCommand { get; set; }
        #endregion

        #region Methods


        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<PinKeyboardViewModel>(RegionNames.PinKeyboardRegion);
            
            base.OnNavigationCompleted();
        }


        private void OnBackCommand()
        {
            ChangeTracker.OperatorPaymentViewOpen = false;
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }

        public override void Close()
        {
            StationRepository.BarcodeScannerTempActive = false;
            Log.Debug(String.Format("{0}.{1}", "Disabling scanner", "PaymentViewModel"));
            MyRegionManager.CloseAllViewsInRegion(RegionNames.PinKeyboardRegion);

            base.Close();
        }

        #endregion
    }
}
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;

namespace SportBetting.WPF.Prism.Modules.UserManagement.ViewModels
{


    /// <summary>
    /// Categories view model.
    /// </summary>
    public class UserRegistrationViewModel : BaseViewModel
    {

        #region Constructors


        public UserRegistrationViewModel()
        {

            
        }

        #endregion
        #region Properties
        #endregion

        #region Commands
        #endregion
        #region Methods

        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<RegistrationViewModel>(RegionNames.UserManagementRegistrationRegion);
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_FORM_USER;
            ChangeTracker.AdminTitle2 = MultistringTags.REGISTER_USER;
            ChangeTracker.RegisterUserChecked = true;

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UserManagementRegistrationRegion);
            
            base.Close();
        }

        #endregion
    }
}
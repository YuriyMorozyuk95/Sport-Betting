using System.Collections.Generic;
using System.Windows.Controls;
using BaseObjects;
using BaseObjects.ViewModels;
using Catel.Data;
using Shared;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;

namespace SportBetting.WPF.Prism.Modules.UserManagement.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class UserChangePasswordViewModel : ModalWindowBaseViewModel
    {
        public UserChangePasswordViewModel()
        {
            ChangePasswordCommand = new Command(OnChangePasswordExecute);
            BackCommand = new Command(OnBack);
            SelectionChanged = new Command<object>(OnSelectionChanged);
        }

        #region Properties

        public static readonly PropertyData OldPasswordProperty = RegisterProperty("OldPassword", typeof(string), string.Empty);
        public static readonly PropertyData OldPasswordMaskedProperty = RegisterProperty("OldPasswordMasked", typeof(string), string.Empty, (sender, e) => ((UserChangePasswordViewModel)sender).OnOldPasswordMaskedChanged(e));
        public static readonly PropertyData NewPasswordProperty = RegisterProperty("NewPassword", typeof(string), string.Empty);
        public static readonly PropertyData NewPasswordMaskedProperty = RegisterProperty("NewPasswordMasked", typeof(string), string.Empty, (sender, e) => ((UserChangePasswordViewModel)sender).OnNewPasswordMaskedChanged(e));
        public static readonly PropertyData NewPasswordConfirmedProperty = RegisterProperty("NewPasswordConfirmed", typeof(string), string.Empty);
        public static readonly PropertyData NewPasswordConfirmedMaskedProperty = RegisterProperty("NewPasswordConfirmedMasked", typeof(string), string.Empty, (sender, e) => ((UserChangePasswordViewModel)sender).OnNewPasswordConfirmedMaskedChanged(e));

        public bool _isEnabledChange;
        private bool _isFocused;

        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                if (_isFocused)
                {
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string OldPassword
        {
            get { return GetValue<string>(OldPasswordProperty); }
            set { SetValue(OldPasswordProperty, value); }
        }

        public bool IsLoggedInUser
        {
            get { return ChangeTracker.CurrentUser is LoggedInUser; }
        }

        /// <summary>
        /// Gets or sets the OldPasswordMasked.
        /// </summary>
        public string OldPasswordMasked
        {
            get { return GetValue<string>(OldPasswordMaskedProperty); }
            set { SetValue(OldPasswordMaskedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string NewPassword
        {
            get { return GetValue<string>(NewPasswordProperty); }
            set { SetValue(NewPasswordProperty, value); }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string NewPasswordMasked
        {
            get { return GetValue<string>(NewPasswordMaskedProperty); }
            set { SetValue(NewPasswordMaskedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string NewPasswordConfirmed
        {
            get { return GetValue<string>(NewPasswordConfirmedProperty); }
            set { SetValue(NewPasswordConfirmedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string NewPasswordConfirmedMasked
        {
            get { return GetValue<string>(NewPasswordConfirmedMaskedProperty); }
            set { SetValue(NewPasswordConfirmedMaskedProperty, value); }
        }

        public bool IsEnabledChange
        {
            get { return _isEnabledChange; }
            set
            {
                _isEnabledChange = value;
                OnPropertyChanged("IsEnabledChange");
            }
        }

        protected FoundUser EditUserId
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }

        #endregion

        #region Commands

        public Command ChangePasswordCommand { get; private set; }
        public Command BackCommand { get; private set; }
        public Command<object> SelectionChanged { get; private set; }

        #endregion

        #region Methods

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            if (string.IsNullOrEmpty(NewPassword))
                validationResults.Add(FieldValidationResult.CreateError(NewPasswordProperty, "field is required"));
            if (string.IsNullOrEmpty(NewPasswordConfirmed))
                validationResults.Add(FieldValidationResult.CreateError(NewPasswordConfirmedProperty, "field is required"));
            if (NewPassword != NewPasswordConfirmed)
                validationResults.Add(FieldValidationResult.CreateError(NewPasswordConfirmedMaskedProperty, TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT).ToString()));
            IsEnabledChange = validationResults.Count == 0;
        }

        [WsdlServiceSyncAspect]
        private void OnChangePasswordExecute()
        {
            if (ValidateViewModel(true))
            {
                var result = WsdlRepository.ChangePasswordFromShop((int)ChangeTracker.CurrentUser.AccountId, StationRepository.GetUid(new LoggedInUser(EditUserId.AccountId, null, 0)), NewPassword);
                if (result)
                    ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
            }
        }

        private void OnSelectionChanged(object args)
        {
            var textBox = args as TextBox;
            if (textBox != null)
            {
                if (textBox.SelectionStart != textBox.Text.Length)
                    textBox.Select(textBox.Text.Length, 0);
            }
        }

        private void OnOldPasswordMaskedChanged(Catel.Data.AdvancedPropertyChangedEventArgs args)
        {
            var oldValue = args.OldValue as string;
            var newValue = args.NewValue as string;

            if (((oldValue ?? string.Empty).Length + 1) == ((newValue ?? string.Empty).Length))
            {
                var change = (newValue ?? string.Empty).Substring((oldValue ?? string.Empty).Length, 1);
                OldPassword += change;
                OldPasswordMasked = OldPasswordMasked.Replace(change, "*");
            }
            else if ((oldValue ?? string.Empty).Length > (newValue ?? string.Empty).Length)
            {
                OldPassword = OldPassword.Substring(0, (newValue ?? string.Empty).Length);
                OldPasswordMasked = OldPasswordMasked.Substring(0, (newValue ?? string.Empty).Length);
            }
        }

        private void OnNewPasswordMaskedChanged(Catel.Data.AdvancedPropertyChangedEventArgs args)
        {
            var oldValue = args.OldValue as string;
            var newValue = args.NewValue as string;

            if (((oldValue ?? string.Empty).Length + 1) == ((newValue ?? string.Empty).Length))
            {
                var change = (newValue ?? string.Empty).Substring((oldValue ?? string.Empty).Length, 1);
                NewPassword += change;
                NewPasswordMasked = NewPasswordMasked.Replace(change, "*");
            }
            else if ((oldValue ?? string.Empty).Length > (newValue ?? string.Empty).Length)
            {
                NewPassword = NewPassword.Substring(0, (newValue ?? string.Empty).Length);
                NewPasswordMasked = NewPasswordMasked.Substring(0, (newValue ?? string.Empty).Length);
            }
        }

        [AsyncMethod]
        private void OnBack()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            PleaseWaitOnBack();
        }

        private void PleaseWaitOnBack()
        {
            Mediator.SendMessage("", MsgTag.HideKeyboard);
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);

        }

        private void OnNewPasswordConfirmedMaskedChanged(Catel.Data.AdvancedPropertyChangedEventArgs args)
        {
            var oldValue = args.OldValue as string;
            var newValue = args.NewValue as string;

            if (((oldValue ?? string.Empty).Length + 1) == ((newValue ?? string.Empty).Length))
            {
                var change = (newValue ?? string.Empty).Substring((oldValue ?? string.Empty).Length, 1);
                NewPasswordConfirmed += change;
                NewPasswordConfirmedMasked = NewPasswordConfirmedMasked.Replace(change, "*");
            }
            else if ((oldValue ?? string.Empty).Length > (newValue ?? string.Empty).Length)
            {
                NewPasswordConfirmed = NewPasswordConfirmed.Substring(0, (newValue ?? string.Empty).Length);
                NewPasswordConfirmedMasked = NewPasswordConfirmedMasked.Substring(0, (newValue ?? string.Empty).Length);
            }
        }

        #endregion

        public string this[string columnName]
        {
            get
            {
                if (string.IsNullOrEmpty(NewPassword))
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                if (string.IsNullOrEmpty(OldPassword))
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                if (OldPassword != NewPassword)
                    return TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT).ToString();

                return string.Empty;
            }
        }

        public string Error { get; private set; }
    }
}
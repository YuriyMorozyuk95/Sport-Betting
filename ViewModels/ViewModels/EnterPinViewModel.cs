using System;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using Catel.Windows.Threading;
using Shared;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.PinKeyboard.ViewModels;
using SportBetting.WPF.Prism.Shared;

namespace SportBetting.WPF.Prism.ModalWindows.ViewModels
{
    using Catel.MVVM;
    using System.ComponentModel;
    using Catel.Messaging;
    using Catel.Logging;
    using Catel.Data;
    using Catel.MVVM.Services;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class EnterPinViewModel : ModalWindowBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnterPinViewModel"/> class.
        /// </summary>

        public EnterPinViewModel()
        {

            
            if (ChangeTracker.MainWindow != null)
                ChangeTracker.MainWindow.Activate();
            this.CloseCommand = new Command(this.OnClose);
            this.ProceedCommand = new Command(this.OnProceed);
            Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.CloseCurrentWindow);

            Mediator.Register<long>(this, CardRemoved, MsgTag.CardRemoved);
            Mediator.Register<string>(this, EnterPinClearTicketNumber, MsgTag.EnterPinClearTicketNumber);
            Mediator.Register<string>(this, EnterPinBackspace, MsgTag.EnterPinBackspace);
            Mediator.Register<string>(this, EnterPinButton, MsgTag.EnterPinButton);

        
        }

        #region Properties

        public event EventHandler<EventArgs<string>> OkClick;
        public event EventHandler<EventArgs<string>> CloseClick;

        private string PinErrorMessage = string.Empty;


        /// <summary>
        /// Gets or sets the PinMasked.
        /// </summary>
        public string PinMasked
        {
            get { return GetValue<string>(PinMaskedProperty); }
            set
            {
                SetValue(PinMaskedProperty, value);
                OnPropertyChanged("PinMasked");
            }
        }
        
        /// <summary>
        /// Register the Masked Pin property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PinMaskedProperty = RegisterProperty("PinMasked", typeof(string), string.Empty,
                                                                    (sender, e) => ((EnterPinViewModel)sender).OnPinMaskedChanged(e));

        /// <summary>
        /// Gets or sets the Pin.
        /// </summary>
        public string Pin
        {
            get { return GetValue<string>(PinProperty); }
            set { SetValue(PinProperty, value); }
        }

        /// <summary>
        /// Register the Pin property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PinProperty = RegisterProperty("Pin", typeof(string), string.Empty);

        #endregion

        #region Commands

        public Command CloseCommand { get; private set; }
        public Command ProceedCommand { get; private set; }

        #endregion

        private void OnClose()
        {
            if (CloseClick != null)
                CloseClick(this, new EventArgs<string>(Pin));

            Close();
        }

        private void OnProceed()
        {
            if (!ValidateViewModel())
                return;
  
            if (OkClick != null)
                OkClick(this, new EventArgs<string>(Pin));
            Close();
            //Mediator.SendMessage<string>(this.Pin, "PinEntered");
            //Mediator.SendMessage<string>(this.Pin, "LoadStoredTicketWindow");

        }

        private void CardRemoved(long obj)
        {
            OnClose();
        }


        private void EnterPinClearTicketNumber(string input)
        {
            this.PinMasked = string.Empty;
        }

        private void EnterPinBackspace(string input)
        {

            var newPinMaskedLength = (this.PinMasked ?? string.Empty).Length - 1;
            if (newPinMaskedLength < 0)
            {
                newPinMaskedLength = 0;
            }
            this.PinMasked = (this.PinMasked ?? string.Empty).Substring(0, newPinMaskedLength);
        }

        private void EnterPinButton(string input)
        {
            // do not allow to enter more then 4 numbers
            if (this.PinMasked.Length >= 4)
                return;
            this.PinMasked += this.RemoveInvalidChars(input);
        }

        private void OnPinMaskedChanged(Catel.Data.AdvancedPropertyChangedEventArgs args)
        {
            var oldValue = this.Pin;
            var newValue = this.PinMasked;

            if ((oldValue.Length + 1) == newValue.Length)
            {
                var change = this.RemoveInvalidChars(newValue.Substring(oldValue.Length, 1));
                this.Pin += change;
                this.PinMasked = new string(Enumerable.Repeat('*', this.Pin.Length).ToArray());
            }
            else if (oldValue.Length > newValue.Length)
            {
                this.Pin = this.Pin.Substring(0, newValue.Length);
                this.PinMasked = new string(Enumerable.Repeat('*', this.Pin.Length).ToArray());
            }
        }

        private string RemoveInvalidChars(string input)
        {
            List<char> chars = new List<char>();

            foreach (var ch in input)
            {
                if (char.IsNumber(ch))
                {
                    chars.Add(ch);
                }
            }

            return new string(chars.ToArray());
        }
        public void CloseCurrentWindow(bool state)
        {
            OnClose();
        }
        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            //if (string.IsNullOrEmpty(this.PinMasked))
            //{
            //    this.PinErrorMessage = "'pin' is required";
            //    validationResults.Add(FieldValidationResult.CreateError(PinMaskedProperty, this.PinErrorMessage,
            //                                                            "PropertyValidation"));
            //}

            if (this.PinMasked.Length != 4)
            {
                this.PinErrorMessage = "'pin' must be 4 symbols";
                validationResults.Add(FieldValidationResult.CreateError(PinMaskedProperty, this.PinErrorMessage,
                                                                        "PropertyValidation"));
            }
        }

    }
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;

namespace SportBetting.WPF.Prism.ModalWindows.ViewModels
{

    public class TestInputViewModel : IBaseViewModel
    {

        private string _text = "";
        private SyncObservableCollection<string> _cardNumbers = new SyncObservableCollection<string>();
        private bool _isTicket = true;
        private bool _isCredit;
        private bool _isPayment;
        private bool _isRegistration;
        private object _itemsLock1 = new object();
        public MessageStorage Mediator { get; private set; }
        private IChangeTracker _changeTracker;
        public IChangeTracker ChangeTracker
        {
            get
            {
                return _changeTracker ?? (_changeTracker = IoCContainer.Kernel.Get<IChangeTracker>());
            }
        }
        #region Constructors

        public TestInputViewModel()
        {
            Mediator = new MessageStorage();
            BarcodeCommand = new Command(BarcodeClick);
            InsertCardCommand = new Command(InsertCard);
            EjectCardCommand = new Command(EjectCard);
            Mediator.Register<string>(this, WriteCardNumber, MsgTag.TestWriteNumber);
        }




        [AsyncMethod]
        private void EjectCard()
        {
            Mediator.SendMessage<long>(0, MsgTag.CardRemoved);

        }
        [AsyncMethod]
        private void InsertCard()
        {
            Mediator.SendMessage<string>("", MsgTag.StartedCardReading);
            Thread.Sleep(1000);
            Mediator.SendMessage<string>(Text, MsgTag.CardInserted);


        }

        [AsyncMethod]
        private void BarcodeClick()
        {
            BarCodeConverter.Clear();

            Mediator.SendMessage(((char)2), MsgTag.EmulateBarcode);

            Thread.Sleep(10);


            if (IsTicket)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.TICKET).ToString()[0], MsgTag.EmulateBarcode);

            }
            if (IsCredit)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.CREDIT_NOTE).ToString()[0], MsgTag.EmulateBarcode);
            }
            if (IsPayment)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.PAYMENT_NOTE).ToString()[0], MsgTag.EmulateBarcode);
            }
            if (IsRegistration)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.REGISTRATION_NOTE).ToString()[0], MsgTag.EmulateBarcode);
            }

            Thread.Sleep(10);
            foreach (var str in Text)
            {
                Mediator.SendMessage(str, MsgTag.EmulateBarcode);


                Thread.Sleep(10);

            }
            Mediator.SendMessage("\r"[0], MsgTag.EmulateBarcode);

        }

        [AsyncMethod]
        private void WriteCardNumber(string obj)
        {
            CardNumbers.Insert(0, obj);
            Thread.Sleep(100);
            ChangeTracker.CardNumber = obj;

        }

        #endregion


        #region Properties

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public bool IsTicket
        {
            get { return _isTicket; }
            set
            {
                _isTicket = value;
                OnPropertyChanged();
            }
        }

        public bool IsCredit
        {
            get { return _isCredit; }
            set
            {
                _isCredit = value;
                OnPropertyChanged();
            }
        }

        public bool IsPayment
        {
            get { return _isPayment; }
            set
            {
                _isPayment = value;
                OnPropertyChanged();
            }
        }

        public bool IsRegistration
        {
            get { return _isRegistration; }
            set
            {
                _isRegistration = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public Command BarcodeCommand { get; set; }
        public Command InsertCardCommand { get; set; }
        public Command EjectCardCommand { get; set; }

        public SyncObservableCollection<string> CardNumbers
        {
            get { return _cardNumbers; }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsClosed { get; set; }
        public void Close()
        {
            
        }

        public void OnNavigationCompleted()
        {
            
        }

        public UserControl View { get; set; }

        public bool IsReady { get; private set; }

        public Window ViewWindow { get; set; }

        public void ShowMessage(string msg1)
        {
            
        }

        public void ShowError(string obj, EventHandler okClick = null, bool bCreateButtonEvent = false, int iAddCounterSeconds = 0)
        {
            
        }
    }
}
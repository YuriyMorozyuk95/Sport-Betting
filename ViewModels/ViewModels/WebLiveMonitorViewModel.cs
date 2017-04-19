using System;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;

namespace ViewModels.ViewModels
{

    [ServiceAspect]
    public class WebLiveMonitorViewModel : BaseViewModel
    {


        public double Filled;


        public double ItemHeight;
        public double HeaderItemHeight;

        private bool _rotating = false;
        public bool rotating
        {
            get
            {
                return _rotating;
            }

            set
            {
                _rotating = value;
                OnPropertyChanged("rotating");
            }
        }

        public int rotationInterval = 10;

        public int liveOddsPage = 1000;
        #region Constructors

        public WebLiveMonitorViewModel()
        {
            HidePleaseWait = false;
            //LineSr.DataSqlUpdateSucceeded += DataCopy_DataSqlUpdateSucceeded;
            Mediator.Register<string>(this, SetUpBrowser, MsgTag.SetUpBrowser);

            MainGridCreated = new Command<UIElement>(OnMainGridCreated);
            ItemCreated = new Command<UIElement>(OnItemCreated);

            Filled = 0;
            MonitorHeight = 0;
            MonitorWidth = 0;
            BrowserWidth = 0;

            ItemHeight = 0;
            HeaderItemHeight = 0;
        }

        #endregion

        #region Properties

        private bool _allowWebBrowser = false;
        public bool AllowWebBrowser
        {
            get
            {
                return _allowWebBrowser;
            }
            set
            {
                _allowWebBrowser = value;
                _webBrowserVisibility = _allowWebBrowser ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged("WebBrowserVisibility");
                OnPropertyChanged("AllowWebBrowser");
            }
        }

        private int _browserWidth = 0;
        public int BrowserWidth
        {
            get { return _browserWidth; }
            set { _browserWidth = value; OnPropertyChanged("BrowserWidth"); }
        }

        private int _browserHeight = 0;
        public int BrowserHeight
        {
            get { return _browserHeight; }
            set { _browserHeight = value; OnPropertyChanged("BrowserHeight"); }
        }

        private double _monitorHeight = 0.00;
        public double MonitorHeight
        {
            get { return _monitorHeight; }
            set { _monitorHeight = value; OnPropertyChanged("MonitorHeight"); }
        }

        private double _monitorWidht = 0.00;
        public double MonitorWidth
        {
            get { return _monitorWidht; }
            set
            {
                _monitorWidht = value;
                SetBrowserWidth((int)_monitorWidht);
                SetBrowserHeight((int)_monitorWidht);
                OnPropertyChanged("MonitorWidth");
            }
        }




        private Visibility _webBrowserVisibility = Visibility.Collapsed;
        public Visibility WebBrowserVisibility
        {
            get { return _webBrowserVisibility; }
            set
            {
                _webBrowserVisibility = value;

                OnPropertyChanged("WebBrowserVisibility");
            }
        }

        private string _webAddress = "about:blank";
        public string WebAddress
        {
            get
            {
                return _webAddress;
            }
            set
            {
                _webAddress = value;
                OnPropertyChanged("WebAddress");
            }
        }

        #endregion

        #region Commands
        public Command<UIElement> MainGridCreated { get; set; }
        public Command<UIElement> ItemCreated { get; set; }
        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            IsClosed = false;
            if (HidePleaseWait)
            {
                WaitOverlayProvider.DisposeAll();
                Log.DebugFormat("hide Please wait:{0}", this.ToString());

            }
            Log.DebugFormat("navigated:{0}", this.ToString());

            //Mediator.ApplyRegistration();
            IsReady = true;
            if (View != null)
                View.Loaded -= View_Loaded;
            if (ViewWindow != null)
            {
                _viewWindow.Loaded -= View_Loaded;

            }

            if (this.ViewWindow != null)
                this.ViewWindow.Activated += ViewWindow_FocusableChanged;
        }

        public override IDispatcher Dispatcher { get; set; }

        private void ViewWindow_FocusableChanged(object sender, EventArgs eventArgs)
        {
            Mediator.SendMessage("", MsgTag.GetFocus);
        }



        private void SetUpBrowser(string obj)
        {
            WebAddress = obj.Replace("1024", "1920");
            if (obj != "" && obj != "about:blank")
            {
                AllowWebBrowser = true;
            }
            else
            {
                AllowWebBrowser = false;
            }
        }

        private void OnMainGridCreated(UIElement obj)
        {
            //if (MonitorHeight != 0 && MonitorHeight >= obj.RenderSize.Height)
            //    return;

            MonitorHeight = obj.RenderSize.Height;
            MonitorWidth = obj.RenderSize.Width;
            SetBrowserWidth((int)MonitorWidth);

        }

        private void OnItemCreated(UIElement obj)
        {
            //if (ItemHeight != 0 && HeaderItemHeight != 0)
            //    return;

            if (HeaderItemHeight < obj.RenderSize.Height)
                HeaderItemHeight = obj.RenderSize.Height;

            if (ItemHeight != obj.RenderSize.Height && obj.RenderSize.Height != HeaderItemHeight)
                ItemHeight = obj.RenderSize.Height;
        }

        private void SetBrowserWidth(int monitorWidth)
        {
            if (monitorWidth <= 1250) BrowserWidth = 1024;
            else if (monitorWidth <= 1350) BrowserWidth = 1280;
            else if (monitorWidth <= 1900) BrowserWidth = 1366;
            else { BrowserWidth = 1920; }
        }

        private void SetBrowserHeight(int monitorWidth)
        {
            if (monitorWidth <= 1250) BrowserHeight = 768;
            else if (monitorWidth <= 1350) BrowserHeight = 1024;
            else if (monitorWidth <= 1900) BrowserHeight = 768;
            else { BrowserHeight = 1080; }
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaseObjects;
using IocContainer;
using MainWpfWindow.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Rhino.Mocks;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.ViewModels;
using SportRadar.DAL.CommonObjects;
using ViewModels.ViewModels;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;

namespace UiTests
{
    [TestClass]
    public class ErrorWindowTests : BaseClass
    {

        [TestMethod]
        [Timeout(20000)]
        public void ShowErrorTest()
        {


            var ErrorWindowService = new ErrorWindowService();
            var QuestionWindowService = new QuestionWindowService();
            var EnterPinWindowService = new EnterPinWindowService();
            AuthorizationService = new AuthorizationService();


            StationRepository = MockRepository.GenerateStub<IStationRepository>();
            LanguageRepository = MockRepository.GenerateStub<ILanguageRepository>();
            LineProvider = MockRepository.GenerateStub<ILineProvider>();
            BusinessPropsHelper = MockRepository.GenerateStub<IBusinessPropsHelper>();
            DataBinding = MockRepository.GenerateStub<IDataBinding>();


            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            IoCContainer.Kernel.Bind<IAuthorizationService>().ToConstant<IAuthorizationService>(AuthorizationService).InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILanguageRepository>().ToConstant<ILanguageRepository>(LanguageRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILineProvider>().ToConstant<ILineProvider>(LineProvider).InSingletonScope();
            IoCContainer.Kernel.Bind<IBusinessPropsHelper>().ToConstant<IBusinessPropsHelper>(BusinessPropsHelper).InSingletonScope();
            IoCContainer.Kernel.Bind<IErrorWindowService>().ToConstant<IErrorWindowService>(ErrorWindowService).InSingletonScope();
            IoCContainer.Kernel.Bind<IQuestionWindowService>().ToConstant<IQuestionWindowService>(QuestionWindowService).InSingletonScope();
            IoCContainer.Kernel.Bind<IEnterPinWindowService>().ToConstant<IEnterPinWindowService>(EnterPinWindowService).InSingletonScope();


            DataBinding.Expect(x => x.TipListInfo).Return(new TipListInfo());
           


            SessionWS sessionId = new SessionWS();
            sessionId.balance = new accountBalance();
            sessionId.username = "test";
            SessionWS sessionIdNull = null;


            WsdlRepository.BackToRecord();
            WsdlRepository.Expect(x => x.OpenSession("", true, "", "", false)).Return(new SessionWS()).IgnoreArguments();
            WsdlRepository.Expect(x => x.ValidateIdCard("1234", "0024", false, out sessionId)).Return(true).OutRef(sessionIdNull);
            WsdlRepository.Expect(x => x.LoginWithIdCard("0024", "1234", "1234")).Return(sessionId);
            WsdlRepository.Replay();

            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.AllowAnonymousBetting).Return(true);
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.Active).Return(1);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());
            StationRepository.Currency = "EUR";

            ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            ChangeTracker.CurrentUser.Cashpool = 100000;
            ChangeTracker.CurrentUser.AvailableCash = 100000;




            BaseClass.Dispatcher.Invoke(() =>
                {
                    BaseClass.Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                    BaseClass.Window.Show();
                });



            Thread.Sleep(1000);

            var mainModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion);

            Task.Run(() =>
                {
                    MessageMediator.SendMessage("1234", MsgTag.CardInserted);
                });

            while (EnterPinWindowService.Model == null)
            {
                Thread.Sleep(10);
            }

            Thread.Sleep(1000);

            EnterPinWindowService.Model.Pin = "1234";
            EnterPinWindowService.Model.PinMasked = "1234";
            EnterPinWindowService.Model.ProceedCommand.Execute("");

            Task.Run(() =>
                { mainModel.ShowMessage("msg1"); });
            Thread.Sleep(1000);

            QuestionWindowService.Model.YesCommand.Execute("");

            Task.Run(() =>
                { mainModel.ShowError("error1"); });
            Thread.Sleep(1000);


            ErrorWindowService.Model.OKCommand.Execute("");


            Task.Run(() =>
                { mainModel.ShowMessage("msg2"); });
            Thread.Sleep(1000);
            QuestionWindowService.Model.YesCommand.Execute("");

            Task.Run(() =>
                { mainModel.ShowError("error2"); });
            Thread.Sleep(1000);
            ErrorWindowService.Model.OKCommand.Execute("");


            Task.Run(() =>
                { QuestionWindowService.ShowMessage("mgs3", null, null, null, null, false, 1); });
            Thread.Sleep(3000);

            BaseClass.Dispatcher.Invoke(() =>
                {
                    BaseClass.Window.Close();


                });



        }

        [TestMethod]
        [Timeout(20000)]
        [Ignore]
        public void Show2ErrorsTest()
        {


            var ErrorWindowService = new ErrorWindowService();
            var QuestionWindowService = new QuestionWindowService();
            var EnterPinWindowService = new EnterPinWindowService();
            AuthorizationService = new AuthorizationService();


            StationRepository = MockRepository.GenerateStub<IStationRepository>();
            LanguageRepository = MockRepository.GenerateStub<ILanguageRepository>();
            LineProvider = MockRepository.GenerateStub<ILineProvider>();
            BusinessPropsHelper = MockRepository.GenerateStub<IBusinessPropsHelper>();
            DataBinding = MockRepository.GenerateStub<IDataBinding>();


            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            IoCContainer.Kernel.Bind<IAuthorizationService>().ToConstant<IAuthorizationService>(AuthorizationService).InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILanguageRepository>().ToConstant<ILanguageRepository>(LanguageRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILineProvider>().ToConstant<ILineProvider>(LineProvider).InSingletonScope();
            IoCContainer.Kernel.Bind<IBusinessPropsHelper>().ToConstant<IBusinessPropsHelper>(BusinessPropsHelper).InSingletonScope();
            IoCContainer.Kernel.Bind<IErrorWindowService>().ToConstant<IErrorWindowService>(ErrorWindowService).InSingletonScope();
            IoCContainer.Kernel.Bind<IQuestionWindowService>().ToConstant<IQuestionWindowService>(QuestionWindowService).InSingletonScope();
            IoCContainer.Kernel.Bind<IEnterPinWindowService>().ToConstant<IEnterPinWindowService>(EnterPinWindowService).InSingletonScope();


            DataBinding.Expect(x => x.TipListInfo).Return(new TipListInfo());
           


            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.AllowAnonymousBetting).Return(true);
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.Active).Return(1);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());
            StationRepository.Currency = "EUR";


            ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            ChangeTracker.CurrentUser.Cashpool = 100000;
            ChangeTracker.CurrentUser.AvailableCash = 100000;



            Dispatcher.Invoke(() =>
            {
                Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                Window.Show();
            });




            var mainModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion);

            while (!mainModel.IsReady)
            {
                Thread.Sleep(1);
            }


            new Thread(() =>
                {
                    Task.Delay(1);
                    MessageMediator.SendMessage<Tuple<string, string, bool, int>>(new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0), MsgTag.Error);
                    MessageMediator.SendMessage<Tuple<string, string, bool, int>>(new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0), MsgTag.Error);

                }).Start();

            MessageMediator.SendMessage<Tuple<string, string, bool, int>>(new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0), MsgTag.Error);

            int windowCount = 0;

            Thread.Sleep(1000);
            Dispatcher.Invoke(() =>
                {
                    windowCount = App.Current.Windows.Count;
                });

            Assert.AreEqual(4, windowCount);


            Thread.Sleep(1000);

            Dispatcher.Invoke(() =>
            {
                windowCount = App.Current.Windows.Count;
            });

            Assert.AreEqual(1, windowCount);

            Dispatcher.Invoke(() =>
            {
                Window.Close();


            });



        }
    }
}
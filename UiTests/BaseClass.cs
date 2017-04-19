using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using Catel.Messaging;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Rhino.Mocks;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.oldcode;

namespace UiTests
{
    [TestClass]
    public class BaseClass
    {

        public IRepository Repository;
        public IAuthorizationService AuthorizationService;
        public IStationRepository StationRepository;
        public ILanguageRepository LanguageRepository;
        public ILineProvider LineProvider;
        public IWsdlRepository WsdlRepository;
        public IBusinessPropsHelper BusinessPropsHelper;
        public ITranslationProvider TranslationProvider;
        public IDataBinding DataBinding;
        public IMyRegionManager MyRegionManager;
        public IChangeTracker ChangeTracker;
        public IMediator MessageMediator;
        public IErrorWindowService ErrorWindowService;
        public IQuestionWindowService QuestionWindowService;
        public IEnterPinWindowService EnterPinWindowService;
        public ILineSr LineSr;
        public IConfidenceFactor ConfidenceFactor;
        public ITransactionQueueHelper TransactionQueueHelper;

        public static App App;
        public static Window Window;

        public static Dispatcher Dispatcher;


        [TestInitialize]
        public void StartUp()
        {
            if (App == null)
            {
                Thread newWindowThread = new Thread((input) =>
                    {
                        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

                        App = new App();
                        App.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                        var foo = new Uri("pack://application:,,,/DefaultViews;component/Resources/CommonStyles.xaml", UriKind.Absolute);
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = foo });
                        var foo2 = new Uri("pack://application:,,,/DefaultViews;component/Resources/DataTemplates.xaml", UriKind.Absolute);
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = foo2 });
                        var foo3 = new Uri("pack://application:,,,/DefaultViews;component/Resources/LiveDataTemplates.xaml", UriKind.Absolute);
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = foo3 });

                        App.Run();
                    });

                newWindowThread.SetApartmentState(ApartmentState.STA);
                newWindowThread.IsBackground = true;

                newWindowThread.Start();
                do
                {
                    Thread.Sleep(10);
                    Dispatcher = Dispatcher.FromThread(newWindowThread);
                } while (Dispatcher == null);
            }
            var dispatcher = new MyDispatcher(Dispatcher);

            IoCContainer.Kernel = new StandardKernel();
            IoCContainer.Kernel.Bind<IWaitOverlayProvider>().To<WaitOverlayProvider>().InSingletonScope();
            IoCContainer.Kernel.Bind<ITicketHandler>().To<TicketHandler>().InSingletonScope();
            
            ChangeTracker = new ChangeTracker();
            IoCContainer.Kernel.Bind<IChangeTracker>().ToConstant<IChangeTracker>(ChangeTracker).InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().To<StationRepository>().InSingletonScope();


            MessageMediator = new MyMessageMediator();
            MyRegionManager = new MyRegionManager();
            TranslationProvider = MockRepository.GenerateStub<ITranslationProvider>();
            LineSr = MockRepository.GenerateStub<ILineSr>();
            Repository = MockRepository.GenerateStub<IRepository>();
            WsdlRepository = MockRepository.GenerateStub<IWsdlRepository>();
            ConfidenceFactor = MockRepository.GenerateStub<IConfidenceFactor>();
            TransactionQueueHelper = MockRepository.GenerateStub<ITransactionQueueHelper>();

            IoCContainer.Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator).InSingletonScope();
            IoCContainer.Kernel.Bind<IMyRegionManager>().ToConstant<IMyRegionManager>(MyRegionManager).InSingletonScope();
            IoCContainer.Kernel.Bind<IDispatcher>().ToConstant<IDispatcher>(dispatcher).InSingletonScope();
            IoCContainer.Kernel.Bind<IRepository>().ToConstant<IRepository>(Repository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILineSr>().ToConstant<ILineSr>(LineSr).InSingletonScope();
            IoCContainer.Kernel.Bind<ITranslationProvider>().ToConstant<ITranslationProvider>(TranslationProvider).InSingletonScope();
            IoCContainer.Kernel.Bind<IWsdlRepository>().ToConstant<IWsdlRepository>(WsdlRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor).InSingletonScope();
            IoCContainer.Kernel.Bind<ITransactionQueueHelper>().ToConstant<ITransactionQueueHelper>(TransactionQueueHelper).InSingletonScope();

            long lastTicketNumber = 0;
            long lastTransactionId = 0;
            WsdlRepository.Stub(x => x.GetBusinessProps(null, out lastTicketNumber,out lastTransactionId)).Return(1).IgnoreArguments();
            Repository.Stub(x => x.FindMatches(null, null, null, null, null)).Return(new SortableObservableCollection<IMatchVw>()).IgnoreArguments();
            TransactionQueueHelper.Stub(x => x.GetCountTransactionQueue()).Return(1).IgnoreArguments();

            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_RESULTS_MINUS_X_DAYS)).Return("{0}");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_MANIPULATION_FEE)).Return("ManipulationFee");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_BONUS)).Return("bonus");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_MINBET)).Return("min bet");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_MAXBET)).Return("max bet");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_MAXWIN)).Return("Max Win");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_AVAILABLE_CASH)).Return("Available cash");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_ODD)).Return("Odd:");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_PRINT_STAKE_TOTAL)).Return("Total Stake");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.CLOSE)).Return("Close");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.OUTRIGHTS)).Return("Outrights");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_PLEASE_ADD_AMOUNT)).Return("{0}");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.HELLO_MSG)).Return("hello {0}");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_CHANGE_PASSWORD)).Return("change password");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_FORM_ALL)).Return("all");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_TICKETWON)).Return("all");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_TICKETWON)).Return("won");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_TICKETLOST)).Return("lost");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_FORM_CANCELLED)).Return("canceled");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_TICKETOPEN)).Return("open");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.SHOW_SELECTED)).Return("show selected");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_NAVIGATION_FORWARD)).Return("forward");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.SHOP_FORM_NO_CONNECTION_TO_SERVER)).Return("no conenction");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TRANSFER_TO_ACCOUNT)).Return("to account");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN)).Return("locked by admin");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_CASH_LOCKED)).Return("locked cash");
            TranslationProvider.Stub(x => x.Translate(MultistringTags.TERMINAL_FORM_REQUIRED)).Return("required");




        }

        [TestCleanup]
        public void close()
        {
            SportRadar.Common.Windows.ThreadHelper.StopAll();
        }

    }
}
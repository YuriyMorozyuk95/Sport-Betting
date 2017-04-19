using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BaseObjects;
using IocContainer;
using MVVMTest;
using MainWpfWindow.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Rhino.Mocks;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Accounting.ViewModels;
using SportBetting.WPF.Prism.Modules.UserManagement.ViewModels;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.ViewModels;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using ViewModels.ViewModels;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;
using UserProfileViewModel = ViewModels.ViewModels.UserProfileViewModel;


namespace UiTests
{
    [TestClass]
    public class UiTest : BaseClass
    {
        [TestMethod]
        [Timeout(200000)]
        public void OpenShopPaymentsViewModelNavigateBackTest()
        {


            DataBinding = MockRepository.GenerateStub<IDataBinding>();
            AuthorizationService = MockRepository.GenerateStub<IAuthorizationService>();
            StationRepository = MockRepository.GenerateStub<IStationRepository>();
            LanguageRepository = MockRepository.GenerateStub<ILanguageRepository>();
            LineProvider = MockRepository.GenerateStub<ILineProvider>();
            BusinessPropsHelper = MockRepository.GenerateStub<IBusinessPropsHelper>();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            IoCContainer.Kernel.Bind<IAuthorizationService>().ToConstant<IAuthorizationService>(AuthorizationService).InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILanguageRepository>().ToConstant<ILanguageRepository>(LanguageRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILineProvider>().ToConstant<ILineProvider>(LineProvider).InSingletonScope();
            IoCContainer.Kernel.Bind<IBusinessPropsHelper>().ToConstant<IBusinessPropsHelper>(BusinessPropsHelper).InSingletonScope();




            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.SyncInterval).Return(20000);
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());



            Dispatcher.Invoke(() =>
                {
                    Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                    Window.Show();
                });

            ChangeTracker.CurrentUser = new OperatorUser("111") { Permissions = new string[] { "read_shop_payments", "write_shop_payments" } };

            var userManagementViewModel = MyRegionManager.NavigateUsingViewModel<UserManagementViewModel>(RegionNames.ContentRegion);

            decimal paymentBalance = 0;
            decimal locationCashPosition = 0;
            decimal totalLocationBalance = 0;
            PaymentFlowData[] list = new PaymentFlowData[3] { new PaymentFlowData(), new PaymentFlowData(), new PaymentFlowData(), };
            long total = 0;
            WsdlRepository.Expect(x => x.GetStationPaymentFlowData("0024", 0, 20, out paymentBalance, out locationCashPosition, out totalLocationBalance, out list, out total)).Return(10).OutRef(locationCashPosition, totalLocationBalance, paymentBalance, list, total).IgnoreArguments();

            Dispatcher.Invoke(() =>
                {
                    userManagementViewModel.ShopPaymentsCommand.Execute(null);
                });
            Thread.Sleep(1000);
            var shopPayments = MyRegionManager.CurrentViewModelInRegion(RegionNames.UsermanagementContentRegion) as ShopPaymentsViewModel;
            while (shopPayments.Payments.Count < 3)
            {
                shopPayments = MyRegionManager.CurrentViewModelInRegion(RegionNames.UsermanagementContentRegion) as ShopPaymentsViewModel;
                Thread.Sleep(100);
            }
            for (int i = 0; i < 5; i++)
            {

                shopPayments = MyRegionManager.CurrentViewModelInRegion(RegionNames.UsermanagementContentRegion) as ShopPaymentsViewModel;

                while (!shopPayments.IsReady)
                {
                    Thread.Sleep(1);
                }

                Dispatcher.Invoke(() =>
                    {
                        shopPayments.onAddPaymentClicked.Execute("");
                    });

                var addcreditModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.UsermanagementContentRegion) as AddCreditPaymentViewModel;
                while (!addcreditModel.IsReady)
                {
                    Thread.Sleep(1);
                }
                Dispatcher.Invoke(() =>
                    { addcreditModel.CloseCommand.Execute(""); });

            }


            shopPayments = MyRegionManager.CurrentViewModelInRegion(RegionNames.UsermanagementContentRegion) as ShopPaymentsViewModel;
            while (shopPayments.Payments.Count < 3)
            {
                shopPayments = MyRegionManager.CurrentViewModelInRegion(RegionNames.UsermanagementContentRegion) as ShopPaymentsViewModel;
                Thread.Sleep(100);
            }



            Dispatcher.Invoke(() =>
            {
                Window.Close();
            });


        }

        [TestMethod]
        [Timeout(200000)]
        public void OpenliveTest()
        {


            DataBinding = MockRepository.GenerateStub<IDataBinding>();
            AuthorizationService = MockRepository.GenerateStub<IAuthorizationService>();
            StationRepository = MockRepository.GenerateStub<IStationRepository>();
            LanguageRepository = MockRepository.GenerateStub<ILanguageRepository>();
            LineProvider = MockRepository.GenerateStub<ILineProvider>();
            BusinessPropsHelper = MockRepository.GenerateStub<IBusinessPropsHelper>();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            IoCContainer.Kernel.Bind<IAuthorizationService>().ToConstant<IAuthorizationService>(AuthorizationService).InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILanguageRepository>().ToConstant<ILanguageRepository>(LanguageRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILineProvider>().ToConstant<ILineProvider>(LineProvider).InSingletonScope();
            IoCContainer.Kernel.Bind<IBusinessPropsHelper>().ToConstant<IBusinessPropsHelper>(BusinessPropsHelper).InSingletonScope();




            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.SyncInterval).Return(20000);
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());



            Dispatcher.Invoke(() =>
            {
                Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                Window.Show();
            });

            var start = DateTime.Now;
            //var match = new MatchVw(new MatchLn());
            var match = TestMatchVw.CreateMatch(1, true);
            var match2 = TestMatchVw.CreateMatch(2, true);
            var match3 = TestMatchVw.CreateMatch(3, true);
            ((TestMatchVw)match).SportDescriptor = SportSr.SPORT_DESCRIPTOR_TENNIS;
            ((TestMatchVw)match2).LiveBetStatus = eMatchStatus.Started;
            ((TestMatchVw)match2).SportDescriptor = SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY;
            //((TestMatchVw)match2).LiveMatchMinute = 10;
            ((TestMatchVw)match2).ShomMinutes = Visibility.Visible;
            ((TestMatchVw)match2).LivePeriodInfoString = "10";
            ((TestMatchVw)match3).StartDate = DateTime.Now.AddDays(1);
            ((TestMatchVw)match3).SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER;
            var matches = new SortableObservableCollection<IMatchVw>()
                {
                    match,
                    match2,
                    match3,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                };

            Repository.BackToRecord();

            Repository.Expect(x => x.FindMatches(null, null, null, null, null)).Return(matches).IgnoreArguments();
            Repository.Replay();
            var userManagementViewModel = MyRegionManager.NavigateUsingViewModel<LiveViewModel>(RegionNames.ContentRegion);
            while (userManagementViewModel.IsReady)
            {
                Thread.Sleep(1);
            }
            Console.WriteLine(DateTime.Now-start);

            Thread.Sleep(800);

            Dispatcher.Invoke(() =>
            {
                Window.Close();
            });


        } 
        
        [TestMethod]
        [Timeout(200000)]
        public void Openlive2Test()
        {


            DataBinding = MockRepository.GenerateStub<IDataBinding>();
            AuthorizationService = MockRepository.GenerateStub<IAuthorizationService>();
            StationRepository = MockRepository.GenerateStub<IStationRepository>();
            LanguageRepository = MockRepository.GenerateStub<ILanguageRepository>();
            LineProvider = MockRepository.GenerateStub<ILineProvider>();
            BusinessPropsHelper = MockRepository.GenerateStub<IBusinessPropsHelper>();
            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            IoCContainer.Kernel.Bind<IAuthorizationService>().ToConstant<IAuthorizationService>(AuthorizationService).InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILanguageRepository>().ToConstant<ILanguageRepository>(LanguageRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILineProvider>().ToConstant<ILineProvider>(LineProvider).InSingletonScope();
            IoCContainer.Kernel.Bind<IBusinessPropsHelper>().ToConstant<IBusinessPropsHelper>(BusinessPropsHelper).InSingletonScope();




            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.SyncInterval).Return(20000);
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());



            Dispatcher.Invoke(() =>
            {
                Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                Window.Show();
            });

            var start = DateTime.Now;
            //var match = new MatchVw(new MatchLn());
            var match = TestMatchVw.CreateMatch(1, true);
            var match2 = TestMatchVw.CreateMatch(2, true);
            var match3 = TestMatchVw.CreateMatch(3, true);
            var match4 = TestMatchVw.CreateMatch(4, true);
            var match5 = TestMatchVw.CreateMatch(5, true);
            ((TestMatchVw)match).SportDescriptor = SportSr.SPORT_DESCRIPTOR_TENNIS;
            ((TestMatchVw)match).AllEnabledOddCount = 10;
            ((TestMatchVw)match).LiveColor = "#AAd01f00";
            ((TestMatchVw)match).LivePeriodInfo = eLivePeriodInfo.Tennis_1st_Set;

            ((TestMatchVw)match2).IsHeader = true;
            ((TestMatchVw)match2).LiveBetStatus = eMatchStatus.Started;
            ((TestMatchVw)match2).LivePeriodInfo = eLivePeriodInfo.Tennis_1st_Set;
           ((TestMatchVw)match2).SportDescriptor = SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY;
           ((TestMatchVw)match2).ShomMinutes = Visibility.Visible;
           ((TestMatchVw)match2).LivePeriodInfoString = "10";
           ((TestMatchVw)match2).AllEnabledOddCount = 10;

           ((TestMatchVw)match3).StartDate = DateTime.Now.AddDays(1);
           ((TestMatchVw)match3).SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER;
           ((TestMatchVw)match3).AllEnabledOddCount = 10;

           ((TestMatchVw)match4).IsEnabled = false;
           ((TestMatchVw)match4).LiveBetStatus = eMatchStatus.Started;
           ((TestMatchVw)match4).LivePeriodInfo = eLivePeriodInfo.Interrupted;
           ((TestMatchVw)match4).SportDescriptor = SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY;
           ((TestMatchVw)match4).ShomMinutes = Visibility.Visible;
           ((TestMatchVw)match4).LivePeriodInfoString = "10";
           ((TestMatchVw)match4).AllEnabledOddCount = 10;          
            
           ((TestMatchVw)match5).LiveBetStatus = eMatchStatus.Started;
           ((TestMatchVw)match5).LivePeriodInfo = eLivePeriodInfo.Interrupted;
           ((TestMatchVw)match5).SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER;
           ((TestMatchVw)match5).ShomMinutes = Visibility.Visible;
           ((TestMatchVw)match5).LivePeriodInfoString = "10";
           ((TestMatchVw)match5).AllEnabledOddCount = 10;

           var matches = new SortableObservableCollection<IMatchVw>()
                {
                    match,
                    match,
                    match2,
                    match2,
                    match5,
                    match5,
                    match4,
                    match4,
                    match3,
                    match3,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                    match,
                };

            Repository.BackToRecord();

            Repository.Expect(x => x.FindMatches(null, null, null, null, null)).Return(matches).IgnoreArguments();
            Repository.Replay();
            var userManagementViewModel = MyRegionManager.NavigateUsingViewModel<LiveViewModel>(RegionNames.ContentRegion);
            while (userManagementViewModel.IsReady)
            {
                Thread.Sleep(1);
            }
            Console.WriteLine(DateTime.Now-start);

            Thread.Sleep(100000);

            Dispatcher.Invoke(() =>
            {
                Window.Close();
            });


        }



        [TestMethod]
        [Timeout(2000000)]
        public void OpenBasketTest()
        {



            AuthorizationService = MockRepository.GenerateStub<IAuthorizationService>();
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


            DataBinding.Expect(x => x.TipListInfo).Return(new TipListInfo());



            Ticket ticket = new Ticket();
            ticket.Stake = 10;
            var matchVw = new TestMatchVw()
                {
                    HomeCompetitorName = "Home competitor",
                    AwayCompetitorName = "Away competitor",
                    TournamentNameToShow = "tournament",
                    IsEnabled = true,
                    LiveScore = "0:0(1:1)",
                    VisibleOddCount = 120,

                };
            var match = new TestMatchLn()
                {
                    MatchView = matchVw,
                    MatchId = 1
                };
            var betdomainVw = new TestBetDomainVw()
                {
                    IsEnabled = true,
                };
            var betdomain = new TestBetDomain()
            {
                BetDomainId = 1,
                Match = match,
                BetDomainView = betdomainVw
            };
            var oddVw = new TestOddVw()
                {
                    DisplayName = "sel 1",
                    Value = 100.21m
                };
            var testOdd = new TestOdd()
            {
                OutcomeId = 1,
                OddId = { Value = 1 },
                BetDomain = betdomain,
                Value = { Value = 1.85M },
                OddView = oddVw,
            };
            var tipitem = new TestTipItem(testOdd)
                {
                    IsEnabled = true,
                    BetDomainName = "Total (2.5)",
                    IsBankEnabled = true,
                    IsBank = true,
                    IsBankEditable = true,
                };
            var tipitem2 = new TestTipItem(testOdd)
            {
                IsEnabled = false,
                BetDomainName = "Total (2.5)",
                IsChecked = true,

            };

            ticket.TipItems.Add(tipitem);
            ticket.TipItems.Add(tipitem2);




            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.AllowAnonymousBetting).Return(true);
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());
            StationRepository.Currency = "EUR";

            ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1", 1);
            ChangeTracker.CurrentUser.Cashpool = 100000;
            ChangeTracker.CurrentUser.AvailableCash = 100000;



            Dispatcher.Invoke(() =>
                {
                    Window = MyRegionManager.FindWindowByViewModel<BasketViewModel>();
                    Window.Show();


                });

            Thread.Sleep(1000);

            Dispatcher.Invoke(() =>
            {
                Window.Close();
            });


        }

        [TestMethod]
        [Timeout(2000000)]
        public void OpenUserTicketsTest()
        {
            AuthorizationService = MockRepository.GenerateStub<IAuthorizationService>();
            StationRepository = MockRepository.GenerateStub<IStationRepository>();
            LanguageRepository = MockRepository.GenerateStub<ILanguageRepository>();
            LineProvider = MockRepository.GenerateStub<ILineProvider>();
            BusinessPropsHelper = MockRepository.GenerateStub<IBusinessPropsHelper>();
            DataBinding = MockRepository.GenerateStub<IDataBinding>();
            ConfidenceFactor = MockRepository.GenerateStub<IConfidenceFactor>();


            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            IoCContainer.Kernel.Bind<IAuthorizationService>().ToConstant<IAuthorizationService>(AuthorizationService).InSingletonScope();
            IoCContainer.Kernel.Bind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILanguageRepository>().ToConstant<ILanguageRepository>(LanguageRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILineProvider>().ToConstant<ILineProvider>(LineProvider).InSingletonScope();
            IoCContainer.Kernel.Bind<IBusinessPropsHelper>().ToConstant<IBusinessPropsHelper>(BusinessPropsHelper).InSingletonScope();
            IoCContainer.Kernel.Bind<IConfidenceFactor>().ToConstant<IConfidenceFactor>(ConfidenceFactor).InSingletonScope();

            ConfidenceFactor.Expect(x => x.CalculateFactor(new Ticket())).Return(1000000).IgnoreArguments();

            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.AllowAnonymousBetting).Return(true);
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());
            StationRepository.Currency = "EUR";

            ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1", 1);
            ChangeTracker.CurrentUser.Cashpool = 100000;
            ChangeTracker.CurrentUser.AvailableCash = 100000;


            Dispatcher.Invoke(() =>
            {
                Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                Window.Show();

            });

            Thread.Sleep(1000);

            ChangeTracker.CurrentUser = new LoggedInUser(1, "111", 1000) { Username = "test" };

            var header = MyRegionManager.CurrentViewModelInRegion(RegionNames.HeaderRegion) as HeaderViewModel;

            header.OpenAuthorizationCommand.Execute("");

            Thread.Sleep(1000);

            var userProfileMenuViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.UserProfileMenuRegion) as UserProfileMenuViewModel;

            var ticket = new UserTicket() { checkSum = "1234", createdAt = DateTime.Now, ticketNumber = "1241252352346" };
            string total;
            WsdlRepository.Expect(x => x.GetUserTickets("1", (ticketCategory)1, new AccountTicketSorting() { field = AccountTicketSortingFields.DateCreated, value = AccountTicketSortingValues.Desc }, 0, 20, out total)).Return(new UserTicket[] { ticket }).OutRef("1000").IgnoreArguments();

            userProfileMenuViewModel.ShowTicketsCommand.Execute("");

            Thread.Sleep(1000);




            Assert.AreEqual(1, ChangeTracker.Tickets.Count);


            var userTicketsViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.UserProfileContentRegion) as UserTicketsViewModel;
            Assert.AreEqual(1, userTicketsViewModel.TicketsStartPage);

            Dispatcher.Invoke(() =>
            {
                userTicketsViewModel.SelectedType = userTicketsViewModel.TicketType[3];
            });
            Dispatcher.Invoke(() =>
                {
                    userTicketsViewModel.NextPage.Execute("");
                });



            Assert.AreEqual(2, userTicketsViewModel.TicketsStartPage);
            Assert.AreEqual(1, userTicketsViewModel.Tickets.Count);

            if (1000 % userTicketsViewModel.Pagesize > 0)
                Assert.AreEqual(1000 / userTicketsViewModel.Pagesize + 1, userTicketsViewModel.AllPages);

            if (1000 % userTicketsViewModel.Pagesize == 0)
                Assert.AreEqual(1000 / userTicketsViewModel.Pagesize, userTicketsViewModel.AllPages);

            Assert.AreEqual(2, userTicketsViewModel.TicketsStartPage);
            Assert.AreEqual(3, userTicketsViewModel.SelectedType.Id);

            userTicketsViewModel.ShowTicketCommand.Execute(userTicketsViewModel.Tickets[0]);

            var nextModel = MyRegionManager.CurrentViewModelType(RegionNames.UserProfileContentRegion);
            Assert.AreEqual(typeof(TicketDetailsViewModel), nextModel);

            UserProfileViewModel userprofileWindow = null;
            Dispatcher.Invoke(() =>
                { userprofileWindow = ChangeTracker.UserProfileWindow.DataContext as UserProfileViewModel; });

            WsdlRepository.BackToRecord();

            WsdlRepository.Expect(x => x.GetUserTickets("1", (ticketCategory)1, new AccountTicketSorting() { field = AccountTicketSortingFields.DateCreated, value = AccountTicketSortingValues.Desc }, 0, 20, out total)).Return(new UserTicket[] { ticket, ticket, ticket, ticket }).OutRef("1000").IgnoreArguments();

            WsdlRepository.Replay();
            Dispatcher.Invoke(() =>
                { userprofileWindow.BackCommand.Execute(""); });

            var prevModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.UserProfileContentRegion);

            Thread.Sleep(1000);

            Assert.AreEqual(4, ((UserTicketsViewModel)prevModel).Tickets.Count);
            Assert.IsTrue(((UserTicketsViewModel)prevModel).AllPages > 0);
            Assert.AreEqual(2, userTicketsViewModel.TicketsStartPage);
            Assert.AreEqual(3, userTicketsViewModel.SelectedType.Id);


            userProfileMenuViewModel.LogoutCommand.Execute("");


            Thread.Sleep(1000);
            WsdlRepository.BackToRecord();
            Dispatcher.Invoke(() =>
            {
                Window.Close();
            });


        }






    }
}

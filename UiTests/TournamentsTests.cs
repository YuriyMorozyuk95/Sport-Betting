using System;
using System.Collections.Generic;
using System.Threading;
using BaseObjects;
using IocContainer;
using MVVMTest;
using MainWpfWindow.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Rhino.Mocks;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.ViewModels;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using ViewModels.ViewModels;
using WsdlRepository;
using WsdlRepository.oldcode;

namespace UiTests
{
    [TestClass]
    public class TournamentsTests : BaseClass
    {


        [TestMethod]
        [Timeout(2000000)]
        public void TournamentsViewTest()
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



            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.AllowAnonymousBetting).Return(true);
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());
            StationRepository.Currency = "EUR";

            ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            ChangeTracker.CurrentUser.Cashpool = 100000;
            ChangeTracker.CurrentUser.AvailableCash = 100000;




            SortableObservableCollection<IMatchVw> matches = new SortableObservableCollection<IMatchVw>();
            matches.Add(CreateMatch(1, true));
            matches.Add(CreateMatch(2));
            matches.Add(CreateMatch(2));
            matches.Add(CreateMatch(3, true));
            matches.Add(CreateMatch(4));
            matches.Add(CreateMatch(4));

            Repository.BackToRecord();

            Repository.Stub(x => x.FindMatches(matches, "", "", null, null)).IgnoreArguments().Return(matches);
            Repository.Stub(x => x.FindResults(null, null, null)).IgnoreArguments();

            Repository.Replay();

            Dispatcher.Invoke(() =>
                {
                    Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                    Window.Show();

                });

            Thread.Sleep(1000);
            var firstModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion);

            Assert.IsNotNull(firstModel);


            MyRegionManager.NavigateUsingViewModel<TournamentsViewModel>(RegionNames.ContentRegion);


            Thread.Sleep(1000);
            var model = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TournamentsViewModel;



            Assert.AreEqual(3, model.Tournaments.Count);
            Assert.AreEqual(true, model.Tournaments[0].IsOutrightGroup);
            Assert.AreEqual(2, model.Tournaments[1].MatchesCount);
            Assert.AreEqual(2, model.Tournaments[0].MatchesCount);
            Assert.AreEqual(2, model.Tournaments[1].Id);

            matches.Add(CreateMatch(2));
            matches.Add(CreateMatch(1, true));
            model.Refresh(true);


            Assert.AreEqual(2, model.Tournaments[1].Id);

            Assert.AreEqual(3, model.Tournaments[1].MatchesCount);
            Assert.AreEqual(int.MinValue, model.Tournaments[0].Id);
            Assert.AreEqual(3, model.Tournaments[0].MatchesCount);



            for (int i = 1; i < model.Tournaments.Count; i++)
            {
                Assert.AreEqual(false, model.Tournaments[i].IsOutrightGroup);

            }

            model.Choice.Execute(model.Tournaments[1]);

            Thread.Sleep(1000);

            var matchesmodel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as MatchesViewModel;

            Assert.IsNotNull(matchesmodel);
            Assert.IsFalse(matchesmodel.SelectedTournaments[2]);


            model = MyRegionManager.NavigatBack(RegionNames.ContentRegion) as TournamentsViewModel;

            while (!model.IsReady)
            {
                Thread.Sleep(100);
            }

            model.Choice.Execute(model.Tournaments[0]);

            Thread.Sleep(1000);

            model = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TournamentsViewModel;


            Assert.IsNotNull(model);

            Assert.AreEqual(2, model.Tournaments.Count);
            Assert.IsFalse(model.Tournaments[0].IsOutrightGroup);
            Assert.IsFalse(model.Tournaments[1].IsOutrightGroup);


            model.CheckedBox.Execute(model.Tournaments[0]);

            var header = MyRegionManager.CurrentViewModelInRegion(RegionNames.HeaderRegion) as HeaderViewModel;

            header.NextView.Execute("");


            matchesmodel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as MatchesViewModel;

            Assert.IsTrue(matchesmodel.SelectedTournaments[1]);


            model = MyRegionManager.NavigatBack(RegionNames.ContentRegion) as TournamentsViewModel;
            while (!model.IsReady)
            {
                Thread.Sleep(100);
            }
            Assert.AreEqual(2, model.Tournaments.Count);
            model.CheckedBox.Execute(model.Tournaments[0]);
            model.Choice.Execute(model.Tournaments[0]);


            matchesmodel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as MatchesViewModel;

            while (!matchesmodel.IsReady)
            {
                Thread.Sleep(100);
            }
            Assert.IsNotNull(matchesmodel);
            Assert.IsTrue(matchesmodel.SelectedTournaments[1]);


            model = MyRegionManager.NavigatBack(RegionNames.ContentRegion) as TournamentsViewModel;

            while (!model.IsReady)
            {
                Thread.Sleep(100);
            }


            Assert.AreEqual(2, model.Tournaments.Count);
            Assert.IsFalse(model.Tournaments[0].IsSelected);
            Assert.IsFalse(model.Tournaments[1].IsSelected);
            Assert.IsFalse(model.Tournaments[0].IsOutrightGroup);
            Assert.IsFalse(model.Tournaments[1].IsOutrightGroup);



            model = MyRegionManager.NavigatBack(RegionNames.ContentRegion) as TournamentsViewModel;
            while (!model.IsReady)
            {
                Thread.Sleep(100);
            }

            Assert.IsNotNull(model);

            Assert.AreEqual(3, model.Tournaments.Count);
            Assert.AreEqual(true, model.Tournaments[0].IsOutrightGroup);
            for (int i = 1; i < model.Tournaments.Count; i++)
            {
                Assert.AreEqual(false, model.Tournaments[i].IsOutrightGroup);

            }


            var firstModel2 = MyRegionManager.NavigatBack(RegionNames.ContentRegion);


            Assert.AreEqual((object)firstModel.GetType(), firstModel2.GetType());


            Dispatcher.Invoke(() =>
                {
                    Window.Close();
                });


        }

        [TestMethod]
        [Timeout(2000000)]
        public void TournamentsViewCheckBoxTest()
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




            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.AllowAnonymousBetting).Return(true);
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());
            StationRepository.Currency = "EUR";


            ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            ChangeTracker.CurrentUser.Cashpool = 100000;
            ChangeTracker.CurrentUser.AvailableCash = 100000;


            SortableObservableCollection<IMatchVw> matches = new SortableObservableCollection<IMatchVw>();
            matches.Add(CreateMatch(1, true));
            matches.Add(CreateMatch(2));
            matches.Add(CreateMatch(2));
            matches.Add(CreateMatch(3, true));
            matches.Add(CreateMatch(4));
            matches.Add(CreateMatch(4));

            Repository.BackToRecord();

            Repository.Stub(x => x.FindMatches(matches, "", "", null, null)).IgnoreArguments().Return(matches);
            Repository.Stub(x => x.FindResults(null, null, null)).IgnoreArguments();

            Repository.Replay();

            Dispatcher.Invoke(() =>
            {
                Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                Window.Show();

            });

            Thread.Sleep(1000);
            var firstModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion);

            Assert.IsNotNull(firstModel);


            MyRegionManager.NavigateUsingViewModel<TournamentsViewModel>(RegionNames.ContentRegion);


            var model = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TournamentsViewModel;

            while (!model.IsReady)
            {
                Thread.Sleep(100);
            }


            Assert.AreEqual(3, model.Tournaments.Count);
            Assert.AreEqual(true, model.Tournaments[0].IsOutrightGroup);
            Assert.AreEqual(2, model.Tournaments[1].MatchesCount);
            Assert.AreEqual(2, model.Tournaments[0].MatchesCount);
            Assert.AreEqual(2, model.Tournaments[1].Id);



            model.CheckedBox.Execute(model.Tournaments[1]);
            model.CheckedBox.Execute(model.Tournaments[2]);


            model.Choice.Execute(model.Tournaments[2]);

            Thread.Sleep(1000);

            var matchesmodel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as MatchesViewModel;

            Assert.IsNotNull(matchesmodel);
            Assert.IsFalse(matchesmodel.SelectedTournaments[4]);
            Assert.AreEqual(1, matchesmodel.SelectedTournaments.Count);



            Dispatcher.Invoke(() =>
            {
                Window.Close();
            });


        }
        [TestMethod]
        [Timeout(2000000)]
        public void TournamentsViewMultyTournamentsTest()
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



            StationRepository.TurnOffCashInInit = true;
            StationRepository.Expect(x => x.AllowAnonymousBetting).Return(true);
            StationRepository.Expect(x => x.IsReady).Return(true);
            StationRepository.Expect(x => x.StationNumber).Return("0024");
            StationRepository.Expect(x => x.HubSettings).Return(new Dictionary<string, string>());
            StationRepository.Currency = "EUR";

            ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("1",1);
            ChangeTracker.CurrentUser.Cashpool = 100000;
            ChangeTracker.CurrentUser.AvailableCash = 100000;





            SortableObservableCollection<IMatchVw> matches = new SortableObservableCollection<IMatchVw>();
            matches.Add(CreateMatch(1, true));
            matches.Add(CreateMatch(2));
            matches.Add(CreateMatch(2));
            matches.Add(CreateMatch(3, true));
            matches.Add(CreateMatch(4));
            matches.Add(CreateMatch(4));

            Repository.BackToRecord();

            Repository.Stub(x => x.FindMatches(matches, "", "", null, null)).IgnoreArguments().Return(matches);
            Repository.Stub(x => x.FindResults(null, null, null)).IgnoreArguments();

            Repository.Replay();

            Dispatcher.Invoke(() =>
            {
                Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                Window.Show();

            });

            Thread.Sleep(1000);
            var firstModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion);

            Assert.IsNotNull(firstModel);


            MyRegionManager.NavigateUsingViewModel<TournamentsViewModel>(RegionNames.ContentRegion);


            var model = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TournamentsViewModel;

            while (!model.IsReady)
            {
                Thread.Sleep(100);
            }


            Assert.AreEqual(3, model.Tournaments.Count);
            Assert.AreEqual(true, model.Tournaments[0].IsOutrightGroup);
            Assert.AreEqual(2, model.Tournaments[1].MatchesCount);
            Assert.AreEqual(2, model.Tournaments[0].MatchesCount);
            Assert.AreEqual(2, model.Tournaments[1].Id);



            model.CheckedBox.Execute(model.Tournaments[1]);
            model.CheckedBox.Execute(model.Tournaments[2]);


            var header = MyRegionManager.CurrentViewModelInRegion(RegionNames.HeaderRegion) as HeaderViewModel;

            header.NextView.Execute("");

            Thread.Sleep(1000);

            var matchesmodel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as MatchesViewModel;
            while (!matchesmodel.IsReady)
            {
                Thread.Sleep(100);
            }
            Assert.IsNotNull(matchesmodel);
            Assert.IsNotNull(matchesmodel.SelectedTournaments);
            Assert.IsFalse(matchesmodel.SelectedTournaments[2]);
            Assert.AreEqual(2, matchesmodel.SelectedTournaments.Count);

            Dispatcher.Invoke(() =>
                { header.PrevView.Execute(""); });


            model = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as TournamentsViewModel;

            while (!model.IsReady)
            {
                Thread.Sleep(100);
            }

            Assert.AreEqual(2, model.SelectedTournaments.Count);


            Dispatcher.Invoke(() =>
            {
                Window.Close();
            });


        }


        private IMatchVw CreateMatch(int index, bool outright = false)
        {
            TestGroupVw tournamentView = new TestGroupVw()
            {
                DisplayName = "test" + index,

            };
            tournamentView.LineObject = new GroupLn()
            {
                GroupId = index,

            };
            var match = new TestMatchVw()
            {
                TournamentView = tournamentView,
                StartDate = DateTime.Now.AddDays(5),
                ExpiryDate = DateTime.Now.AddDays(5),
                IsOutright = outright,

            };
            return match;
        }
    }
}
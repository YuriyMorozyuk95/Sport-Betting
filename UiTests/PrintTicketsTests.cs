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
using SportBetting.WPF.Prism.Modules.UserManagement.ViewModels;
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
    public class PrintTicketsTests : BaseClass
    {


        [TestMethod]
        [Timeout(2000000)]
        public void PrintTicketsViewTest()
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





            Repository.Replay();

            Dispatcher.Invoke(() =>
                {
                    Window = MyRegionManager.FindWindowByViewModel<MainViewModel>();
                    Window.Show();

                });

            Thread.Sleep(1000);
            var firstModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion);

            Assert.IsNotNull(firstModel);


            MyRegionManager.NavigateUsingViewModel<UserManagementViewModel>(RegionNames.ContentRegion);


            Thread.Sleep(1000);
            var model = MyRegionManager.CurrentViewModelInRegion(RegionNames.ContentRegion) as UserManagementViewModel;

            model.PrintLastTicketCommand.Execute("");


            var printTicketViewModel = MyRegionManager.CurrentViewModelInRegion(RegionNames.UsermanagementContentRegion) as PrintTicketViewModel;
            Thread.Sleep(1000);

            while (!printTicketViewModel.IsReady)
            {
                Thread.Sleep(100);
            }


            Thread.Sleep(100000);

            //Dispatcher.Invoke(() =>
            //    {
            //        Window.Close();
            //    });


        }



    }
}
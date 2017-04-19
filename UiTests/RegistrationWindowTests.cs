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
    public class RegistrationWindowTests : BaseClass
    {

        [TestMethod]
        [Timeout(2000000)]
        public void ShowErrorTest()
        {


            AuthorizationService = new AuthorizationService();


            StationRepository = MockRepository.GenerateStub<IStationRepository>();
            LanguageRepository = MockRepository.GenerateStub<ILanguageRepository>();
            LineProvider = MockRepository.GenerateStub<ILineProvider>();
            BusinessPropsHelper = MockRepository.GenerateStub<IBusinessPropsHelper>();
            DataBinding = MockRepository.GenerateStub<IDataBinding>();
            WsdlRepository = MockRepository.GenerateStub<IWsdlRepository>();


            IoCContainer.Kernel.Bind<IDataBinding>().ToConstant<IDataBinding>(DataBinding).InSingletonScope();
            IoCContainer.Kernel.Bind<IAuthorizationService>().ToConstant<IAuthorizationService>(AuthorizationService).InSingletonScope();
            IoCContainer.Kernel.Rebind<IStationRepository>().ToConstant<IStationRepository>(StationRepository).InSingletonScope();
            IoCContainer.Kernel.Rebind<IWsdlRepository>().ToConstant<IWsdlRepository>(WsdlRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILanguageRepository>().ToConstant<ILanguageRepository>(LanguageRepository).InSingletonScope();
            IoCContainer.Kernel.Bind<ILineProvider>().ToConstant<ILineProvider>(LineProvider).InSingletonScope();
            IoCContainer.Kernel.Bind<IBusinessPropsHelper>().ToConstant<IBusinessPropsHelper>(BusinessPropsHelper).InSingletonScope();

            StationRepository.LayoutName = "DefaultViews";
            TranslationProvider.Expect(x => x.Translate(null)).Return("I've read").IgnoreArguments();
            StationRepository.Expect(x => x.GetRegistrationForm()).Return(new formField[1]
                {
                    new formField() { name = "test", type = "String" }
                }
            );

            BaseClass.Dispatcher.Invoke(() =>
                {
                    BaseClass.Window = MyRegionManager.FindWindowByViewModel<AuthViewModel>();
                    BaseClass.Window.Show();
                });



            Thread.Sleep(1000);


            Task.Run(() =>
                { MyRegionManager.NavigateUsingViewModel<RegistrationViewModel>(RegionNames.AuthContentRegion); });

           Thread.Sleep(1000000);

            BaseClass.Dispatcher.Invoke(() =>
                {
                    BaseClass.Window.Close();


                });



        }

      
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BaseObjects;
using BaseObjects.ViewModels;
using Catel.Windows.Threading;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared;
using System.Windows;
using SportRadar.Common.Windows;
using SportRadar.DAL.Connection;
using TranslationByMarkupExtension;

namespace SportBetting.WPF.Prism.Modules.UserManagement.ViewModels
{
    public class RestartViewModel : BaseViewModel
    {

        #region Constructors

        public RestartViewModel()
        {
           
            SoftRestartCommand = new Command(OnSoftRestartCommand);
            HardRestartCommand = new Command(OnHardRestartCommand);
            ShutdownCommand = new Command(OnShutdownCommand);
            ResetDbShutdownCommand = new Command(OnResetDB);
            //Mediator.Register<string>(this, ShowSystemInfo, MsgTag.ShowSystemInfo);
        }



        #endregion


        #region Commands

     
        public Command SoftRestartCommand { get; set; }
        public Command ResetDbShutdownCommand { get; set; }
        public Command HardRestartCommand { get; set; }
        public Command ShutdownCommand { get; set; }
        #endregion


        #region Methods

        public override void OnNavigationCompleted()
        {
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_PRINT_SYSTEM;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_FORM_RESTART_TERMINAL;
            ChangeTracker.RestartChecked = true;

            base.OnNavigationCompleted();
        }


        private void OnSoftRestartCommand()
        {
            Mediator.SendMessage<long>(0, MsgTag.RestartApplication);
        }

        private void OnHardRestartCommand()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
        }
        private void OnShutdownCommand()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
        }


        [AsyncMethod]
        private void OnResetDB()
        {
            OnResetDBPleaseWait();
        }

        [PleaseWaitAspect]
        private void OnResetDBPleaseWait()
        {
            ThreadHelper.StopAll();
            var serviceRuntimeUserCertificateStore = new X509Store(StoreName.My);
            serviceRuntimeUserCertificateStore.Open(OpenFlags.ReadWrite);
            for (int i = 0; i < serviceRuntimeUserCertificateStore.Certificates.Count;)
            {
                var certificate = serviceRuntimeUserCertificateStore.Certificates[i];
                if (certificate.Subject.Contains("Sportradar AG"))
                {
                    serviceRuntimeUserCertificateStore.Remove(certificate);
                    continue;
                }
                i++;
            }
            PrinterHandler.DeleteAllPrinterObjects();
            ConnectionManager.CloseAll();
            DatabaseManager.DropDatabase(false);
            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");

        }

        #endregion

    }
}

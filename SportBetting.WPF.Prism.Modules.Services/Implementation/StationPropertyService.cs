using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.Common.Windows;
using WsdlRepository;

namespace SportBetting.WPF.Prism.Modules.Services.Implementation
{
    public class StationPropertyService
    {
        private int _syncInterval = Convert.ToInt32(ConfigurationManager.AppSettings["STATIONPROPERTY_SYNC_INTERVAL"]) * 1000;

        private IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        #region IService Members


        public void StartService()
        {
            ThreadHelper.RunThread("StationPropertyService", Run);

        }

       

        private void Run(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {
                var result = StationRepository.Refresh();

                if (StationRepository.SyncInterval > 0)
                    _syncInterval = StationRepository.SyncInterval * 1000;
                if (result)
                {
                    Thread.Sleep(_syncInterval);
                }
                else
                {
                    Thread.Sleep(3000);
                }
            }
        }

        #endregion
    }
}

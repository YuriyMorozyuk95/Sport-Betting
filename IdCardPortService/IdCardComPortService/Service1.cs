using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Management;
using Nbt.Services.Preference;
using System.Threading;

namespace IdCardComPortService
{
    public partial class Service1 : ServiceBase
    {

        private string[] parameters; 
        private IPrefSupplier prefFile; //Used for hardware settings

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ThreadStart tsTask = new ThreadStart(TaskLoop);
            Thread MyTask = new Thread(tsTask);
            MyTask.Start();
        }

        protected void TaskLoop()
        {

            while (true)
            {
                ScheduledTask();

                System.Threading.Thread.Sleep(10000);
            }
        }

        protected void ScheduledTask()
        {

            AddLog("ID Card Service started");

            string path = System.Environment.GetEnvironmentVariable("TERMINAL_CONFIG");
            var portnames = GetVirtualComPort();

            if (portnames != null && portnames.Count > 0)
            {
                string confFile = path + "PrefFile.txt";

                foreach (var port in portnames)
                {
                    prefFile = new TextFilePref(confFile, confFile, true, false);
                    prefFile.SetStringEntry("IdCardReaderPort", port);

                    AddLog("Reader detected on port: " + port);
                }          
            }
            else
            {
                AddLog("Reader not detected");
            }
        }

        protected override void OnStop()
        {
            AddLog("ID Card Service stopped");
        }

        public void AddLog(string log)
        {
            try
            {
                if (!EventLog.SourceExists("IDCardService"))
                {
                    EventLog.CreateEventSource("IDCardService", "IDCardService");
                }
                eventLog1.Source = "IDCardService";
                eventLog1.WriteEntry(log);
            }
            catch { }
        }

        public static IList<string> GetVirtualComPort()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSSerial_PortName");
            var result = searcher.Get();
            var ports = new List<string>();
            var i = 0;
            foreach (ManagementObject queryObj in result)
            {
                if (queryObj["InstanceName"].ToString().Contains("FTDI"))
                {
                    var query =
                        "SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0 and Caption like 'ITL USB (High Speed)%' and Caption like '%" +
                        queryObj["PortName"].ToString() + "%'";
                    ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("root\\CIMV2", query);
                    if (searcher2.Get().Count > 0)
                        continue;
                    ports.Add(queryObj["PortName"].ToString());
                }
            }

            return ports;
        }
    }
}

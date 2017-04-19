using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SportBetting.WPF.Prism.Modules.Aspects;
using NLogger;

namespace IDCardReader
{
    public class DaemonThread
    {
        public static AutoResetEvent autoEvent = new AutoResetEvent(false);
        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DaemonThread));
        public Invoker invoker;
        public event EventHandler EjectCard;
        public event EventHandler StartedCardReading;
        public event EventHandler<CardEventArgs<string>> InsertCard;
        public event EventHandler<CardEventArgs<string>> CardError;
        public string CardNumber;
        private string writeData = null;
        public Thread newThread = null;
        public bool ShowMsg = false;

        private volatile bool _readDeviceID = false;
        private volatile bool _readDeviceSN = false;
        private Regex _regexItem = new Regex("^[a-zA-Z0-9 ]*$");
        Command comm0 = new Command(Operation.ActivateChip, "010000035404085A");            // init card
        Command comm1 = new Command(Operation.ActivateChip, "010000016E6E");                // deactivate
        Command comm2 = new Command(Operation.ActivateChip, "010000004E044B");              // activate
        //Command comm3 = new Command(Operation.TransferData, "010000064200B0000032C7");
        Command comm3 = new Command(Operation.TransferData, "010000064200B0000019EC");      // binary read from card

        //private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        //private Command resetCommand = null;
        public DaemonThread(Invoker invoker)
        {
            this.invoker = invoker;

            ThreadStart threadDelegate = new ThreadStart(FireCardStateChanged);
            newThread = new Thread(threadDelegate);
            newThread.Start();
            new Thread(() => CheckingThread()).Start();
        }

        private void CheckingThread()
        {
            while (true)
            {
                if (newThread.IsAlive)
                    Thread.Sleep(1000);
                else
                {
                    newThread.Start();
                }
            }
        }

        public void RequestReadID()
        {
            _readDeviceID = true;
        }
        public void RequestReadSN()
        {
            _readDeviceSN = true;
        }
        //fire event when card inserted or removed
        public void FireCardStateChanged()
        {
            int exception_time_counter = 0;
            const int MAX_EXCEPTION_TIME = 2;
            bool show_msg_only_once = false; 

            Console.WriteLine("Thread started");
            while (true)
            {
                try
                {
                    while (true)
                    {
                        Command checkForCard = new Command(Operation.CheckPosition, "010000013838");
                        invoker.InvokeCommand(checkForCard);
                        if (Link862Reader.PrevState != Link862Reader.State)
                        {
                            Link862Reader.PrevState = Link862Reader.State;
                            break;
                        }
                        if (_readDeviceID == true)
                        {
                            //Command checkDescriptors = new Command(Operation.checkDescriptors, "010000013737");
                            Command checkDescriptors = new Command(Operation.CheckDescriptors, "01000002370135");
                            invoker.InvokeCommand(checkDescriptors);
                            _readDeviceID = false;
                        }
                        else if (_readDeviceSN == true)
                        {
                            Command checkSN = new Command(Operation.CheckSN, "01000002370034");
                            invoker.InvokeCommand(checkSN);
                            _readDeviceSN = false;
                        }  

                        if (show_msg_only_once)
                        {
                            show_msg_only_once = false;
                        }
                        exception_time_counter = 0;
                     
                    }

                    if (Link862Reader.State == 1)
                    {
                        if (StartedCardReading != null)
                            StartedCardReading(null, null);

                        Log.Debug("card inserted, read card number");
                        Link862Reader.CardNumber = "";


                        if (!ReadCardNumber())
                        {
                            CardError(this, null);
                            writeData = null;
                            continue;
                        }
                        if (!string.IsNullOrEmpty(writeData))
                        {
                            if (string.IsNullOrEmpty(writeData))
                                throw new ArgumentNullException();
                            writeData = writeData.Trim();
                            var numbertowrite = Link862Reader.ByteArrayToHexString(Encoding.ASCII.GetBytes(writeData));

                            var lengthHex = String.Format("{0:X}", writeData.Length);
                            var array =
                                Link862Reader.HexStringToByteArray("0100001F4200D00000" + lengthHex + "" + numbertowrite);

                            byte xorValue = array[0];
                            for (int i = 1; i < array.Length; i++)
                            {
                                xorValue ^= array[i];
                            }

                            string xor = Link862Reader.ByteArrayToHexString(new byte[] {xorValue});
                            Log.Debug("write card number command");
                            //invoker.InvokeCommand(new Command(Operation.SaveData, "0100001F4200D00000193F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3FAA"));
                            invoker.InvokeCommand(new Command(Operation.SaveData,
                                                              "0100001F4200D00000" + lengthHex + "" + numbertowrite + "" +
                                                              xor));
                            Log.DebugFormat("write card number:{0}", writeData);
                            writeData = null;

                            if (!ReadCardNumber())
                            {
                                CardError(this, null);
                                continue;
                            }
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            if (Link862Reader.CardNumber.Length > 25)
                                Link862Reader.CardNumber = Link862Reader.CardNumber.Substring(0, 25);

                            if (Link862Reader.CardNumber.Length != 25 && !Link862Reader.IsNew)
                            {
                                if (!ReadCardNumber())
                                {
                                    CardError(this, null);
                                    continue;
                                }
                            }
                        }
                        if (Link862Reader.CardNumber.Length != 25 && !Link862Reader.IsNew)
                        {
                            if (CardError != null)
                            {
                                Link862Reader.COMPort.Close();
                                Link862Reader.COMPort.Open();
                                CardError(this, null);
                                continue;
                            }
                        }

                        if (InsertCard != null)
                            InsertCard(this, new CardEventArgs<string>(Link862Reader.CardNumber));
                    }
                    else
                    {
                        Log.Debug("card ejected");
                        Link862Reader.CardNumber = "";
                        if (EjectCard != null)
                            EjectCard(this, null);
                    }
                }
                catch (Exception ex)
                {
                   /* if (ex is ArgumentNullException)
                    {
                        throw;
                    }*/
                    if (ShowMsg)
                    {
                        if (exception_time_counter++ > MAX_EXCEPTION_TIME)
                        {
                            if (show_msg_only_once == false)
                            {
                                CardError(this, null);
                                show_msg_only_once = true;
                            }
                            exception_time_counter = 0;
                        }
                    }
                    Thread.Sleep(1000);
                    Link862Reader.TryReconnect();
                }
               
            }
        }

        public void ResetReader()
        {
            invoker.InvokeCommand(new Command(Operation.SaveData, "010000017F7F")); //reset command
        }

        public void WriteDataToCard(string data) 
        {
            if (Link862Reader.State == 1)
            {
                writeData = data;
                Link862Reader.PrevState = 0;
            }
        }

        private bool ReadCardNumber()
        {
            // try read card max 5 times while get correct number
            for (int i = 0; i < 5; i++)
            {
                invoker.InvokeCommand(comm2);
                invoker.InvokeCommand(comm3);

              
                if (_regexItem.IsMatch(Link862Reader.CardNumber) && Link862Reader.CardNumber.Length == 25)
                { 
                    return true;
                }

                if (Link862Reader.IsNew)
                {
                    return true;
                }

                // reinit card if first attempt was unsuccessful
                invoker.InvokeCommand(comm1);
                invoker.InvokeCommand(comm0);
            }
            return false;
        }



    }
}

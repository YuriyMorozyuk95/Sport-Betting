using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLogger;

namespace IDCardReader
{
    class CommandParser
    {
        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(CommandParser));

        public static void Parse(Command command, ReaderEvent response)
        {
            //Console.WriteLine(response.Data);
            switch (command.Operation)
            {
                case Operation.TransferData:
                    {
                        if (response.Data.Length < 5)
                        {
                            Link862Reader.COMPort.Close();
                            Link862Reader.COMPort.Open();
                            break;
                        }

                        Log.Debug(response.Data);
                        if (response.Data.Contains("!!"))
                        {
                            Link862Reader.COMPort.DiscardOutBuffer();
                            Link862Reader.COMPort.DiscardInBuffer();
                            Link862Reader.COMPort.Dispose();
                            Link862Reader.COMPort.Close();
                            Link862Reader.COMPort.Open();
                            Link862Reader.CardNumber = "";
                            Link862Reader.IsNew = false;
                            break;
                        }
                        string cardNumber = response.Data.Substring(4);
                        cardNumber = cardNumber.Replace("\0", "");
                        var cardNumberNoquestins = cardNumber;
                        cardNumber = cardNumber.Replace("?", "");
                        Link862Reader.CardNumber = cardNumber;
                        Link862Reader.IsNew = cardNumberNoquestins.Contains("?????") && cardNumber.Length < 2;
                        if (Link862Reader.IsNew)
                            Link862Reader.CardNumber = "?????????????????????????";

                        Log.Debug("Card number:" + cardNumber);
                    }
                    break;
                case Operation.ActivateChip:
                    break;
                case Operation.SaveData:
                    Link862Reader.COMPort.Close();
                    Link862Reader.COMPort.Open();
                    break;
                case Operation.CheckPosition:
                    if (response.Data.Contains("ss"))
                        Link862Reader.State = 1;
                    if (response.Data.Contains("pp"))
                        Link862Reader.State = 0;

                    break;
                case Operation.CheckDescriptors:
                    try
                    {
                        if (response.Data.Length > 5)
                        {
                            int len = response.Data.Length - 5;
                            Link862Reader.IdReaderHW = response.Data.Substring(4, len);
                        }
                        else
                        {
                            Link862Reader.IdReaderHW = null;
                        }
                    }
                    catch 
                    {
                        Link862Reader.IdReaderHW = null;
                    }
                    break;
                case Operation.CheckSN:
                    try
                    {
                        if (response.Data.Length > 5)
                        {
                            int len = response.Data.Length - 5;
                            Link862Reader.IdReaderSN = response.Data.Substring(4, len);
                        }
                        else
                        {
                            Link862Reader.IdReaderSN = null;
                        }
                    }
                    catch
                    {
                        Link862Reader.IdReaderSN = null;
                    }
                    break;
            }

        }
    }
}

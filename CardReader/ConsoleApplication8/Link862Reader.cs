using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using SerialPortManager;
using System.Text;
using System.Threading;
using System.Management;
using Microsoft.Win32;
using NLogger;


namespace IDCardReader
{

    public class ReaderEvent : EventArgs
    {
        public int state;
        public Operation operation;
        public string Data { get; set; }
    }

    public class Link862Reader
    {

        private static readonly object _locker = new object();
        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Link862Reader));

       //public event EventHandler<ReaderEvent> Response;
       public static SerialPort COMPort = new SerialPort();
       

        public static string IdReaderHW;
        public static string IdReaderSN;
        public static string CardNumber;
        public static short PrevState = 0;
        public static short State = 0;
        public bool IsReady = false;
        private DateTime lastCommandResponce;
        private Command LastCommand;
        private static SerialPortManager.SerialPortManager spm = SerialPortManager.SerialPortManager.Instance;

        public Link862Reader()
        {
            //log4net.Config.XmlConfigurator.Configure();
            Init();
        }

        public Link862Reader(string str)
        {
           // use this constructor only for Hardware ID reading!!!
        }
        public void RequestReadID()
        {         
            if (WorkingThread != null)
            {
                WorkingThread.RequestReadID();
            }
            else
            {
                IdReaderHW = null;
            }
        }

        public void RequestReadSN()
        {       
            if (WorkingThread != null)
            {
                WorkingThread.RequestReadSN();
            }
            else
            {
                IdReaderSN = null;
            }
        }

        public string ReadReaderID ()
        {
            return IdReaderHW;
        }
        public string ReadReaderSN()
        {
            return IdReaderSN;
        }
        public static DaemonThread WorkingThread { get; set; }

        public static bool IsNew { get; set; }

        public void Init()
        {
            // If the port is open, close it.
            if (COMPort.IsOpen)
            {
                COMPort.Close();
            }
            // Set the port's settings
            COMPort.BaudRate = 38400;
            COMPort.DataBits = 8;
            COMPort.StopBits = StopBits.One;
            COMPort.Parity = Parity.None;
            COMPort.Handshake = Handshake.None;

            var portname = GetVirtualComPort();

            if (string.IsNullOrEmpty(portname))
                return;

            var port = new SerialPort(portname);

            if (port.IsOpen)
                return;

            COMPort.PortName = portname;

            try
            {
                if (COMPort.IsOpen)
                {
                    COMPort.Close();
                }
                // Open the port
                COMPort.Open();
                IsReady = true;
                // When data is recieved through the port, call this method
                //COMPort.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Error("", e);
                IsReady = false;
            }
            catch (IOException e)
            {
                Log.Error("", e);
                IsReady = false;
            }
            catch (ArgumentException e)
            {
                Log.Error("", e);
                IsReady = false;
            }
        }

        //public void DispatchCardEvent(object sender, ReaderEvent @event)
        //{
        //    EventHandler<ReaderEvent> handler = Response;

        //    if (handler != null)
        //    {
        //        handler(sender, @event);
        //    }
        //}

       

        public static string GetVirtualComPort()
        {

            string port = null;
            List<CommunicationResource> sp = spm.GetSafeSerialPortsMap();
            foreach (CommunicationResource cr in sp)
            {
                if (cr.PortType == ResourceType.ID_CARD_READER_SERIAL_PORT)
                {
                    port = cr.PortName;
                }
            }
            return port;
        }

        public static void TryReconnect()
        {
           
            Link862Reader.IdReaderSN = null;
            Link862Reader.IdReaderHW = null;
            try
            {
            
                if (COMPort.IsOpen)
                {
                    COMPort.Close();
                }
            }
            catch 
            {

            }

            string port = spm.GetIdReaderSerialPort();
            if (!string.IsNullOrEmpty (port))
            {
                try
                {
                    if (COMPort.IsOpen)
                    {
                        COMPort.Close();
                    }
                    // Open the port
                    COMPort.PortName = port;
                    COMPort.BaudRate = 38400;
                    COMPort.DataBits = 8;
                    COMPort.StopBits = StopBits.One;
                    COMPort.Parity = Parity.None;
                    COMPort.Handshake = Handshake.None;
                    COMPort.Open();
                    if (COMPort.IsOpen)
                    {
                        COMPort.DiscardInBuffer();
                        COMPort.DiscardOutBuffer();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        //private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    lock (_locker)
        //    {
        //        try
        //        {

        //            lastCommandResponce = DateTime.Now;
        //            // If the com port has been closed, do nothing
        //            if (!COMPort.IsOpen) return;
        //            // Obtain the number of bytes waiting in the port's buffer
        //            int bytes = COMPort.BytesToRead;
        //            // Create a byte array buffer to hold the incoming data
        //            IList<byte> buffer = new List<byte>();
        //            // Read the data from the port and store it in our buffer
        //            var buffertmp = new byte[bytes];
        //            COMPort.Read(buffertmp, 0, bytes);
        //            foreach (var b in buffertmp)
        //            {
        //                buffer.Add(b);
        //            }
        //            while (buffer.Count < LastCommand.ResponceLength)
        //            {
        //                buffertmp = new byte[bytes];
        //                COMPort.Read(buffertmp, 0, bytes);
        //                foreach (var b in buffertmp)
        //                {
        //                    buffer.Add(b);
        //                }
        //            }
        //            var res = new byte[buffer.Count];
        //            for (int i = 0; i < buffer.Count; i++)
        //            {
        //                res[i] = buffer[i];
        //            }

        //            SendResponse(res);
        //        }
        //        catch (Exception ex)
        //        {

        //            throw;
        //        }

        //        Monitor.Pulse(_locker);

        //    }
        //}

        /// <summary> Send the user's data currently entered in the 'send' box.</summary>
        public ReaderEvent Send(Command command)
        {
            ReaderEvent e1 = new ReaderEvent();
            LastCommand = command;

            try
            {
                SendCommand(command.Data);
                Thread.Sleep(250);

                lastCommandResponce = DateTime.Now;
                // If the com port has been closed, do nothing
                if (!COMPort.IsOpen)
                    COMPort.Open();
                // Obtain the number of bytes waiting in the port's buffer
                int bytes = 1;
                // Create a byte array buffer to hold the incoming data
                IList<byte> buffer = new List<byte>();
                // Read the data from the port and store it in our buffer
                var buffertmp = new byte[bytes];
                COMPort.Read(buffertmp, 0, bytes);
                foreach (var b in buffertmp)
                {
                    buffer.Add(b);
                }
                while (COMPort.BytesToRead > 0)
                {
                    if (COMPort.BytesToRead < 1)
                        break;
                    buffertmp = new byte[bytes];
                    COMPort.Read(buffertmp, 0, bytes);
                    foreach (var b in buffertmp)
                    {
                        buffer.Add(b);
                    }
                }
                var res = new byte[buffer.Count];
                for (int i = 0; i < buffer.Count; i++)
                {
                    res[i] = buffer[i];
                }

                e1.Data = Encoding.ASCII.GetString(res);
                return e1;
              
            }
            catch (Exception ex)
            {
                throw;
             
            }
        }

        private void SendCommand(string command)
        {
            // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
            try
            {
                if (!COMPort.IsOpen)
                {
                    COMPort.Open();
                }
                if (COMPort.IsOpen)
                {
                    byte[] data = HexStringToByteArray(command);
                    // Send the binary data out the port
                    COMPort.Write(data, 0, data.Length);
                }

            }
            catch (Exception ex)
            {
               // throw;
            }
           
        }

        //private void SendResponse(byte[] buffer)
        //{
        //    ReaderEvent e1 = new ReaderEvent();
        //    e1.Data = Encoding.ASCII.GetString(buffer);

        //    DispatchCardEvent(this, e1);
        //}

        /// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
        /// <param name="s"> The string containing the hex digits (with or without spaces). </param>
        /// <returns> Returns an array of bytes. </returns>
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpperInvariant();
        }
    }

   
}

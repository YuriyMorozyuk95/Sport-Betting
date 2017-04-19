using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;


namespace SerialPortManager
{
    public enum ResourceType
    {
        ID_CARD_READER_SERIAL_PORT = 0,
        CASH_VALIDATOR_SERIAL_PORT
    }

    public class TerminalSerialPortsMap
    {
        private string id_card_port;
        private string cash_validator_port;

        public TerminalSerialPortsMap(string id, string cash)
        {
            id_card_port = id;
            cash_validator_port = cash;
        }

        public string CashValidatorPort
        {
            get { return cash_validator_port; }
        }
        public string IdCardReaderPort
        {
            get { return id_card_port; }
        }
    }

    public class CommunicationResource
    {
        private string resource_name;
        private ResourceType resource_type;

        public string PortName
        {
            get { return resource_name; }
        }

        public ResourceType PortType
        {
            get { return resource_type; }
        }
        public CommunicationResource (ResourceType rt, string rn)
        {
            resource_type = rt;
            resource_name = rn;
        }
    }

    public class SerialPortManager
    {
        private static volatile SerialPortManager instance;
        private static object syncRoot = new Object();
        ComPort serialPort;
        private TerminalSerialPortsMap tsp = null;

        private SerialPortManager()
        {
            string id = GetIdReaderSerialPort();
            string cash = GetICashValidatorPort();
            tsp = new TerminalSerialPortsMap (id, cash);
        }

        public TerminalSerialPortsMap GetSafeSerialPortsMap()
        {
            return tsp;
        }

        public string GetIdReaderSerialPort()
        {
            string[] avalible_serial_ports = CoreGetAvailableResources();
            string id_reader_port = null;

            foreach (string port in avalible_serial_ports)
            {
                try
                {
                    if (DetectIdReaderDevice(port))
                    {
                        id_reader_port = port;
                        break;
                    }
                }
                catch (Exception e)
                {

                }

            }

            return id_reader_port;
        }

        public string GetICashValidatorPort()
        {
            string[] avalible_serial_ports = CoreGetAvailableResources();
            string cash_validator_port = null;

            foreach (string port in avalible_serial_ports)
            {
                try
                {
                    if (DetectCashValidatorDevice(port))
                    {
                        cash_validator_port = port;
                        break;
                    }
                }
                catch (Exception e)
                {

                }

            }

            return cash_validator_port;
        }


        public List<CommunicationResource> GetAllPereferialPorts()
        {
            string[] avalible_serial_ports = CoreGetAvailableResources();

             List<CommunicationResource> portList = new List<CommunicationResource>();

            foreach (string port in avalible_serial_ports)
            {
                try
                {
                    if (DetectCashValidatorDevice(port))
                    {
                        CommunicationResource device = new CommunicationResource(ResourceType.CASH_VALIDATOR_SERIAL_PORT, port);
                        portList.Add(device);                  
                    }

                    if (DetectIdReaderDevice(port))
                    {
                        CommunicationResource device = new CommunicationResource(ResourceType.ID_CARD_READER_SERIAL_PORT, port);
                        portList.Add(device); 
                    }
                }
                catch (Exception e)
                {

                }

            }
            return portList;
        }

       

        private string[] CoreGetAvailableResources ()
        {
            return SerialPort.GetPortNames();
        }

        private bool CoreTryOpen(string port, int baud_rate)
        {
            bool result = port.StartsWith ("COM", true, CultureInfo.InvariantCulture);
            if (result)
            {
                serialPort = new ComPort (port, baud_rate);
                result = serialPort.TryOpen();
            }
            return result;
        }

        private bool CoreTryClose()
        {
            bool result = false;
            if (serialPort != null)
            {
                result = serialPort.TryClose();
                serialPort = null;
               
            }
            return result;
        }

        private bool DetectIdReaderDevice (string port)
        {
            bool result = false;

            lock (syncRoot)
            {
                if (CoreTryOpen(port, 38400))
                {
                    byte[] reply = serialPort.SendCmd(new byte[] {1, 0, 0, 2, 55, 2, 54});
                    CoreTryClose();

                    if (reply != null)
                    {
                        if (reply.Length > 7)
                        {
                            string answer = System.Text.Encoding.ASCII.GetString(reply, 3, reply[3]);
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        private bool DetectCoinAcceptorDevice ()
        {
            bool result = false;

               byte[] reply = serialPort.SendCmd (new byte[] {2, 0, 1, 254, 255});
               
                if (reply != null)
                {
                    if (reply.Length == 10)
                    {
                        result = true;
                    }
                }
  
            return result;
        }

        private bool DetectBillValidatorDevice()
        {
            bool result = false;

            byte[]reply = serialPort.SendCmd(new byte[] { 40, 0, 127, 192, 137 });

            if (reply != null)
            {
                if (reply.Length == 10)
                {
                    result = true;
                }
            }
            return result;
        }

        private bool DetectCashValidatorDevice (string port)
        {
            bool result = false;

            lock (syncRoot)
            {
                if (CoreTryOpen(port, 9600))
                {
                    result = DetectCoinAcceptorDevice();
                    if (!result)
                    {
                        result = DetectBillValidatorDevice();
                    }
                    CoreTryClose();
                }
            }

            return result;
        }

        public static SerialPortManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new SerialPortManager();
                        }
                    }
                }

                return instance;
            }
        }

    }
}

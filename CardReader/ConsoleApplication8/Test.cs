using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using IDCardReader;

namespace Nbt.IDCardReader
{
    public class Test
    {

        public static void Main(string[] args)
        {

            var reader = new Link862Reader();

            Invoker i1 = new Invoker(reader);
            DaemonThread thread = new DaemonThread(i1);
            Link862Reader.WorkingThread = thread;




            Thread.Sleep(2000);
            var random = new Random().Next(10000, 99999).ToString() + new Random().Next(10000, 99999).ToString() + new Random().Next(10000, 99999).ToString() + new Random().Next(10000, 99999).ToString() + new Random().Next(10000, 99999).ToString();
            //Console.WriteLine(random);
            //thread.WriteDataToCard(random);

            //Command comm2 = new Command(Operation.ActivateChip, "010000017F7F");
            //Command comm3 = new Command(Operation.TransferData, "010000064200B000000AFF");

            //i1.InvokeCommand(comm2);
            //i1.InvokeCommand(comm3);

            Console.ReadKey();
        }
    }
}

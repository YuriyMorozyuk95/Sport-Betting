using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace IDCardReader
{
    public class Invoker
    {

        //private static readonly object Locker = new object();
        protected Link862Reader Reader;
        public Queue<Command> CommandPool = new Queue<Command>();

        public Invoker(Link862Reader reader)
        {
            this.Reader = reader;
            //this.Reader.Response += OnResponse;
        }

        public void InvokeCommand(Command command)
        {
            var result = Reader.Send(command);

            CommandParser.Parse(command, result);
        }

        //private void OnResponse(object sender, ReaderEvent e)
        //{
        //    if (CommandPool.Count > 0)
        //    {
        //        Command command = CommandPool.Dequeue();
        //        if (command != null) CommandParser.Parse(command, e);
        //    }
        //}

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDCardReader
{
    public class CardEventArgs<T> : EventArgs
    {
        public CardEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }
}

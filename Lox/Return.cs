using System;

namespace Lox
{
    internal class Return : Exception
    {
        public object Value { get; }

        public Return(object value)
        {
            Value = value;
        }
    }
}
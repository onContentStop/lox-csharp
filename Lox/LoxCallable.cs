using System.Collections.Generic;

namespace Lox
{
    internal interface ILoxCallable
    {
        object Call(Interpreter interpreter, IEnumerable<object> arguments);
        int Arity();
    }
}
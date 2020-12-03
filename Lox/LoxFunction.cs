using System.Collections.Generic;
using System.Linq;
using Lox.Syntax;

namespace Lox
{
    internal class LoxFunction : ILoxCallable
    {
        private readonly Statement.Function _declaration;
        private readonly Environment _closure;

        public LoxFunction(Statement.Function declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            var environment = new Environment(_closure);
            var argumentsList = arguments.ToList();
            var parameters = _declaration.Parameters.ToList();
            for (var i = 0; i < parameters.Count; ++i)
            {
                environment.Define(parameters[i].Lexeme, argumentsList.ToList()[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.Value;
            }

            return null;
        }

        public int Arity()
        {
            return _declaration.Parameters.Count();
        }

        public override string ToString()
        {
            return $"<function '{_declaration.Name.Lexeme}'>";
        }
    }
}
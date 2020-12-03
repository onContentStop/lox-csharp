using System.Collections.Generic;

namespace Lox
{
    internal sealed class Environment
    {
        private readonly Environment _enclosingEnvironment;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public Environment(Environment enclosingEnvironment = null)
        {
            _enclosingEnvironment = enclosingEnvironment;
        }

        public void Define(string name, object value)
        {
            _values[name] = value;
        }

        public object Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out var value))
            {
                return value;
            }

            if (_enclosingEnvironment != null)
            {
                return _enclosingEnvironment.Get(name);
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return;
            }

            if (_enclosingEnvironment != null)
            {
                _enclosingEnvironment.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
using System;

namespace Lox
{
    internal class ConsoleErrorReporter : IErrorReporter
    {
        public void Report(Token token, string message)
        {
            Report(token.Line, message, token.Type == TokenType.EndOfFileToken ? " at end" : $"at '{token.Lexeme}'");
        }

        public bool HadError { get; set; } = false;
        public bool HadRuntimeError { get; set; } = false;

        public void ReportRuntimeError(RuntimeError error)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine($"{error.Message} \n[line {error.Token.Line}]");
            Console.ResetColor();
            HadRuntimeError = true;
        }

        public void Report(int line, string message, string where = "")
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            Console.ResetColor();
            HadError = true;
        }
    }
}
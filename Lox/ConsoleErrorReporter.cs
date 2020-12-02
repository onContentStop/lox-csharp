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

        public void Report(int line, string message, string where = "")
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            Console.ResetColor();
            HadError = true;
        }
    }
}
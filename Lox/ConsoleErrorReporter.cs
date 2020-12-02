using System;

namespace Lox
{
    internal class ConsoleErrorReporter : IErrorReporter
    {
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
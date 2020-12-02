using System;
using System.Collections.Generic;
using System.IO;

namespace Lox
{
    internal static class Lox
    {
        private static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                Environment.ExitCode = 64;
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunPrompt()
        {
            var errorReporter = new ConsoleErrorReporter();
            var showTokens = true;
            while (true)
            {
                Console.Write("|> ");
                var line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }

                switch (line)
                {
                    case "#showTokens":
                        showTokens = !showTokens;
                        Console.WriteLine(showTokens ? "Showing tokens." : "Not showing tokens.");
                        continue;
                }

                var tokens = Run(line, errorReporter);
                if (showTokens)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    foreach (var token in tokens)
                    {
                        Console.WriteLine($"{token}");
                    }

                    Console.ResetColor();
                }

                errorReporter.HadError = false;
            }
        }

        private static void RunFile(string path)
        {
            var errorReporter = new ConsoleErrorReporter();
            var contents = File.ReadAllText(path);
            Run(contents, errorReporter);

            if (errorReporter.HadError)
            {
                Environment.ExitCode = 65;
            }
        }

        private static IEnumerable<Token> Run(string source, IErrorReporter errorReporter)
        {
            var scanner = new Scanner(source, errorReporter);
            var tokens = scanner.ScanTokens();

            return tokens;
        }
    }
}
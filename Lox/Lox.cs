using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Lox.Syntax;

namespace Lox
{
    internal static class Lox
    {
        private static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                System.Environment.ExitCode = 64;
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
            var interpreter = new Interpreter(errorReporter);
            var showTokens = false;
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

                var (tokens, _) = Run(interpreter, line, errorReporter);
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
            var interpreter = new Interpreter(errorReporter);
            Run(interpreter, contents, errorReporter);

            if (errorReporter.HadError)
            {
                System.Environment.ExitCode = 65;
            }

            if (errorReporter.HadRuntimeError)
            {
                System.Environment.ExitCode = 70;
            }
        }

        private static (IEnumerable<Token>, IEnumerable<Statement>) Run(Interpreter interpreter, string source,
            IErrorReporter errorReporter)
        {
            var scanner = new Scanner(source, errorReporter);
            var tokens = scanner.ScanTokens().ToImmutableArray();
            var parser = new Parser(tokens, errorReporter);
            var statements = parser.Parse().ToImmutableArray();
            if (!errorReporter.HadError)
            {
                interpreter.Interpret(statements);
            }

            return (tokens, statements);
        }
    }
}
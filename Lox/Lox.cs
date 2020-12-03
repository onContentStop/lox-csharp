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
            var interpreter = new Interpreter(errorReporter);
            var showTokens = false;
            var showTree = false;
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
                    case "#showTree":
                        showTree = !showTree;
                        Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees.");
                        continue;
                }

                var (tokens, expression) = Run(interpreter, line, errorReporter);
                if (showTokens)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    foreach (var token in tokens)
                    {
                        Console.WriteLine($"{token}");
                    }

                    Console.ResetColor();
                }

                if (showTree && !errorReporter.HadError)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine(new SyntaxTreePrinter().Print(expression));
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
                Environment.ExitCode = 65;
            }

            if (errorReporter.HadRuntimeError)
            {
                Environment.ExitCode = 70;
            }
        }

        private static (IEnumerable<Token>, Expression) Run(Interpreter interpreter, string source,
            IErrorReporter errorReporter)
        {
            var scanner = new Scanner(source, errorReporter);
            var tokens = scanner.ScanTokens();
            var tokensList = tokens.ToImmutableList();
            var parser = new Parser(tokensList, errorReporter);
            var expression = parser.Parse();
            if (!errorReporter.HadError)
            {
                interpreter.Interpret(expression);
            }

            return (tokensList, expression);
        }
    }
}
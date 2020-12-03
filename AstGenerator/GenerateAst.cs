using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AstGenerator
{
    internal static class GenerateAst
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: generate_ast <output_directory>");
                Environment.ExitCode = 64;
                return;
            }

            var outputDir = args[0];
            DefineAst(outputDir, "Expression", new List<string>
            {
                "Assignment : Token name, Expression value",
                "Binary     : Expression left, Token operatorToken, Expression right",
                "Call       : Expression callee, Token parenthesisToken, IEnumerable<Expression> arguments",
                "Grouping   : Expression expression",
                "Literal    : object value",
                "Logical    : Expression left, Token operatorToken, Expression right",
                "Unary      : Token operatorToken, Expression right",
                "Variable   : Token name",
            });

            DefineAst(outputDir, "Statement", new List<string>
            {
                "Block               : IEnumerable<Statement> statements",
                "ExpressionStatement : Expression expression",
                "Function            : Token name, IEnumerable<Token> parameters, IEnumerable<Statement> body",
                "If                  : Expression condition, Statement thenBranch, Statement elseBranch",
                "Print               : Expression expression",
                "Return              : Token keyword, Expression value",
                "VariableDeclaration : Token name, Expression initializer",
                "While               : Expression condition, Statement body",
            });
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            var path = $"{outputDir}/Syntax/{baseName}.cs";
            using var writer = new StreamWriter(path);

            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine();
            writer.WriteLine("namespace Lox.Syntax");
            writer.WriteLine("{");

            writer.WriteLine($"    public abstract class {baseName}");
            writer.WriteLine("    {");

            DefineVisitor(writer, baseName, types);

            foreach (var type in types)
            {
                var split = type.Split(':');
                var className = split[0].Trim();
                var fields = split[1].Trim();
                DefineType(writer, baseName, className, fields);
            }

            writer.WriteLine();
            writer.WriteLine("        public abstract T Accept<T>(IVisitor<T> visitor);");

            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        private static void DefineVisitor(TextWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine("        public interface IVisitor<out T>");
            writer.WriteLine("        {");

            foreach (var typeName in types.Select(type => type.Split(':')[0].Trim()))
            {
                writer.WriteLine($"            T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            writer.WriteLine("        }");
        }

        private static void DefineType(TextWriter writer, string baseName, string className, string fields)
        {
            writer.WriteLine();
            writer.WriteLine($"        public sealed class {className} : {baseName}");
            writer.WriteLine("        {");

            var fieldsSplit = fields.Split(',').Select(x => x.Trim()).ToImmutableArray();
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            foreach (var field in fieldsSplit)
            {
                var type = field.Split(' ')[0];
                var name = ToPascalCase(field.Split(' ')[1]);
                writer.WriteLine($"            public {type} {name} {{ get; }}");
            }

            writer.WriteLine();
            writer.Write($"            public {className}(");
            writer.Write(string.Join(", ", fields));
            writer.WriteLine(")");
            writer.WriteLine("            {");
            foreach (var field in fieldsSplit)
            {
                var name = field.Split(' ')[1];
                var nameUpper = ToPascalCase(name);
                writer.WriteLine($"                {nameUpper} = {name};");
            }

            writer.WriteLine("            }");

            writer.WriteLine();
            writer.WriteLine("            public override T Accept<T>(IVisitor<T> visitor)");
            writer.WriteLine("            {");
            writer.WriteLine($"                return visitor.Visit{className}{baseName}(this);");
            writer.WriteLine("            }");

            writer.WriteLine("        }");
        }

        private static string ToPascalCase(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);

            return new string(a);
        }
    }
}
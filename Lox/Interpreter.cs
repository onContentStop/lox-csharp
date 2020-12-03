using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lox.Syntax;

namespace Lox
{
    internal class Interpreter : Expression.IVisitor<object>, Statement.IVisitor<object>
    {
        private class ClockFunction : ILoxCallable
        {
            public object Call(Interpreter interpreter, IEnumerable<object> arguments)
            {
                var now = DateTime.Now;
                return new decimal(now.Second + now.Millisecond * 1e-3);
            }

            public int Arity()
            {
                return 0;
            }

            public override string ToString()
            {
                return "<native function>";
            }
        }

        private readonly IErrorReporter _errorReporter;
        public Environment Globals { get; }
        private Environment _environment;

        public Interpreter(IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
            Globals = new Environment();
            _environment = Globals;

            Globals.Define("clock", new ClockFunction());
        }

        public void Interpret(IEnumerable<Statement> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                _errorReporter.ReportRuntimeError(error);
            }
        }

        private void Execute(Statement statement)
        {
            statement.Accept(this);
        }

        public void ExecuteBlock(IEnumerable<Statement> statements, Environment environment)
        {
            var previous = _environment;
            try
            {
                _environment = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        public object VisitBinaryExpression(Expression.Binary expression)
        {
            var left = Evaluate(expression.Left);
            var right = Evaluate(expression.Right);

            switch (expression.OperatorToken.Type)
            {
                case TokenType.MinusToken:
                    CheckNumberOperands(expression.OperatorToken, left, right);
                    return (decimal) left - (decimal) right;
                case TokenType.SlashToken:
                    CheckNumberOperands(expression.OperatorToken, left, right);
                    return (decimal) left / (decimal) right;
                case TokenType.StarToken:
                    CheckNumberOperands(expression.OperatorToken, left, right);
                    return (decimal) left * (decimal) right;
                case TokenType.PlusToken:
                    return left switch
                    {
                        decimal d1 when right is decimal d2 => d1 + d2,
                        string s1 when right is string s2 => s1 + s2,
                        _ => throw new RuntimeError(expression.OperatorToken,
                            "Operands must be two numbers or two strings.")
                    };
                case TokenType.GreaterToken:
                    CheckNumberOperands(expression.OperatorToken, left, right);
                    return (decimal) left > (decimal) right;
                case TokenType.GreaterEqualsToken:
                    CheckNumberOperands(expression.OperatorToken, left, right);
                    return (decimal) left >= (decimal) right;
                case TokenType.LessToken:
                    CheckNumberOperands(expression.OperatorToken, left, right);
                    return (decimal) left < (decimal) right;
                case TokenType.LessEqualsToken:
                    CheckNumberOperands(expression.OperatorToken, left, right);
                    return (decimal) left <= (decimal) right;
                case TokenType.BangEqualsToken:
                    return !IsEqual(left, right);
                case TokenType.EqualsEqualsToken:
                    return IsEqual(left, right);
                default:
                    throw new Exception("Unknown binary expression was reached");
            }
        }

        public object VisitCallExpression(Expression.Call expression)
        {
            var callee = Evaluate(expression.Callee);

            var arguments = expression.Arguments.Select(Evaluate).ToList();

            if (callee is ILoxCallable function)
            {
                if (arguments.Count != function.Arity())
                {
                    throw new RuntimeError(expression.ParenthesisToken,
                        $"Expected {function.Arity()} arguments, but got {arguments.Count}.");
                }

                return function.Call(this, arguments);
            }
            else
            {
                throw new RuntimeError(expression.ParenthesisToken,
                    "Cannot call this expression; it is not a function or class.");
            }
        }

        public object VisitGroupingExpression(Expression.Grouping expression)
        {
            return Evaluate(expression.Expression);
        }

        public object VisitLiteralExpression(Expression.Literal expression)
        {
            return expression.Value;
        }

        public object VisitLogicalExpression(Expression.Logical expression)
        {
            var left = Evaluate(expression.Left);

            if (expression.OperatorToken.Type == TokenType.OrKeyword)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if (!IsTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(expression.Right);
        }

        public object VisitUnaryExpression(Expression.Unary expression)
        {
            var right = Evaluate(expression.Right);

            switch (expression.OperatorToken.Type)
            {
                case TokenType.BangToken:
                    return !IsTruthy(right);
                case TokenType.MinusToken:
                    CheckNumberOperand(expression.OperatorToken, right);
                    return -(decimal) right;
                default:
                    throw new Exception("Unknown unary expression was reached");
            }
        }

        public object VisitVariableExpression(Expression.Variable expression)
        {
            return _environment.Get(expression.Name);
        }

        public object VisitAssignmentExpression(Expression.Assignment expression)
        {
            var value = Evaluate(expression.Value);
            _environment.Assign(expression.Name, value);
            return value;
        }

        private static bool IsEqual(object left, object right)
        {
            return left switch
            {
                null when right == null => true,
                null => false,
                _ => left.Equals(right)
            };
        }

        private static void CheckNumberOperand(Token operatorToken, object operand)
        {
            if (operand is decimal)
            {
                return;
            }

            throw new RuntimeError(operatorToken, "Operand must be a number.");
        }

        private static void CheckNumberOperands(Token operatorToken, object left, object right)
        {
            if (left is decimal && right is decimal)
            {
                return;
            }

            throw new RuntimeError(operatorToken, "Operands must be numbers.");
        }

        private object Evaluate(Expression expression)
        {
            return expression.Accept(this);
        }

        private static bool IsTruthy(object value)
        {
            return value switch
            {
                null => false,
                bool b => b,
                _ => true
            };
        }

        private static string Stringify(object value)
        {
            switch (value)
            {
                case null:
                    return "nil";
                case decimal d:
                {
                    var text = d.ToString(CultureInfo.CurrentCulture);
                    if (text.EndsWith(".0"))
                    {
                        text = text.Substring(0, text.Length - 2);
                    }

                    return text;
                }
                default:
                    return value.ToString();
            }
        }

        public object VisitExpressionStatementStatement(Statement.ExpressionStatement statement)
        {
            Evaluate(statement.Expression);
            return null;
        }

        public object VisitFunctionStatement(Statement.Function statement)
        {
            var function = new LoxFunction(statement, _environment);
            _environment.Define(statement.Name.Lexeme, function);
            return null;
        }

        public object VisitIfStatement(Statement.If statement)
        {
            if (IsTruthy(statement.Condition))
            {
                Execute(statement.ThenBranch);
            }
            else if (statement.ElseBranch != null)
            {
                Execute(statement.ElseBranch);
            }
            // else do nothing

            return null;
        }

        public object VisitPrintStatement(Statement.Print statement)
        {
            var value = Evaluate(statement.Expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStatement(Statement.Return statement)
        {
            object value = null;
            if (statement.Value != null)
            {
                value = Evaluate(statement.Value);
            }

            throw new Return(value);
        }

        public object VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
        {
            object value = null;
            if (statement.Initializer != null)
            {
                value = Evaluate(statement.Initializer);
            }

            _environment.Define(statement.Name.Lexeme, value);
            return null;
        }

        public object VisitWhileStatement(Statement.While statement)
        {
            while (IsTruthy(Evaluate(statement.Condition)))
            {
                Execute(statement.Body);
            }

            return null;
        }

        public object VisitBlockStatement(Statement.Block statement)
        {
            ExecuteBlock(statement.Statements, new Environment(_environment));
            return null;
        }
    }
}
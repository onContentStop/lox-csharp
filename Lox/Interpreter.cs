using System;
using System.Transactions;
using Lox.Syntax;

namespace Lox
{
    internal class Interpreter : Expression.IVisitor<object>
    {
        private readonly IErrorReporter _errorReporter;

        public Interpreter(IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
        }

        public void Interpret(Expression expression)
        {
            try
            {
                var value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError error)
            {
                _errorReporter.ReportRuntimeError(error);
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
            }

            throw new Exception("Unknown binary expression was reached");
        }

        public object VisitGroupingExpression(Expression.Grouping expression)
        {
            return Evaluate(expression.Expression);
        }

        public object VisitLiteralExpression(Expression.Literal expression)
        {
            return expression.Value;
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
                    var text = value.ToString();
                    if (text != null && text.EndsWith(".0"))
                    {
                        text = text.Substring(0, text.Length - 2);
                    }

                    return text;
                }
                default:
                    return value.ToString();
            }
        }
    }
}
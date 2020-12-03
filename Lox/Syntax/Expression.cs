using System.Collections.Generic;

namespace Lox.Syntax
{
    public abstract class Expression
    {
        public interface IVisitor<out T>
        {
            T VisitAssignmentExpression(Assignment expression);
            T VisitBinaryExpression(Binary expression);
            T VisitGroupingExpression(Grouping expression);
            T VisitLiteralExpression(Literal expression);
            T VisitUnaryExpression(Unary expression);
            T VisitVariableExpression(Variable expression);
        }

        public sealed class Assignment : Expression
        {
            public Token Name { get; }
            public Expression Value { get; }

            public Assignment(Token name, Expression value)
            {
                Name = name;
                Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitAssignmentExpression(this);
            }
        }

        public sealed class Binary : Expression
        {
            public Expression Left { get; }
            public Token OperatorToken { get; }
            public Expression Right { get; }

            public Binary(Expression left, Token operatorToken, Expression right)
            {
                Left = left;
                OperatorToken = operatorToken;
                Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBinaryExpression(this);
            }
        }

        public sealed class Grouping : Expression
        {
            public Expression Expression { get; }

            public Grouping(Expression expression)
            {
                Expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGroupingExpression(this);
            }
        }

        public sealed class Literal : Expression
        {
            public object Value { get; }

            public Literal(object value)
            {
                Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLiteralExpression(this);
            }
        }

        public sealed class Unary : Expression
        {
            public Token OperatorToken { get; }
            public Expression Right { get; }

            public Unary(Token operatorToken, Expression right)
            {
                OperatorToken = operatorToken;
                Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitUnaryExpression(this);
            }
        }

        public sealed class Variable : Expression
        {
            public Token Name { get; }

            public Variable(Token name)
            {
                Name = name;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVariableExpression(this);
            }
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}

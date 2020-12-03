using System.Collections.Generic;

namespace Lox.Syntax
{
    public abstract class Statement
    {
        public interface IVisitor<out T>
        {
            T VisitExpressionStatementStatement(ExpressionStatement statement);
            T VisitPrintStatement(Print statement);
            T VisitVariableDeclarationStatement(VariableDeclaration statement);
            T VisitBlockStatement(Block statement);
        }

        public sealed class ExpressionStatement : Statement
        {
            public Expression Expression { get; }

            public ExpressionStatement(Expression expression)
            {
                Expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitExpressionStatementStatement(this);
            }
        }

        public sealed class Print : Statement
        {
            public Expression Expression { get; }

            public Print(Expression expression)
            {
                Expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitPrintStatement(this);
            }
        }

        public sealed class VariableDeclaration : Statement
        {
            public Token Name { get; }
            public Expression Initializer { get; }

            public VariableDeclaration(Token name, Expression initializer)
            {
                Name = name;
                Initializer = initializer;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVariableDeclarationStatement(this);
            }
        }

        public sealed class Block : Statement
        {
            public IEnumerable<Statement> Statements { get; }

            public Block(IEnumerable<Statement> statements)
            {
                Statements = statements;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBlockStatement(this);
            }
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}

using System.Collections.Generic;

namespace Lox.Syntax
{
    public abstract class Statement
    {
        public interface IVisitor<out T>
        {
            T VisitExpressionStatementStatement(ExpressionStatement statement);
            T VisitIfStatement(If statement);
            T VisitPrintStatement(Print statement);
            T VisitVariableDeclarationStatement(VariableDeclaration statement);
            T VisitWhileStatement(While statement);
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

        public sealed class If : Statement
        {
            public Expression Condition { get; }
            public Statement ThenBranch { get; }
            public Statement ElseBranch { get; }

            public If(Expression condition, Statement thenBranch, Statement elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitIfStatement(this);
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

        public sealed class While : Statement
        {
            public Expression Condition { get; }
            public Statement Body { get; }

            public While(Expression condition, Statement body)
            {
                Condition = condition;
                Body = body;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitWhileStatement(this);
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

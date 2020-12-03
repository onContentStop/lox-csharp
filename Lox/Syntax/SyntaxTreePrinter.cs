using System.Text;

namespace Lox.Syntax
{
    public class SyntaxTreePrinter : Expression.IVisitor<string>
    {
        public string Print(Expression expression)
        {
            return expression.Accept(this);
        }

        public string VisitAssignmentExpression(Expression.Assignment expression)
        {
            return Parenthesize($"assign {expression.Name}", expression.Value);
        }

        public string VisitBinaryExpression(Expression.Binary expression)
        {
            return Parenthesize(expression.OperatorToken.Lexeme, expression.Left, expression.Right);
        }

        public string VisitGroupingExpression(Expression.Grouping expression)
        {
            return Parenthesize("group", expression.Expression);
        }

        public string VisitLiteralExpression(Expression.Literal expression)
        {
            return expression.Value == null ? "nil" : expression.Value.ToString();
        }

        public string VisitLogicalExpression(Expression.Logical expression)
        {
            return Parenthesize(expression.OperatorToken.Lexeme, expression.Left, expression.Right);
        }

        public string VisitUnaryExpression(Expression.Unary expression)
        {
            return Parenthesize(expression.OperatorToken.Lexeme, expression.Right);
        }

        public string VisitVariableExpression(Expression.Variable expression)
        {
            return $"{expression.Name.Lexeme}";
        }

        private string Parenthesize(string name, params Expression[] expressions)
        {
            var builder = new StringBuilder();
            builder.Append("(");
            builder.Append(name);
            foreach (var expression in expressions)
            {
                builder.Append(" ");
                builder.Append(expression.Accept(this));
            }

            builder.Append(")");

            return builder.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lox.Syntax;

namespace Lox
{
    internal sealed class Parser
    {
        private class ParseError : Exception
        {
        }

        private readonly IErrorReporter _errorReporter;
        private readonly ImmutableList<Token> _tokens;
        private int _current;

        public Parser(IEnumerable<Token> tokens, IErrorReporter errorReporter)
        {
            _tokens = tokens.ToImmutableList();
            _errorReporter = errorReporter;
            _current = 0;
        }

        public IEnumerable<Statement> Parse()
        {
            var statements = new List<Statement>();
            while (!AtEnd)
            {
                statements.Add(ParseDeclaration());
            }

            return statements;
        }

        private Statement ParseDeclaration()
        {
            try
            {
                if (Match(TokenType.VarKeyword))
                {
                    return ParseVariableDeclaration();
                }

                return ParseStatement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Statement ParseVariableDeclaration()
        {
            var name = Consume(TokenType.IdentifierToken, "Expected variable name.");

            Expression initializer = null;
            if (Match(TokenType.EqualsToken))
            {
                initializer = ParseExpression();
            }

            Consume(TokenType.SemicolonToken, "Expected ';' after variable declaration.");
            return new Statement.VariableDeclaration(name, initializer);
        }

        private Statement ParseStatement()
        {
            if (Match(TokenType.PrintKeyword))
            {
                return ParsePrintStatement();
            }

            if (Match(TokenType.LeftBraceToken))
            {
                return new Statement.Block(ParseBlockStatement());
            }

            return ParseExpressionStatement();
        }

        private IEnumerable<Statement> ParseBlockStatement()
        {
            var statements = new List<Statement>();

            while (!Check(TokenType.RightBraceToken) && !AtEnd)
            {
                statements.Add(ParseDeclaration());
            }

            Consume(TokenType.RightBraceToken, "Expected '}' after block.");
            return statements;
        }

        private Statement ParsePrintStatement()
        {
            var value = ParseExpression();
            Consume(TokenType.SemicolonToken, "Expected ';' after value.");
            return new Statement.Print(value);
        }

        private Statement ParseExpressionStatement()
        {
            var expression = ParseExpression();
            Consume(TokenType.SemicolonToken, "Expected ';' after expression.");
            return new Statement.ExpressionStatement(expression);
        }

        private Expression ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private Expression ParseAssignmentExpression()
        {
            var expression = ParseEqualityExpression();

            if (Match(TokenType.EqualsToken))
            {
                var equalsToken = Previous;
                var value = ParseAssignmentExpression();

                if (expression is Expression.Variable v)
                {
                    var name = v.Name;
                    return new Expression.Assignment(name, value);
                }
                
                _errorReporter.Report(equalsToken, "Invalid assignment target.");
            }

            return expression;
        }

        private Expression ParseEqualityExpression()
        {
            var expression = ParseComparisonExpression();

            while (Match(TokenType.BangEqualsToken, TokenType.EqualsEqualsToken))
            {
                var operatorToken = Previous;
                var right = ParseComparisonExpression();
                expression = new Expression.Binary(expression, operatorToken, right);
            }

            return expression;
        }

        private Expression ParseComparisonExpression()
        {
            var expression = ParseTermExpression();

            while (Match(TokenType.GreaterToken, TokenType.GreaterEqualsToken, TokenType.LessToken,
                TokenType.LessEqualsToken))
            {
                var operatorToken = Previous;
                var right = ParseTermExpression();
                expression = new Expression.Binary(expression, operatorToken, right);
            }

            return expression;
        }

        private Expression ParseTermExpression()
        {
            var expression = ParseFactorExpression();

            while (Match(TokenType.MinusToken, TokenType.PlusToken))
            {
                var operatorToken = Previous;
                var right = ParseFactorExpression();
                expression = new Expression.Binary(expression, operatorToken, right);
            }

            return expression;
        }

        private Expression ParseFactorExpression()
        {
            var expression = ParseUnaryExpression();

            while (Match(TokenType.SlashToken, TokenType.StarToken))
            {
                var operatorToken = Previous;
                var right = ParseUnaryExpression();
                expression = new Expression.Binary(expression, operatorToken, right);
            }

            return expression;
        }

        private Expression ParseUnaryExpression()
        {
            if (Match(TokenType.BangToken, TokenType.MinusToken))
            {
                var operatorToken = Previous;
                var right = ParseUnaryExpression();
                return new Expression.Unary(operatorToken, right);
            }

            return ParsePrimaryExpression();
        }

        private Expression ParsePrimaryExpression()
        {
            if (Match(TokenType.FalseKeyword))
            {
                return new Expression.Literal(false);
            }

            if (Match(TokenType.TrueKeyword))
            {
                return new Expression.Literal(true);
            }

            if (Match(TokenType.NilKeyword))
            {
                return new Expression.Literal(null);
            }

            if (Match(TokenType.NumberToken, TokenType.StringToken))
            {
                return new Expression.Literal(Previous.Literal);
            }

            if (Match(TokenType.IdentifierToken))
            {
                return new Expression.Variable(Previous);
            }

            if (Match(TokenType.LeftParenthesisToken))
            {
                var expression = ParseExpression();
                Consume(TokenType.RightParenthesisToken, "Expected ')' after expression.");
                return new Expression.Grouping(expression);
            }

            throw Error(Peek, "Expected expression.");
        }

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek, errorMessage);
        }

        private ParseError Error(Token token, string message)
        {
            _errorReporter.Report(token, message);
            return new ParseError();
        }

        private bool Match(params TokenType[] types)
        {
            if (!types.Any(Check)) return false;

            Advance();
            return true;
        }

        private bool Check(TokenType type)
        {
            if (AtEnd)
            {
                return false;
            }

            return Peek.Type == type;
        }

        private Token Advance()
        {
            if (!AtEnd)
            {
                ++_current;
            }

            return Previous;
        }

        private void Synchronize()
        {
            Advance();

            while (!AtEnd)
            {
                if (Previous.Type == TokenType.SemicolonToken)
                {
                    return;
                }

                if (Peek.Type == TokenType.ClassKeyword || Peek.Type == TokenType.FunKeyword ||
                    Peek.Type == TokenType.VarKeyword || Peek.Type == TokenType.ForKeyword ||
                    Peek.Type == TokenType.IfKeyword || Peek.Type == TokenType.WhileKeyword ||
                    Peek.Type == TokenType.PrintKeyword || Peek.Type == TokenType.ReturnKeyword)
                    return;

                Advance();
            }
        }

        private bool AtEnd => Peek.Type == TokenType.EndOfFileToken;
        private Token Peek => _tokens[_current];
        private Token Previous => _tokens[_current - 1];
    }
}
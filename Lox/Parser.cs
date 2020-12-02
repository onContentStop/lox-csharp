using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Expression = Lox.Syntax.Expression;

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

        public Expression Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseError)
            {
                return null;
            }
        }

        private Expression Expression()
        {
            return Equality();
        }

        private Expression Equality()
        {
            var expression = Comparison();

            while (Match(TokenType.BangEqualsToken, TokenType.EqualsEqualsToken))
            {
                var operatorToken = Previous;
                var right = Comparison();
                expression = new Expression.Binary(expression, operatorToken, right);
            }

            return expression;
        }

        private Expression Comparison()
        {
            var expression = Term();

            while (Match(TokenType.GreaterToken, TokenType.GreaterEqualsToken, TokenType.LessToken,
                TokenType.LessEqualsToken))
            {
                var operatorToken = Previous;
                var right = Term();
                expression = new Expression.Binary(expression, operatorToken, right);
            }

            return expression;
        }

        private Expression Term()
        {
            var expression = Factor();

            while (Match(TokenType.MinusToken, TokenType.PlusToken))
            {
                var operatorToken = Previous;
                var right = Factor();
                expression = new Expression.Binary(expression, operatorToken, right);
            }

            return expression;
        }

        private Expression Factor()
        {
            var expression = Unary();

            while (Match(TokenType.SlashToken, TokenType.StarToken))
            {
                var operatorToken = Previous;
                var right = Unary();
                expression = new Expression.Binary(expression, operatorToken, right);
            }

            return expression;
        }

        private Expression Unary()
        {
            if (Match(TokenType.BangToken, TokenType.MinusToken))
            {
                var operatorToken = Previous;
                var right = Unary();
                return new Expression.Unary(operatorToken, right);
            }

            return Primary();
        }

        private Expression Primary()
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

            if (Match(TokenType.LeftParenthesisToken))
            {
                var expression = Expression();
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
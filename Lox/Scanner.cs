using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lox
{
    internal class Scanner
    {
        private static readonly ImmutableDictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"and", TokenType.AndKeyword},
            {"class", TokenType.ClassKeyword},
            {"else", TokenType.ElseKeyword},
            {"false", TokenType.FalseKeyword},
            {"for", TokenType.ForKeyword},
            {"fun", TokenType.FunKeyword},
            {"if", TokenType.IfKeyword},
            {"nil", TokenType.NilKeyword},
            {"or", TokenType.OrKeyword},
            {"print", TokenType.PrintKeyword},
            {"return", TokenType.ReturnKeyword},
            {"super", TokenType.SuperKeyword},
            {"this", TokenType.ThisKeyword},
            {"true", TokenType.TrueKeyword},
            {"var", TokenType.VarKeyword},
            {"while", TokenType.WhileKeyword},
        }.ToImmutableDictionary();

        private readonly IErrorReporter _errorReporter;
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
        private int _start;
        private int _current;
        private int _line;

        public Scanner(string source, IErrorReporter errorReporter)
        {
            _source = source;
            _errorReporter = errorReporter;
            _start = 0;
            _current = 0;
            _line = 1;
        }

        public IEnumerable<Token> ScanTokens()
        {
            while (!AtEnd)
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EndOfFileToken, "", null, _line));
            return _tokens;
        }

        private void ScanToken()
        {
            var c = Advance();
            switch (c)
            {
                case '(':
                    AddToken(TokenType.LeftParenthesisToken);
                    break;
                case ')':
                    AddToken(TokenType.RightParenthesisToken);
                    break;
                case '{':
                    AddToken(TokenType.LeftBraceToken);
                    break;
                case '}':
                    AddToken(TokenType.RightBraceToken);
                    break;
                case ',':
                    AddToken(TokenType.CommaToken);
                    break;
                case '.':
                    AddToken(TokenType.DotToken);
                    break;
                case '-':
                    AddToken(TokenType.MinusToken);
                    break;
                case '+':
                    AddToken(TokenType.PlusToken);
                    break;
                case ';':
                    AddToken(TokenType.SemicolonToken);
                    break;
                case '*':
                    AddToken(TokenType.StarToken);
                    break;
                case '!':
                    AddToken(Match('=') ? TokenType.BangEqualsToken : TokenType.BangToken);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EqualsEqualsToken : TokenType.EqualsToken);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LessEqualsToken : TokenType.LessToken);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GreaterEqualsToken : TokenType.GreaterToken);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        ScanLineComment();
                    }
                    else if (Match('*'))
                    {
                        ScanBlockComment();
                    }
                    else
                    {
                        AddToken(TokenType.SlashToken);
                    }

                    break;
                case ' ':
                case '\r':
                case '\t':
                    // ignore whitespace
                    break;
                case '\n':
                    ++_line;
                    break;

                case '"':
                    ScanString();
                    break;
                default:
                    if (char.IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (char.IsLetter(c) || c == '_')
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        _errorReporter.Report(_line, "Unexpected character.");
                    }

                    break;
            }
        }

        private void ScanBlockComment()
        {
            while (true)
            {
                if (AtEnd)
                {
                    _errorReporter.Report(_line, "Unterminated block comment.");
                    break;
                }

                if (Peek == '\n')
                {
                    ++_line;
                }

                if (Peek == '*' && PeekNext == '/')
                {
                    Advance();
                    Advance();
                    return;
                }

                Advance();
            }
        }

        private void ScanIdentifier()
        {
            while (char.IsLetter(Peek) || char.IsDigit(Peek) || Peek == '_')
            {
                Advance();
            }

            var text = _source.Substring(_start, _current - _start);
            AddToken(Keywords.TryGetValue(text, out var type) ? type : TokenType.IdentifierToken);
        }

        private void ScanNumber()
        {
            while (char.IsDigit(Peek))
            {
                Advance();
            }

            if (Peek == '.' && char.IsDigit(PeekNext))
            {
                Advance();
                while (char.IsDigit(Peek))
                {
                    Advance();
                }
            }

            var text = _source.Substring(_start, _current - _start);
            if (decimal.TryParse(text, out var value))
            {
                AddToken(TokenType.NumberToken, value);
            }
            else
            {
                _errorReporter.Report(_line, "Number literal cannot be represented by the decimal type", $" at {text}");
            }
        }

        private void ScanString()
        {
            while (Peek != '"' && !AtEnd)
            {
                if (Peek == '\n')
                {
                    ++_line;
                }

                Advance();
            }

            if (AtEnd)
            {
                _errorReporter.Report(_line, "Unterminated string.");
                return;
            }

            Advance();
            var value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.StringToken, value);
        }

        private void ScanLineComment()
        {
            while (Peek != '\n' && !AtEnd)
            {
                Advance();
            }
        }

        private bool AtEnd => _current >= _source.Length;
        private char Peek => AtEnd ? '\0' : _source[_current];
        private char PeekNext => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];

        private char Advance()
        {
            ++_current;
            return _source[_current - 1];
        }

        private bool Match(char expected)
        {
            if (Peek != expected)
            {
                return false;
            }

            ++_current;
            return true;
        }

        private void AddToken(TokenType type, object literal = null)
        {
            var text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }
    }
}
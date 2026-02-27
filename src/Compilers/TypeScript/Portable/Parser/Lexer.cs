// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal class Lexer : IDisposable
    {
        private readonly SourceText _text;
        private readonly SlidingTextWindow _textWindow;
        private readonly SyntaxListBuilder _leadingTriviaCache = new SyntaxListBuilder(10);
        private readonly SyntaxListBuilder _trailingTriviaCache = new SyntaxListBuilder(10);
        private readonly StringBuilder _builder = new StringBuilder();

        public Lexer(SourceText text)
        {
            _text = text;
            _textWindow = new SlidingTextWindow(text);
        }

        public void Dispose()
        {
            _textWindow.Dispose();
        }

        public SyntaxToken Lex()
        {
            _leadingTriviaCache.Clear();
            ScanTrivia(_leadingTriviaCache);

            var info = default(DiagnosticInfo[]);
            var start = _textWindow.Position;
            var kind = ScanSyntaxToken(ref info);
            var end = _textWindow.Position;
            var width = end - start;

            _trailingTriviaCache.Clear();
            ScanTrivia(_trailingTriviaCache);

            var leading = _leadingTriviaCache.ToList();
            var trailing = _trailingTriviaCache.ToList();

            // Handle errors (attach to token)
            // TODO: Errors

            var token = SyntaxToken.Create(kind, leading.Node, trailing.Node);

            if (kind == SyntaxKind.IdentifierToken || kind == SyntaxKind.NumericLiteralToken || kind == SyntaxKind.StringLiteralToken)
            {
                var text = _textWindow.Text.ToString(new TextSpan(start, width));
                return new SyntaxToken(kind, text, leading.Node, trailing.Node);
            }

            return token;
        }

        private void ScanTrivia(SyntaxListBuilder trivia)
        {
            while (true)
            {
                var ch = _textWindow.PeekChar();
                if (ch == SlidingTextWindow.InvalidCharacter)
                {
                    break;
                }

                if (SyntaxFacts.IsWhitespace(ch))
                {
                    ScanWhitespace(trivia);
                    continue;
                }

                if (SyntaxFacts.IsNewLine(ch))
                {
                    ScanEndOfLine(trivia);
                    continue;
                }

                if (ch == '/')
                {
                    var ch2 = _textWindow.PeekChar(1);
                    if (ch2 == '/')
                    {
                        ScanSingleLineComment(trivia);
                        continue;
                    }
                    if (ch2 == '*')
                    {
                        ScanMultiLineComment(trivia);
                        continue;
                    }
                }

                break;
            }
        }

        private void ScanWhitespace(SyntaxListBuilder trivia)
        {
            var start = _textWindow.Position;
            while (SyntaxFacts.IsWhitespace(_textWindow.PeekChar()))
            {
                _textWindow.AdvanceChar();
            }
            var text = _textWindow.Text.ToString(new TextSpan(start, _textWindow.Position - start));
            trivia.Add(SyntaxFactory.Whitespace(text));
        }

        private void ScanEndOfLine(SyntaxListBuilder trivia)
        {
            var start = _textWindow.Position;
            var ch = _textWindow.PeekChar();
            if (ch == '\r')
            {
                _textWindow.AdvanceChar();
                if (_textWindow.PeekChar() == '\n')
                {
                    _textWindow.AdvanceChar();
                }
            }
            else
            {
                _textWindow.AdvanceChar();
            }
            var text = _textWindow.Text.ToString(new TextSpan(start, _textWindow.Position - start));
            trivia.Add(SyntaxFactory.EndOfLine(text));
        }

        private void ScanSingleLineComment(SyntaxListBuilder trivia)
        {
            var start = _textWindow.Position;
            _textWindow.AdvanceChar(2); // //
            while (true)
            {
                var ch = _textWindow.PeekChar();
                if (ch == SlidingTextWindow.InvalidCharacter || SyntaxFacts.IsNewLine(ch))
                {
                    break;
                }
                _textWindow.AdvanceChar();
            }
            var text = _textWindow.Text.ToString(new TextSpan(start, _textWindow.Position - start));
            trivia.Add(SyntaxFactory.Comment(text));
        }

        private void ScanMultiLineComment(SyntaxListBuilder trivia)
        {
            var start = _textWindow.Position;
            _textWindow.AdvanceChar(2); // /*
            while (true)
            {
                var ch = _textWindow.PeekChar();
                if (ch == SlidingTextWindow.InvalidCharacter)
                {
                    // Error: unterminated comment
                    break;
                }
                if (ch == '*' && _textWindow.PeekChar(1) == '/')
                {
                    _textWindow.AdvanceChar(2);
                    break;
                }
                _textWindow.AdvanceChar();
            }
            var text = _textWindow.Text.ToString(new TextSpan(start, _textWindow.Position - start));
            trivia.Add(SyntaxFactory.Comment(text));
        }

        private SyntaxKind ScanSyntaxToken(ref DiagnosticInfo[]? diagnostics)
        {
            char ch = _textWindow.PeekChar();
            if (ch == SlidingTextWindow.InvalidCharacter)
            {
                return SyntaxKind.EndOfFileToken;
            }

            if (SyntaxFacts.IsIdentifierStartCharacter(ch))
            {
                return ScanIdentifierOrKeyword();
            }

            if (char.IsDigit(ch))
            {
                return ScanNumericLiteral();
            }

            switch (ch)
            {
                case '{': _textWindow.AdvanceChar(); return SyntaxKind.OpenBraceToken;
                case '}': _textWindow.AdvanceChar(); return SyntaxKind.CloseBraceToken;
                case '(': _textWindow.AdvanceChar(); return SyntaxKind.OpenParenToken;
                case ')': _textWindow.AdvanceChar(); return SyntaxKind.CloseParenToken;
                case '[': _textWindow.AdvanceChar(); return SyntaxKind.OpenBracketToken;
                case ']': _textWindow.AdvanceChar(); return SyntaxKind.CloseBracketToken;
                case ';': _textWindow.AdvanceChar(); return SyntaxKind.SemicolonToken;
                case ',': _textWindow.AdvanceChar(); return SyntaxKind.CommaToken;
                case '.':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '.' && _textWindow.PeekChar(1) == '.')
                    {
                        _textWindow.AdvanceChar(2);
                        return SyntaxKind.DotDotDotToken;
                    }
                    return SyntaxKind.DotToken;
                case ':': _textWindow.AdvanceChar(); return SyntaxKind.ColonToken;
                case '?': _textWindow.AdvanceChar(); return SyntaxKind.QuestionToken; // Could be ?? or ?.
                case '~': _textWindow.AdvanceChar(); return SyntaxKind.TildeToken;
                case '<':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '<') {
                        _textWindow.AdvanceChar();
                        if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.LessThanLessThanEqualsToken; }
                        return SyntaxKind.LessThanLessThanToken;
                    }
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.LessThanEqualsToken; } // <=
                    return SyntaxKind.LessThanToken;
                case '>':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '>') {
                        _textWindow.AdvanceChar();
                        if (_textWindow.PeekChar() == '>') {
                            _textWindow.AdvanceChar();
                            if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken; }
                            return SyntaxKind.GreaterThanGreaterThanGreaterThanToken;
                        }
                        if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.GreaterThanGreaterThanEqualsToken; }
                        return SyntaxKind.GreaterThanGreaterThanToken;
                    }
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.GreaterThanEqualsToken; } // >=
                    return SyntaxKind.GreaterThanToken;
                case '=':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '=') {
                        _textWindow.AdvanceChar();
                        if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.EqualsEqualsToken; } // === logic (map to EqualsEquals for now or add TripleEquals)
                        return SyntaxKind.EqualsEqualsToken;
                    }
                    if (_textWindow.PeekChar() == '>') { _textWindow.AdvanceChar(); return SyntaxKind.EqualsGreaterThanToken; } // => (Need to add to Kind)
                    return SyntaxKind.EqualsToken;
                case '!':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '=') {
                        _textWindow.AdvanceChar();
                        if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.ExclamationEqualsToken; } // !==
                        return SyntaxKind.ExclamationEqualsToken;
                    }
                    return SyntaxKind.ExclamationToken;
                case '+':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '+') { _textWindow.AdvanceChar(); return SyntaxKind.PlusPlusToken; }
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.PlusEqualsToken; }
                    return SyntaxKind.PlusToken;
                case '-':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '-') { _textWindow.AdvanceChar(); return SyntaxKind.MinusMinusToken; }
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.MinusEqualsToken; }
                    return SyntaxKind.MinusToken;
                case '*':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.AsteriskEqualsToken; }
                    return SyntaxKind.AsteriskToken;
                case '/':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.SlashEqualsToken; }
                    return SyntaxKind.SlashToken;
                case '%':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.PercentEqualsToken; }
                    return SyntaxKind.PercentToken;
                case '&':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '&') { _textWindow.AdvanceChar(); return SyntaxKind.AmpersandAmpersandToken; }
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.AmpersandEqualsToken; }
                    return SyntaxKind.AmpersandToken;
                case '|':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '|') { _textWindow.AdvanceChar(); return SyntaxKind.BarBarToken; }
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.BarEqualsToken; }
                    return SyntaxKind.BarToken;
                case '^':
                    _textWindow.AdvanceChar();
                    if (_textWindow.PeekChar() == '=') { _textWindow.AdvanceChar(); return SyntaxKind.CaretEqualsToken; }
                    return SyntaxKind.CaretToken;
                case '"':
                case '\'':
                    return ScanStringLiteral();
            }

            _textWindow.AdvanceChar();
            return SyntaxKind.None; // Bad token
        }

        private SyntaxKind ScanIdentifierOrKeyword()
        {
            var start = _textWindow.Position;
            while (SyntaxFacts.IsIdentifierPartCharacter(_textWindow.PeekChar()))
            {
                _textWindow.AdvanceChar();
            }

            int length = _textWindow.Position - start;
            // Get text to check keyword
            string text = _textWindow.Text.ToString(new TextSpan(start, length));
            return SyntaxFacts.GetKeywordKind(text);
        }

        private SyntaxKind ScanNumericLiteral()
        {
            while (char.IsDigit(_textWindow.PeekChar()))
            {
                _textWindow.AdvanceChar();
            }
            if (_textWindow.PeekChar() == '.' && char.IsDigit(_textWindow.PeekChar(1)))
            {
                _textWindow.AdvanceChar();
                while (char.IsDigit(_textWindow.PeekChar()))
                {
                    _textWindow.AdvanceChar();
                }
            }
            return SyntaxKind.NumericLiteralToken;
        }

        private SyntaxKind ScanStringLiteral()
        {
            char quote = _textWindow.NextChar(); // " or '
            while (true)
            {
                char ch = _textWindow.NextChar();
                if (ch == SlidingTextWindow.InvalidCharacter || SyntaxFacts.IsNewLine(ch))
                {
                    // Error: unterminated string
                    break;
                }
                if (ch == quote)
                {
                    break;
                }
                if (ch == '\\')
                {
                    _textWindow.AdvanceChar(); // Skip escaped char
                }
            }
            return SyntaxKind.StringLiteralToken;
        }
    }
}

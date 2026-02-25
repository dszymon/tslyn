using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal class Lexer
    {
        private readonly string _text;
        private int _position;
        private int _start;

        public Lexer(string text)
        {
            _text = text;
            _position = 0;
        }

        public SyntaxToken Lex()
        {
            _start = _position;
            var leadingTrivia = LexTrivia(isTrailing: false);

            _start = _position;
            if (_position >= _text.Length)
            {
                return SyntaxFactory.Token(leadingTrivia, SyntaxKind.EndOfFileToken, null);
            }

            char ch = _text[_position];
            SyntaxKind kind = SyntaxKind.BadToken;
            string? text = null;
            string? valueText = null;

            if (IsIdentifierStart(ch))
            {
                int start = _position;
                while (_position < _text.Length && IsIdentifierPart(_text[_position]))
                {
                    _position++;
                }
                string id = _text.Substring(start, _position - start);
                kind = GetKeywordKind(id);
                if (kind == SyntaxKind.IdentifierToken)
                {
                    text = id;
                    valueText = id;
                }
            }
            else if (char.IsDigit(ch))
            {
                kind = SyntaxKind.NumericLiteralToken;
                int start = _position;
                while (_position < _text.Length && char.IsDigit(_text[_position]))
                {
                    _position++;
                }
                text = _text.Substring(start, _position - start);
                valueText = text; // Simplified value
            }
            else if (ch == '"' || ch == '\'' || ch == '`') // Strings and Templates
            {
                char quote = ch;
                kind = quote == '`' ? SyntaxKind.NoSubstitutionTemplateToken : SyntaxKind.StringLiteralToken; // Simplified
                int start = _position;
                _position++;
                while (_position < _text.Length && _text[_position] != quote)
                {
                    if (_text[_position] == '\\') _position++;
                    _position++;
                }
                if (_position < _text.Length) _position++;
                text = _text.Substring(start, _position - start);
                valueText = text.Substring(1, text.Length - 2); // Naive unquote
            }
            else
            {
                // Punctuation
                _position++;
                switch (ch)
                {
                    case '{': kind = SyntaxKind.OpenBraceToken; break;
                    case '}': kind = SyntaxKind.CloseBraceToken; break;
                    case '(': kind = SyntaxKind.OpenParenToken; break;
                    case ')': kind = SyntaxKind.CloseParenToken; break;
                    case '[': kind = SyntaxKind.OpenBracketToken; break;
                    case ']': kind = SyntaxKind.CloseBracketToken; break;
                    case ';': kind = SyntaxKind.SemicolonToken; break;
                    case ',': kind = SyntaxKind.CommaToken; break;
                    case '.': kind = SyntaxKind.DotToken; break; // Check for ...
                    case ':': kind = SyntaxKind.ColonToken; break;
                    case '?': kind = SyntaxKind.QuestionToken; break; // Check for ??, ?.
                    case '=':
                        if (Match('>')) { kind = SyntaxKind.EqualsGreaterThanToken; _position++; }
                        else if (Match('=')) {
                             if (Match('=')) { kind = SyntaxKind.EqualsEqualsEqualsToken; _position++; }
                             else kind = SyntaxKind.EqualsEqualsToken;
                             _position++;
                        }
                        else kind = SyntaxKind.EqualsToken;
                        break;
                    case '+': kind = SyntaxKind.PlusToken; break; // ++, +=
                    case '-': kind = SyntaxKind.MinusToken; break; // --, -=
                    case '*': kind = SyntaxKind.AsteriskToken; break; // **, *=
                    case '/': kind = SyntaxKind.SlashToken; break; // comments handled in LexTrivia
                    case '%': kind = SyntaxKind.PercentToken; break;
                    case '&': kind = SyntaxKind.AmpersandToken; break;
                    case '|': kind = SyntaxKind.BarToken; break;
                    case '^': kind = SyntaxKind.CaretToken; break;
                    case '!': kind = SyntaxKind.ExclamationToken; break;
                    case '<': kind = SyntaxKind.LessThanToken; break;
                    case '>': kind = SyntaxKind.GreaterThanToken; break;
                    default: kind = SyntaxKind.BadToken; text = ch.ToString(); break;
                }
            }

            var trailingTrivia = LexTrivia(isTrailing: true);

            if (text == null && kind != SyntaxKind.BadToken)
            {
                return SyntaxFactory.Token(leadingTrivia, kind, trailingTrivia);
            }

            if (kind == SyntaxKind.IdentifierToken)
            {
                return SyntaxToken.Identifier(kind, leadingTrivia, text!, valueText!, trailingTrivia);
            }

            if (kind == SyntaxKind.StringLiteralToken || kind == SyntaxKind.NumericLiteralToken)
            {
                 // Need generic WithValue here but SyntaxToken factories are limited in my previous impl.
                 // Using Identifier for now if WithValue not exposed statically generically enough
                 return SyntaxToken.WithValue(kind, text!, valueText!, leadingTrivia, trailingTrivia);
            }

            return SyntaxFactory.Token(leadingTrivia, kind, trailingTrivia);
        }

        private GreenNode? LexTrivia(bool isTrailing)
        {
            // Simple whitespace/newline lexing
            // In a real lexer, this would handle lists of trivia
            // For now, let's just eat whitespace and return a single node if any
            int start = _position;
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
            }

            if (_position > start)
            {
                string triviaText = _text.Substring(start, _position - start);
                return SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, triviaText);
            }
            return null;
        }

        private bool Match(char ch)
        {
            return _position < _text.Length && _text[_position] == ch;
        }

        private static bool IsIdentifierStart(char ch)
        {
            return char.IsLetter(ch) || ch == '_' || ch == '$';
        }

        private static bool IsIdentifierPart(char ch)
        {
            return IsIdentifierStart(ch) || char.IsDigit(ch);
        }

        private static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "interface": return SyntaxKind.InterfaceKeyword;
                case "class": return SyntaxKind.ClassKeyword;
                case "enum": return SyntaxKind.EnumKeyword;
                case "module": return SyntaxKind.ModuleKeyword;
                case "namespace": return SyntaxKind.NamespaceKeyword;
                case "function": return SyntaxKind.FunctionKeyword;
                case "var": return SyntaxKind.VarKeyword;
                case "let": return SyntaxKind.LetKeyword;
                case "const": return SyntaxKind.ConstKeyword;
                case "import": return SyntaxKind.ImportKeyword;
                case "export": return SyntaxKind.ExportKeyword;
                case "if": return SyntaxKind.IfKeyword;
                case "else": return SyntaxKind.ElseKeyword;
                case "for": return SyntaxKind.ForKeyword;
                case "while": return SyntaxKind.WhileKeyword;
                case "do": return SyntaxKind.DoKeyword;
                case "switch": return SyntaxKind.SwitchKeyword;
                case "case": return SyntaxKind.CaseKeyword;
                case "default": return SyntaxKind.DefaultKeyword;
                case "break": return SyntaxKind.BreakKeyword;
                case "continue": return SyntaxKind.ContinueKeyword;
                case "return": return SyntaxKind.ReturnKeyword;
                case "throw": return SyntaxKind.ThrowKeyword;
                case "try": return SyntaxKind.TryKeyword;
                case "catch": return SyntaxKind.CatchKeyword;
                case "finally": return SyntaxKind.FinallyKeyword;
                case "true": return SyntaxKind.TrueKeyword;
                case "false": return SyntaxKind.FalseKeyword;
                case "null": return SyntaxKind.NullKeyword;
                case "undefined": return SyntaxKind.UndefinedKeyword;
                case "number": return SyntaxKind.NumberKeyword;
                case "string": return SyntaxKind.StringKeyword;
                case "boolean": return SyntaxKind.BooleanKeyword;
                case "any": return SyntaxKind.AnyKeyword;
                case "void": return SyntaxKind.VoidKeyword;
                case "public": return SyntaxKind.PublicKeyword;
                case "private": return SyntaxKind.PrivateKeyword;
                case "protected": return SyntaxKind.ProtectedKeyword;
                case "static": return SyntaxKind.StaticKeyword;
                case "readonly": return SyntaxKind.ReadonlyKeyword;
                case "extends": return SyntaxKind.ExtendsKeyword;
                case "implements": return SyntaxKind.ImplementsKeyword;
                case "new": return SyntaxKind.NewKeyword;
                case "this": return SyntaxKind.ThisKeyword;
                case "super": return SyntaxKind.SuperKeyword;
                case "from": return SyntaxKind.FromKeyword;
                case "as": return SyntaxKind.AsKeyword;
                default: return SyntaxKind.IdentifierToken;
            }
        }
    }
}

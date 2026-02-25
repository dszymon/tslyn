// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax;
using Xunit;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Syntax
{
    public class LexerTests
    {
        private List<Microsoft.CodeAnalysis.SyntaxToken> Lex(string text)
        {
            var sourceText = SourceText.From(text);
            using var lexer = new Lexer(sourceText);
            var tokens = new List<Microsoft.CodeAnalysis.SyntaxToken>();
            while (true)
            {
                // Lexer.Lex returns InternalSyntax.SyntaxToken (Green)
                var greenToken = lexer.Lex();
                if (greenToken.Kind == SyntaxKind.EndOfFileToken)
                    break;

                // Construct Red SyntaxToken (detached)
                var redToken = new Microsoft.CodeAnalysis.SyntaxToken(null, greenToken, 0, 0);
                tokens.Add(redToken);
            }
            return tokens;
        }

        [Fact]
        public void TestKeywords()
        {
            var text = "if else while return var let const function";
            var tokens = Lex(text);

            Assert.Equal(8, tokens.Count);
            Assert.Equal(SyntaxKind.IfKeyword, tokens[0].Kind());
            Assert.Equal(SyntaxKind.ElseKeyword, tokens[1].Kind());
            Assert.Equal(SyntaxKind.WhileKeyword, tokens[2].Kind());
            Assert.Equal(SyntaxKind.ReturnKeyword, tokens[3].Kind());
            Assert.Equal(SyntaxKind.VarKeyword, tokens[4].Kind());
            Assert.Equal(SyntaxKind.LetKeyword, tokens[5].Kind());
            Assert.Equal(SyntaxKind.ConstKeyword, tokens[6].Kind());
            Assert.Equal(SyntaxKind.FunctionKeyword, tokens[7].Kind());
        }

        [Fact]
        public void TestIdentifiers()
        {
            var text = "foo bar baz";
            var tokens = Lex(text);

            Assert.Equal(3, tokens.Count);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind());
            Assert.Equal("foo", tokens[0].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[1].Kind());
            Assert.Equal("bar", tokens[1].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[2].Kind());
            Assert.Equal("baz", tokens[2].Text);
        }

        [Fact]
        public void TestNumbers()
        {
            var text = "123 45.67";
            var tokens = Lex(text);

            Assert.Equal(2, tokens.Count);
            Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind());
            Assert.Equal("123", tokens[0].Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[1].Kind());
            Assert.Equal("45.67", tokens[1].Text);
        }

        [Fact]
        public void TestStrings()
        {
            var text = "\"hello\" 'world'";
            var tokens = Lex(text);

            Assert.Equal(2, tokens.Count);
            Assert.Equal(SyntaxKind.StringLiteralToken, tokens[0].Kind());
            Assert.Equal("\"hello\"", tokens[0].Text);
            Assert.Equal(SyntaxKind.StringLiteralToken, tokens[1].Kind());
            Assert.Equal("'world'", tokens[1].Text);
        }

        [Fact]
        public void TestPunctuation()
        {
            var text = "{ } ( ) ; , . + - * /";
            var tokens = Lex(text);

            Assert.Equal(11, tokens.Count);
            Assert.Equal(SyntaxKind.OpenBraceToken, tokens[0].Kind());
            Assert.Equal(SyntaxKind.CloseBraceToken, tokens[1].Kind());
            Assert.Equal(SyntaxKind.OpenParenToken, tokens[2].Kind());
            Assert.Equal(SyntaxKind.CloseParenToken, tokens[3].Kind());
            Assert.Equal(SyntaxKind.SemicolonToken, tokens[4].Kind());
            Assert.Equal(SyntaxKind.CommaToken, tokens[5].Kind());
            Assert.Equal(SyntaxKind.DotToken, tokens[6].Kind());
            Assert.Equal(SyntaxKind.PlusToken, tokens[7].Kind());
            Assert.Equal(SyntaxKind.MinusToken, tokens[8].Kind());
            Assert.Equal(SyntaxKind.AsteriskToken, tokens[9].Kind());
            Assert.Equal(SyntaxKind.SlashToken, tokens[10].Kind());
        }

        [Fact]
        public void TestCompoundPunctuation()
        {
            var text = "++ -- += -= == === != !== <= >=";
            var tokens = Lex(text);

            Assert.Equal(10, tokens.Count);
            Assert.Equal(SyntaxKind.PlusPlusToken, tokens[0].Kind());
            Assert.Equal(SyntaxKind.MinusMinusToken, tokens[1].Kind());
            Assert.Equal(SyntaxKind.PlusEqualsToken, tokens[2].Kind());
            Assert.Equal(SyntaxKind.MinusEqualsToken, tokens[3].Kind());
            Assert.Equal(SyntaxKind.EqualsEqualsToken, tokens[4].Kind());
            Assert.Equal(SyntaxKind.EqualsEqualsToken, tokens[5].Kind()); // === mapped to EqualsEqualsToken for now
            Assert.Equal(SyntaxKind.ExclamationEqualsToken, tokens[6].Kind());
            Assert.Equal(SyntaxKind.ExclamationEqualsToken, tokens[7].Kind()); // !== mapped
            Assert.Equal(SyntaxKind.LessThanEqualsToken, tokens[8].Kind());
            Assert.Equal(SyntaxKind.GreaterThanEqualsToken, tokens[9].Kind());
        }

        [Fact]
        public void TestCommentsAndWhitespace()
        {
            var text = "var/*comment*/x // line comment\n = 1;";
            var tokens = Lex(text);

            // tokens: var, x, =, 1, ;
            Assert.Equal(5, tokens.Count);

            // var
            Assert.Equal(SyntaxKind.VarKeyword, tokens[0].Kind());
            Assert.Equal(0, tokens[0].LeadingTrivia.Count);
            Assert.Equal(1, tokens[0].TrailingTrivia.Count);
            Assert.Equal("/*comment*/", tokens[0].TrailingTrivia[0].ToString());

            // x
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[1].Kind());
            Assert.Equal("x", tokens[1].Text);
            Assert.Equal(0, tokens[1].LeadingTrivia.Count);
            Assert.True(tokens[1].TrailingTrivia.Count > 0);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.TypeScript.Syntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal class LanguageParser : IDisposable
    {
        private readonly Lexer _lexer;
        private SyntaxToken _currentToken;
        private readonly TypeScriptParseOptions _options;

        internal LanguageParser(Lexer lexer, TypeScriptParseOptions? options = null)
        {
            _lexer = lexer;
            _options = options ?? TypeScriptParseOptions.Default;
            _currentToken = _lexer.Lex();
        }

        public void Dispose()
        {
            _lexer.Dispose();
        }

        private SyntaxToken CurrentToken => _currentToken;

        private SyntaxToken EatToken()
        {
            var token = _currentToken;
            _currentToken = _lexer.Lex();
            return token;
        }

        private SyntaxToken EatToken(SyntaxKind kind)
        {
            if (_currentToken.Kind == kind)
            {
                return EatToken();
            }
            return CreateMissingToken(kind);
        }

        private SyntaxToken? EatOptionalToken(SyntaxKind kind)
        {
            if (_currentToken.Kind == kind)
            {
                return EatToken();
            }
            return null;
        }

        private SyntaxToken CreateMissingToken(SyntaxKind kind)
        {
            return SyntaxToken.CreateMissing(kind, null, null);
        }

        internal CompilationUnitSyntax ParseCompilationUnit()
        {
            var statements = new SyntaxListBuilder<StatementSyntax>(8);

            while (_currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                var statement = ParseStatement();
                if (statement.FullWidth == 0)
                {
                    // Error recovery: Skip token to avoid infinite loop if we made no progress
                    var badToken = EatToken();

                    // In a real implementation, we would attach this bad token as skipped trivia
                    // to the next statement or the previous one.
                    // For this prototype, simply consuming it prevents the hang.
                    // We could also wrap it in a "bad statement" node if we had one.
                }
                else
                {
                    statements.Add(statement);
                }
            }

            var eof = EatToken(SyntaxKind.EndOfFileToken);
            return SyntaxFactory.CompilationUnit(statements.ToList(), eof);
        }

        internal StatementSyntax ParseStatement()
        {
            if (_currentToken.Kind == SyntaxKind.InterfaceKeyword)
            {
                return ParseInterfaceDeclaration();
            }

            return ParseExpressionStatement();
        }

        internal InterfaceDeclarationSyntax ParseInterfaceDeclaration()
        {
            var interfaceKeyword = EatToken(SyntaxKind.InterfaceKeyword);
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);

            var members = new SyntaxListBuilder<TypeElementSyntax>(8);
            while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                members.Add(ParseTypeElement());
            }

            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);

            return SyntaxFactory.InterfaceDeclaration(interfaceKeyword, identifier, openBrace, members.ToList(), closeBrace);
        }

        internal TypeElementSyntax ParseTypeElement()
        {
            // Assume PropertySignature for now
            return ParsePropertySignature();
        }

        internal PropertySignatureSyntax ParsePropertySignature()
        {
            var name = ParseIdentifierName();
            var questionToken = EatOptionalToken(SyntaxKind.QuestionToken);
            var typeAnnotation = ParseOptionalTypeAnnotation();
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);

            return SyntaxFactory.PropertySignature(name, questionToken, typeAnnotation, semicolon);
        }

        internal TypeAnnotationSyntax? ParseOptionalTypeAnnotation()
        {
            if (_currentToken.Kind == SyntaxKind.ColonToken)
            {
                var colon = EatToken();
                var type = ParseType();
                return SyntaxFactory.TypeAnnotation(colon, type);
            }
            return null;
        }

        internal TypeSyntax ParseType()
        {
            if (IsPredefinedType(_currentToken.Kind))
            {
                return ParsePredefinedType();
            }
            if (_currentToken.Kind == SyntaxKind.IdentifierToken)
            {
                return ParseTypeReference();
            }

            // Fallback
            return ParseTypeReference(); // will create identifier from current token
        }

        private bool IsPredefinedType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.NumberKeyword:
                case SyntaxKind.StringKeyword:
                case SyntaxKind.BooleanKeyword:
                case SyntaxKind.VoidKeyword:
                case SyntaxKind.AnyKeyword:
                    return true;
                default:
                    return false;
            }
        }

        internal TypeSyntax ParsePredefinedType()
        {
            var keyword = EatToken();
            // Use IdentifierName for predefined types for now to avoid generator issues
            // Convert keyword token to identifier token
            return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(keyword.Text));
        }

        internal TypeReferenceSyntax ParseTypeReference()
        {
            var name = ParseIdentifierName();
            return SyntaxFactory.TypeReference(name);
        }

        internal ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            var semicolon = EatToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.ExpressionStatement(expression, semicolon);
        }

        internal ExpressionSyntax ParseExpression()
        {
            // Very basic expression parsing
            if (_currentToken.Kind == SyntaxKind.IdentifierToken)
            {
                return ParseIdentifierName();
            }

            // Fallback
            var missing = CreateMissingToken(SyntaxKind.IdentifierToken);
            return SyntaxFactory.IdentifierName(missing);
        }

        internal IdentifierNameSyntax ParseIdentifierName()
        {
            var token = EatToken(SyntaxKind.IdentifierToken);
            return SyntaxFactory.IdentifierName(token);
        }
    }
}

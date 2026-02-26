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
                    // Error recovery: Skip token to avoid infinite loop
                    var badToken = EatToken();
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
            switch (_currentToken.Kind)
            {
                case SyntaxKind.InterfaceKeyword:
                    return ParseInterfaceDeclaration();
                case SyntaxKind.FunctionKeyword:
                    return ParseFunctionDeclaration();
                case SyntaxKind.VarKeyword:
                case SyntaxKind.LetKeyword:
                case SyntaxKind.ConstKeyword:
                    return ParseVariableStatement();
                case SyntaxKind.OpenBraceToken:
                    return ParseBlock();
                case SyntaxKind.ReturnKeyword:
                    return ParseReturnStatement();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                case SyntaxKind.SemicolonToken:
                    return ParseEmptyStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        internal IfStatementSyntax ParseIfStatement()
        {
            var ifKeyword = EatToken(SyntaxKind.IfKeyword);
            var openParen = EatToken(SyntaxKind.OpenParenToken);
            var condition = ParseExpression();
            var closeParen = EatToken(SyntaxKind.CloseParenToken);
            var statement = ParseStatement();
            ElseClauseSyntax? elseClause = null;
            if (_currentToken.Kind == SyntaxKind.ElseKeyword)
            {
                var elseKeyword = EatToken();
                var elseStatement = ParseStatement();
                elseClause = SyntaxFactory.ElseClause(elseKeyword, elseStatement);
            }
            return SyntaxFactory.IfStatement(ifKeyword, openParen, condition, closeParen, statement, elseClause);
        }

        internal WhileStatementSyntax ParseWhileStatement()
        {
            var whileKeyword = EatToken(SyntaxKind.WhileKeyword);
            var openParen = EatToken(SyntaxKind.OpenParenToken);
            var condition = ParseExpression();
            var closeParen = EatToken(SyntaxKind.CloseParenToken);
            var statement = ParseStatement();
            return SyntaxFactory.WhileStatement(whileKeyword, openParen, condition, closeParen, statement);
        }

        internal ForStatementSyntax ParseForStatement()
        {
            var forKeyword = EatToken(SyntaxKind.ForKeyword);
            var openParen = EatToken(SyntaxKind.OpenParenToken);

            StatementSyntax? initializer = null;
            if (_currentToken.Kind != SyntaxKind.SemicolonToken)
            {
                if (_currentToken.Kind == SyntaxKind.VarKeyword || _currentToken.Kind == SyntaxKind.LetKeyword || _currentToken.Kind == SyntaxKind.ConstKeyword)
                {
                    initializer = ParseVariableStatement();
                }
                else
                {
                    initializer = ParseExpressionStatement();
                }
            }
            else
            {
                 initializer = ParseEmptyStatement();
            }

            ExpressionSyntax? condition = null;
            if (_currentToken.Kind != SyntaxKind.SemicolonToken)
            {
                condition = ParseExpression();
            }
            var secondSemicolon = EatToken(SyntaxKind.SemicolonToken);

            ExpressionSyntax? increment = null;
            if (_currentToken.Kind != SyntaxKind.CloseParenToken)
            {
                increment = ParseExpression();
            }
            var closeParen = EatToken(SyntaxKind.CloseParenToken);

            var statement = ParseStatement();

            return SyntaxFactory.ForStatement(forKeyword, openParen, initializer, condition, secondSemicolon, increment, closeParen, statement);
        }

        internal EmptyStatementSyntax ParseEmptyStatement()
        {
            var semicolon = EatToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.EmptyStatement(semicolon);
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

        internal FunctionDeclarationSyntax ParseFunctionDeclaration()
        {
            var functionKeyword = EatToken(SyntaxKind.FunctionKeyword);
            var identifier = EatOptionalToken(SyntaxKind.IdentifierToken);
            var parameterList = ParseParameterList();
            var typeAnnotation = ParseOptionalTypeAnnotation();

            BlockSyntax? body = null;
            if (_currentToken.Kind == SyntaxKind.OpenBraceToken)
            {
                body = ParseBlock();
            }
            // else semicolon? or ambient?

            return SyntaxFactory.FunctionDeclaration(functionKeyword, identifier, parameterList, typeAnnotation, body);
        }

        internal ParameterListSyntax ParseParameterList()
        {
            var openParen = EatToken(SyntaxKind.OpenParenToken);
            var parameters = new SeparatedSyntaxListBuilder<ParameterSyntax>(8);

            if (_currentToken.Kind != SyntaxKind.CloseParenToken)
            {
                parameters.Add(ParseParameter());
                while (_currentToken.Kind == SyntaxKind.CommaToken)
                {
                    parameters.AddSeparator(EatToken());
                    parameters.Add(ParseParameter());
                }
            }

            var closeParen = EatToken(SyntaxKind.CloseParenToken);
            return SyntaxFactory.ParameterList(openParen, parameters.ToList(), closeParen);
        }

        internal ParameterSyntax ParseParameter()
        {
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            var typeAnnotation = ParseOptionalTypeAnnotation();
            return SyntaxFactory.Parameter(identifier, typeAnnotation);
        }

        internal VariableStatementSyntax ParseVariableStatement()
        {
            var keyword = EatToken(); // var, let, const
            var decl = ParseVariableDeclaration(); // TODO: List support
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.VariableStatement(keyword, decl, semicolon);
        }

        internal VariableDeclarationSyntax ParseVariableDeclaration()
        {
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            var typeAnnotation = ParseOptionalTypeAnnotation();
            EqualsValueClauseSyntax? initializer = null;
            if (_currentToken.Kind == SyntaxKind.EqualsToken)
            {
                initializer = ParseEqualsValueClause();
            }
            return SyntaxFactory.VariableDeclaration(identifier, typeAnnotation, initializer);
        }

        internal EqualsValueClauseSyntax ParseEqualsValueClause()
        {
            var equals = EatToken(SyntaxKind.EqualsToken);
            var value = ParseExpression();
            return SyntaxFactory.EqualsValueClause(equals, value);
        }

        internal BlockSyntax ParseBlock()
        {
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);
            var statements = new SyntaxListBuilder<StatementSyntax>(8);
            while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                statements.Add(ParseStatement());
            }
            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);
            return SyntaxFactory.Block(openBrace, statements.ToList(), closeBrace);
        }

        internal ReturnStatementSyntax ParseReturnStatement()
        {
            var returnKeyword = EatToken(SyntaxKind.ReturnKeyword);
            ExpressionSyntax? expression = null;
            if (_currentToken.Kind != SyntaxKind.SemicolonToken && _currentToken.Kind != SyntaxKind.CloseBraceToken)
            {
                // Simple heuristic for automatic semicolon insertion logic approximation
                // If next token is on new line, return might be void.
                // For now, assume if expression starts, parse it.
                expression = ParseExpression();
            }
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.ReturnStatement(returnKeyword, expression, semicolon);
        }

        internal TypeElementSyntax ParseTypeElement()
        {
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
            return ParseTypeReference();
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
            return SyntaxFactory.PredefinedType(keyword);
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
            return ParseBinaryExpression(0);
        }

        internal ExpressionSyntax ParseBinaryExpression(int parentPrecedence)
        {
            var left = ParseUnaryExpression();

            while (true)
            {
                int precedence = GetBinaryOperatorPrecedence(_currentToken.Kind);
                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }

                var opToken = EatToken();
                var right = ParseBinaryExpression(precedence);
                left = SyntaxFactory.BinaryExpression(GetBinaryExpressionKind(opToken.Kind), left, opToken, right);
            }

            return left;
        }

        private int GetBinaryOperatorPrecedence(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.SlashToken:
                case SyntaxKind.PercentToken:
                    return 10;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 9;
                case SyntaxKind.LessThanToken:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.LessThanEqualsToken:
                case SyntaxKind.GreaterThanEqualsToken:
                    return 8;
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.ExclamationEqualsToken:
                    return 7;
                case SyntaxKind.AmpersandAmpersandToken:
                    return 6;
                case SyntaxKind.BarBarToken:
                    return 5;
                case SyntaxKind.EqualsToken:
                    return 4;
                default:
                    return 0;
            }
        }

        private SyntaxKind GetBinaryExpressionKind(SyntaxKind tokenKind)
        {
            switch (tokenKind)
            {
                case SyntaxKind.PlusToken: return SyntaxKind.AddExpression;
                case SyntaxKind.MinusToken: return SyntaxKind.SubtractExpression;
                case SyntaxKind.AsteriskToken: return SyntaxKind.MultiplyExpression;
                case SyntaxKind.SlashToken: return SyntaxKind.DivideExpression;
                case SyntaxKind.EqualsEqualsToken: return SyntaxKind.EqualsExpression;
                case SyntaxKind.ExclamationEqualsToken: return SyntaxKind.NotEqualsExpression;
                case SyntaxKind.LessThanToken: return SyntaxKind.LessThanExpression;
                case SyntaxKind.LessThanEqualsToken: return SyntaxKind.LessThanOrEqualExpression;
                case SyntaxKind.GreaterThanToken: return SyntaxKind.GreaterThanExpression;
                case SyntaxKind.GreaterThanEqualsToken: return SyntaxKind.GreaterThanOrEqualExpression;
                case SyntaxKind.AmpersandAmpersandToken: return SyntaxKind.LogicalAndExpression;
                case SyntaxKind.BarBarToken: return SyntaxKind.LogicalOrExpression;
                case SyntaxKind.EqualsToken: return SyntaxKind.AssignmentExpression;
                default: return SyntaxKind.None;
            }
        }

        internal ExpressionSyntax ParseUnaryExpression()
        {
            return ParsePostfixExpression();
        }

        internal ExpressionSyntax ParsePostfixExpression()
        {
            var expr = ParsePrimaryExpression();
            while (true)
            {
                if (_currentToken.Kind == SyntaxKind.DotToken)
                {
                     var dot = EatToken();
                     var name = ParseIdentifierName();
                     expr = SyntaxFactory.MemberAccessExpression(expr, dot, name);
                }
                else if (_currentToken.Kind == SyntaxKind.OpenParenToken)
                {
                     var args = ParseArgumentList();
                     expr = SyntaxFactory.CallExpression(expr, args);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        internal ArgumentListSyntax ParseArgumentList()
        {
            var openParen = EatToken(SyntaxKind.OpenParenToken);
            var arguments = new SeparatedSyntaxListBuilder<ArgumentSyntax>(8);
            if (_currentToken.Kind != SyntaxKind.CloseParenToken)
            {
                arguments.Add(ParseArgument());
                while (_currentToken.Kind == SyntaxKind.CommaToken)
                {
                    arguments.AddSeparator(EatToken());
                    arguments.Add(ParseArgument());
                }
            }
            var closeParen = EatToken(SyntaxKind.CloseParenToken);
            return SyntaxFactory.ArgumentList(openParen, arguments.ToList(), closeParen);
        }

        internal ArgumentSyntax ParseArgument()
        {
             var expr = ParseExpression();
             return SyntaxFactory.Argument(expr);
        }

        internal ExpressionSyntax ParsePrimaryExpression()
        {
            switch (_currentToken.Kind)
            {
                case SyntaxKind.NumericLiteralToken:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, EatToken());
                case SyntaxKind.StringLiteralToken:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, EatToken());
                case SyntaxKind.TrueKeyword:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression, EatToken());
                case SyntaxKind.FalseKeyword:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression, EatToken());
                case SyntaxKind.NullKeyword:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression, EatToken());
                case SyntaxKind.IdentifierToken:
                    return ParseIdentifierName();
                default:
                    // Error recovery
                    return SyntaxFactory.IdentifierName(CreateMissingToken(SyntaxKind.IdentifierToken));
            }
        }

        internal IdentifierNameSyntax ParseIdentifierName()
        {
            var token = EatToken(SyntaxKind.IdentifierToken);
            return SyntaxFactory.IdentifierName(token);
        }
    }
}

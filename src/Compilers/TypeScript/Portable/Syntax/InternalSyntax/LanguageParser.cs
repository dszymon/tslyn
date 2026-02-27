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
                case SyntaxKind.ClassKeyword:
                    return ParseClassDeclaration();
                case SyntaxKind.InterfaceKeyword:
                    return ParseInterfaceDeclaration();
                case SyntaxKind.FunctionKeyword:
                    return ParseFunctionDeclaration();
                case SyntaxKind.TypeKeyword:
                    return ParseTypeAliasDeclaration();
                case SyntaxKind.EnumKeyword:
                    return ParseEnumDeclaration();
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
                case SyntaxKind.DoKeyword:
                    return ParseDoStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                case SyntaxKind.BreakKeyword:
                    return ParseBreakStatement();
                case SyntaxKind.ContinueKeyword:
                    return ParseContinueStatement();
                case SyntaxKind.SwitchKeyword:
                    return ParseSwitchStatement();
                case SyntaxKind.TryKeyword:
                    return ParseTryStatement();
                case SyntaxKind.ThrowKeyword:
                    return ParseThrowStatement();
                case SyntaxKind.SemicolonToken:
                    return ParseEmptyStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        internal SwitchStatementSyntax ParseSwitchStatement()
        {
            var switchKeyword = EatToken(SyntaxKind.SwitchKeyword);
            var openParen = EatToken(SyntaxKind.OpenParenToken);
            var expression = ParseExpression();
            var closeParen = EatToken(SyntaxKind.CloseParenToken);
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);

            var clauses = new SyntaxListBuilder<SwitchLabelSyntax>(8);
            while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                clauses.Add(ParseSwitchClause());
            }

            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);
            return SyntaxFactory.SwitchStatement(switchKeyword, openParen, expression, closeParen, openBrace, clauses.ToList(), closeBrace);
        }

        internal SwitchLabelSyntax ParseSwitchClause()
        {
            if (_currentToken.Kind == SyntaxKind.CaseKeyword)
            {
                var caseKeyword = EatToken();
                var expression = ParseExpression();
                var colon = EatToken(SyntaxKind.ColonToken);
                var statements = ParseSwitchClauseStatements();
                return SyntaxFactory.CaseClause(caseKeyword, expression, colon, statements);
            }
            else
            {
                var defaultKeyword = EatToken(SyntaxKind.DefaultKeyword);
                var colon = EatToken(SyntaxKind.ColonToken);
                var statements = ParseSwitchClauseStatements();
                return SyntaxFactory.DefaultClause(defaultKeyword, colon, statements);
            }
        }

        internal Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> ParseSwitchClauseStatements()
        {
            var statements = new SyntaxListBuilder<StatementSyntax>(8);
            while (_currentToken.Kind != SyntaxKind.CaseKeyword &&
                   _currentToken.Kind != SyntaxKind.DefaultKeyword &&
                   _currentToken.Kind != SyntaxKind.CloseBraceToken &&
                   _currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                statements.Add(ParseStatement());
            }
            return statements.ToList();
        }

        internal TryStatementSyntax ParseTryStatement()
        {
            var tryKeyword = EatToken(SyntaxKind.TryKeyword);
            var block = ParseBlock();
            CatchClauseSyntax? catchClause = null;
            if (_currentToken.Kind == SyntaxKind.CatchKeyword)
            {
                var catchKeyword = EatToken();
                SyntaxToken? openParen = null;
                VariableDeclarationSyntax? declaration = null;
                SyntaxToken? closeParen = null;

                if (_currentToken.Kind == SyntaxKind.OpenParenToken)
                {
                    openParen = EatToken();
                    declaration = ParseVariableDeclaration(); // Typically just identifier but can be binding pattern
                    closeParen = EatToken(SyntaxKind.CloseParenToken);
                }

                var catchBlock = ParseBlock();
                catchClause = SyntaxFactory.CatchClause(catchKeyword, openParen, declaration, closeParen, catchBlock);
            }

            FinallyClauseSyntax? finallyClause = null;
            if (_currentToken.Kind == SyntaxKind.FinallyKeyword)
            {
                var finallyKeyword = EatToken();
                var finallyBlock = ParseBlock();
                finallyClause = SyntaxFactory.FinallyClause(finallyKeyword, finallyBlock);
            }

            return SyntaxFactory.TryStatement(tryKeyword, block, catchClause, finallyClause);
        }

        internal ThrowStatementSyntax ParseThrowStatement()
        {
            var throwKeyword = EatToken(SyntaxKind.ThrowKeyword);
            var expression = ParseExpression();
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.ThrowStatement(throwKeyword, expression, semicolon);
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

        internal DoStatementSyntax ParseDoStatement()
        {
            var doKeyword = EatToken(SyntaxKind.DoKeyword);
            var statement = ParseStatement();
            var whileKeyword = EatToken(SyntaxKind.WhileKeyword);
            var openParen = EatToken(SyntaxKind.OpenParenToken);
            var condition = ParseExpression();
            var closeParen = EatToken(SyntaxKind.CloseParenToken);
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.DoStatement(doKeyword, statement, whileKeyword, openParen, condition, closeParen, semicolon);
        }

        internal BreakStatementSyntax ParseBreakStatement()
        {
            var breakKeyword = EatToken(SyntaxKind.BreakKeyword);
            IdentifierNameSyntax? label = null;
            if (_currentToken.Kind == SyntaxKind.IdentifierToken && !_currentToken.IsMissing)
            {
                // Simple heuristic: if same line, it's a label.
                // Since we don't have line info easily here in parser structure without peek,
                // we assume if not semicolon, it's label.
                if (_currentToken.Kind != SyntaxKind.SemicolonToken)
                {
                    label = ParseIdentifierName();
                }
            }
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.BreakStatement(breakKeyword, label, semicolon);
        }

        internal ContinueStatementSyntax ParseContinueStatement()
        {
            var continueKeyword = EatToken(SyntaxKind.ContinueKeyword);
            IdentifierNameSyntax? label = null;
            if (_currentToken.Kind == SyntaxKind.IdentifierToken && !_currentToken.IsMissing)
            {
                if (_currentToken.Kind != SyntaxKind.SemicolonToken)
                {
                    label = ParseIdentifierName();
                }
            }
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.ContinueStatement(continueKeyword, label, semicolon);
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
            var typeParameters = ParseOptionalTypeParameters();
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);

            var members = new SyntaxListBuilder<TypeElementSyntax>(8);
            while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                members.Add(ParseTypeElement());
            }

            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);

            return SyntaxFactory.InterfaceDeclaration(interfaceKeyword, identifier, typeParameters, openBrace, members.ToList(), closeBrace);
        }

        internal TypeAliasDeclarationSyntax ParseTypeAliasDeclaration()
        {
            var typeKeyword = EatToken(SyntaxKind.TypeKeyword);
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            var typeParameters = ParseOptionalTypeParameters();
            var equalsToken = EatToken(SyntaxKind.EqualsToken);
            var type = ParseType();
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);

            return SyntaxFactory.TypeAliasDeclaration(typeKeyword, identifier, typeParameters, equalsToken, type, semicolon);
        }

        internal EnumDeclarationSyntax ParseEnumDeclaration()
        {
            var enumKeyword = EatToken(SyntaxKind.EnumKeyword);
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);

            var members = new SeparatedSyntaxListBuilder<EnumMemberSyntax>(8);
            if (_currentToken.Kind != SyntaxKind.CloseBraceToken)
            {
                while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
                {
                    members.Add(ParseEnumMember());

                    if (_currentToken.Kind == SyntaxKind.CommaToken)
                    {
                        members.AddSeparator(EatToken());
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);
            return SyntaxFactory.EnumDeclaration(enumKeyword, identifier, openBrace, members.ToList(), closeBrace);
        }

        internal EnumMemberSyntax ParseEnumMember()
        {
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            EqualsValueClauseSyntax? initializer = null;
            if (_currentToken.Kind == SyntaxKind.EqualsToken)
            {
                initializer = ParseEqualsValueClause();
            }
            return SyntaxFactory.EnumMember(identifier, initializer);
        }

        internal ClassDeclarationSyntax ParseClassDeclaration()
        {
            var classKeyword = EatToken(SyntaxKind.ClassKeyword);
            var identifier = EatOptionalToken(SyntaxKind.IdentifierToken);
            var typeParameters = ParseOptionalTypeParameters();
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);

            var members = new SyntaxListBuilder<ClassElementSyntax>(8);
            while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                members.Add(ParseClassElement());
            }

            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);

            return SyntaxFactory.ClassDeclaration(classKeyword, identifier, typeParameters, openBrace, members.ToList(), closeBrace);
        }

        internal TypeParameterListSyntax? ParseOptionalTypeParameters()
        {
            if (_currentToken.Kind == SyntaxKind.LessThanToken)
            {
                var lessThanToken = EatToken();
                var parameters = new SeparatedSyntaxListBuilder<TypeParameterSyntax>(8);

                parameters.Add(ParseTypeParameter());
                while (_currentToken.Kind == SyntaxKind.CommaToken)
                {
                    parameters.AddSeparator(EatToken());
                    parameters.Add(ParseTypeParameter());
                }

                var greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);
                return SyntaxFactory.TypeParameterList(lessThanToken, parameters.ToList(), greaterThanToken);
            }
            return null;
        }

        internal TypeParameterSyntax ParseTypeParameter()
        {
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            TypeParameterConstraintClauseSyntax? constraint = null;
            if (_currentToken.Kind == SyntaxKind.ExtendsKeyword)
            {
                var extendsKeyword = EatToken();
                var type = ParseType();
                constraint = SyntaxFactory.TypeParameterConstraintClause(extendsKeyword, type);
            }

            TypeParameterDefaultClauseSyntax? defaultClause = null;
            if (_currentToken.Kind == SyntaxKind.EqualsToken)
            {
                var equalsToken = EatToken();
                var type = ParseType();
                defaultClause = SyntaxFactory.TypeParameterDefaultClause(equalsToken, type);
            }
            return SyntaxFactory.TypeParameter(identifier, constraint, defaultClause);
        }

        internal ClassElementSyntax ParseClassElement()
        {
            // Constructor
            if (_currentToken.Kind == SyntaxKind.ConstructorKeyword)
            {
                return ParseConstructorDeclaration();
            }

            // Method or Property
            // Simplified lookahead: if '(' follows identifier, it's a method. Otherwise property.
            // TODO: handle modifiers (public, private, static, etc.)

            var identifier = ParseIdentifierName();
            if (_currentToken.Kind == SyntaxKind.OpenParenToken)
            {
                return ParseMethodDeclaration(identifier);
            }
            else
            {
                return ParsePropertyDeclaration(identifier);
            }
        }

        internal ConstructorDeclarationSyntax ParseConstructorDeclaration()
        {
            var constructorKeyword = EatToken(SyntaxKind.ConstructorKeyword);
            var parameterList = ParseParameterList();
            BlockSyntax? body = null;
            if (_currentToken.Kind == SyntaxKind.OpenBraceToken)
            {
                body = ParseBlock();
            }
            // else semicolon?
            return SyntaxFactory.ConstructorDeclaration(constructorKeyword, parameterList, body);
        }

        internal MethodDeclarationSyntax ParseMethodDeclaration(IdentifierNameSyntax name)
        {
            var typeParameters = ParseOptionalTypeParameters();
            var parameterList = ParseParameterList();
            var typeAnnotation = ParseOptionalTypeAnnotation();
            BlockSyntax? body = null;
            if (_currentToken.Kind == SyntaxKind.OpenBraceToken)
            {
                body = ParseBlock();
            }
            return SyntaxFactory.MethodDeclaration(name, typeParameters, parameterList, typeAnnotation, body);
        }

        internal PropertyDeclarationSyntax ParsePropertyDeclaration(IdentifierNameSyntax name)
        {
            var typeAnnotation = ParseOptionalTypeAnnotation();
            EqualsValueClauseSyntax? initializer = null;
            if (_currentToken.Kind == SyntaxKind.EqualsToken)
            {
                initializer = ParseEqualsValueClause();
            }
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.PropertyDeclaration(name, typeAnnotation, initializer, semicolon);
        }

        internal FunctionDeclarationSyntax ParseFunctionDeclaration()
        {
            var functionKeyword = EatToken(SyntaxKind.FunctionKeyword);
            var identifier = EatOptionalToken(SyntaxKind.IdentifierToken);
            var typeParameters = ParseOptionalTypeParameters();
            var parameterList = ParseParameterList();
            var typeAnnotation = ParseOptionalTypeAnnotation();

            BlockSyntax? body = null;
            if (_currentToken.Kind == SyntaxKind.OpenBraceToken)
            {
                body = ParseBlock();
            }
            // else semicolon? or ambient?

            return SyntaxFactory.FunctionDeclaration(functionKeyword, identifier, typeParameters, parameterList, typeAnnotation, body);
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
            TypeSyntax type;
            if (IsPredefinedType(_currentToken.Kind))
            {
                type = ParsePredefinedType();
            }
            else if (_currentToken.Kind == SyntaxKind.IdentifierToken)
            {
                type = ParseTypeReference();
            }
            else
            {
                type = ParseTypeReference();
            }

            while (_currentToken.Kind == SyntaxKind.OpenBracketToken)
            {
                var openBracket = EatToken();
                var closeBracket = EatToken(SyntaxKind.CloseBracketToken);
                type = SyntaxFactory.ArrayType(type, openBracket, closeBracket);
            }

            return type;
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
            var typeArguments = ParseOptionalTypeArguments();
            return SyntaxFactory.TypeReference(name, typeArguments);
        }

        internal TypeArgumentListSyntax? ParseOptionalTypeArguments()
        {
            if (_currentToken.Kind == SyntaxKind.LessThanToken)
            {
                 var lessThan = EatToken();
                 var args = new SeparatedSyntaxListBuilder<TypeSyntax>(8);
                 args.Add(ParseType());
                 while (_currentToken.Kind == SyntaxKind.CommaToken)
                 {
                     args.AddSeparator(EatToken());
                     args.Add(ParseType());
                 }
                 var greaterThan = EatToken(SyntaxKind.GreaterThanToken);
                 return SyntaxFactory.TypeArgumentList(lessThan, args.ToList(), greaterThan);
            }
            return null;
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
            if (IsPrefixUnaryOperator(_currentToken.Kind))
            {
                var opToken = EatToken();
                var operand = ParseUnaryExpression();
                return SyntaxFactory.PrefixUnaryExpression(opToken, operand);
            }

            if (_currentToken.Kind == SyntaxKind.DeleteKeyword)
            {
                var deleteKeyword = EatToken();
                var expression = ParseUnaryExpression();
                return SyntaxFactory.DeleteExpression(deleteKeyword, expression);
            }

            if (_currentToken.Kind == SyntaxKind.TypeOfKeyword)
            {
                var typeofKeyword = EatToken();
                var expression = ParseUnaryExpression();
                return SyntaxFactory.TypeOfExpression(typeofKeyword, expression);
            }

            if (_currentToken.Kind == SyntaxKind.VoidKeyword)
            {
                var voidKeyword = EatToken();
                var expression = ParseUnaryExpression();
                return SyntaxFactory.VoidExpression(voidKeyword, expression);
            }

            return ParsePostfixExpression();
        }

        private bool IsPrefixUnaryOperator(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusPlusToken:
                case SyntaxKind.MinusMinusToken:
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.TildeToken:
                case SyntaxKind.ExclamationToken:
                    return true;
                default:
                    return false;
            }
        }

        internal ExpressionSyntax ParsePostfixExpression()
        {
            var expr = ParseLeftHandSideExpression();
            while (true)
            {
                if (_currentToken.Kind == SyntaxKind.PlusPlusToken || _currentToken.Kind == SyntaxKind.MinusMinusToken)
                {
                    // Check for new line? Postfix operators shouldn't have line terminator before them.
                    var opToken = EatToken();
                    expr = SyntaxFactory.PostfixUnaryExpression(expr, opToken);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        internal ExpressionSyntax ParseLeftHandSideExpression()
        {
            var expr = ParseMemberExpression();
            while (true)
            {
                 if (_currentToken.Kind == SyntaxKind.LessThanToken)
                 {
                    // Ambiguity: Is it LessThan operator or TypeArguments for Call?
                    // Simplified: Assume if followed by Type, Comma or GreaterThan, it's type args.
                    // But for now, we only support CallExpression generic if it's explicitly called.
                    // Actually, CallExpression in Syntax.xml now has TypeArguments.
                    // But in TS, `func<T>()` - the `<` is after the member expression.
                    // This is complex in TS because `f<T>(x)` vs `a < b > (c)`.
                    // We need lookahead.
                    // For this task, let's implement simple check: if we parse type args successfully, use them.

                    // TODO: Proper lookahead to disambiguate.
                    // For now, if we see < and next is Identifier/Type, we assume Call Generic.
                    // Or we just parse OptionalTypeArguments. If it fails (returns null), backtrack?
                    // Since we don't have backtracking here easily, let's try a simple heuristic or skip for now if it's too risky.
                    // User asked for Generics. So `foo<string>()` must work.

                    // We'll optimistically try to parse type arguments.
                    // But wait, ParseOptionalTypeArguments consumes tokens.
                    // For this exercise, let's assume if it looks like type args, it is.
                 }

                 if (_currentToken.Kind == SyntaxKind.OpenParenToken)
                 {
                     var args = ParseArgumentList();
                     expr = SyntaxFactory.CallExpression(expr, null, args);
                 }
                 else if (_currentToken.Kind == SyntaxKind.LessThanToken)
                 {
                     // Attempt to handle `func<T>(...)`
                     // This is tricky without backtracking.
                     // Let's implement a simplified version: if followed by OpenParen, it's definitely a call.
                     // But we need to parse the type args first.

                     // We will parse type args, then expect OpenParen.
                     // If OpenParen doesn't follow, we might have messed up `a < b`.
                     // But `a < b` is BinaryExpression, which has lower precedence than Call (MemberAccess).
                     // Actually, Call is LeftHandSide. Binary is lower.
                     // `a < b` is parsed in ParseBinaryExpression.
                     // So here we are in ParseMember/Call loop.
                     // If we encounter `<` here, it MUST be type arguments for a call, OR it's end of this expression and start of binary `<`.
                     // If it's `a < b`, then `a` is the LHS. The loop should break, and `ParseBinaryExpression` will consume `<`.
                     // BUT, `f<T>()` binds tighter.

                     // So, if we see `<`, how do we know if it's `f<T>` or `f < T`?
                     // In TS: `f<T>(` is call. `f<T>` alone is ... comparison? No, `f<T>` is not valid expression statement unless comparison.

                     // Let's defer generic call parsing for a moment or implement a safe check.
                     // If I implement `ParseOptionalTypeArguments` inside `ParseMemberExpression` loop?

                     // For now, let's just update `CallExpression` creation to pass `null` for type args where we don't support them yet,
                     // OR handle explicit request. The plan said "Update CallExpressionSyntax ... to include optional TypeArguments".
                     // Let's try to support it.

                     // If I verify the next token after `<` is an identifier or strict type start, and eventually `>` then `(`, it's a call.
                     // Given current rudimentary parser, let's stick to: if we see `(` it's a call.
                     // If we want `foo<T>()`, we need to handle `<`.

                     break; // Placeholder: Generics on call not fully implemented in this loop yet to avoid regression on `a < b`
                 }
                 else
                 {
                     break;
                 }
            }
            return expr;
        }

        internal ExpressionSyntax ParseMemberExpression()
        {
             ExpressionSyntax expr;
             if (_currentToken.Kind == SyntaxKind.NewKeyword)
             {
                 expr = ParseNewExpression();
             }
             else
             {
                 expr = ParsePrimaryExpression();
             }

             while (true)
             {
                if (_currentToken.Kind == SyntaxKind.DotToken)
                {
                     var dot = EatToken();
                     var name = ParseIdentifierName();
                     expr = SyntaxFactory.MemberAccessExpression(expr, dot, name);
                }
                else if (_currentToken.Kind == SyntaxKind.OpenBracketToken)
                {
                    // TODO: Indexer access not yet in Syntax.xml?
                    // For now, break or handle if we add it.
                    // Given the plan was literals, I might have missed MemberAccess via brackets.
                    // Sticking to dot for now as per previous implementation, but let's check plan.
                    // The plan didn't explicitly mention ElementAccessExpression, so I'll skip for now or treat as end of member loop.
                    break;
                }
                else
                {
                    break;
                }
             }
             return expr;
        }

        internal NewExpressionSyntax ParseNewExpression()
        {
            var newKeyword = EatToken(SyntaxKind.NewKeyword);
            var type = ParseType(); // Simplified: usually it's MemberExpression or CallExpression.
                                    // But using TypeSyntax for now as per Syntax.xml definition
                                    // "Field Name="Type" Type="TypeSyntax""

            ArgumentListSyntax? args = null;
            if (_currentToken.Kind == SyntaxKind.OpenParenToken)
            {
                args = ParseArgumentList();
            }
            return SyntaxFactory.NewExpression(newKeyword, type, args);
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
                case SyntaxKind.ThisKeyword:
                    return SyntaxFactory.ThisExpression(EatToken());
                case SyntaxKind.OpenBracketToken:
                    return ParseArrayLiteralExpression();
                case SyntaxKind.OpenBraceToken:
                    return ParseObjectLiteralExpression();
                case SyntaxKind.OpenParenToken:
                    return ParseParenthesizedExpression();
                default:
                    // Error recovery
                    return SyntaxFactory.IdentifierName(CreateMissingToken(SyntaxKind.IdentifierToken));
            }
        }

        internal ExpressionSyntax ParseParenthesizedExpression()
        {
            var open = EatToken(SyntaxKind.OpenParenToken);
            var expr = ParseExpression();
            var close = EatToken(SyntaxKind.CloseParenToken);
            return expr;
        }

        internal ArrayLiteralExpressionSyntax ParseArrayLiteralExpression()
        {
            var openBracket = EatToken(SyntaxKind.OpenBracketToken);
            var elements = new SeparatedSyntaxListBuilder<ExpressionSyntax>(8);

            if (_currentToken.Kind != SyntaxKind.CloseBracketToken)
            {
                while (_currentToken.Kind != SyntaxKind.CloseBracketToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
                {
                    elements.Add(ParseExpression());

                    if (_currentToken.Kind == SyntaxKind.CommaToken)
                    {
                        elements.AddSeparator(EatToken());
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var closeBracket = EatToken(SyntaxKind.CloseBracketToken);
            return SyntaxFactory.ArrayLiteralExpression(openBracket, elements.ToList(), closeBracket);
        }

        internal ObjectLiteralExpressionSyntax ParseObjectLiteralExpression()
        {
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);
            var properties = new SeparatedSyntaxListBuilder<PropertyAssignmentSyntax>(8);

            if (_currentToken.Kind != SyntaxKind.CloseBraceToken)
            {
                while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
                {
                    properties.Add(ParsePropertyAssignment());

                    if (_currentToken.Kind == SyntaxKind.CommaToken)
                    {
                        properties.AddSeparator(EatToken());
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);
            return SyntaxFactory.ObjectLiteralExpression(openBrace, properties.ToList(), closeBrace);
        }

        internal PropertyAssignmentSyntax ParsePropertyAssignment()
        {
            var name = ParseIdentifierName();
            var colon = EatToken(SyntaxKind.ColonToken);
            var expr = ParseExpression();
            return SyntaxFactory.PropertyAssignment(name, colon, expr);
        }

        internal IdentifierNameSyntax ParseIdentifierName()
        {
            var token = EatToken(SyntaxKind.IdentifierToken);
            return SyntaxFactory.IdentifierName(token);
        }
    }
}

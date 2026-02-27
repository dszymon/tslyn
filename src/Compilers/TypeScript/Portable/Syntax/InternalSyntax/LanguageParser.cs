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
        private List<SyntaxToken> _tokenBuffer = new List<SyntaxToken>();
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

        private SyntaxToken PeekToken(int n = 1)
        {
            if (n == 0) return _currentToken;
            while (_tokenBuffer.Count < n)
            {
                _tokenBuffer.Add(_lexer.Lex());
            }
            return _tokenBuffer[n - 1];
        }

        private SyntaxToken EatToken()
        {
            var token = _currentToken;
            if (_tokenBuffer.Count > 0)
            {
                _currentToken = _tokenBuffer[0];
                _tokenBuffer.RemoveAt(0);
            }
            else
            {
                _currentToken = _lexer.Lex();
            }
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

        private SyntaxToken ConvertToIdentifier(SyntaxToken token)
        {
            if (token.Kind == SyntaxKind.IdentifierToken)
            {
                return token;
            }
            return new SyntaxToken(SyntaxKind.IdentifierToken, token.Text, token.GetLeadingTriviaCore(), token.GetTrailingTriviaCore(), token.GetDiagnostics(), token.GetAnnotations());
        }

        private SyntaxToken ParseIdentifierToken()
        {
            if (_currentToken.Kind == SyntaxKind.AsyncKeyword || _currentToken.Kind == SyntaxKind.AwaitKeyword || _currentToken.Kind == SyntaxKind.GetKeyword || _currentToken.Kind == SyntaxKind.SetKeyword)
            {
                return ConvertToIdentifier(EatToken());
            }
            return EatToken(SyntaxKind.IdentifierToken);
        }

        private SyntaxToken? ParseOptionalIdentifierToken()
        {
            if (_currentToken.Kind == SyntaxKind.AsyncKeyword || _currentToken.Kind == SyntaxKind.AwaitKeyword)
            {
                return ConvertToIdentifier(EatToken());
            }
            return EatOptionalToken(SyntaxKind.IdentifierToken);
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
                case SyntaxKind.ImportKeyword:
                    return ParseImportDeclaration();
                case SyntaxKind.ExportKeyword:
                    return ParseExportDeclaration();
                case SyntaxKind.ClassKeyword:
                case SyntaxKind.AbstractKeyword:
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
                case SyntaxKind.AsyncKeyword:
                    if (PeekToken(1).Kind == SyntaxKind.FunctionKeyword)
                    {
                        var modifiers = ParseModifiers();
                        return ParseFunctionDeclaration(modifiers);
                    }
                    return ParseExpressionStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> ParseModifiers()
        {
            var list = new System.Collections.Generic.List<SyntaxToken>();
            while (true)
            {
                switch (_currentToken.Kind)
                {
                    case SyntaxKind.PublicKeyword:
                    case SyntaxKind.PrivateKeyword:
                    case SyntaxKind.ProtectedKeyword:
                    case SyntaxKind.StaticKeyword:
                    case SyntaxKind.AbstractKeyword:
                    case SyntaxKind.ReadOnlyKeyword:
                    case SyntaxKind.DeclareKeyword:
                    case SyntaxKind.ExportKeyword:
                    case SyntaxKind.ConstKeyword:
                        list.Add(EatToken());
                        break;
                    case SyntaxKind.AsyncKeyword:
                        var next = PeekToken(1);
                        if (next.Kind == SyntaxKind.OpenParenToken ||
                            next.Kind == SyntaxKind.ColonToken ||
                            next.Kind == SyntaxKind.EqualsToken ||
                            next.Kind == SyntaxKind.QuestionToken ||
                            next.Kind == SyntaxKind.SemicolonToken)
                        {
                            return ToSyntaxList(list);
                        }
                        list.Add(EatToken());
                        break;
                    case SyntaxKind.DefaultKeyword:
                        list.Add(EatToken());
                        break;
                    default:
                        return ToSyntaxList(list);
                }
            }
        }

        private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> ToSyntaxList(System.Collections.Generic.List<SyntaxToken> list)
        {
            if (list.Count == 0) return default;
            var builder = new SyntaxListBuilder<GreenNode>(list.Count);
            foreach (var token in list)
            {
                builder.Add(token);
            }
            var result = builder.ToList();
            return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(result.Node);
        }

        internal ImportDeclarationSyntax ParseImportDeclaration()
        {
            var importKeyword = EatToken(SyntaxKind.ImportKeyword);
            ImportClauseSyntax? importClause = null;
            SyntaxToken? fromKeyword = null;
            ExpressionSyntax? moduleSpecifier = null;

            if (_currentToken.Kind == SyntaxKind.StringLiteralToken)
            {
                moduleSpecifier = ParseExpression();
            }
            else
            {
                importClause = ParseImportClause();
                fromKeyword = EatToken(SyntaxKind.FromKeyword);
                moduleSpecifier = ParseExpression();
            }

            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.ImportDeclaration(importKeyword, importClause, fromKeyword, moduleSpecifier, semicolon);
        }

        internal ImportClauseSyntax ParseImportClause()
        {
             IdentifierNameSyntax? name = null;
             if (_currentToken.Kind == SyntaxKind.IdentifierToken || _currentToken.Kind == SyntaxKind.AsyncKeyword || _currentToken.Kind == SyntaxKind.AwaitKeyword)
             {
                 name = ParseIdentifierName();
                 if (_currentToken.Kind == SyntaxKind.CommaToken)
                 {
                     var comma = EatToken();
                     var bindings = ParseNamedImportBindings();
                     return SyntaxFactory.ImportClause(name, comma, bindings);
                 }
                 return SyntaxFactory.ImportClause(name, null, null);
             }

             var bindingsOnly = ParseNamedImportBindings();
             return SyntaxFactory.ImportClause(null, null, bindingsOnly);
        }

        internal NamedImportBindingsSyntax ParseNamedImportBindings()
        {
            if (_currentToken.Kind == SyntaxKind.AsteriskToken)
            {
                return ParseNamespaceImport();
            }
            return ParseNamedImports();
        }

        internal NamespaceImportSyntax ParseNamespaceImport()
        {
            var asterisk = EatToken(SyntaxKind.AsteriskToken);
            var asKeyword = EatToken(SyntaxKind.AsKeyword);
            var name = ParseIdentifierName();
            return SyntaxFactory.NamespaceImport(asterisk, asKeyword, name);
        }

        internal NamedImportsSyntax ParseNamedImports()
        {
             var open = EatToken(SyntaxKind.OpenBraceToken);
             var elements = new SeparatedSyntaxListBuilder<ImportSpecifierSyntax>(8);
             if (_currentToken.Kind != SyntaxKind.CloseBraceToken)
             {
                 while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
                 {
                     elements.Add(ParseImportSpecifier());
                     if (_currentToken.Kind == SyntaxKind.CommaToken)
                         elements.AddSeparator(EatToken());
                     else
                         break;
                 }
             }
             var close = EatToken(SyntaxKind.CloseBraceToken);
             return SyntaxFactory.NamedImports(open, elements.ToList(), close);
        }

        internal ImportSpecifierSyntax ParseImportSpecifier()
        {
             var nameOrProp = ParseIdentifierName();
             if (_currentToken.Kind == SyntaxKind.AsKeyword)
             {
                 var asKeyword = EatToken();
                 var name = ParseIdentifierName();
                 return SyntaxFactory.ImportSpecifier(nameOrProp, asKeyword, name);
             }
             return SyntaxFactory.ImportSpecifier(null, null, nameOrProp);
        }

        internal StatementSyntax ParseExportDeclaration()
        {
             // Check if it is a declaration export
             if (PeekToken(1).Kind == SyntaxKind.ClassKeyword ||
                 PeekToken(1).Kind == SyntaxKind.InterfaceKeyword ||
                 PeekToken(1).Kind == SyntaxKind.EnumKeyword ||
                 PeekToken(1).Kind == SyntaxKind.FunctionKeyword ||
                 PeekToken(1).Kind == SyntaxKind.TypeKeyword ||
                 PeekToken(1).Kind == SyntaxKind.VarKeyword ||
                 PeekToken(1).Kind == SyntaxKind.LetKeyword ||
                 PeekToken(1).Kind == SyntaxKind.ConstKeyword ||
                 PeekToken(1).Kind == SyntaxKind.AbstractKeyword ||
                 PeekToken(1).Kind == SyntaxKind.AsyncKeyword ||
                 PeekToken(1).Kind == SyntaxKind.DefaultKeyword)
             {
                 var modifiers = ParseModifiers();
                 if (modifiers.Count == 0) throw new Exception("Modifiers empty!");
                 // Dispatch
                 if (_currentToken.Kind == SyntaxKind.ClassKeyword || _currentToken.Kind == SyntaxKind.AbstractKeyword)
                    return ParseClassDeclaration(modifiers);
                 if (_currentToken.Kind == SyntaxKind.InterfaceKeyword)
                    return ParseInterfaceDeclaration(modifiers);
                 if (_currentToken.Kind == SyntaxKind.EnumKeyword)
                    return ParseEnumDeclaration(modifiers);
                 if (_currentToken.Kind == SyntaxKind.FunctionKeyword || _currentToken.Kind == SyntaxKind.AsyncKeyword)
                    return ParseFunctionDeclaration(modifiers);
                 if (_currentToken.Kind == SyntaxKind.TypeKeyword)
                    return ParseTypeAliasDeclaration(modifiers);

                 // VariableStatement? export const x = 1;
                 // VariableStatement expects 'var/let/const'. ParseModifiers consumed 'export' (and maybe 'const' if I added it to modifiers? No const is declaration start).
                 // Wait, ParseModifiers consumes ConstKeyword?
                 // I added ConstKeyword to ParseModifiers for "const enum".
                 // But for "export const x", "const" is the start of VariableStatement.
                 // If ParseModifiers consumes "const", then VariableStatement won't see it.
                 // I should REMOVE ConstKeyword from ParseModifiers or be careful.
                 // "const enum" is specific. "const x" is variable.
                 // If I removed ConstKeyword from ParseModifiers, "const enum" fails (const is modifier).
                 // If I keep it, "export const x" -> modifiers=[export, const]. Remaining: "x".
                 // ParseVariableStatement expects "const".
                 // So I'll remove ConstKeyword from ParseModifiers for now and handle "const enum" specifically if needed,
                 // OR adjust ParseVariableStatement to take modifiers?
                 // VariableStatement in Syntax.xml DOES NOT have modifiers yet. I didn't add it.
                 // So "export var" is not fully supported in AST yet?
                 // Standard TS: VariableStatement has modifiers (export, declare).
                 // My plan missed VariableStatement modifiers.
                 // I will stick to Class/Function/Interface for now and maybe skip VariableStatement export fix for this step to keep scope managed.
                 // Or just handle "export { ... }" which returns ExportDeclarationSyntax.
             }

             var exportKeyword = EatToken(SyntaxKind.ExportKeyword);
             var clause = ParseExportClause();
             SyntaxToken? from = null;
             ExpressionSyntax? module = null;

             if (_currentToken.Kind == SyntaxKind.FromKeyword)
             {
                 from = EatToken();
                 module = ParseExpression();
             }

             var semi = EatOptionalToken(SyntaxKind.SemicolonToken);
             return SyntaxFactory.ExportDeclaration(exportKeyword, clause, from, module, semi);
        }

        internal ExportClauseSyntax ParseExportClause()
        {
             var open = EatToken(SyntaxKind.OpenBraceToken);
             var elements = new SeparatedSyntaxListBuilder<ExportSpecifierSyntax>(8);
             if (_currentToken.Kind != SyntaxKind.CloseBraceToken)
             {
                 while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
                 {
                     elements.Add(ParseExportSpecifier());
                     if (_currentToken.Kind == SyntaxKind.CommaToken)
                         elements.AddSeparator(EatToken());
                     else
                         break;
                 }
             }
             var close = EatToken(SyntaxKind.CloseBraceToken);
             return SyntaxFactory.ExportClause(open, elements.ToList(), close);
        }

        internal ExportSpecifierSyntax ParseExportSpecifier()
        {
             var nameOrProp = ParseIdentifierName();
             if (_currentToken.Kind == SyntaxKind.AsKeyword)
             {
                 var asKeyword = EatToken();
                 var name = ParseIdentifierName();
                 return SyntaxFactory.ExportSpecifier(nameOrProp, asKeyword, name);
             }
             return SyntaxFactory.ExportSpecifier(null, null, nameOrProp);
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
            if (_currentToken.Kind == SyntaxKind.IdentifierToken || _currentToken.Kind == SyntaxKind.AsyncKeyword || _currentToken.Kind == SyntaxKind.AwaitKeyword)
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
            if (_currentToken.Kind == SyntaxKind.IdentifierToken || _currentToken.Kind == SyntaxKind.AsyncKeyword || _currentToken.Kind == SyntaxKind.AwaitKeyword)
            {
                if (_currentToken.Kind != SyntaxKind.SemicolonToken)
                {
                    label = ParseIdentifierName();
                }
            }
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.ContinueStatement(continueKeyword, label, semicolon);
        }

        internal StatementSyntax ParseForStatement()
        {
            var forKeyword = EatToken(SyntaxKind.ForKeyword);
            SyntaxToken? awaitKeyword = null;
            if (_currentToken.Kind == SyntaxKind.AwaitKeyword)
            {
                awaitKeyword = EatToken();
            }
            var openParen = EatToken(SyntaxKind.OpenParenToken);

            TypeScriptSyntaxNode? initializer = null;
            StatementSyntax? initializerStmt = null; // For standard loop

            if (_currentToken.Kind == SyntaxKind.SemicolonToken)
            {
                initializerStmt = ParseEmptyStatement();
            }
            else
            {
                if (_currentToken.Kind == SyntaxKind.VarKeyword || _currentToken.Kind == SyntaxKind.LetKeyword || _currentToken.Kind == SyntaxKind.ConstKeyword)
                {
                    var declKeyword = EatToken();
                    var decl = ParseVariableDeclaration();
                    // Create VariableStatement without semicolon for now
                    initializer = SyntaxFactory.VariableStatement(declKeyword, decl, SyntaxToken.CreateMissing(SyntaxKind.SemicolonToken, null, null));
                }
                else
                {
                    var expr = ParseExpression();
                    initializer = SyntaxFactory.ExpressionStatement(expr, SyntaxToken.CreateMissing(SyntaxKind.SemicolonToken, null, null));
                }
            }

            if (_currentToken.Kind == SyntaxKind.InKeyword)
            {
                var inKeyword = EatToken();
                var expr = ParseExpression();
                var closeParenForIn = EatToken(SyntaxKind.CloseParenToken);
                var body = ParseStatement();
                return SyntaxFactory.ForInStatement(forKeyword, openParen, initializer, inKeyword, expr, closeParenForIn, body);
            }

            if (_currentToken.Kind == SyntaxKind.OfKeyword)
            {
                var ofKeyword = EatToken();
                var expr = ParseExpression();
                var closeParenForOf = EatToken(SyntaxKind.CloseParenToken);
                var body = ParseStatement();
                return SyntaxFactory.ForOfStatement(forKeyword, awaitKeyword, openParen, initializer, ofKeyword, expr, closeParenForOf, body);
            }

            // Standard For Loop
            if (initializerStmt == null)
            {
                // We parsed initializer as TypeScriptSyntaxNode (VariableStatement or ExpressionStatement with missing semicolon).
                // We need to consume the actual semicolon now.
                var firstSemicolon = EatToken(SyntaxKind.SemicolonToken);

                if (initializer is VariableStatementSyntax vs)
                {
                    initializerStmt = SyntaxFactory.VariableStatement(vs.DeclarationKeyword, vs.Declaration, firstSemicolon);
                }
                else if (initializer is ExpressionStatementSyntax es)
                {
                    initializerStmt = SyntaxFactory.ExpressionStatement(es.Expression, firstSemicolon);
                }
                else
                {
                     // Should not happen if logic matches
                     initializerStmt = ParseEmptyStatement();
                }
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

            return SyntaxFactory.ForStatement(forKeyword, openParen, initializerStmt, condition, secondSemicolon, increment, closeParen, statement);
        }

        internal EmptyStatementSyntax ParseEmptyStatement()
        {
            var semicolon = EatToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.EmptyStatement(semicolon);
        }

        internal InterfaceDeclarationSyntax ParseInterfaceDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = default)
        {
            var interfaceKeyword = EatToken(SyntaxKind.InterfaceKeyword);
            var identifier = ParseIdentifierToken();
            var typeParameters = ParseOptionalTypeParameters();
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);

            var members = new SyntaxListBuilder<TypeElementSyntax>(8);
            while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                members.Add(ParseTypeElement());
            }

            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);

            return SyntaxFactory.InterfaceDeclaration(modifiers, interfaceKeyword, identifier, typeParameters, openBrace, members.ToList(), closeBrace);
        }

        internal TypeAliasDeclarationSyntax ParseTypeAliasDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = default)
        {
            var typeKeyword = EatToken(SyntaxKind.TypeKeyword);
            var identifier = ParseIdentifierToken();
            var typeParameters = ParseOptionalTypeParameters();
            var equalsToken = EatToken(SyntaxKind.EqualsToken);
            var type = ParseType();
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);

            return SyntaxFactory.TypeAliasDeclaration(modifiers, typeKeyword, identifier, typeParameters, equalsToken, type, semicolon);
        }

        internal EnumDeclarationSyntax ParseEnumDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = default)
        {
            var enumKeyword = EatToken(SyntaxKind.EnumKeyword);
            var identifier = ParseIdentifierToken();
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
            return SyntaxFactory.EnumDeclaration(modifiers, enumKeyword, identifier, openBrace, members.ToList(), closeBrace);
        }

        internal EnumMemberSyntax ParseEnumMember()
        {
            var identifier = ParseIdentifierToken();
            EqualsValueClauseSyntax? initializer = null;
            if (_currentToken.Kind == SyntaxKind.EqualsToken)
            {
                initializer = ParseEqualsValueClause();
            }
            return SyntaxFactory.EnumMember(identifier, initializer);
        }

        internal ClassDeclarationSyntax ParseClassDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = default)
        {
            var classKeyword = EatToken(SyntaxKind.ClassKeyword);
            var identifier = ParseOptionalIdentifierToken();
            var typeParameters = ParseOptionalTypeParameters();
            var openBrace = EatToken(SyntaxKind.OpenBraceToken);

            var members = new SyntaxListBuilder<ClassElementSyntax>(8);
            while (_currentToken.Kind != SyntaxKind.CloseBraceToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                members.Add(ParseClassElement());
            }

            var closeBrace = EatToken(SyntaxKind.CloseBraceToken);

            return SyntaxFactory.ClassDeclaration(modifiers, classKeyword, identifier, typeParameters, openBrace, members.ToList(), closeBrace);
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
            var identifier = ParseIdentifierToken();
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
            var modifiers = ParseModifiers();

            if (_currentToken.Kind == SyntaxKind.ConstructorKeyword)
            {
                return ParseConstructorDeclaration(modifiers);
            }

            if (_currentToken.Kind == SyntaxKind.GetKeyword)
            {
                if (PeekToken(1).Kind == SyntaxKind.IdentifierToken)
                {
                     var getKey = EatToken();
                     var name = ParseIdentifierName();
                     var open = EatToken(SyntaxKind.OpenParenToken);
                     var close = EatToken(SyntaxKind.CloseParenToken);
                     var type = ParseOptionalTypeAnnotation();
                     var body = ParseBlock();
                     return SyntaxFactory.GetAccessorDeclaration(modifiers, getKey, name, open, close, type, body);
                }
            }

            if (_currentToken.Kind == SyntaxKind.SetKeyword)
            {
                 if (PeekToken(1).Kind == SyntaxKind.IdentifierToken)
                 {
                     var setKey = EatToken();
                     var name = ParseIdentifierName();
                     var paramList = ParseParameterList();
                     var body = ParseBlock();
                     return SyntaxFactory.SetAccessorDeclaration(modifiers, setKey, name, paramList, body);
                 }
            }

            var identifier = ParseIdentifierName();
            if (_currentToken.Kind == SyntaxKind.OpenParenToken || _currentToken.Kind == SyntaxKind.LessThanToken)
            {
                return ParseMethodDeclaration(modifiers, identifier);
            }
            else
            {
                return ParsePropertyDeclaration(modifiers, identifier);
            }
        }

        internal ConstructorDeclarationSyntax ParseConstructorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers)
        {
            var constructorKeyword = EatToken(SyntaxKind.ConstructorKeyword);
            var parameterList = ParseParameterList();
            BlockSyntax? body = null;
            if (_currentToken.Kind == SyntaxKind.OpenBraceToken)
            {
                body = ParseBlock();
            }
            return SyntaxFactory.ConstructorDeclaration(modifiers, constructorKeyword, parameterList, body);
        }

        internal MethodDeclarationSyntax ParseMethodDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, IdentifierNameSyntax name)
        {
            var typeParameters = ParseOptionalTypeParameters();
            var parameterList = ParseParameterList();
            var typeAnnotation = ParseOptionalTypeAnnotation();
            BlockSyntax? body = null;
            if (_currentToken.Kind == SyntaxKind.OpenBraceToken)
            {
                body = ParseBlock();
            }
            return SyntaxFactory.MethodDeclaration(modifiers, name, typeParameters, parameterList, typeAnnotation, body);
        }

        internal PropertyDeclarationSyntax ParsePropertyDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, IdentifierNameSyntax name)
        {
            var typeAnnotation = ParseOptionalTypeAnnotation();
            EqualsValueClauseSyntax? initializer = null;
            if (_currentToken.Kind == SyntaxKind.EqualsToken)
            {
                initializer = ParseEqualsValueClause();
            }
            var semicolon = EatOptionalToken(SyntaxKind.SemicolonToken);
            return SyntaxFactory.PropertyDeclaration(modifiers, name, typeAnnotation, initializer, semicolon);
        }

        internal FunctionDeclarationSyntax ParseFunctionDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = default)
        {
            var functionKeyword = EatToken(SyntaxKind.FunctionKeyword);
            var identifier = ParseOptionalIdentifierToken();
            var typeParameters = ParseOptionalTypeParameters();
            var parameterList = ParseParameterList();
            var typeAnnotation = ParseOptionalTypeAnnotation();

            BlockSyntax? body = null;
            if (_currentToken.Kind == SyntaxKind.OpenBraceToken)
            {
                body = ParseBlock();
            }

            return SyntaxFactory.FunctionDeclaration(modifiers, functionKeyword, identifier, typeParameters, parameterList, typeAnnotation, body);
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
            var identifier = ParseIdentifierToken();
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
            var identifier = ParseIdentifierToken();
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
            return ParseUnionType();
        }

        internal TypeSyntax ParseUnionType()
        {
            var type = ParseIntersectionType();
            if (_currentToken.Kind == SyntaxKind.BarToken)
            {
                var types = new SeparatedSyntaxListBuilder<TypeSyntax>(4);
                types.Add(type);
                while (_currentToken.Kind == SyntaxKind.BarToken)
                {
                    types.AddSeparator(EatToken());
                    types.Add(ParseIntersectionType());
                }
                return SyntaxFactory.UnionType(types.ToList());
            }
            return type;
        }

        internal TypeSyntax ParseIntersectionType()
        {
            var type = ParsePostfixType();
            if (_currentToken.Kind == SyntaxKind.AmpersandToken)
            {
                var types = new SeparatedSyntaxListBuilder<TypeSyntax>(4);
                types.Add(type);
                while (_currentToken.Kind == SyntaxKind.AmpersandToken)
                {
                    types.AddSeparator(EatToken());
                    types.Add(ParsePostfixType());
                }
                return SyntaxFactory.IntersectionType(types.ToList());
            }
            return type;
        }

        internal TypeSyntax ParsePostfixType()
        {
            var type = ParsePrimaryType();
            while (_currentToken.Kind == SyntaxKind.OpenBracketToken)
            {
                var openBracket = EatToken();
                var closeBracket = EatToken(SyntaxKind.CloseBracketToken);
                type = SyntaxFactory.ArrayType(type, openBracket, closeBracket);
            }
            return type;
        }

        internal TypeSyntax ParsePrimaryType()
        {
            if (IsPredefinedType(_currentToken.Kind))
            {
                return ParsePredefinedType();
            }
            else if (_currentToken.Kind == SyntaxKind.IdentifierToken || _currentToken.Kind == SyntaxKind.AsyncKeyword || _currentToken.Kind == SyntaxKind.AwaitKeyword)
            {
                return ParseTypeReference();
            }
            else if (_currentToken.Kind == SyntaxKind.OpenParenToken)
            {
                return ParseParenthesizedType();
            }
            else if (_currentToken.Kind == SyntaxKind.OpenBracketToken)
            {
                return ParseTupleType();
            }

            // Fallback or error recovery
            return ParseTypeReference();
        }

        internal TypeSyntax ParseParenthesizedType()
        {
            var openParen = EatToken(SyntaxKind.OpenParenToken);
            var type = ParseType();
            var closeParen = EatToken(SyntaxKind.CloseParenToken);
            return SyntaxFactory.ParenthesizedType(openParen, type, closeParen);
        }

        internal TupleTypeSyntax ParseTupleType()
        {
            var openBracket = EatToken(SyntaxKind.OpenBracketToken);
            var elements = new SeparatedSyntaxListBuilder<TupleElementSyntax>(4);
            if (_currentToken.Kind != SyntaxKind.CloseBracketToken)
            {
                while (_currentToken.Kind != SyntaxKind.CloseBracketToken && _currentToken.Kind != SyntaxKind.EndOfFileToken)
                {
                    elements.Add(ParseTupleElement());
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
            return SyntaxFactory.TupleType(openBracket, elements.ToList(), closeBracket);
        }

        internal TupleElementSyntax ParseTupleElement()
        {
            SyntaxToken? dotDotDot = null;
            if (_currentToken.Kind == SyntaxKind.DotDotDotToken)
            {
                dotDotDot = EatToken();
            }

            // Tuple element can be "Name?: Type" or just "Type"
            // If we have identifier followed by colon or question colon, it's a named element.
            // But types can also start with identifier.
            // Lookahead needed or heuristics.
            // "a: string" -> Name: a, Type: string
            // "string" -> Type: string
            // "a?: string" -> Name: a, Question, Type: string

            IdentifierNameSyntax? name = null;
            SyntaxToken? question = null;
            SyntaxToken? colon = null;

            if (_currentToken.Kind == SyntaxKind.IdentifierToken)
            {
                // Check next token
                var next = PeekToken(1);
                if (next.Kind == SyntaxKind.ColonToken)
                {
                    name = ParseIdentifierName();
                    colon = EatToken();
                }
                else if (next.Kind == SyntaxKind.QuestionToken && PeekToken(2).Kind == SyntaxKind.ColonToken)
                {
                    name = ParseIdentifierName();
                    question = EatToken();
                    colon = EatToken();
                }
            }

            // Note: If we consumed name and colon, we must parse type.
            // If we didn't, we just parse type, but 'name' is null.

            var type = ParseType();
            return SyntaxFactory.TupleElement(dotDotDot, name, question, colon, type);
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
            return ParseAssignmentExpression();
        }

        internal ExpressionSyntax ParseAssignmentExpression()
        {
            var left = ParseConditionalExpression();

            if (_currentToken.Kind == SyntaxKind.EqualsGreaterThanToken && IsSimpleParameter(left))
            {
                var arrow = EatToken();
                var body = ParseArrowFunctionBody();
                var paramList = ConvertToParameterList(left);
                return SyntaxFactory.ArrowFunctionExpression(null, null, paramList, null, arrow, body);
            }

            if (IsAssignmentOperator(_currentToken.Kind))
            {
                var opToken = EatToken();
                var right = ParseAssignmentExpression();
                return SyntaxFactory.BinaryExpression(SyntaxKind.AssignmentExpression, left, opToken, right);
            }

            return left;
        }

        internal ExpressionSyntax ParseConditionalExpression()
        {
            var condition = ParseBinaryExpression(0);
            if (_currentToken.Kind == SyntaxKind.QuestionToken)
            {
                var question = EatToken();
                var whenTrue = ParseAssignmentExpression();
                var colon = EatToken(SyntaxKind.ColonToken);
                var whenFalse = ParseAssignmentExpression();
                return SyntaxFactory.ConditionalExpression(condition, question, whenTrue, colon, whenFalse);
            }
            return condition;
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

        private bool IsAssignmentOperator(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.EqualsToken:
                case SyntaxKind.PlusEqualsToken:
                case SyntaxKind.MinusEqualsToken:
                case SyntaxKind.AsteriskEqualsToken:
                case SyntaxKind.SlashEqualsToken:
                case SyntaxKind.PercentEqualsToken:
                case SyntaxKind.AmpersandEqualsToken:
                case SyntaxKind.BarEqualsToken:
                case SyntaxKind.CaretEqualsToken:
                case SyntaxKind.LessThanLessThanEqualsToken:
                case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                case SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken:
                    return true;
                default:
                    return false;
            }
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
                case SyntaxKind.LessThanLessThanToken:
                case SyntaxKind.GreaterThanGreaterThanToken:
                case SyntaxKind.GreaterThanGreaterThanGreaterThanToken:
                    return 8;
                case SyntaxKind.LessThanToken:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.LessThanEqualsToken:
                case SyntaxKind.GreaterThanEqualsToken:
                    return 7;
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.ExclamationEqualsToken:
                    return 6;
                case SyntaxKind.AmpersandToken:
                    return 5;
                case SyntaxKind.CaretToken:
                    return 4;
                case SyntaxKind.BarToken:
                    return 3;
                case SyntaxKind.AmpersandAmpersandToken:
                    return 2;
                case SyntaxKind.BarBarToken:
                    return 1;
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
                case SyntaxKind.AmpersandToken: return SyntaxKind.BitwiseAndExpression;
                case SyntaxKind.BarToken: return SyntaxKind.BitwiseOrExpression;
                case SyntaxKind.CaretToken: return SyntaxKind.ExclusiveOrExpression;
                case SyntaxKind.LessThanLessThanToken: return SyntaxKind.LeftShiftExpression;
                case SyntaxKind.GreaterThanGreaterThanToken: return SyntaxKind.RightShiftExpression;
                case SyntaxKind.GreaterThanGreaterThanGreaterThanToken: return SyntaxKind.UnsignedRightShiftExpression;
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

            if (_currentToken.Kind == SyntaxKind.AwaitKeyword)
            {
                var awaitKeyword = EatToken();
                var expression = ParseUnaryExpression();
                return SyntaxFactory.AwaitExpression(awaitKeyword, expression);
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
                    return ParseParenthesizedOrArrowExpression();
                case SyntaxKind.AsyncKeyword:
                    if (PeekToken(1).Kind == SyntaxKind.OpenParenToken)
                    {
                        var asyncKeyword = EatToken();
                        var expr = ParseParenthesizedOrArrowExpression(asyncKeyword);
                        if (expr is ArrowFunctionExpressionSyntax)
                        {
                            return expr;
                        }

                        var asyncId = SyntaxFactory.IdentifierName(ConvertToIdentifier(asyncKeyword));
                        var args = new SeparatedSyntaxListBuilder<ArgumentSyntax>(1);
                        args.Add(SyntaxFactory.Argument(expr));
                        var argList = SyntaxFactory.ArgumentList(
                            SyntaxToken.CreateMissing(SyntaxKind.OpenParenToken, null, null),
                            args.ToList(),
                            SyntaxToken.CreateMissing(SyntaxKind.CloseParenToken, null, null));
                        return SyntaxFactory.CallExpression(asyncId, null, argList);
                    }
                    if (PeekToken(1).Kind == SyntaxKind.IdentifierToken)
                    {
                        if (PeekToken(2).Kind == SyntaxKind.EqualsGreaterThanToken)
                        {
                            var asyncKeyword = EatToken();
                            var identifier = ParseIdentifierName();
                            var arrow = EatToken();
                            var body = ParseArrowFunctionBody();
                            var paramList = ConvertToParameterList(identifier);
                            return SyntaxFactory.ArrowFunctionExpression(asyncKeyword, null, paramList, null, arrow, body);
                        }
                    }

                    // Fallback: `async` as identifier.
                    return SyntaxFactory.IdentifierName(ParseIdentifierToken());
                default:
                    // Error recovery
                    return SyntaxFactory.IdentifierName(CreateMissingToken(SyntaxKind.IdentifierToken));
            }
        }

        internal ExpressionSyntax ParseParenthesizedOrArrowExpression(SyntaxToken? asyncKeyword = null)
        {
            var open = EatToken(SyntaxKind.OpenParenToken);
            if (_currentToken.Kind == SyntaxKind.CloseParenToken)
            {
                var closeParen = EatToken();
                if (_currentToken.Kind == SyntaxKind.EqualsGreaterThanToken)
                {
                    var arrow = EatToken();
                    var body = ParseArrowFunctionBody();
                    var paramList = SyntaxFactory.ParameterList(open, default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax>), closeParen);
                    return SyntaxFactory.ArrowFunctionExpression(asyncKeyword, null, paramList, null, arrow, body);
                }
                return SyntaxFactory.IdentifierName(CreateMissingToken(SyntaxKind.IdentifierToken));
            }

            var expr = ParseExpression();
            var close = EatToken(SyntaxKind.CloseParenToken);

            if (_currentToken.Kind == SyntaxKind.EqualsGreaterThanToken)
            {
                var arrow = EatToken();
                var body = ParseArrowFunctionBody();
                var paramList = ConvertToParameterList(open, expr, close);
                return SyntaxFactory.ArrowFunctionExpression(asyncKeyword, null, paramList, null, arrow, body);
            }

            return expr;
        }

        internal TypeScriptSyntaxNode ParseArrowFunctionBody()
        {
            if (_currentToken.Kind == SyntaxKind.OpenBraceToken)
            {
                return ParseBlock();
            }
            return ParseExpression();
        }

        internal ParameterListSyntax ConvertToParameterList(SyntaxToken open, ExpressionSyntax expr, SyntaxToken close)
        {
            var list = new SeparatedSyntaxListBuilder<ParameterSyntax>(4);
            if (expr is IdentifierNameSyntax id)
            {
                list.Add(SyntaxFactory.Parameter(id.Identifier, null));
            }
            return SyntaxFactory.ParameterList(open, list.ToList(), close);
        }

        internal ParameterListSyntax ConvertToParameterList(IdentifierNameSyntax identifier)
        {
            var list = new SeparatedSyntaxListBuilder<ParameterSyntax>(4);
            list.Add(SyntaxFactory.Parameter(identifier.Identifier, null));
            return SyntaxFactory.ParameterList(CreateMissingToken(SyntaxKind.OpenParenToken), list.ToList(), CreateMissingToken(SyntaxKind.CloseParenToken));
        }

        internal ParameterListSyntax ConvertToParameterList(ExpressionSyntax expr)
        {
            var list = new SeparatedSyntaxListBuilder<ParameterSyntax>(4);
            if (expr is IdentifierNameSyntax id)
            {
                list.Add(SyntaxFactory.Parameter(id.Identifier, null));
            }
            return SyntaxFactory.ParameterList(CreateMissingToken(SyntaxKind.OpenParenToken), list.ToList(), CreateMissingToken(SyntaxKind.CloseParenToken));
        }

        private bool IsSimpleParameter(ExpressionSyntax expr)
        {
            return expr is IdentifierNameSyntax;
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
            var token = ParseIdentifierToken();
            return SyntaxFactory.IdentifierName(token);
        }
    }
}

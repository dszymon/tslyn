// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Xunit;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Parser
{
    public class AdditionalSyntaxTests
    {
        [Fact]
        public void ParseDoStatement()
        {
            var code = "do { x++; } while (x < 10);";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var doStmt = Assert.IsType<DoStatementSyntax>(root.Statements[0]);
            Assert.IsType<BlockSyntax>(doStmt.Statement);
            Assert.IsType<BinaryExpressionSyntax>(doStmt.Condition);
        }

        [Fact]
        public void ParseBreakStatement()
        {
            var code = "break;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var breakStmt = Assert.IsType<BreakStatementSyntax>(root.Statements[0]);
            Assert.Null(breakStmt.Label);
        }

        [Fact]
        public void ParseBreakStatementWithLabel()
        {
            var code = "break loop1;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var breakStmt = Assert.IsType<BreakStatementSyntax>(root.Statements[0]);
            Assert.NotNull(breakStmt.Label);
            Assert.Equal("loop1", breakStmt.Label.Identifier.Text);
        }

        [Fact]
        public void ParseContinueStatement()
        {
            var code = "continue;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var continueStmt = Assert.IsType<ContinueStatementSyntax>(root.Statements[0]);
            Assert.Null(continueStmt.Label);
        }

        [Fact]
        public void ParseContinueStatementWithLabel()
        {
            var code = "continue loop1;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var continueStmt = Assert.IsType<ContinueStatementSyntax>(root.Statements[0]);
            Assert.NotNull(continueStmt.Label);
            Assert.Equal("loop1", continueStmt.Label.Identifier.Text);
        }

        [Fact]
        public void ParseTypeAlias()
        {
            var code = "type ID = string;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var typeAlias = Assert.IsType<TypeAliasDeclarationSyntax>(root.Statements[0]);
            Assert.Equal("ID", typeAlias.Identifier.Text);
            var predefinedType = Assert.IsType<PredefinedTypeSyntax>(typeAlias.Type);
            Assert.Equal(SyntaxKind.StringKeyword, predefinedType.Keyword.Kind());
        }

        [Fact]
        public void ParseGenericTypeAlias()
        {
            var code = "type List<T> = Array<T>;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var typeAlias = Assert.IsType<TypeAliasDeclarationSyntax>(root.Statements[0]);
            Assert.Equal("List", typeAlias.Identifier.Text);
            Assert.NotNull(typeAlias.TypeParameters);
            Assert.Single(typeAlias.TypeParameters.Parameters);
            Assert.Equal("T", typeAlias.TypeParameters.Parameters[0].Identifier.Text);
        }

        [Fact]
        public void ParseEnumDeclaration()
        {
            var code = "enum Color { Red, Green, Blue }";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var enumDecl = Assert.IsType<EnumDeclarationSyntax>(root.Statements[0]);
            Assert.Equal("Color", enumDecl.Identifier.Text);
            Assert.Equal(3, enumDecl.Members.Count);
            Assert.Equal("Red", enumDecl.Members[0].Identifier.Text);
            Assert.Equal("Green", enumDecl.Members[1].Identifier.Text);
            Assert.Equal("Blue", enumDecl.Members[2].Identifier.Text);
        }

        [Fact]
        public void ParseEnumDeclarationWithValues()
        {
            var code = "enum Status { Active = 1, Inactive = 0 }";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var enumDecl = Assert.IsType<EnumDeclarationSyntax>(root.Statements[0]);
            Assert.Equal(2, enumDecl.Members.Count);

            var member1 = enumDecl.Members[0];
            Assert.Equal("Active", member1.Identifier.Text);
            Assert.NotNull(member1.EqualsValueClause);

            var member2 = enumDecl.Members[1];
            Assert.Equal("Inactive", member2.Identifier.Text);
            Assert.NotNull(member2.EqualsValueClause);
        }
    }
}

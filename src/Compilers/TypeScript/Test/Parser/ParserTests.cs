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
    public class ParserTests
    {
        [Fact]
        public void ParseSimpleExpressionStatement()
        {
            var text = "x;";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            Assert.Equal(1, root.Statements.Count);
            var statement = (ExpressionStatementSyntax)root.Statements[0];
            var expr = (IdentifierNameSyntax)statement.Expression;
            Assert.Equal("x", expr.Identifier.Text);
            Assert.Equal(";", statement.SemicolonToken.Text);
            Assert.False(statement.SemicolonToken.IsMissing);
        }

        [Fact]
        public void ParseMultipleStatements()
        {
            var text = "x; y;";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            Assert.Equal(2, root.Statements.Count);
            var s1 = (ExpressionStatementSyntax)root.Statements[0];
            Assert.Equal("x", ((IdentifierNameSyntax)s1.Expression).Identifier.Text);

            var s2 = (ExpressionStatementSyntax)root.Statements[1];
            Assert.Equal("y", ((IdentifierNameSyntax)s2.Expression).Identifier.Text);
        }

        [Fact]
        public void ParseMissingSemicolon()
        {
            var text = "x";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            Assert.Equal(1, root.Statements.Count);
            var statement = (ExpressionStatementSyntax)root.Statements[0];
            var expr = (IdentifierNameSyntax)statement.Expression;
            Assert.Equal("x", expr.Identifier.Text);

            Assert.True(statement.SemicolonToken.IsMissing);
            Assert.Equal(0, statement.SemicolonToken.Width);
        }
    }
}

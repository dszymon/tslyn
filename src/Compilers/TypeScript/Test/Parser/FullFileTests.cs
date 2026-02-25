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
    public class FullFileTests
    {
        [Fact]
        public void ParseMixedContentFile()
        {
            var code = @"
interface User {
    id: number;
    name: string;
}

x;
y;
";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            Assert.Equal(3, root.Statements.Count);

            // 1. Interface
            var interfaceDecl = Assert.IsType<InterfaceDeclarationSyntax>(root.Statements[0]);
            Assert.Equal("User", interfaceDecl.Identifier.Text);
            Assert.Equal(2, interfaceDecl.Members.Count);

            // 2. Statement x;
            var stmt1 = Assert.IsType<ExpressionStatementSyntax>(root.Statements[1]);
            var expr1 = Assert.IsType<IdentifierNameSyntax>(stmt1.Expression);
            Assert.Equal("x", expr1.Identifier.Text);

            // 3. Statement y;
            var stmt2 = Assert.IsType<ExpressionStatementSyntax>(root.Statements[2]);
            var expr2 = Assert.IsType<IdentifierNameSyntax>(stmt2.Expression);
            Assert.Equal("y", expr2.Identifier.Text);
        }
    }
}

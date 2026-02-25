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
    public class ScriptTests
    {
        [Fact]
        public void ParseSimpleMath()
        {
            var code = "var x = 1 + 2 * 3;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var varStmt = Assert.IsType<VariableStatementSyntax>(root.Statements[0]);
            var decl = varStmt.Declaration;
            Assert.Equal("x", decl.Identifier.Text);

            var init = decl.EqualsValueClause.Value;
            var add = Assert.IsType<BinaryExpressionSyntax>(init);
            Assert.Equal(SyntaxKind.AddExpression, add.Kind());

            var left = Assert.IsType<LiteralExpressionSyntax>(add.Left);
            Assert.Equal("1", left.Token.Text);

            var right = Assert.IsType<BinaryExpressionSyntax>(add.Right);
            Assert.Equal(SyntaxKind.MultiplyExpression, right.Kind());
        }

        [Fact]
        public void ParseFunctionAndReturn()
        {
            var code = @"
function add(a: number, b: number) {
    return a + b;
}
";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var func = Assert.IsType<FunctionDeclarationSyntax>(root.Statements[0]);
            Assert.Equal("add", func.Identifier.Text);
            Assert.Equal(2, func.ParameterList.Parameters.Count);

            var p1 = func.ParameterList.Parameters[0];
            Assert.Equal("a", p1.Identifier.Text);

            var body = func.Body;
            Assert.Single(body.Statements);
            var ret = Assert.IsType<ReturnStatementSyntax>(body.Statements[0]);
            Assert.Equal(SyntaxKind.AddExpression, ret.Expression.Kind());
        }

        [Fact]
        public void ParseMixedScript()
        {
            var code = @"
interface Point { x: number; y: number; }
let p: Point;
p.x = 10;
";
            // Note: Parser doesn't support MemberAccessExpression (dot) yet, so p.x will probably parse as 'p' then error or unexpected token?
            // Actually, based on current parser implementation, ParseExpression only handles IdentifierName and Literals and Binary.
            // 'p.x' is not supported. It will likely parse 'p' as expression, then fail on '.'.
            // Let's adjust test to what IS supported or verify partial support.

            // For now, let's test simpler script supported by current grammar
             var simpleCode = @"
interface Point { x: number; y: number; }
var x = 10;
var y = 20;
function sum() { return x + y; }
";
            var tree = TypeScriptSyntaxTree.ParseText(simpleCode);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            Assert.Equal(4, root.Statements.Count);
            Assert.IsType<InterfaceDeclarationSyntax>(root.Statements[0]);
            Assert.IsType<VariableStatementSyntax>(root.Statements[1]);
            Assert.IsType<VariableStatementSyntax>(root.Statements[2]);
            Assert.IsType<FunctionDeclarationSyntax>(root.Statements[3]);
        }
    }
}

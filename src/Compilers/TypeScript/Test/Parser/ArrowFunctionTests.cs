using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Xunit;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Parser
{
    public class ArrowFunctionTests
    {
        [Fact]
        public void ParseEmptyParamsArrow()
        {
            var code = "() => 1;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements[0]);
            var arrow = Assert.IsType<ArrowFunctionExpressionSyntax>(stmt.Expression);

            Assert.Empty(arrow.ParameterList.Parameters);
            Assert.IsType<LiteralExpressionSyntax>(arrow.Body);
        }

        [Fact]
        public void ParseSimpleParamArrow()
        {
            var code = "x => x;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements[0]);
            var arrow = Assert.IsType<ArrowFunctionExpressionSyntax>(stmt.Expression);

            Assert.Single(arrow.ParameterList.Parameters);
            Assert.Equal("x", arrow.ParameterList.Parameters[0].Identifier.Text);
            Assert.IsType<IdentifierNameSyntax>(arrow.Body);
        }

        [Fact]
        public void ParseParenthesizedParamArrow()
        {
            var code = "(x) => { return x; };";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements[0]);
            var arrow = Assert.IsType<ArrowFunctionExpressionSyntax>(stmt.Expression);

            Assert.Single(arrow.ParameterList.Parameters);
            Assert.IsType<BlockSyntax>(arrow.Body);
        }

        [Fact]
        public void ParseArrowInBinary()
        {
             // Test precedence/parsing in expression context
             var code = "const f = x => x + 1;";
             var tree = TypeScriptSyntaxTree.ParseText(code);
             var root = (CompilationUnitSyntax)tree.GetRoot();

             var varStmt = Assert.IsType<VariableStatementSyntax>(root.Statements[0]);
             var decl = varStmt.DeclarationList.Declarations[0];
             var init = decl.EqualsValueClause.Value;

             var arrow = Assert.IsType<ArrowFunctionExpressionSyntax>(init);
             Assert.Single(arrow.ParameterList.Parameters);

             var bin = Assert.IsType<BinaryExpressionSyntax>(arrow.Body);
        }
    }
}

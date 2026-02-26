using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Xunit;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Parser
{
    public class ControlFlowTests
    {
        [Fact]
        public void ParseIfStatement()
        {
            var code = "if (x) { return; }";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var ifStmt = Assert.IsType<IfStatementSyntax>(root.Statements.First());
            Assert.IsType<IdentifierNameSyntax>(ifStmt.Condition);
            Assert.IsType<BlockSyntax>(ifStmt.Statement);
            Assert.Null(ifStmt.Else);
        }

        [Fact]
        public void ParseIfElseStatement()
        {
            var code = "if (x) y; else z;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var ifStmt = Assert.IsType<IfStatementSyntax>(root.Statements.First());
            Assert.NotNull(ifStmt.Else);
            var elseStmt = Assert.IsType<ExpressionStatementSyntax>(ifStmt.Else.Statement);
        }

        [Fact]
        public void ParseWhileStatement()
        {
            var code = "while (true) {}";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var whileStmt = Assert.IsType<WhileStatementSyntax>(root.Statements.First());
            Assert.IsType<LiteralExpressionSyntax>(whileStmt.Condition);
        }

        [Fact]
        public void ParseForStatement_Simple()
        {
            var code = "for (i = 0; i < 10; i = i + 1) {}";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var forStmt = Assert.IsType<ForStatementSyntax>(root.Statements.First());
            Assert.IsType<ExpressionStatementSyntax>(forStmt.Initializer);
            Assert.IsType<BinaryExpressionSyntax>(forStmt.Condition);
            Assert.IsType<BinaryExpressionSyntax>(forStmt.Increment);
        }

        [Fact]
        public void ParseCallAndMemberAccess()
        {
            var code = "console.log(x);";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());
            var call = Assert.IsType<CallExpressionSyntax>(stmt.Expression);
            var member = Assert.IsType<MemberAccessExpressionSyntax>(call.Expression);

            Assert.Equal("log", member.Name.Identifier.Text);
            var console = Assert.IsType<IdentifierNameSyntax>(member.Expression);
            Assert.Equal("console", console.Identifier.Text);

            Assert.Single(call.ArgumentList.Arguments);
        }

        [Fact]
        public void ParseBinaryExpression()
        {
            var code = "i < 10;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());
            Assert.IsType<BinaryExpressionSyntax>(stmt.Expression);
        }
    }
}

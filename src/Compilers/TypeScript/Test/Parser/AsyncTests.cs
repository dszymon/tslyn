
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Xunit;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Parser
{
    public class AsyncTests
    {
        [Fact]
        public void ParseAsyncFunctionDeclaration()
        {
            var text = "async function foo() {}";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            Assert.Single(root.Statements);
            var func = Assert.IsType<FunctionDeclarationSyntax>(root.Statements[0]);
            Assert.Contains(func.Modifiers, m => m.Text == "async");
            Assert.Equal("function", func.FunctionKeyword.Text);
            Assert.Equal("foo", func.Identifier.Text);
        }

        [Fact]
        public void ParseAwaitExpression()
        {
            var text = "async function foo() { await x; }";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var func = (FunctionDeclarationSyntax)root.Statements[0];
            var block = func.Body;
            Assert.NotNull(block);
            var stmt = (ExpressionStatementSyntax)block.Statements[0];
            var awaitExpr = Assert.IsType<AwaitExpressionSyntax>(stmt.Expression);
            Assert.Equal("await", awaitExpr.AwaitKeyword.Text);
            Assert.IsType<IdentifierNameSyntax>(awaitExpr.Expression);
        }

        [Fact]
        public void ParseAsyncArrowFunction()
        {
             var text = "const f = async () => {};";
             var tree = TypeScriptSyntaxTree.ParseText(text);
             var root = (CompilationUnitSyntax)tree.GetRoot();

             var varStmt = (VariableStatementSyntax)root.Statements[0];
             var decl = varStmt.Declaration;
             var init = decl.EqualsValueClause;
             Assert.NotNull(init);
             var arrow = Assert.IsType<ArrowFunctionExpressionSyntax>(init.Value);
             Assert.Equal("async", arrow.AsyncKeyword.Text);
        }

        [Fact]
        public void ParseAsyncArrowFunctionWithParam()
        {
             var text = "const f = async x => x;";
             var tree = TypeScriptSyntaxTree.ParseText(text);
             var root = (CompilationUnitSyntax)tree.GetRoot();

             var varStmt = (VariableStatementSyntax)root.Statements[0];
             var decl = varStmt.Declaration;
             var init = decl.EqualsValueClause;
             Assert.NotNull(init);
             var arrow = Assert.IsType<ArrowFunctionExpressionSyntax>(init.Value);
             Assert.Equal("async", arrow.AsyncKeyword.Text);
             Assert.Single(arrow.ParameterList.Parameters);
             Assert.Equal("x", arrow.ParameterList.Parameters[0].Identifier.Text);
        }

        [Fact]
        public void ParseAsyncMethod()
        {
            var text = "class C { async m() {} }";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = (ClassDeclarationSyntax)root.Statements[0];
            var method = Assert.IsType<MethodDeclarationSyntax>(cls.Members[0]);
            Assert.Contains(method.Modifiers, m => m.Text == "async");
            Assert.Equal("m", method.Name.Identifier.Text);
        }

        [Fact]
        public void ParseAsyncAsIdentifierCall()
        {
            var text = "async(x);";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = (ExpressionStatementSyntax)root.Statements[0];
            var call = Assert.IsType<CallExpressionSyntax>(stmt.Expression);
            var id = Assert.IsType<IdentifierNameSyntax>(call.Expression);
            Assert.Equal("async", id.Identifier.Text);
            Assert.Single(call.ArgumentList.Arguments);
        }

        [Fact]
        public void ParseAsyncAsProperty()
        {
            var text = "class C { async = 1; }";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = (ClassDeclarationSyntax)root.Statements[0];
            var prop = Assert.IsType<PropertyDeclarationSyntax>(cls.Members[0]);
            Assert.Equal("async", prop.Name.Identifier.Text);
            Assert.NotNull(prop.EqualsValueClause);
        }

        [Fact]
        public void ParseAsyncAsVariable()
        {
            var text = "let async = 1;";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = (VariableStatementSyntax)root.Statements[0];
            var decl = stmt.Declaration;
            Assert.Equal("async", decl.Identifier.Text);
        }
    }
}

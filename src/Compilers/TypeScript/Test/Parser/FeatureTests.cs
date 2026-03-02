using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Xunit;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Parser
{
    public class FeatureTests
    {
        [Fact]
        public void ParseForInStatement()
        {
            var code = "for (var x in y) {}";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = Assert.IsType<ForInStatementSyntax>(root.Statements[0]);
            var decl = Assert.IsType<VariableStatementSyntax>(stmt.Initializer);
            Assert.Equal("var", decl.DeclarationKeyword.Text);
            Assert.Equal("x", decl.DeclarationList.Declarations[0].Identifier.Text);

            Assert.IsType<IdentifierNameSyntax>(stmt.Expression); // y
        }

        [Fact]
        public void ParseForOfStatement()
        {
            var code = "for (const item of list) {}";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = Assert.IsType<ForOfStatementSyntax>(root.Statements[0]);
            Assert.Equal(SyntaxKind.None, stmt.AwaitKeyword.Kind());

            var decl = Assert.IsType<VariableStatementSyntax>(stmt.Initializer);
            Assert.Equal("const", decl.DeclarationKeyword.Text);

            Assert.IsType<IdentifierNameSyntax>(stmt.Expression); // list
        }

        [Fact]
        public void ParseForAwaitOfStatement()
        {
            var code = "for await (let x of asyncIterable) {}";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var stmt = Assert.IsType<ForOfStatementSyntax>(root.Statements[0]);
            Assert.Equal("await", stmt.AwaitKeyword.Text);

            var decl = Assert.IsType<VariableStatementSyntax>(stmt.Initializer);
            Assert.Equal("let", decl.DeclarationKeyword.Text);
        }

        [Fact]
        public void ParseClassModifiers()
        {
            var code = "export abstract class C {}";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = Assert.IsType<ClassDeclarationSyntax>(root.Statements[0]);
            Assert.Equal(2, cls.Modifiers.Count);
            Assert.Equal("export", cls.Modifiers[0].Text);
            Assert.Equal("abstract", cls.Modifiers[1].Text);
        }

        [Fact]
        public void ParseClassMemberModifiersAndAccessors()
        {
            var code = @"
class C {
    public static x: number;
    private method() {}
    get prop() { return this.x; }
    set prop(v) { this.x = v; }
}
";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = (ClassDeclarationSyntax)root.Statements[0];
            Assert.Equal(4, cls.Members.Count);

            var prop = Assert.IsType<PropertyDeclarationSyntax>(cls.Members[0]);
            Assert.Equal(2, prop.Modifiers.Count);
            Assert.Equal("public", prop.Modifiers[0].Text);
            Assert.Equal("static", prop.Modifiers[1].Text);

            var method = Assert.IsType<MethodDeclarationSyntax>(cls.Members[1]);
            Assert.Single(method.Modifiers);
            Assert.Equal("private", method.Modifiers[0].Text);

            var getter = Assert.IsType<GetAccessorDeclarationSyntax>(cls.Members[2]);
            Assert.Equal("prop", getter.Name.Identifier.Text);
            Assert.NotNull(getter.Body);

            var setter = Assert.IsType<SetAccessorDeclarationSyntax>(cls.Members[3]);
            Assert.Equal("prop", setter.Name.Identifier.Text);
            Assert.Single(setter.ParameterList.Parameters);
            Assert.NotNull(setter.Body);
        }

        [Fact]
        public void ParseExportFunction()
        {
            var code = "export async function foo() {}";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var func = Assert.IsType<FunctionDeclarationSyntax>(root.Statements[0]);
            Assert.Equal(2, func.Modifiers.Count);
            Assert.Equal("export", func.Modifiers[0].Text);
            Assert.Equal("async", func.Modifiers[1].Text);
        }
    }
}

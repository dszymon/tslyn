using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Xunit;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Parser
{
    public class ModuleTests
    {
        [Fact]
        public void ParseSideEffectImport()
        {
            var code = "import \"mod\";";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var decl = Assert.IsType<ImportDeclarationSyntax>(root.Statements[0]);
            Assert.Null(decl.ImportClause);
            Assert.Equal(SyntaxKind.None, decl.FromKeyword.Kind());
            var specifier = Assert.IsType<LiteralExpressionSyntax>(decl.ModuleSpecifier);
            Assert.Equal("\"mod\"", specifier.Token.Text);
        }

        [Fact]
        public void ParseDefaultImport()
        {
            var code = "import d from \"mod\";";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var decl = Assert.IsType<ImportDeclarationSyntax>(root.Statements[0]);
            Assert.NotNull(decl.ImportClause);
            Assert.NotNull(decl.ImportClause.Name);
            Assert.Equal("d", decl.ImportClause.Name.Identifier.Text);
            Assert.Null(decl.ImportClause.NamedBindings);
        }

        [Fact]
        public void ParseNamedImports()
        {
            var code = "import { a, b as c } from \"mod\";";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var decl = Assert.IsType<ImportDeclarationSyntax>(root.Statements[0]);
            var clause = decl.ImportClause;
            Assert.Null(clause.Name);
            var bindings = Assert.IsType<NamedImportsSyntax>(clause.NamedBindings);

            Assert.Equal(2, bindings.Elements.Count);

            var e1 = bindings.Elements[0];
            Assert.Equal("a", e1.Name.Identifier.Text);
            Assert.Null(e1.PropertyName);

            var e2 = bindings.Elements[1];
            Assert.Equal("c", e2.Name.Identifier.Text);
            Assert.NotNull(e2.PropertyName);
            Assert.Equal("b", e2.PropertyName.Identifier.Text);
        }

        [Fact]
        public void ParseNamespaceImport()
        {
            var code = "import * as ns from \"mod\";";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var decl = Assert.IsType<ImportDeclarationSyntax>(root.Statements[0]);
            var bindings = Assert.IsType<NamespaceImportSyntax>(decl.ImportClause.NamedBindings);
            Assert.Equal("ns", bindings.Name.Identifier.Text);
        }

        [Fact]
        public void ParseExportNamed()
        {
            var code = "export { a, b as c };";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var decl = Assert.IsType<ExportDeclarationSyntax>(root.Statements[0]);
            Assert.Equal(SyntaxKind.None, decl.FromKeyword.Kind());
            Assert.Null(decl.ModuleSpecifier);

            var clause = decl.ExportClause;
            Assert.Equal(2, clause.Elements.Count);
            Assert.Equal("a", clause.Elements[0].Name.Identifier.Text);
            Assert.Equal("c", clause.Elements[1].Name.Identifier.Text);
        }

        [Fact]
        public void ParseExportReExport()
        {
            var code = "export { a } from \"mod\";";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var decl = Assert.IsType<ExportDeclarationSyntax>(root.Statements[0]);
            Assert.NotNull(decl.FromKeyword);
            Assert.IsType<LiteralExpressionSyntax>(decl.ModuleSpecifier);
        }
    }
}

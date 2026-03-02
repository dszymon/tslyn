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
    public class GenericsTests
    {
        [Fact]
        public void ParseGenericClass()
        {
            var code = "class Box<T> { value: T; }";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = Assert.IsType<ClassDeclarationSyntax>(root.Statements[0]);
            Assert.Equal("Box", cls.Identifier.Text);

            Assert.NotNull(cls.TypeParameters);
            Assert.Single(cls.TypeParameters.Parameters);
            Assert.Equal("T", cls.TypeParameters.Parameters[0].Identifier.Text);

            var prop = Assert.IsType<PropertyDeclarationSyntax>(cls.Members[0]);
            var typeRef = Assert.IsType<TypeReferenceSyntax>(prop.TypeAnnotation.Type);
            Assert.Equal("T", typeRef.TypeName.Identifier.Text);
        }

        [Fact]
        public void ParseGenericFunction()
        {
            var code = "function identity<T>(arg: T): T { return arg; }";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var func = Assert.IsType<FunctionDeclarationSyntax>(root.Statements[0]);
            Assert.NotNull(func.TypeParameters);
            Assert.Single(func.TypeParameters.Parameters);
            Assert.Equal("T", func.TypeParameters.Parameters[0].Identifier.Text);

            var param = func.ParameterList.Parameters[0];
            var typeRef = Assert.IsType<TypeReferenceSyntax>(param.TypeAnnotation.Type);
            Assert.Equal("T", typeRef.TypeName.Identifier.Text);
        }

        [Fact]
        public void ParseGenericInterfaceWithConstraint()
        {
            var code = "interface Container<T extends Item> {}";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var iface = Assert.IsType<InterfaceDeclarationSyntax>(root.Statements[0]);
            var tp = iface.TypeParameters.Parameters[0];
            Assert.Equal("T", tp.Identifier.Text);
            Assert.NotNull(tp.Constraint);
            var constraintType = Assert.IsType<TypeReferenceSyntax>(tp.Constraint.Type);
            Assert.Equal("Item", constraintType.TypeName.Identifier.Text);
        }

        [Fact]
        public void ParseGenericTypeReference()
        {
            var code = "let x: List<string>;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var varStmt = Assert.IsType<VariableStatementSyntax>(root.Statements[0]);
            var typeRef = Assert.IsType<TypeReferenceSyntax>(varStmt.DeclarationList.Declarations[0].TypeAnnotation.Type);
            Assert.Equal("List", typeRef.TypeName.Identifier.Text);

            Assert.NotNull(typeRef.TypeArguments);
            Assert.Single(typeRef.TypeArguments.Arguments);
            var arg = Assert.IsType<PredefinedTypeSyntax>(typeRef.TypeArguments.Arguments[0]);
            Assert.Equal(SyntaxKind.StringKeyword, arg.Keyword.Kind());
        }
    }
}

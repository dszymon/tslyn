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
    public class ClassTests
    {
        [Fact]
        public void ParseEmptyClass()
        {
            var text = "class C {}";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            Assert.Equal(1, root.Statements.Count);
            var classDecl = Assert.IsType<ClassDeclarationSyntax>(root.Statements[0]);
            Assert.Equal("C", classDecl.Identifier.Text);
            Assert.Equal(0, classDecl.Members.Count);
        }

        [Fact]
        public void ParseClassWithProperties()
        {
            var text = @"
class Point {
    x: number;
    y: number = 0;
}
";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var classDecl = (ClassDeclarationSyntax)root.Statements[0];
            Assert.Equal(2, classDecl.Members.Count);

            var p1 = Assert.IsType<PropertyDeclarationSyntax>(classDecl.Members[0]);
            Assert.Equal("x", p1.Name.Identifier.Text);
            Assert.NotNull(p1.TypeAnnotation);
            Assert.Null(p1.EqualsValueClause);

            var p2 = Assert.IsType<PropertyDeclarationSyntax>(classDecl.Members[1]);
            Assert.Equal("y", p2.Name.Identifier.Text);
            Assert.NotNull(p2.TypeAnnotation);
            Assert.NotNull(p2.EqualsValueClause);
        }

        [Fact]
        public void ParseClassWithMethods()
        {
            var text = @"
class Calculator {
    add(a: number, b: number): number {
        return a + b;
    }
}
";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var classDecl = (ClassDeclarationSyntax)root.Statements[0];
            Assert.Single(classDecl.Members);

            var method = Assert.IsType<MethodDeclarationSyntax>(classDecl.Members[0]);
            Assert.Equal("add", method.Name.Identifier.Text);
            Assert.Equal(2, method.ParameterList.Parameters.Count);
            Assert.NotNull(method.TypeAnnotation);
            Assert.NotNull(method.Body);
        }

        [Fact]
        public void ParseClassWithConstructor()
        {
            var text = @"
class User {
    constructor(name: string) {}
}
";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var classDecl = (ClassDeclarationSyntax)root.Statements[0];
            Assert.Single(classDecl.Members);

            var ctor = Assert.IsType<ConstructorDeclarationSyntax>(classDecl.Members[0]);
            Assert.Equal(SyntaxKind.ConstructorKeyword, ctor.ConstructorKeyword.Kind());
            Assert.Single(ctor.ParameterList.Parameters);
            Assert.NotNull(ctor.Body);
        }
    }
}

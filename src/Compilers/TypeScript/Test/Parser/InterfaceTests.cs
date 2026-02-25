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
    public class InterfaceTests
    {
        [Fact]
        public void ParseEmptyInterface()
        {
            var text = "interface I {}";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            Assert.Equal(1, root.Statements.Count);
            var interfaceDecl = Assert.IsType<InterfaceDeclarationSyntax>(root.Statements[0]);
            Assert.Equal("I", interfaceDecl.Identifier.Text);
            Assert.Equal(0, interfaceDecl.Members.Count);
        }

        [Fact]
        public void ParseInterfaceWithProperties()
        {
            var text = "interface Point { x: number; y: number; }";
            var tree = TypeScriptSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var interfaceDecl = (InterfaceDeclarationSyntax)root.Statements[0];
            Assert.Equal(2, interfaceDecl.Members.Count);

            var p1 = Assert.IsType<PropertySignatureSyntax>(interfaceDecl.Members[0]);
            Assert.Equal("x", p1.Name.Identifier.Text);
            var t1 = Assert.IsType<PredefinedTypeSyntax>(p1.TypeAnnotation.Type);
            Assert.Equal(SyntaxKind.NumberKeyword, t1.Keyword.Kind());

            var p2 = Assert.IsType<PropertySignatureSyntax>(interfaceDecl.Members[1]);
            Assert.Equal("y", p2.Name.Identifier.Text);
            var t2 = Assert.IsType<PredefinedTypeSyntax>(p2.TypeAnnotation.Type);
            Assert.Equal(SyntaxKind.NumberKeyword, t2.Keyword.Kind());
        }

        [Fact]
        public void ParseOptionalProperty()
        {
             var text = "interface I { x?: string; }";
             var tree = TypeScriptSyntaxTree.ParseText(text);
             var root = (CompilationUnitSyntax)tree.GetRoot();

             var interfaceDecl = (InterfaceDeclarationSyntax)root.Statements[0];
             var p = (PropertySignatureSyntax)interfaceDecl.Members[0];

             Assert.Equal("x", p.Name.Identifier.Text);
             Assert.False(p.QuestionToken.IsMissing);
             Assert.Equal("?", p.QuestionToken.Text);

             var type = Assert.IsType<PredefinedTypeSyntax>(p.TypeAnnotation.Type);
             Assert.Equal(SyntaxKind.StringKeyword, type.Keyword.Kind());
        }

        [Fact]
        public void ParseTypeReference()
        {
             var text = "interface I { x: Foo; }";
             var tree = TypeScriptSyntaxTree.ParseText(text);
             var root = (CompilationUnitSyntax)tree.GetRoot();

             var interfaceDecl = (InterfaceDeclarationSyntax)root.Statements[0];
             var p = (PropertySignatureSyntax)interfaceDecl.Members[0];

             var type = Assert.IsType<TypeReferenceSyntax>(p.TypeAnnotation.Type);
             Assert.Equal("Foo", type.TypeName.Identifier.Text);
        }
    }
}

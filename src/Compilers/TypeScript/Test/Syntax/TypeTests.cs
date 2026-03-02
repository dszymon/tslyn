// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Xunit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax;

using StatementSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.StatementSyntax;
using VariableStatementSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.VariableStatementSyntax;
using TypeSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.TypeSyntax;
using UnionTypeSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.UnionTypeSyntax;
using IntersectionTypeSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.IntersectionTypeSyntax;
using PredefinedTypeSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.PredefinedTypeSyntax;
using TypeReferenceSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.TypeReferenceSyntax;
using ParenthesizedTypeSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.ParenthesizedTypeSyntax;
using ArrayTypeSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.ArrayTypeSyntax;
using TupleTypeSyntax = Microsoft.CodeAnalysis.TypeScript.Syntax.TupleTypeSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Syntax
{
    public class TypeTests
    {
        private StatementSyntax ParseStatement(string text)
        {
             var sourceText = SourceText.From(text);
             using var lexer = new Lexer(sourceText);
             using var parser = new LanguageParser(lexer);

             var greenStmt = parser.ParseStatement();

             // Create a red node. Parent is null.
             return (StatementSyntax)greenStmt.CreateRed(null, 0);
        }

        [Fact]
        public void TestUnionType()
        {
            var source = "var x: string | number;";
            var node = ParseStatement(source);
            var varStmt = Assert.IsType<VariableStatementSyntax>(node);
            var decl = varStmt.DeclarationList.Declarations[0];
            var type = decl.TypeAnnotation.Type;
            var unionType = Assert.IsType<UnionTypeSyntax>(type);
            Assert.Equal(2, unionType.Types.Count);
            Assert.IsType<PredefinedTypeSyntax>(unionType.Types[0]);
            Assert.IsType<PredefinedTypeSyntax>(unionType.Types[1]);
        }

        [Fact]
        public void TestIntersectionType()
        {
            var source = "var x: A & B;";
            var node = ParseStatement(source);
            var varStmt = Assert.IsType<VariableStatementSyntax>(node);
            var decl = varStmt.DeclarationList.Declarations[0];
            var type = decl.TypeAnnotation.Type;
            var intersectionType = Assert.IsType<IntersectionTypeSyntax>(type);
            Assert.Equal(2, intersectionType.Types.Count);
            Assert.IsType<TypeReferenceSyntax>(intersectionType.Types[0]);
            Assert.IsType<TypeReferenceSyntax>(intersectionType.Types[1]);
        }

        [Fact]
        public void TestUnionAndIntersectionPrecedence()
        {
            // A | B & C -> A | (B & C)
            var source = "var x: A | B & C;";
            var node = ParseStatement(source);
            var varStmt = Assert.IsType<VariableStatementSyntax>(node);
            var decl = varStmt.DeclarationList.Declarations[0];
            var type = decl.TypeAnnotation.Type;
            var unionType = Assert.IsType<UnionTypeSyntax>(type);
            Assert.Equal(2, unionType.Types.Count);
            Assert.IsType<TypeReferenceSyntax>(unionType.Types[0]); // A
            Assert.IsType<IntersectionTypeSyntax>(unionType.Types[1]); // B & C
        }

        [Fact]
        public void TestParenthesizedType()
        {
            var source = "var x: (string | number)[];";
            var node = ParseStatement(source);
            var varStmt = Assert.IsType<VariableStatementSyntax>(node);
            var decl = varStmt.DeclarationList.Declarations[0];
            var type = decl.TypeAnnotation.Type;
            var arrayType = Assert.IsType<ArrayTypeSyntax>(type);
            var elementType = arrayType.ElementType;
            var parenType = Assert.IsType<ParenthesizedTypeSyntax>(elementType);
            Assert.IsType<UnionTypeSyntax>(parenType.Type);
        }

        [Fact]
        public void TestTupleType()
        {
            var source = "var x: [string, number];";
            var node = ParseStatement(source);
            var varStmt = Assert.IsType<VariableStatementSyntax>(node);
            var decl = varStmt.DeclarationList.Declarations[0];
            var type = decl.TypeAnnotation.Type;
            var tupleType = Assert.IsType<TupleTypeSyntax>(type);
            Assert.Equal(2, tupleType.Elements.Count);
            Assert.IsType<PredefinedTypeSyntax>(tupleType.Elements[0].Type);
            Assert.IsType<PredefinedTypeSyntax>(tupleType.Elements[1].Type);
        }

        [Fact]
        public void TestNamedTupleType()
        {
            var source = "var x: [name: string, age?: number];";
            var node = ParseStatement(source);
            var varStmt = Assert.IsType<VariableStatementSyntax>(node);
            var decl = varStmt.DeclarationList.Declarations[0];
            var type = decl.TypeAnnotation.Type;
            var tupleType = Assert.IsType<TupleTypeSyntax>(type);
            Assert.Equal(2, tupleType.Elements.Count);

            var elem1 = tupleType.Elements[0];
            Assert.Equal("name", elem1.Name.Identifier.Text);
            Assert.True(elem1.QuestionToken.Kind() == SyntaxKind.None);
            Assert.IsType<PredefinedTypeSyntax>(elem1.Type);

            var elem2 = tupleType.Elements[1];
            Assert.Equal("age", elem2.Name.Identifier.Text);
            Assert.True(elem2.QuestionToken.Kind() == SyntaxKind.QuestionToken);
            Assert.IsType<PredefinedTypeSyntax>(elem2.Type);
        }

        [Fact]
        public void TestTupleWithRest()
        {
            var source = "var x: [string, ...number[]];";
            var node = ParseStatement(source);
            var varStmt = Assert.IsType<VariableStatementSyntax>(node);
            var decl = varStmt.DeclarationList.Declarations[0];
            var type = decl.TypeAnnotation.Type;
            var tupleType = Assert.IsType<TupleTypeSyntax>(type);

            var elem2 = tupleType.Elements[1];
            Assert.NotNull(elem2.DotDotDotToken);
            Assert.IsType<ArrayTypeSyntax>(elem2.Type);
        }
    }
}

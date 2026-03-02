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
    public class ArrayTypeTests
    {
        [Fact]
        public void ParseSimpleArrayType()
        {
            var code = "let x: number[];";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var varStmt = Assert.IsType<VariableStatementSyntax>(root.Statements[0]);
            var arrayType = Assert.IsType<ArrayTypeSyntax>(varStmt.DeclarationList.Declarations[0].TypeAnnotation.Type);

            var elementType = Assert.IsType<PredefinedTypeSyntax>(arrayType.ElementType);
            Assert.Equal(SyntaxKind.NumberKeyword, elementType.Keyword.Kind());
        }

        [Fact]
        public void ParseNestedArrayType()
        {
            var code = "let matrix: number[][];";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var varStmt = Assert.IsType<VariableStatementSyntax>(root.Statements[0]);
            var outerArray = Assert.IsType<ArrayTypeSyntax>(varStmt.DeclarationList.Declarations[0].TypeAnnotation.Type);
            var innerArray = Assert.IsType<ArrayTypeSyntax>(outerArray.ElementType);
            var elementType = Assert.IsType<PredefinedTypeSyntax>(innerArray.ElementType);
            Assert.Equal(SyntaxKind.NumberKeyword, elementType.Keyword.Kind());
        }

        [Fact]
        public void ParseArrayOfGeneric()
        {
            var code = "let list: List<string>[];";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var varStmt = Assert.IsType<VariableStatementSyntax>(root.Statements[0]);
            var arrayType = Assert.IsType<ArrayTypeSyntax>(varStmt.DeclarationList.Declarations[0].TypeAnnotation.Type);

            var typeRef = Assert.IsType<TypeReferenceSyntax>(arrayType.ElementType);
            Assert.Equal("List", typeRef.TypeName.Identifier.Text);
            Assert.NotNull(typeRef.TypeArguments);
        }
    }
}

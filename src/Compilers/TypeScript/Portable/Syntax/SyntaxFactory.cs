// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.TypeScript.Syntax;

namespace Microsoft.CodeAnalysis.TypeScript
{
    public static partial class SyntaxFactory
    {
        public static SyntaxToken Token(SyntaxKind kind)
        {
            return new SyntaxToken(null, Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax.SyntaxToken.Create(kind), 0, 0);
        }

        public static SyntaxToken Token(SyntaxKind kind, SyntaxTriviaList leading, SyntaxTriviaList trailing)
        {
            // Simplified implementation
            return new SyntaxToken(null, Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax.SyntaxToken.Create(kind), 0, 0);
        }

        public static SyntaxToken Token(string text)
        {
             return Identifier(text);
        }

        public static SyntaxToken Identifier(string text)
        {
            return new SyntaxToken(null, Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax.SyntaxToken.Identifier(text), 0, 0);
        }

        public static SyntaxToken Identifier(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing)
        {
             // Simplified
            return new SyntaxToken(null, Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax.SyntaxToken.Identifier(text), 0, 0);
        }

        public static IdentifierNameSyntax IdentifierName(string name)
        {
            return IdentifierName(Identifier(name));
        }

        // List factories
        public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode
        {
            var builder = new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxListBuilder(8);
            foreach (var node in nodes)
            {
                builder.Add(node.Green);
            }
            var listNode = builder.ToList();
            return new SyntaxList<TNode>(listNode.Node?.CreateRed(null, 0));
        }

        public static SyntaxList<TNode> List<TNode>() where TNode : SyntaxNode
        {
            return default;
        }

        // Trivia factories
        public static SyntaxTrivia ElasticMarker => default;

        public static SyntaxTrivia EndOfLine(string text)
        {
             // Simplified
             return default;
        }

        public static SyntaxTrivia Space => Whitespace(" ");

        public static SyntaxTrivia Whitespace(string text)
        {
             // Simplified
             return default;
        }
    }
}

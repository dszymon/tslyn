using System;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal static class SyntaxFactory
    {
        internal static readonly SyntaxTrivia Space = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, " ");
        internal static readonly SyntaxTrivia ElasticSpace = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, " "); // Should be elastic

        // Cache for tokens without trivia
        private static readonly SyntaxToken[] s_tokens = new SyntaxToken[Enum.GetValues(typeof(SyntaxKind)).Length];

        static SyntaxFactory()
        {
            // Initialize cache with tokens that have fixed text
            foreach (SyntaxKind kind in Enum.GetValues(typeof(SyntaxKind)))
            {
                if (kind == SyntaxKind.None) continue;

                string text = SyntaxFacts.GetText(kind);
                if (!string.IsNullOrEmpty(text))
                {
                    // For now, we assume tokens with fixed text (keywords, punctuation)
                    // can be cached as simple tokens without trivia.
                    // This mirrors Roslyn's approach for "TokensWithNoTrivia".
                    s_tokens[(int)kind] = new SyntaxToken(kind);
                }
            }
        }

        internal static SyntaxToken Token(SyntaxKind kind)
        {
            if ((int)kind < s_tokens.Length)
            {
                var cached = s_tokens[(int)kind];
                if (cached != null)
                {
                    return cached;
                }
            }

            return SyntaxToken.Create(kind);
        }

        internal static SyntaxToken Token(GreenNode? leading, SyntaxKind kind, GreenNode? trailing)
        {
            return SyntaxToken.Create(kind, leading, trailing);
        }

        internal static SyntaxToken Identifier(string text)
        {
            return SyntaxToken.Identifier(text);
        }

        internal static SyntaxToken Identifier(GreenNode? leading, string text, GreenNode? trailing)
        {
            return SyntaxToken.Identifier(text, leading, trailing);
        }

        internal static SyntaxToken Literal(string text, int value)
        {
            return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, text, value);
        }

        internal static SyntaxToken Literal(string text, string value)
        {
            return SyntaxToken.WithValue(SyntaxKind.StringLiteralToken, text, value);
        }

        public static SyntaxList<TNode> List<TNode>(TNode node) where TNode : TypeScriptSyntaxNode
        {
            return new SyntaxList<TNode>(SyntaxList.List(node));
        }

        public static SyntaxList<TNode> List<TNode>(params TNode[] nodes) where TNode : TypeScriptSyntaxNode
        {
            return new SyntaxList<TNode>(SyntaxList.List(nodes));
        }

        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(TNode node) where TNode : TypeScriptSyntaxNode
        {
            return new SeparatedSyntaxList<TNode>(new SyntaxList<TNode>(node));
        }

        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(params TypeScriptSyntaxNode[] nodes) where TNode : TypeScriptSyntaxNode
        {
            return new SeparatedSyntaxList<TNode>(new SyntaxList<TNode>(SyntaxList.List(nodes)));
        }
    }
}

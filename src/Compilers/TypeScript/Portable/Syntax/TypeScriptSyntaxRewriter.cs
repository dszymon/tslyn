// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.TypeScript.Syntax;

namespace Microsoft.CodeAnalysis.TypeScript
{
    public abstract partial class TypeScriptSyntaxRewriter : TypeScriptSyntaxVisitor<SyntaxNode?>
    {
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            if (node != null)
            {
                return ((TypeScriptSyntaxNode)node).Accept(this);
            }
            return null;
        }

        public virtual SyntaxToken VisitToken(SyntaxToken token)
        {
            return token;
        }

        public virtual SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            return trivia;
        }

        public virtual SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list) where TNode : SyntaxNode
        {
            List<TNode>? alternate = null;
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var item = list[i];
                var visited = (TNode?)Visit(item);
                if (item != visited && alternate == null)
                {
                    alternate = new List<TNode>(n);
                    for (int j = 0; j < i; j++) alternate.Add(list[j]);
                }
                if (alternate != null && visited != null)
                {
                    alternate.Add(visited);
                }
            }
            if (alternate != null)
            {
                return SyntaxFactory.List(alternate);
            }
            return list;
        }

        public virtual SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
        {
             // Simplified implementation for now, assuming no changes or full rebuild
             // TODO: Implement SeparatedSyntaxList rewriting properly
             return list;
        }

        public virtual SyntaxTokenList VisitList(SyntaxTokenList list)
        {
            return list;
        }

        public virtual SyntaxTriviaList VisitList(SyntaxTriviaList list)
        {
            return list;
        }
    }
}

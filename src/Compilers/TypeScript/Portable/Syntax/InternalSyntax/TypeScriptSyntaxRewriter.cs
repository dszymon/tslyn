// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal partial class TypeScriptSyntaxRewriter : TypeScriptSyntaxVisitor<TypeScriptSyntaxNode>
    {
        public virtual TypeScriptSyntaxNode Visit(TypeScriptSyntaxNode node)
        {
            return node.Accept(this);
        }

        public virtual Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> VisitList<TNode>(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode> list) where TNode : TypeScriptSyntaxNode
        {
            Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxListBuilder? alternate = null;
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var item = list[i];
                var visited = (TNode?)Visit(item);
                if (item != visited && alternate == null)
                {
                    alternate = new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxListBuilder(n);
                    for (int j = 0; j < i; j++) alternate.Add(list[j]);
                }

                if (alternate != null && visited != null)
                {
                    alternate.Add(visited);
                }
            }

            if (alternate != null)
            {
                return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TNode>(alternate.ToList().Node);
            }

            return list;
        }

        public virtual Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> VisitList<TNode>(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> list) where TNode : TypeScriptSyntaxNode
        {
             // TODO implementation for separated list
             return list;
        }
    }
}

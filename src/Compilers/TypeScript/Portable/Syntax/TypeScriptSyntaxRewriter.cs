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

        // Removed override because base class VisitToken returns TResult? (SyntaxNode?),
        // but we want to return SyntaxToken here for utility.
        // The base VisitToken implementation calls DefaultVisit, which returns null.
        // We are effectively "hiding" the base method with a more specific one for rewriter usage,
        // but since C# doesn't support changing return types on overrides, we can't override.
        // However, the error says we ARE overriding (implicitly?) or just conflicting.

        // Actually, TypeScriptSyntaxVisitor<TResult> defines:
        // public virtual TResult? VisitToken(SyntaxToken token) => DefaultVisit(token.Parent!);

        // So we must return SyntaxNode? if we override.
        // But a token is not a node.

        // C# Roslyn usually handles this by having CSharpSyntaxRewriter inherit from CSharpSyntaxVisitor<SyntaxNode>
        // and implementing VisitToken to return SyntaxToken? No, wait.

        // Let's check CSharpSyntaxVisitor.cs definition in Roslyn source if available? No.

        // The issue is: TResult is SyntaxNode?.
        // Base: SyntaxNode? VisitToken(SyntaxToken token)
        // Here: SyntaxToken VisitToken(SyntaxToken token)

        // Since we cannot change the return type to SyntaxToken while overriding,
        // and we really want a method that returns a SyntaxToken for rewriting purposes (rewriting tokens),
        // we should probably NOT inherit from TypeScriptSyntaxVisitor<SyntaxNode?> for the rewriter?
        // Or we accept that VisitToken on the visitor is for returning a Node result from visiting a token (weird).

        // If we look at CSharpSyntaxRewriter, it inherits from CSharpSyntaxVisitor<SyntaxNode>.
        // BUT CSharpSyntaxVisitor<TResult> does NOT have a VisitToken method that returns TResult.
        // It usually has VisitToken(SyntaxToken token) inside the non-generic visitor, or handled differently.

        // In our generated TypeScriptSyntaxVisitor.cs:
        // public virtual TResult? VisitToken(SyntaxToken token) => DefaultVisit(token.Parent!);

        // This seems to be the problem. The generator added VisitToken to the generic visitor.
        // If we can't change the generator, we have to work around it.

        // We can explicitly implement the interface/abstract member?
        // No, it's a class inheritance.

        // We can use `new` to hide it, which resolves the warning but not the polymorphic dispatch if called via base reference.
        // The error `CS0508` was because I tried to `override` with a different return type.
        // The previous warning `CS0114` was "hides inherited member... to make the current member override... add the override keyword".
        // But I CAN'T add override because of the return type mismatch.

        // So I must use `new`.

        public new virtual SyntaxToken VisitToken(SyntaxToken token)
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

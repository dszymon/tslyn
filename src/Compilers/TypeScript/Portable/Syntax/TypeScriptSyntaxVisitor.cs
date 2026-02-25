// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.TypeScript.Syntax;

namespace Microsoft.CodeAnalysis.TypeScript
{
    public partial class TypeScriptSyntaxVisitor<TResult>
    {
        public virtual TResult? DefaultVisit(SyntaxNode node)
        {
            return default;
        }

        public virtual TResult? Visit(SyntaxNode? node)
        {
            if (node != null)
            {
                return ((TypeScriptSyntaxNode)node).Accept(this);
            }

            return default;
        }

        public virtual TResult? VisitToken(SyntaxToken token) => DefaultVisit(token.Parent!);
    }

    public partial class TypeScriptSyntaxVisitor
    {
        public virtual void DefaultVisit(SyntaxNode node)
        {
        }

        public virtual void Visit(SyntaxNode? node)
        {
            if (node != null)
            {
                ((TypeScriptSyntaxNode)node).Accept(this);
            }
        }

        public virtual void VisitToken(SyntaxToken token) => DefaultVisit(token.Parent!);
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal partial class TypeScriptSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(TypeScriptSyntaxNode node)
        {
            return default;
        }

        public virtual TResult Visit(GreenNode? node)
        {
            if (node != null)
            {
                return ((TypeScriptSyntaxNode)node).Accept(this);
            }

            return default;
        }

        public virtual TResult VisitToken(SyntaxToken token) => DefaultVisit(token);
    }

    internal partial class TypeScriptSyntaxVisitor
    {
        public virtual void DefaultVisit(TypeScriptSyntaxNode node)
        {
        }

        public virtual void Visit(GreenNode? node)
        {
            if (node != null)
            {
                ((TypeScriptSyntaxNode)node).Accept(this);
            }
        }

        public virtual void VisitToken(SyntaxToken token) => DefaultVisit(token);
    }
}

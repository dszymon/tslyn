// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal static class TypeScriptSyntaxNodeCache
    {
        public static void AddNode(TypeScriptSyntaxNode node, int hash)
        {
            SyntaxNodeCache.AddNode(node, hash);
        }

        public static TypeScriptSyntaxNode? TryGetNode(int kind, GreenNode? child1, out int hash)
        {
            return (TypeScriptSyntaxNode?)SyntaxNodeCache.TryGetNode(kind, child1, out hash);
        }

        public static TypeScriptSyntaxNode? TryGetNode(int kind, GreenNode? child1, GreenNode? child2, out int hash)
        {
            return (TypeScriptSyntaxNode?)SyntaxNodeCache.TryGetNode(kind, child1, child2, out hash);
        }

        public static TypeScriptSyntaxNode? TryGetNode(int kind, GreenNode? child1, GreenNode? child2, GreenNode? child3, out int hash)
        {
            return (TypeScriptSyntaxNode?)SyntaxNodeCache.TryGetNode(kind, child1, child2, child3, out hash);
        }

        public static TypeScriptSyntaxNode? TryGetNode(int kind, GreenNode? child1, SyntaxFactoryContext context, out int hash)
        {
            return (TypeScriptSyntaxNode?)SyntaxNodeCache.TryGetNode(kind, child1, GetNodeFlags(context), out hash);
        }

        public static TypeScriptSyntaxNode? TryGetNode(int kind, GreenNode? child1, GreenNode? child2, SyntaxFactoryContext context, out int hash)
        {
            return (TypeScriptSyntaxNode?)SyntaxNodeCache.TryGetNode(kind, child1, child2, GetNodeFlags(context), out hash);
        }

        public static TypeScriptSyntaxNode? TryGetNode(int kind, GreenNode? child1, GreenNode? child2, GreenNode? child3, SyntaxFactoryContext context, out int hash)
        {
            return (TypeScriptSyntaxNode?)SyntaxNodeCache.TryGetNode(kind, child1, child2, child3, GetNodeFlags(context), out hash);
        }

        private static GreenNode.NodeFlags GetNodeFlags(SyntaxFactoryContext context)
        {
            var flags = SyntaxNodeCache.GetDefaultNodeFlags();

            if (context.IsInAsync)
            {
                flags |= GreenNode.NodeFlags.FactoryContextIsInAsync;
            }

            if (context.IsInQuery)
            {
                flags |= GreenNode.NodeFlags.FactoryContextIsInQuery;
            }

            // FieldKeywordContext might not be relevant for TS yet, but keeping for parity if needed.
            // TypeScript doesn't have "field" keyword context in the same way C# does (maybe),
            // but let's check SyntaxFactoryContext definition.
            // For now, I'll comment it out if it doesn't exist or assume it does if I copied it.
            // I'll stick to basics.

            return flags;
        }
    }
}

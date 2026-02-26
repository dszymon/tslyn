// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.TypeScript
{
    public static class TypeScriptExtensions
    {
        public static SyntaxKind Kind(this SyntaxToken token)
        {
            return (SyntaxKind)token.RawKind;
        }

        public static SyntaxKind Kind(this SyntaxNode node)
        {
            return (SyntaxKind)node.RawKind;
        }

        public static SyntaxKind Kind(this SyntaxNodeOrToken nodeOrToken)
        {
            return (SyntaxKind)nodeOrToken.RawKind;
        }
    }
}

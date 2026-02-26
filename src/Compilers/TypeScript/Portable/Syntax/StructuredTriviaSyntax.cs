// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.TypeScript.Syntax
{
    public abstract class StructuredTriviaSyntax : TypeScriptSyntaxNode
    {
        internal StructuredTriviaSyntax(InternalSyntax.TypeScriptSyntaxNode green, SyntaxNode? parent, int position)
            : base(green, parent, position)
        {
        }
    }
}

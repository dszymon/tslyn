// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal abstract class StructuredTriviaSyntax : TypeScriptSyntaxNode
    {
        internal StructuredTriviaSyntax(SyntaxKind kind)
            : base(kind)
        {
        }

        internal StructuredTriviaSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics)
            : base(kind, diagnostics)
        {
        }

        internal StructuredTriviaSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations)
        {
        }

        public override bool IsStructuredTrivia => true;

        public static StructuredTriviaSyntax Create(SyntaxTrivia trivia)
        {
            // TODO: Implement creation logic
            return null;
        }
    }
}

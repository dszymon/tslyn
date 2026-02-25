// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal abstract class TypeScriptSyntaxNode : GreenNode
    {
        protected TypeScriptSyntaxNode(SyntaxKind kind)
            : base((ushort)kind)
        {
            TypeScriptSyntaxNodeCache.AddNode(this);
        }

        protected TypeScriptSyntaxNode(SyntaxKind kind, int fullWidth)
            : base((ushort)kind, fullWidth)
        {
            TypeScriptSyntaxNodeCache.AddNode(this);
        }

        protected TypeScriptSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base((ushort)kind, diagnostics, annotations)
        {
            TypeScriptSyntaxNodeCache.AddNode(this);
        }

        protected TypeScriptSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations, int fullWidth)
            : base((ushort)kind, diagnostics, annotations, fullWidth)
        {
            TypeScriptSyntaxNodeCache.AddNode(this);
        }

        public override string Language => "TypeScript";

        public SyntaxKind Kind => (SyntaxKind)this.RawKind;

        public string KindText => Kind.ToString();

        public override int RawContextualKind => (int)Kind;

        public override bool IsStructuredTrivia => this is StructuredTriviaSyntax;

        public override bool IsDirective => this is DirectiveTriviaSyntax;

        public override bool IsSkippedTokensTrivia => this.Kind == SyntaxKind.ConflictMarkerTrivia; // Placeholder, adjust as needed

        // To be implemented by generated code
        public abstract TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor);

        public abstract void Accept(TypeScriptSyntaxVisitor visitor);

        internal protected virtual void WriteTo(System.IO.TextWriter writer)
        {
             // Basic implementation, overridden by tokens/trivia
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal abstract class TypeScriptSyntaxNode : GreenNode
    {
        internal TypeScriptSyntaxNode(SyntaxKind kind)
            : base((ushort)kind)
        {
        }

        internal TypeScriptSyntaxNode(SyntaxKind kind, int fullWidth)
            : base((ushort)kind, fullWidth)
        {
        }

        internal TypeScriptSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics)
            : base((ushort)kind, diagnostics)
        {
        }

        internal TypeScriptSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, int fullWidth)
            : base((ushort)kind, diagnostics, fullWidth)
        {
        }

        internal TypeScriptSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base((ushort)kind, diagnostics, annotations)
        {
        }

        internal TypeScriptSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations, int fullWidth)
            : base((ushort)kind, diagnostics, annotations, fullWidth)
        {
        }

        public override string Language => "TypeScript";

        public SyntaxKind Kind => (SyntaxKind)this.RawKind;

        public override string KindText => this.Kind.ToString();

        public override int RawContextualKind => this.RawKind;

        public override bool IsSkippedTokensTrivia => this.Kind == SyntaxKind.SkippedTokensTrivia;

        // Documentation comments not yet implemented
        public override bool IsDocumentationCommentTrivia => false;

        public override int GetSlotOffset(int index)
        {
            int offset = 0;
            for (int i = 0; i < index; i++)
            {
                var child = this.GetSlot(i);
                if (child != null)
                {
                    offset += child.FullWidth;
                }
            }
            return offset;
        }

        public SyntaxToken GetFirstToken() => (SyntaxToken)this.GetFirstTerminal();
        public SyntaxToken GetLastToken() => (SyntaxToken)this.GetLastTerminal();

        public virtual GreenNode? GetLeadingTrivia() => null;
        public override GreenNode? GetLeadingTriviaCore() => this.GetLeadingTrivia();

        public virtual GreenNode? GetTrailingTrivia() => null;
        public override GreenNode? GetTrailingTriviaCore() => this.GetTrailingTrivia();

        public abstract TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor);
        public abstract void Accept(TypeScriptSyntaxVisitor visitor);

        // Changed from protected to internal to match usage in generated code
        internal void SetFactoryContext(SyntaxFactoryContext context)
        {
            // TODO: Implement context flags if needed
        }

        public override CodeAnalysis.SyntaxToken CreateSeparator(SyntaxNode element)
        {
            return Microsoft.CodeAnalysis.TypeScript.SyntaxFactory.Token(SyntaxKind.CommaToken);
        }

        public override bool IsTriviaWithEndOfLine()
        {
            return this.Kind == SyntaxKind.EndOfLineTrivia;
        }

        public override SyntaxNode GetStructure(Microsoft.CodeAnalysis.SyntaxTrivia trivia)
        {
            // TODO: Implement structured trivia support
            return null;
        }
    }
}

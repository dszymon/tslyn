// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal class SyntaxTrivia : TypeScriptSyntaxNode
    {
        internal readonly string Text;

        internal SyntaxTrivia(SyntaxKind kind, string text)
            : base(kind)
        {
            this.Text = text;
            this.FullWidth = text.Length;
            this.SetFlags(NodeFlags.IsNotMissing);
        }

        internal SyntaxTrivia(SyntaxKind kind, string text, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations)
        {
            this.Text = text;
            this.FullWidth = text.Length;
            this.SetFlags(NodeFlags.IsNotMissing);
        }

        public override bool IsTrivia => true;

        public override string ToString() => this.Text;

        public override string ToFullString() => this.Text;

        public override int Width => this.Text.Length;

        public override int GetLeadingTriviaWidth() => 0;

        public override int GetTrailingTriviaWidth() => 0;

        protected override GreenNode? GetSlot(int index) => null;

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
        {
             throw new System.NotImplementedException();
        }

        public override TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor)
        {
            return visitor.VisitTrivia(this);
        }

        public override void Accept(TypeScriptSyntaxVisitor visitor)
        {
            visitor.VisitTrivia(this);
        }

        internal static SyntaxTrivia Create(SyntaxKind kind, string text)
        {
            return new SyntaxTrivia(kind, text);
        }

        private string GetDebuggerDisplay()
        {
            return string.Format("{0} {1}", this.Kind, this.Text);
        }
    }
}

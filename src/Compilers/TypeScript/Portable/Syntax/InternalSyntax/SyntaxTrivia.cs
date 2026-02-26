// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal class SyntaxTrivia : TypeScriptSyntaxNode
    {
        public readonly string Text;

        internal SyntaxTrivia(SyntaxKind kind, string text, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
            : base(kind, diagnostics, annotations)
        {
            this.Text = text;
            this.FullWidth = text.Length;
        }

        public override bool IsTrivia => true;

        public override string ToString() => Text;

        public override string ToFullString() => Text;

        internal override GreenNode? GetSlot(int index) => null;

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
        {
            throw new InvalidOperationException("Cannot create a red node for unstructured trivia.");
        }

        public override object? GetValue() => this.Text;
        public override string GetValueText() => this.Text;
        public override int Width => this.FullWidth;
        public override int GetSlotOffset(int index) => 0;

        public override TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor) => throw new NotImplementedException();
        public override void Accept(TypeScriptSyntaxVisitor visitor) => throw new NotImplementedException();

        internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        {
            return new SyntaxTrivia(this.Kind, this.Text, this.GetDiagnostics(), annotations);
        }

        internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        {
            return new SyntaxTrivia(this.Kind, this.Text, diagnostics, this.GetAnnotations());
        }
    }
}

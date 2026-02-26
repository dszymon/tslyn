// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.TypeScript.Syntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal class SyntaxToken : TypeScriptSyntaxNode
    {
        protected readonly GreenNode? _leading;
        protected readonly GreenNode? _trailing;
        protected readonly string? _text;

        internal SyntaxToken(SyntaxKind kind)
            : base(kind)
        {
            this.SetFlags(NodeFlags.IsNotMissing);
        }

        internal SyntaxToken(SyntaxKind kind, GreenNode? leading, GreenNode? trailing)
            : base(kind)
        {
            this.SetFlags(NodeFlags.IsNotMissing);
            _leading = leading;
            _trailing = trailing;
            if (leading != null) AdjustFlagsAndWidth(leading);
            if (trailing != null) AdjustFlagsAndWidth(trailing);
        }

        internal SyntaxToken(SyntaxKind kind, string? text, GreenNode? leading, GreenNode? trailing)
            : this(kind, leading, trailing)
        {
            // Constructor chaining handles flags
            _text = text;
            if (text != null)
            {
                this.FullWidth += text.Length;
            }
            else
            {
                this.FullWidth += SyntaxFacts.GetText(kind).Length;
            }
        }

        internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations)
        {
            this.SetFlags(NodeFlags.IsNotMissing);
        }

        // Copy constructor-ish
        internal SyntaxToken(SyntaxKind kind, string? text, GreenNode? leading, GreenNode? trailing, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations)
        {
            this.SetFlags(NodeFlags.IsNotMissing);
            _leading = leading;
            _trailing = trailing;
            _text = text;
            if (leading != null) AdjustFlagsAndWidth(leading);
            if (trailing != null) AdjustFlagsAndWidth(trailing);

            int textWidth = text?.Length ?? SyntaxFacts.GetText(kind).Length;
            this.FullWidth += textWidth;
        }

        public override bool IsToken => true;

        public string Text => _text ?? SyntaxFacts.GetText(Kind);
        public override object? GetValue() => _text ?? SyntaxFacts.GetText(Kind);
        public override string GetValueText() => Text;

        public override int Width => Text.Length;

        public override int GetSlotOffset(int index) => 0;

        internal override GreenNode? GetSlot(int index) => null;

        public override GreenNode? GetLeadingTriviaCore() => _leading;
        public override GreenNode? GetTrailingTriviaCore() => _trailing;

        public override void Accept(TypeScriptSyntaxVisitor visitor) => visitor.VisitToken(this);
        public override TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor) => visitor.VisitToken(this);

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
        {
            return new SyntaxTokenWithTrivia(this, parent, position);
        }

        internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        {
            return new SyntaxToken(this.Kind, _text, _leading, _trailing, this.GetDiagnostics(), annotations);
        }

        internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        {
            return new SyntaxToken(this.Kind, _text, _leading, _trailing, diagnostics, this.GetAnnotations());
        }

        public override string ToString() => Text;

        public override string ToFullString()
        {
            var sb = new System.Text.StringBuilder();
            WriteTo(new StringWriter(sb), true, true);
            return sb.ToString();
        }

        protected override void WriteTokenTo(TextWriter writer, bool leading, bool trailing)
        {
            if (leading && _leading != null)
            {
                _leading.WriteTo(writer, true, true);
            }

            writer.Write(Text);

            if (trailing && _trailing != null)
            {
                _trailing.WriteTo(writer, true, true);
            }
        }

        // Factory methods
        internal static SyntaxToken Create(SyntaxKind kind) => new SyntaxToken(kind, null, null, null);
        internal static SyntaxToken Create(SyntaxKind kind, GreenNode? leading, GreenNode? trailing) => new SyntaxToken(kind, null, leading, trailing);
        internal static SyntaxToken Identifier(string text) => new SyntaxToken(SyntaxKind.IdentifierToken, text, null, null);
        internal static SyntaxToken WithValue(SyntaxKind kind, string text, object? value) => new SyntaxToken(kind, text, null, null);

        internal static SyntaxToken CreateMissing(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
        {
            var token = new SyntaxToken(kind, string.Empty, null, null, diagnostics, annotations);
            token.ClearFlags(NodeFlags.IsNotMissing);
            return token;
        }

        // Internal Red Node Wrapper for Tokens
        internal class SyntaxTokenWithTrivia : Microsoft.CodeAnalysis.TypeScript.Syntax.TypeScriptSyntaxNode
        {
            internal SyntaxTokenWithTrivia(SyntaxToken green, SyntaxNode? parent, int position)
                : base(green, parent, position)
            {
            }

            public override TResult Accept<TResult>(Microsoft.CodeAnalysis.TypeScript.TypeScriptSyntaxVisitor<TResult> visitor)
            {
                 return visitor.VisitToken(new Microsoft.CodeAnalysis.SyntaxToken(this.Parent, this.Green, this.Position, 0));
            }

            public override void Accept(Microsoft.CodeAnalysis.TypeScript.TypeScriptSyntaxVisitor visitor)
            {
                 visitor.VisitToken(new Microsoft.CodeAnalysis.SyntaxToken(this.Parent, this.Green, this.Position, 0));
            }

            internal override SyntaxNode? GetNodeSlot(int index) => null;
            internal override SyntaxNode? GetCachedSlot(int index) => null;

            protected override SyntaxTree SyntaxTreeCore => throw new NotImplementedException();
            protected internal override SyntaxNode NormalizeWhitespaceCore(string indentation, string eol, bool elasticTrivia) => this;

            protected internal override SyntaxNode ReplaceCore<TNode>(
                IEnumerable<TNode>? nodes,
                Func<TNode, TNode, SyntaxNode>? computeReplacementNode,
                IEnumerable<Microsoft.CodeAnalysis.SyntaxToken>? tokens,
                Func<Microsoft.CodeAnalysis.SyntaxToken, Microsoft.CodeAnalysis.SyntaxToken, Microsoft.CodeAnalysis.SyntaxToken>? computeReplacementToken,
                IEnumerable<Microsoft.CodeAnalysis.SyntaxTrivia>? trivia,
                Func<Microsoft.CodeAnalysis.SyntaxTrivia, Microsoft.CodeAnalysis.SyntaxTrivia, Microsoft.CodeAnalysis.SyntaxTrivia>? computeReplacementTrivia)
                => this;

            protected internal override SyntaxNode ReplaceNodeInListCore(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes) => this;
            protected internal override SyntaxNode InsertNodesInListCore(SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore) => this;
            protected internal override SyntaxNode ReplaceTokenInListCore(Microsoft.CodeAnalysis.SyntaxToken originalToken, IEnumerable<Microsoft.CodeAnalysis.SyntaxToken> newTokens) => this;
            protected internal override SyntaxNode InsertTokensInListCore(Microsoft.CodeAnalysis.SyntaxToken originalToken, IEnumerable<Microsoft.CodeAnalysis.SyntaxToken> newTokens, bool insertBefore) => this;
            protected internal override SyntaxNode ReplaceTriviaInListCore(Microsoft.CodeAnalysis.SyntaxTrivia originalTrivia, IEnumerable<Microsoft.CodeAnalysis.SyntaxTrivia> newTrivia) => this;
            protected internal override SyntaxNode InsertTriviaInListCore(Microsoft.CodeAnalysis.SyntaxTrivia originalTrivia, IEnumerable<Microsoft.CodeAnalysis.SyntaxTrivia> newTrivia, bool insertBefore) => this;
            protected internal override SyntaxNode RemoveNodesCore(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options) => this;
            protected override bool IsEquivalentToCore(SyntaxNode node, bool topLevel = false) => false;
        }
    }
}

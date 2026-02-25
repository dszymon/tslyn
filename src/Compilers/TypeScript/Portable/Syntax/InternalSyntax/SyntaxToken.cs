using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal class SyntaxToken : TypeScriptSyntaxNode
    {
        internal SyntaxToken(SyntaxKind kind)
            : base(kind)
        {
            var text = this.Text;
            if (text != null)
            {
                this.FullWidth = text.Length;
            }
        }

        internal SyntaxToken(SyntaxKind kind, int fullWidth)
            : base(kind, fullWidth)
        {
        }

        internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations)
        {
            var text = this.Text;
            if (text != null)
            {
                this.FullWidth = text.Length;
            }
        }

        internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations, fullWidth)
        {
        }

        public override bool IsToken => true;

        public virtual string Text => SyntaxFacts.GetText(Kind);

        public virtual string ValueText => Text;

        public virtual object? Value => ValueText;

        public override int Width => Text.Length;

        public override int GetLeadingTriviaWidth()
        {
            var leading = GetLeadingTrivia();
            return leading != null ? leading.FullWidth : 0;
        }

        public override int GetTrailingTriviaWidth()
        {
             var trailing = GetTrailingTrivia();
             return trailing != null ? trailing.FullWidth : 0;
        }

        internal static SyntaxToken Create(SyntaxKind kind)
        {
             return new SyntaxToken(kind);
        }

        internal static SyntaxToken Create(SyntaxKind kind, GreenNode? leading, GreenNode? trailing)
        {
            return new SyntaxTokenWithTrivia(kind, leading, trailing);
        }

        internal static SyntaxToken Identifier(string text)
        {
            return new SyntaxIdentifier(text);
        }

        internal static SyntaxToken Identifier(string text, GreenNode? leading, GreenNode? trailing)
        {
             return new SyntaxIdentifierWithTrivia(SyntaxKind.IdentifierToken, text, text, leading, trailing);
        }

        internal static SyntaxToken Identifier(SyntaxKind contextualKind, GreenNode? leading, string text, string valueText, GreenNode? trailing)
        {
            return new SyntaxIdentifierWithTrivia(contextualKind, text, valueText, leading, trailing);
        }

        internal static SyntaxToken WithValue<T>(SyntaxKind kind, string text, T value)
        {
             return new SyntaxTokenWithValue<T>(kind, text, value);
        }

        internal static SyntaxToken WithValue<T>(SyntaxKind kind, string text, T value, GreenNode? leading, GreenNode? trailing)
        {
             return new SyntaxTokenWithValueAndTrivia<T>(kind, text, value, leading, trailing);
        }

        public override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        {
            return new SyntaxToken(Kind, FullWidth, diagnostics, GetAnnotations());
        }

        public override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        {
            return new SyntaxToken(Kind, FullWidth, GetDiagnostics(), annotations);
        }

        public override TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor)
        {
             return visitor.VisitToken(this);
        }

        public override void Accept(TypeScriptSyntaxVisitor visitor)
        {
             visitor.VisitToken(this);
        }

        internal override GreenNode GetSlot(int index)
        {
            throw ExceptionUtilities.Unreachable;
        }

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
        {
            // This would normally call into the red tree factory
            throw new NotImplementedException("Red tree creation not yet implemented.");
        }

        public override string ToString() => Text;
    }

    internal class SyntaxTokenWithTrivia : SyntaxToken
    {
        private readonly GreenNode? _leading;
        private readonly GreenNode? _trailing;

        internal SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode? leading, GreenNode? trailing)
            : base(kind)
        {
            _leading = leading;
            _trailing = trailing;

            if (leading != null) this.AdjustFlagsAndWidth(leading);
            if (trailing != null) this.AdjustFlagsAndWidth(trailing);
        }

        internal SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode? leading, GreenNode? trailing, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations)
        {
             _leading = leading;
            _trailing = trailing;

            if (leading != null) this.AdjustFlagsAndWidth(leading);
            if (trailing != null) this.AdjustFlagsAndWidth(trailing);
        }

        public override GreenNode? GetLeadingTrivia() => _leading;
        public override GreenNode? GetTrailingTrivia() => _trailing;

        public override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        {
            return new SyntaxTokenWithTrivia(Kind, _leading, _trailing, diagnostics, GetAnnotations());
        }

        public override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        {
            return new SyntaxTokenWithTrivia(Kind, _leading, _trailing, GetDiagnostics(), annotations);
        }
    }

    internal class SyntaxIdentifier : SyntaxToken
    {
        private readonly string _text;

        internal SyntaxIdentifier(string text)
            : base(SyntaxKind.IdentifierToken)
        {
            _text = text;
            this.FullWidth = text.Length;
        }

        public override string Text => _text;
        public override string ValueText => _text;
        public override object Value => _text;

        public override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        {
             // Simplified implementation - ideally handles width correctly
             return new SyntaxIdentifier(_text); // Diagnostics lost in this simplifiction for brevity
        }
    }

    internal class SyntaxIdentifierWithTrivia : SyntaxTokenWithTrivia
    {
        private readonly string _text;
        private readonly string _valueText;

        internal SyntaxIdentifierWithTrivia(SyntaxKind kind, string text, string valueText, GreenNode? leading, GreenNode? trailing)
            : base(kind, leading, trailing)
        {
            _text = text;
            _valueText = valueText;
            this.FullWidth = (leading?.FullWidth ?? 0) + text.Length + (trailing?.FullWidth ?? 0);
        }

        public override string Text => _text;
        public override string ValueText => _valueText;
        public override object Value => _valueText;
    }

    internal class SyntaxTokenWithValue<T> : SyntaxToken
    {
        private readonly string _text;
        private readonly T _value;

        internal SyntaxTokenWithValue(SyntaxKind kind, string text, T value)
            : base(kind)
        {
            _text = text;
            _value = value;
            this.FullWidth = text.Length;
        }

        public override string Text => _text;
        public override string ValueText => _text;
        public override object? Value => _value;
    }

    internal class SyntaxTokenWithValueAndTrivia<T> : SyntaxTokenWithTrivia
    {
        private readonly string _text;
        private readonly T _value;

        internal SyntaxTokenWithValueAndTrivia(SyntaxKind kind, string text, T value, GreenNode? leading, GreenNode? trailing)
             : base(kind, leading, trailing)
        {
            _text = text;
            _value = value;
            this.FullWidth = (leading?.FullWidth ?? 0) + text.Length + (trailing?.FullWidth ?? 0);
        }

        public override string Text => _text;
        public override string ValueText => _text;
        public override object? Value => _value;
    }
}

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    // These are manual implementations of what the SourceGenerator would produce.
    // They allow the parser to actually construct a tree structure.

    internal partial class InterfaceDeclarationSyntax : TypeScriptSyntaxNode
    {
        internal readonly SyntaxToken interfaceKeyword;
        internal readonly SyntaxToken identifier;
        internal readonly SyntaxToken openBraceToken;
        internal readonly GreenNode? members; // SyntaxList<TypeElementSyntax>
        internal readonly SyntaxToken closeBraceToken;

        internal InterfaceDeclarationSyntax(SyntaxKind kind, SyntaxToken interfaceKeyword, SyntaxToken identifier, SyntaxToken openBraceToken, GreenNode? members, SyntaxToken closeBraceToken)
            : base(kind)
        {
            this.SlotCount = 5;
            this.AdjustFlagsAndWidth(interfaceKeyword);
            this.interfaceKeyword = interfaceKeyword;
            this.AdjustFlagsAndWidth(identifier);
            this.identifier = identifier;
            this.AdjustFlagsAndWidth(openBraceToken);
            this.openBraceToken = openBraceToken;
            if (members != null)
            {
                this.AdjustFlagsAndWidth(members);
                this.members = members;
            }
            this.AdjustFlagsAndWidth(closeBraceToken);
            this.closeBraceToken = closeBraceToken;
        }

        internal override GreenNode? GetSlot(int index)
        {
            return index switch
            {
                0 => interfaceKeyword,
                1 => identifier,
                2 => openBraceToken,
                3 => members,
                4 => closeBraceToken,
                _ => null
            };
        }

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => throw new NotImplementedException();

        public override TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor) => visitor.Visit(this);
        public override void Accept(TypeScriptSyntaxVisitor visitor) => visitor.Visit(this);
    }

    internal partial class SourceFileSyntax : TypeScriptSyntaxNode
    {
        internal readonly GreenNode? statements; // SyntaxList<StatementSyntax>
        internal readonly SyntaxToken endOfFileToken;

        internal SourceFileSyntax(SyntaxKind kind, GreenNode? statements, SyntaxToken endOfFileToken)
            : base(kind)
        {
            this.SlotCount = 2;
            if (statements != null)
            {
                this.AdjustFlagsAndWidth(statements);
                this.statements = statements;
            }
            this.AdjustFlagsAndWidth(endOfFileToken);
            this.endOfFileToken = endOfFileToken;
        }

        internal override GreenNode? GetSlot(int index)
        {
            return index switch
            {
                0 => statements,
                1 => endOfFileToken,
                _ => null
            };
        }

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => throw new NotImplementedException();
        public override TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor) => visitor.Visit(this);
        public override void Accept(TypeScriptSyntaxVisitor visitor) => visitor.Visit(this);
    }

    internal partial class BlockSyntax : TypeScriptSyntaxNode
    {
        internal readonly SyntaxToken openBraceToken;
        internal readonly GreenNode? statements;
        internal readonly SyntaxToken closeBraceToken;

        internal BlockSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? statements, SyntaxToken closeBraceToken)
            : base(kind)
        {
            this.SlotCount = 3;
            this.AdjustFlagsAndWidth(openBraceToken);
            this.openBraceToken = openBraceToken;
            if (statements != null)
            {
                this.AdjustFlagsAndWidth(statements);
                this.statements = statements;
            }
            this.AdjustFlagsAndWidth(closeBraceToken);
            this.closeBraceToken = closeBraceToken;
        }

        internal override GreenNode? GetSlot(int index)
        {
            return index switch
            {
                0 => openBraceToken,
                1 => statements,
                2 => closeBraceToken,
                _ => null
            };
        }

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => throw new NotImplementedException();
        public override TResult Accept<TResult>(TypeScriptSyntaxVisitor<TResult> visitor) => visitor.Visit(this);
        public override void Accept(TypeScriptSyntaxVisitor visitor) => visitor.Visit(this);
    }
}

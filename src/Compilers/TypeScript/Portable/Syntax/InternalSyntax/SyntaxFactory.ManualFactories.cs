using System;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal static partial class SyntaxFactory
    {
        public static SourceFileSyntax SourceFile(SyntaxList<TypeScriptSyntaxNode> statements, SyntaxToken eof)
        {
            return new SourceFileSyntax(SyntaxKind.SourceFile, statements.Node, eof);
        }

        public static BlockSyntax Block(SyntaxToken open, SyntaxList<TypeScriptSyntaxNode> statements, SyntaxToken close)
        {
            return new BlockSyntax(SyntaxKind.Block, open, statements.Node, close);
        }

        public static InterfaceDeclarationSyntax InterfaceDeclaration(SyntaxToken keyword, SyntaxToken id, SyntaxToken open, SyntaxList<TypeScriptSyntaxNode> members, SyntaxToken close)
        {
            return new InterfaceDeclarationSyntax(SyntaxKind.InterfaceDeclaration, keyword, id, open, members.Node, close);
        }
    }
}

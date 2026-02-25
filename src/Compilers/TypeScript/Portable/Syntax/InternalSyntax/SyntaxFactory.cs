// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal static partial class SyntaxFactory
    {
        public static SyntaxTrivia Whitespace(string text)
        {
            return new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, text);
        }

        public static SyntaxTrivia EndOfLine(string text)
        {
            return new SyntaxTrivia(SyntaxKind.EndOfLineTrivia, text);
        }

        public static SyntaxTrivia Comment(string text)
        {
            if (text.StartsWith("//"))
            {
                return new SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, text);
            }
            return new SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, text);
        }

        public static SyntaxToken Token(SyntaxKind kind)
        {
            return SyntaxToken.Create(kind);
        }

        public static SyntaxToken Identifier(string text)
        {
            return SyntaxToken.Identifier(text);
        }

        public static SyntaxToken Token(string text)
        {
             // Try to find keyword
             var kind = SyntaxFacts.GetKeywordKind(text);
             if (kind != SyntaxKind.IdentifierToken && kind != SyntaxKind.None)
             {
                 return Token(kind);
             }
             return Identifier(text);
        }
    }
}

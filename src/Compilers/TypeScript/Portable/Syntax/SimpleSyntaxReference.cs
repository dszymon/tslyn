// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax
{
    internal class SimpleSyntaxReference : SyntaxReference
    {
        private readonly SyntaxNode _node;

        public SimpleSyntaxReference(SyntaxNode node)
        {
            _node = node;
        }

        public override SyntaxTree SyntaxTree => _node.SyntaxTree;

        public override TextSpan Span => _node.Span;

        public override SyntaxNode GetSyntax(CancellationToken cancellationToken = default)
        {
            return _node;
        }
    }
}

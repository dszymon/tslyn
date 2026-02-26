// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax
{
    public abstract class TypeScriptSyntaxNode : SyntaxNode
    {
        internal TypeScriptSyntaxNode(InternalSyntax.TypeScriptSyntaxNode green, SyntaxNode? parent, int position)
            : base(green, parent, position)
        {
        }

        public override string Language => "TypeScript";

        public SyntaxKind Kind => (SyntaxKind)this.Green.RawKind;

        internal override SyntaxNode? GetCachedSlot(int index)
        {
            return null;
        }

        internal override SyntaxNode? GetNodeSlot(int index)
        {
            return null;
        }

        public abstract TResult Accept<TResult>(Microsoft.CodeAnalysis.TypeScript.TypeScriptSyntaxVisitor<TResult> visitor);
        public abstract void Accept(Microsoft.CodeAnalysis.TypeScript.TypeScriptSyntaxVisitor visitor);

        protected override SyntaxTree SyntaxTreeCore
        {
            get
            {
                return this._syntaxTree ?? ComputeSyntaxTree(this);
            }
        }

        private static SyntaxTree ComputeSyntaxTree(TypeScriptSyntaxNode node)
        {
            var parent = node.Parent;
            if (parent != null)
            {
                var tree = parent.SyntaxTree;
                if (tree != null)
                {
                    node._syntaxTree = tree;
                    return tree;
                }
            }

            // Root
            var newTree = TypeScriptSyntaxTree.CreateWithoutClone(node);
            Interlocked.CompareExchange(ref node._syntaxTree, newTree, null);
            return node._syntaxTree;
        }

        protected internal override SyntaxNode NormalizeWhitespaceCore(string indentation, string eol, bool elasticTrivia)
        {
            return this; // TODO
        }

        protected internal override SyntaxNode ReplaceCore<TNode>(IEnumerable<TNode>? nodes, Func<TNode, TNode, SyntaxNode>? computeReplacementNode, IEnumerable<SyntaxToken>? tokens, Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken, IEnumerable<SyntaxTrivia>? trivia, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia)
        {
            throw new NotImplementedException();
        }

        protected internal override SyntaxNode ReplaceNodeInListCore(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes)
        {
            throw new NotImplementedException();
        }

        protected internal override SyntaxNode InsertNodesInListCore(SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore)
        {
            throw new NotImplementedException();
        }

        protected internal override SyntaxNode ReplaceTokenInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens)
        {
            throw new NotImplementedException();
        }

        protected internal override SyntaxNode InsertTokensInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, bool insertBefore)
        {
            throw new NotImplementedException();
        }

        protected internal override SyntaxNode ReplaceTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia)
        {
            throw new NotImplementedException();
        }

        protected internal override SyntaxNode InsertTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore)
        {
            throw new NotImplementedException();
        }

        protected internal override SyntaxNode RemoveNodesCore(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options)
        {
            throw new NotImplementedException();
        }

        protected override bool IsEquivalentToCore(SyntaxNode node, bool topLevel = false)
        {
            return false; // TODO
        }

        public new TypeScriptSyntaxNode WithAnnotations(IEnumerable<SyntaxAnnotation> annotations)
        {
            return (TypeScriptSyntaxNode)this.WithAdditionalAnnotationsInternal(annotations);
        }
    }
}

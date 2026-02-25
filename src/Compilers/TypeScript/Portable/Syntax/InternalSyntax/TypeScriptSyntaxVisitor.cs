using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal abstract class TypeScriptSyntaxVisitor<TResult>
    {
        public virtual TResult Visit(TypeScriptSyntaxNode node)
        {
            if (node == null)
            {
                return default;
            }

            return node.Accept(this);
        }

        public virtual TResult DefaultVisit(TypeScriptSyntaxNode node)
        {
            return default;
        }

        public virtual TResult VisitToken(SyntaxToken token)
        {
            return DefaultVisit(token);
        }

        public virtual TResult VisitTrivia(SyntaxTrivia trivia)
        {
            return DefaultVisit(trivia);
        }
    }

    internal abstract class TypeScriptSyntaxVisitor
    {
        public virtual void Visit(TypeScriptSyntaxNode node)
        {
            if (node != null)
            {
                node.Accept(this);
            }
        }

        public virtual void DefaultVisit(TypeScriptSyntaxNode node)
        {
        }

        public virtual void VisitToken(SyntaxToken token)
        {
            DefaultVisit(token);
        }

        public virtual void VisitTrivia(SyntaxTrivia trivia)
        {
            DefaultVisit(trivia);
        }
    }
}

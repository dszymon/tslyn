// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.TypeScript.Syntax;

namespace Microsoft.CodeAnalysis.TypeScript
{
    public abstract class TypeScriptSyntaxTree : SyntaxTree
    {
        public static SyntaxTree ParseText(string text, TypeScriptParseOptions? options = null, string path = "", CancellationToken cancellationToken = default)
        {
            return ParseText(SourceText.From(text), options, path, cancellationToken);
        }

        public static SyntaxTree ParseText(SourceText text, TypeScriptParseOptions? options = null, string path = "", CancellationToken cancellationToken = default)
        {
            return ParsedSyntaxTree.Create(text, options ?? TypeScriptParseOptions.Default, path, cancellationToken);
        }

        internal static SyntaxTree CreateWithoutClone(TypeScriptSyntaxNode root)
        {
            return new ParsedSyntaxTree(root);
        }

        public override bool HasCompilationUnitRoot => true;

        public new TypeScriptParseOptions Options => (TypeScriptParseOptions)this.OptionsCore;
        protected abstract override ParseOptions OptionsCore { get; }

        public override LineVisibility GetLineVisibility(int line, CancellationToken cancellationToken = default) => LineVisibility.Visible;

        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode? node) => new List<Diagnostic>();
        public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken) => new List<Diagnostic>();
        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token) => new List<Diagnostic>();
        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia) => new List<Diagnostic>();
        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken) => new List<Diagnostic>();

        public override Location GetLocation(TextSpan span)
        {
            return new SourceLocation(this, span);
        }

        public new TypeScriptSyntaxNode GetRoot(CancellationToken cancellationToken = default) => (TypeScriptSyntaxNode)GetRootCore(cancellationToken);

        protected abstract override SyntaxNode GetRootCore(CancellationToken cancellationToken);

        protected abstract override bool TryGetRootCore(out SyntaxNode? root);

        // Internal implementation
        internal class ParsedSyntaxTree : TypeScriptSyntaxTree
        {
            private readonly SourceText? _text;
            private readonly TypeScriptParseOptions _options;
            private readonly string _path;
            private TypeScriptSyntaxNode? _root;

            internal static ParsedSyntaxTree Create(SourceText text, TypeScriptParseOptions options, string path, CancellationToken cancellationToken)
            {
                var tree = new ParsedSyntaxTree(text, options, path);
                tree.GetRoot(cancellationToken); // Parse immediately
                return tree;
            }

            internal ParsedSyntaxTree(SourceText text, TypeScriptParseOptions options, string path)
            {
                _text = text;
                _options = options;
                _path = path;
            }

            internal ParsedSyntaxTree(TypeScriptSyntaxNode root)
            {
                _root = root;
                _options = TypeScriptParseOptions.Default;
                _path = "";
                _text = null;
            }

            public override string FilePath => _path;

            public override SourceText GetText(CancellationToken cancellationToken = default)
            {
                if (_text != null) return _text;
                return SourceText.From(_root!.ToFullString());
            }

            public override bool TryGetText(out SourceText? text)
            {
                text = _text;
                return text != null;
            }

            public override Encoding? Encoding => _text?.Encoding;

            public override int Length => _text?.Length ?? _root?.FullWidth ?? 0;

            protected override ParseOptions OptionsCore => _options;

            protected override SyntaxNode GetRootCore(CancellationToken cancellationToken)
            {
                if (_root == null)
                {
                    var root = ParseRoot();
                    Interlocked.CompareExchange(ref _root, root, null);
                }
                return _root;
            }

            protected override Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken)
            {
                return Task.FromResult<SyntaxNode>(GetRootCore(cancellationToken));
            }

            private TypeScriptSyntaxNode ParseRoot()
            {
                 using (var lexer = new Syntax.InternalSyntax.Lexer(_text!))
                 using (var parser = new Syntax.InternalSyntax.LanguageParser(lexer))
                 {
                     var green = parser.ParseCompilationUnit();
                     var red = (TypeScriptSyntaxNode)green.CreateRed(null, 0);
                     var root = (TypeScriptSyntaxNode)SyntaxNode.CloneNodeAsRoot(red, this);
                     return root;
                 }
            }

            protected override bool TryGetRootCore(out SyntaxNode? root)
            {
                if (_root != null)
                {
                    root = _root;
                    return true;
                }
                root = null;
                return false;
            }

            public override SyntaxReference GetReference(SyntaxNode node)
            {
                return new SimpleSyntaxReference(node);
            }

            public override SyntaxTree WithChangedText(SourceText newText)
            {
                return new ParsedSyntaxTree(newText, _options, _path);
            }

            public override SyntaxTree WithFilePath(string path)
            {
                return new ParsedSyntaxTree(_text ?? SourceText.From(""), _options, path); // Partial support
            }

            public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
            {
                return new ParsedSyntaxTree((TypeScriptSyntaxNode)root); // Ignore options for now or update
            }

            public override bool HasHiddenRegions() => false;

            public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default)
            {
                return new FileLinePositionSpan(_path, GetText().Lines.GetLinePositionSpan(span));
            }

            public override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default)
            {
                return GetLineSpan(span, cancellationToken);
            }

            public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false)
            {
                return false; // TODO
            }

            public override IEnumerable<LineMapping> GetLineMappings(CancellationToken cancellationToken = default)
            {
                return new List<LineMapping>();
            }

            public override IList<TextChange> GetChanges(SyntaxTree oldTree)
            {
                return new List<TextChange>();
            }

            public override IList<TextSpan> GetChangedSpans(SyntaxTree oldTree)
            {
                return new List<TextSpan>();
            }
        }
    }
}

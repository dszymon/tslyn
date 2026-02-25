// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.TypeScript
{
    public sealed class TypeScriptParseOptions : ParseOptions, IEquatable<TypeScriptParseOptions>
    {
        public static TypeScriptParseOptions Default { get; } = new TypeScriptParseOptions();

        public TypeScriptParseOptions(DocumentationMode documentationMode = DocumentationMode.Parse, SourceCodeKind kind = SourceCodeKind.Regular)
            : base(kind, documentationMode)
        {
        }

        public override string Language => "TypeScript";

        public override IEnumerable<string> PreprocessorSymbolNames => ImmutableArray<string>.Empty;

        internal override void ValidateOptions(ArrayBuilder<Diagnostic> builder)
        {
            // TODO implementation
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TypeScriptParseOptions);
        }

        public bool Equals(TypeScriptParseOptions? other)
        {
            if (other == null) return false;
            // Base ParseOptions equality check if possible, or just check properties
            if (this.Kind != other.Kind) return false;
            if (this.DocumentationMode != other.DocumentationMode) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return (int)Kind ^ (int)DocumentationMode;
        }

        public override ParseOptions CommonWithKind(SourceCodeKind kind)
        {
            return new TypeScriptParseOptions(DocumentationMode, kind);
        }

        protected override ParseOptions CommonWithDocumentationMode(DocumentationMode documentationMode)
        {
            return new TypeScriptParseOptions(documentationMode, Kind);
        }

        protected override ParseOptions CommonWithFeatures(IEnumerable<KeyValuePair<string, string>> features)
        {
            return this;
        }

        public override IReadOnlyDictionary<string, string> Features => new Dictionary<string, string>();
    }
}

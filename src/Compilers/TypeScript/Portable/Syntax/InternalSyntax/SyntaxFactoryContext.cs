// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal struct SyntaxFactoryContext
    {
        public bool IsInAsync { get; set; }
        public bool IsInQuery { get; set; }
        // Add more as needed
    }
}

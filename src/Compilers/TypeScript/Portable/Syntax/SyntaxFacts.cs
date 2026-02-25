// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.TypeScript
{
    public static class SyntaxFacts
    {
        public static bool IsWhitespace(char c)
        {
            // TODO: Add full unicode whitespace support
            return c == ' ' || c == '\t' || c == '\v' || c == '\f';
        }

        public static bool IsNewLine(char c)
        {
            return c == '\r' || c == '\n'; // Add other unicode newlines
        }

        public static bool IsIdentifierStartCharacter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c == '$';
        }

        public static bool IsIdentifierPartCharacter(char c)
        {
            return IsIdentifierStartCharacter(c) || (c >= '0' && c <= '9');
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.IfKeyword: return "if";
                case SyntaxKind.ElseKeyword: return "else";
                case SyntaxKind.ForKeyword: return "for";
                case SyntaxKind.WhileKeyword: return "while";
                case SyntaxKind.DoKeyword: return "do";
                case SyntaxKind.FunctionKeyword: return "function";
                case SyntaxKind.VarKeyword: return "var";
                case SyntaxKind.LetKeyword: return "let";
                case SyntaxKind.ConstKeyword: return "const";
                case SyntaxKind.ReturnKeyword: return "return";
                case SyntaxKind.TrueKeyword: return "true";
                case SyntaxKind.FalseKeyword: return "false";
                case SyntaxKind.NullKeyword: return "null";
                case SyntaxKind.UndefinedKeyword: return "undefined";
                case SyntaxKind.BreakKeyword: return "break";
                case SyntaxKind.CaseKeyword: return "case";
                case SyntaxKind.CatchKeyword: return "catch";
                case SyntaxKind.ClassKeyword: return "class";
                case SyntaxKind.ContinueKeyword: return "continue";
                case SyntaxKind.DebuggerKeyword: return "debugger";
                case SyntaxKind.DefaultKeyword: return "default";
                case SyntaxKind.DeleteKeyword: return "delete";
                case SyntaxKind.EnumKeyword: return "enum";
                case SyntaxKind.ExportKeyword: return "export";
                case SyntaxKind.ExtendsKeyword: return "extends";
                case SyntaxKind.FinallyKeyword: return "finally";
                case SyntaxKind.ImportKeyword: return "import";
                case SyntaxKind.InKeyword: return "in";
                case SyntaxKind.InstanceOfKeyword: return "instanceof";
                case SyntaxKind.NewKeyword: return "new";
                case SyntaxKind.SuperKeyword: return "super";
                case SyntaxKind.SwitchKeyword: return "switch";
                case SyntaxKind.ThisKeyword: return "this";
                case SyntaxKind.ThrowKeyword: return "throw";
                case SyntaxKind.TryKeyword: return "try";
                case SyntaxKind.TypeOfKeyword: return "typeof";
                case SyntaxKind.VoidKeyword: return "void";
                case SyntaxKind.WithKeyword: return "with";

                // Strict mode
                case SyntaxKind.ImplementsKeyword: return "implements";
                case SyntaxKind.InterfaceKeyword: return "interface";
                case SyntaxKind.PackageKeyword: return "package";
                case SyntaxKind.PrivateKeyword: return "private";
                case SyntaxKind.ProtectedKeyword: return "protected";
                case SyntaxKind.PublicKeyword: return "public";
                case SyntaxKind.StaticKeyword: return "static";
                case SyntaxKind.YieldKeyword: return "yield";

                // Contextual
                case SyntaxKind.AbstractKeyword: return "abstract";
                case SyntaxKind.AsKeyword: return "as";
                case SyntaxKind.AsyncKeyword: return "async";
                case SyntaxKind.AwaitKeyword: return "await";
                case SyntaxKind.ConstructorKeyword: return "constructor";
                case SyntaxKind.DeclareKeyword: return "declare";
                case SyntaxKind.GetKeyword: return "get";
                case SyntaxKind.InferKeyword: return "infer";
                case SyntaxKind.IsKeyword: return "is";
                case SyntaxKind.KeyOfKeyword: return "keyof";
                case SyntaxKind.ModuleKeyword: return "module";
                case SyntaxKind.NamespaceKeyword: return "namespace";
                case SyntaxKind.NeverKeyword: return "never";
                case SyntaxKind.ReadOnlyKeyword: return "readonly";
                case SyntaxKind.RequireKeyword: return "require";
                case SyntaxKind.NumberKeyword: return "number";
                case SyntaxKind.ObjectKeyword: return "object";
                case SyntaxKind.SetKeyword: return "set";
                case SyntaxKind.StringKeyword: return "string";
                case SyntaxKind.SymbolKeyword: return "symbol";
                case SyntaxKind.TypeKeyword: return "type";
                case SyntaxKind.UniqueKeyword: return "unique";
                case SyntaxKind.UnknownKeyword: return "unknown";
                case SyntaxKind.FromKeyword: return "from";
                case SyntaxKind.GlobalKeyword: return "global";
                case SyntaxKind.BigIntKeyword: return "bigint";
                case SyntaxKind.OfKeyword: return "of";
                case SyntaxKind.BooleanKeyword: return "boolean";
                case SyntaxKind.AnyKeyword: return "any";

                case SyntaxKind.OpenParenToken: return "(";
                case SyntaxKind.CloseParenToken: return ")";
                case SyntaxKind.OpenBraceToken: return "{";
                case SyntaxKind.CloseBraceToken: return "}";
                case SyntaxKind.OpenBracketToken: return "[";
                case SyntaxKind.CloseBracketToken: return "]";
                case SyntaxKind.SemicolonToken: return ";";
                case SyntaxKind.CommaToken: return ",";
                case SyntaxKind.DotToken: return ".";
                case SyntaxKind.PlusToken: return "+";
                case SyntaxKind.MinusToken: return "-";
                case SyntaxKind.AsteriskToken: return "*";
                case SyntaxKind.SlashToken: return "/";
                case SyntaxKind.EqualsToken: return "=";
                case SyntaxKind.EqualsEqualsToken: return "==";
                case SyntaxKind.ExclamationToken: return "!";
                case SyntaxKind.ExclamationEqualsToken: return "!=";
                case SyntaxKind.TildeToken: return "~";
                case SyntaxKind.PercentToken: return "%";
                case SyntaxKind.AmpersandToken: return "&";
                case SyntaxKind.BarToken: return "|";
                case SyntaxKind.ColonToken: return ":";
                case SyntaxKind.LessThanToken: return "<";
                case SyntaxKind.GreaterThanToken: return ">";
                case SyntaxKind.QuestionToken: return "?";
                case SyntaxKind.PlusPlusToken: return "++";
                case SyntaxKind.MinusMinusToken: return "--";
                case SyntaxKind.PlusEqualsToken: return "+=";
                case SyntaxKind.MinusEqualsToken: return "-=";
                case SyntaxKind.AsteriskEqualsToken: return "*=";
                case SyntaxKind.SlashEqualsToken: return "/=";
                case SyntaxKind.PercentEqualsToken: return "%=";
                case SyntaxKind.AmpersandAmpersandToken: return "&&";
                case SyntaxKind.BarBarToken: return "||";
                case SyntaxKind.AmpersandEqualsToken: return "&=";
                case SyntaxKind.BarEqualsToken: return "|=";
                case SyntaxKind.CaretToken: return "^";
                case SyntaxKind.CaretEqualsToken: return "^=";
                case SyntaxKind.EqualsGreaterThanToken: return "=>";
                case SyntaxKind.LessThanEqualsToken: return "<=";
                case SyntaxKind.GreaterThanEqualsToken: return ">=";
                case SyntaxKind.EndOfFileToken: return "";
                default: return string.Empty;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "if": return SyntaxKind.IfKeyword;
                case "else": return SyntaxKind.ElseKeyword;
                case "for": return SyntaxKind.ForKeyword;
                case "while": return SyntaxKind.WhileKeyword;
                case "do": return SyntaxKind.DoKeyword;
                case "function": return SyntaxKind.FunctionKeyword;
                case "var": return SyntaxKind.VarKeyword;
                case "let": return SyntaxKind.LetKeyword;
                case "const": return SyntaxKind.ConstKeyword;
                case "return": return SyntaxKind.ReturnKeyword;
                case "true": return SyntaxKind.TrueKeyword;
                case "false": return SyntaxKind.FalseKeyword;
                case "null": return SyntaxKind.NullKeyword;
                case "undefined": return SyntaxKind.UndefinedKeyword;
                case "break": return SyntaxKind.BreakKeyword;
                case "case": return SyntaxKind.CaseKeyword;
                case "catch": return SyntaxKind.CatchKeyword;
                case "class": return SyntaxKind.ClassKeyword;
                case "continue": return SyntaxKind.ContinueKeyword;
                case "debugger": return SyntaxKind.DebuggerKeyword;
                case "default": return SyntaxKind.DefaultKeyword;
                case "delete": return SyntaxKind.DeleteKeyword;
                case "enum": return SyntaxKind.EnumKeyword;
                case "export": return SyntaxKind.ExportKeyword;
                case "extends": return SyntaxKind.ExtendsKeyword;
                case "finally": return SyntaxKind.FinallyKeyword;
                case "import": return SyntaxKind.ImportKeyword;
                case "in": return SyntaxKind.InKeyword;
                case "instanceof": return SyntaxKind.InstanceOfKeyword;
                case "new": return SyntaxKind.NewKeyword;
                case "super": return SyntaxKind.SuperKeyword;
                case "switch": return SyntaxKind.SwitchKeyword;
                case "this": return SyntaxKind.ThisKeyword;
                case "throw": return SyntaxKind.ThrowKeyword;
                case "try": return SyntaxKind.TryKeyword;
                case "typeof": return SyntaxKind.TypeOfKeyword;
                case "void": return SyntaxKind.VoidKeyword;
                case "with": return SyntaxKind.WithKeyword;
                // Strict mode
                case "implements": return SyntaxKind.ImplementsKeyword;
                case "interface": return SyntaxKind.InterfaceKeyword;
                case "package": return SyntaxKind.PackageKeyword;
                case "private": return SyntaxKind.PrivateKeyword;
                case "protected": return SyntaxKind.ProtectedKeyword;
                case "public": return SyntaxKind.PublicKeyword;
                case "static": return SyntaxKind.StaticKeyword;
                case "yield": return SyntaxKind.YieldKeyword;
                // Contextual
                case "abstract": return SyntaxKind.AbstractKeyword;
                case "as": return SyntaxKind.AsKeyword;
                case "async": return SyntaxKind.AsyncKeyword;
                case "await": return SyntaxKind.AwaitKeyword;
                case "constructor": return SyntaxKind.ConstructorKeyword;
                case "declare": return SyntaxKind.DeclareKeyword;
                case "get": return SyntaxKind.GetKeyword;
                case "infer": return SyntaxKind.InferKeyword;
                case "is": return SyntaxKind.IsKeyword;
                case "keyof": return SyntaxKind.KeyOfKeyword;
                case "module": return SyntaxKind.ModuleKeyword;
                case "namespace": return SyntaxKind.NamespaceKeyword;
                case "never": return SyntaxKind.NeverKeyword;
                case "readonly": return SyntaxKind.ReadOnlyKeyword;
                case "require": return SyntaxKind.RequireKeyword;
                case "number": return SyntaxKind.NumberKeyword;
                case "object": return SyntaxKind.ObjectKeyword;
                case "set": return SyntaxKind.SetKeyword;
                case "string": return SyntaxKind.StringKeyword;
                case "symbol": return SyntaxKind.SymbolKeyword;
                case "type": return SyntaxKind.TypeKeyword;
                case "unique": return SyntaxKind.UniqueKeyword;
                case "unknown": return SyntaxKind.UnknownKeyword;
                case "from": return SyntaxKind.FromKeyword;
                case "global": return SyntaxKind.GlobalKeyword;
                case "bigint": return SyntaxKind.BigIntKeyword;
                case "of": return SyntaxKind.OfKeyword;
                case "boolean": return SyntaxKind.BooleanKeyword;
                case "any": return SyntaxKind.AnyKeyword;

                default: return SyntaxKind.IdentifierToken;
            }
        }

        public static bool IsDocumentationCommentTrivia(SyntaxKind kind)
        {
            return kind == SyntaxKind.SingleLineCommentTrivia || kind == SyntaxKind.MultiLineCommentTrivia; // Approximation
        }
    }
}

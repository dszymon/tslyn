namespace Microsoft.CodeAnalysis.TypeScript
{
    internal static class SyntaxFacts
    {
        public static bool IsAnyToken(SyntaxKind kind)
        {
            if (kind >= SyntaxKind.ExclamationToken && kind <= SyntaxKind.OfKeyword)
                return true;
            if (kind >= SyntaxKind.IdentifierToken && kind <= SyntaxKind.NoSubstitutionTemplateToken)
                return true;
            return false;
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.ExclamationToken: return "!";
                case SyntaxKind.PercentToken: return "%";
                case SyntaxKind.AmpersandToken: return "&";
                case SyntaxKind.OpenParenToken: return "(";
                case SyntaxKind.CloseParenToken: return ")";
                case SyntaxKind.AsteriskToken: return "*";
                case SyntaxKind.PlusToken: return "+";
                case SyntaxKind.CommaToken: return ",";
                case SyntaxKind.MinusToken: return "-";
                case SyntaxKind.DotToken: return ".";
                case SyntaxKind.SlashToken: return "/";
                case SyntaxKind.ColonToken: return ":";
                case SyntaxKind.SemicolonToken: return ";";
                case SyntaxKind.LessThanToken: return "<";
                case SyntaxKind.EqualsToken: return "=";
                case SyntaxKind.GreaterThanToken: return ">";
                case SyntaxKind.QuestionToken: return "?";
                case SyntaxKind.AtToken: return "@";
                case SyntaxKind.OpenBracketToken: return "[";
                case SyntaxKind.CloseBracketToken: return "]";
                case SyntaxKind.CaretToken: return "^";
                case SyntaxKind.OpenBraceToken: return "{";
                case SyntaxKind.BarToken: return "|";
                case SyntaxKind.CloseBraceToken: return "}";
                case SyntaxKind.TildeToken: return "~";

                // Compound
                case SyntaxKind.ExclamationEqualsToken: return "!=";
                case SyntaxKind.ExclamationEqualsEqualsToken: return "!==";
                case SyntaxKind.PercentEqualsToken: return "%=";
                case SyntaxKind.AmpersandAmpersandToken: return "&&";
                case SyntaxKind.AmpersandEqualsToken: return "&=";
                case SyntaxKind.AsteriskAsteriskToken: return "**";
                case SyntaxKind.AsteriskAsteriskEqualsToken: return "**=";
                case SyntaxKind.AsteriskEqualsToken: return "*=";
                case SyntaxKind.PlusPlusToken: return "++";
                case SyntaxKind.PlusEqualsToken: return "+=";
                case SyntaxKind.MinusMinusToken: return "--";
                case SyntaxKind.MinusEqualsToken: return "-=";
                case SyntaxKind.DotDotDotToken: return "...";
                case SyntaxKind.SlashEqualsToken: return "/=";
                case SyntaxKind.LessThanLessThanToken: return "<<";
                case SyntaxKind.LessThanLessThanEqualsToken: return "<<=";
                case SyntaxKind.LessThanEqualsToken: return "<=";
                case SyntaxKind.EqualsEqualsToken: return "==";
                case SyntaxKind.EqualsEqualsEqualsToken: return "===";
                case SyntaxKind.EqualsGreaterThanToken: return "=>";
                case SyntaxKind.GreaterThanEqualsToken: return ">=";
                case SyntaxKind.GreaterThanGreaterThanToken: return ">>";
                case SyntaxKind.GreaterThanGreaterThanEqualsToken: return ">>=";
                case SyntaxKind.GreaterThanGreaterThanGreaterThanToken: return ">>>";
                case SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken: return ">>>=";
                case SyntaxKind.QuestionQuestionToken: return "??";
                case SyntaxKind.QuestionQuestionEqualsToken: return "??=";
                case SyntaxKind.CaretEqualsToken: return "^=";
                case SyntaxKind.BarEqualsToken: return "|=";
                case SyntaxKind.BarBarToken: return "||";
                case SyntaxKind.BarBarEqualsToken: return "||=";
                case SyntaxKind.AmpersandAmpersandEqualsToken: return "&&=";

                // Keywords
                case SyntaxKind.BreakKeyword: return "break";
                case SyntaxKind.CaseKeyword: return "case";
                case SyntaxKind.CatchKeyword: return "catch";
                case SyntaxKind.ClassKeyword: return "class";
                case SyntaxKind.ConstKeyword: return "const";
                case SyntaxKind.ContinueKeyword: return "continue";
                case SyntaxKind.DebuggerKeyword: return "debugger";
                case SyntaxKind.DefaultKeyword: return "default";
                case SyntaxKind.DeleteKeyword: return "delete";
                case SyntaxKind.DoKeyword: return "do";
                case SyntaxKind.ElseKeyword: return "else";
                case SyntaxKind.EnumKeyword: return "enum";
                case SyntaxKind.ExportKeyword: return "export";
                case SyntaxKind.ExtendsKeyword: return "extends";
                case SyntaxKind.FalseKeyword: return "false";
                case SyntaxKind.FinallyKeyword: return "finally";
                case SyntaxKind.ForKeyword: return "for";
                case SyntaxKind.FunctionKeyword: return "function";
                case SyntaxKind.IfKeyword: return "if";
                case SyntaxKind.ImportKeyword: return "import";
                case SyntaxKind.InKeyword: return "in";
                case SyntaxKind.InstanceOfKeyword: return "instanceof";
                case SyntaxKind.NewKeyword: return "new";
                case SyntaxKind.NullKeyword: return "null";
                case SyntaxKind.ReturnKeyword: return "return";
                case SyntaxKind.SuperKeyword: return "super";
                case SyntaxKind.SwitchKeyword: return "switch";
                case SyntaxKind.ThisKeyword: return "this";
                case SyntaxKind.ThrowKeyword: return "throw";
                case SyntaxKind.TrueKeyword: return "true";
                case SyntaxKind.TryKeyword: return "try";
                case SyntaxKind.TypeOfKeyword: return "typeof";
                case SyntaxKind.VarKeyword: return "var";
                case SyntaxKind.VoidKeyword: return "void";
                case SyntaxKind.WhileKeyword: return "while";
                case SyntaxKind.WithKeyword: return "with";

                case SyntaxKind.ImplementsKeyword: return "implements";
                case SyntaxKind.InterfaceKeyword: return "interface";
                case SyntaxKind.LetKeyword: return "let";
                case SyntaxKind.PackageKeyword: return "package";
                case SyntaxKind.PrivateKeyword: return "private";
                case SyntaxKind.ProtectedKeyword: return "protected";
                case SyntaxKind.PublicKeyword: return "public";
                case SyntaxKind.StaticKeyword: return "static";
                case SyntaxKind.YieldKeyword: return "yield";

                case SyntaxKind.AbstractKeyword: return "abstract";
                case SyntaxKind.AsKeyword: return "as";
                case SyntaxKind.AssertsKeyword: return "asserts";
                case SyntaxKind.AnyKeyword: return "any";
                case SyntaxKind.AsyncKeyword: return "async";
                case SyntaxKind.AwaitKeyword: return "await";
                case SyntaxKind.BooleanKeyword: return "boolean";
                case SyntaxKind.ConstructorKeyword: return "constructor";
                case SyntaxKind.DeclareKeyword: return "declare";
                case SyntaxKind.GetKeyword: return "get";
                case SyntaxKind.InferKeyword: return "infer";
                case SyntaxKind.IsKeyword: return "is";
                case SyntaxKind.KeyOfKeyword: return "keyof";
                case SyntaxKind.ModuleKeyword: return "module";
                case SyntaxKind.NamespaceKeyword: return "namespace";
                case SyntaxKind.NeverKeyword: return "never";
                case SyntaxKind.ReadonlyKeyword: return "readonly";
                case SyntaxKind.RequireKeyword: return "require";
                case SyntaxKind.NumberKeyword: return "number";
                case SyntaxKind.ObjectKeyword: return "object";
                case SyntaxKind.SetKeyword: return "set";
                case SyntaxKind.StringKeyword: return "string";
                case SyntaxKind.SymbolKeyword: return "symbol";
                case SyntaxKind.TypeKeyword: return "type";
                case SyntaxKind.UndefinedKeyword: return "undefined";
                case SyntaxKind.UniqueKeyword: return "unique";
                case SyntaxKind.UnknownKeyword: return "unknown";
                case SyntaxKind.FromKeyword: return "from";
                case SyntaxKind.GlobalKeyword: return "global";
                case SyntaxKind.BigIntKeyword: return "bigint";
                case SyntaxKind.OverrideKeyword: return "override";
                case SyntaxKind.OfKeyword: return "of";

                default: return string.Empty;
            }
        }
    }
}

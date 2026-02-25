// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

namespace Microsoft.CodeAnalysis.TypeScript
{
    // DO NOT CHANGE NUMBERS ASSIGNED TO EXISTING KINDS OR YOU WILL BREAK BINARY COMPATIBILITY
    public enum SyntaxKind : ushort
    {
        None = 0,
        List = GreenNode.ListKind,

        // Punctuation
        /// <summary>Represents <c>!</c> token.</summary>
        ExclamationToken,
        /// <summary>Represents <c>%</c> token.</summary>
        PercentToken,
        /// <summary>Represents <c>&amp;</c> token.</summary>
        AmpersandToken,
        /// <summary>Represents <c>(</c> token.</summary>
        OpenParenToken,
        /// <summary>Represents <c>)</c> token.</summary>
        CloseParenToken,
        /// <summary>Represents <c>*</c> token.</summary>
        AsteriskToken,
        /// <summary>Represents <c>+</c> token.</summary>
        PlusToken,
        /// <summary>Represents <c>,</c> token.</summary>
        CommaToken,
        /// <summary>Represents <c>-</c> token.</summary>
        MinusToken,
        /// <summary>Represents <c>.</c> token.</summary>
        DotToken,
        /// <summary>Represents <c>/</c> token.</summary>
        SlashToken,
        /// <summary>Represents <c>:</c> token.</summary>
        ColonToken,
        /// <summary>Represents <c>;</c> token.</summary>
        SemicolonToken,
        /// <summary>Represents <c>&lt;</c> token.</summary>
        LessThanToken,
        /// <summary>Represents <c>=</c> token.</summary>
        EqualsToken,
        /// <summary>Represents <c>&gt;</c> token.</summary>
        GreaterThanToken,
        /// <summary>Represents <c>?</c> token.</summary>
        QuestionToken,
        /// <summary>Represents <c>@</c> token.</summary>
        AtToken,
        /// <summary>Represents <c>[</c> token.</summary>
        OpenBracketToken,
        /// <summary>Represents <c>]</c> token.</summary>
        CloseBracketToken,
        /// <summary>Represents <c>^</c> token.</summary>
        CaretToken,
        /// <summary>Represents <c>{</c> token.</summary>
        OpenBraceToken,
        /// <summary>Represents <c>|</c> token.</summary>
        BarToken,
        /// <summary>Represents <c>}</c> token.</summary>
        CloseBraceToken,
        /// <summary>Represents <c>~</c> token.</summary>
        TildeToken,

        // Compound Punctuation
        /// <summary>Represents <c>!=</c> token.</summary>
        ExclamationEqualsToken,
        /// <summary>Represents <c>!==</c> token.</summary>
        ExclamationEqualsEqualsToken,
        /// <summary>Represents <c>%=</c> token.</summary>
        PercentEqualsToken,
        /// <summary>Represents <c>&amp;&amp;</c> token.</summary>
        AmpersandAmpersandToken,
        /// <summary>Represents <c>&amp;=</c> token.</summary>
        AmpersandEqualsToken,
        /// <summary>Represents <c>**</c> token.</summary>
        AsteriskAsteriskToken,
        /// <summary>Represents <c>**=</c> token.</summary>
        AsteriskAsteriskEqualsToken,
        /// <summary>Represents <c>*=</c> token.</summary>
        AsteriskEqualsToken,
        /// <summary>Represents <c>++</c> token.</summary>
        PlusPlusToken,
        /// <summary>Represents <c>+=</c> token.</summary>
        PlusEqualsToken,
        /// <summary>Represents <c>--</c> token.</summary>
        MinusMinusToken,
        /// <summary>Represents <c>-=</c> token.</summary>
        MinusEqualsToken,
        /// <summary>Represents <c>...</c> token.</summary>
        DotDotDotToken,
        /// <summary>Represents <c>/=</c> token.</summary>
        SlashEqualsToken,
        /// <summary>Represents <c>&lt;&lt;</c> token.</summary>
        LessThanLessThanToken,
        /// <summary>Represents <c>&lt;&lt;=</c> token.</summary>
        LessThanLessThanEqualsToken,
        /// <summary>Represents <c>&lt;=</c> token.</summary>
        LessThanEqualsToken,
        /// <summary>Represents <c>==</c> token.</summary>
        EqualsEqualsToken,
        /// <summary>Represents <c>===</c> token.</summary>
        EqualsEqualsEqualsToken,
        /// <summary>Represents <c>=&gt;</c> token.</summary>
        EqualsGreaterThanToken,
        /// <summary>Represents <c>&gt;=</c> token.</summary>
        GreaterThanEqualsToken,
        /// <summary>Represents <c>&gt;&gt;</c> token.</summary>
        GreaterThanGreaterThanToken,
        /// <summary>Represents <c>&gt;&gt;=</c> token.</summary>
        GreaterThanGreaterThanEqualsToken,
        /// <summary>Represents <c>&gt;&gt;&gt;</c> token.</summary>
        GreaterThanGreaterThanGreaterThanToken,
        /// <summary>Represents <c>&gt;&gt;&gt;=</c> token.</summary>
        GreaterThanGreaterThanGreaterThanEqualsToken,
        /// <summary>Represents <c>??</c> token.</summary>
        QuestionQuestionToken,
        /// <summary>Represents <c>??=</c> token.</summary>
        QuestionQuestionEqualsToken,
        /// <summary>Represents <c>^=</c> token.</summary>
        CaretEqualsToken,
        /// <summary>Represents <c>|=</c> token.</summary>
        BarEqualsToken,
        /// <summary>Represents <c>||</c> token.</summary>
        BarBarToken,
        /// <summary>Represents <c>||=</c> token.</summary>
        BarBarEqualsToken,
        /// <summary>Represents <c>&amp;&amp;=</c> token.</summary>
        AmpersandAmpersandEqualsToken,

        // Keywords
        BreakKeyword,
        CaseKeyword,
        CatchKeyword,
        ClassKeyword,
        ConstKeyword,
        ContinueKeyword,
        DebuggerKeyword,
        DefaultKeyword,
        DeleteKeyword,
        DoKeyword,
        ElseKeyword,
        EnumKeyword,
        ExportKeyword,
        ExtendsKeyword,
        FalseKeyword,
        FinallyKeyword,
        ForKeyword,
        FunctionKeyword,
        IfKeyword,
        ImportKeyword,
        InKeyword,
        InstanceOfKeyword,
        NewKeyword,
        NullKeyword,
        ReturnKeyword,
        SuperKeyword,
        SwitchKeyword,
        ThisKeyword,
        ThrowKeyword,
        TrueKeyword,
        TryKeyword,
        TypeOfKeyword,
        VarKeyword,
        VoidKeyword,
        WhileKeyword,
        WithKeyword,

        // Strict Mode Reserved Words
        ImplementsKeyword,
        InterfaceKeyword,
        LetKeyword,
        PackageKeyword,
        PrivateKeyword,
        ProtectedKeyword,
        PublicKeyword,
        StaticKeyword,
        YieldKeyword,

        // Contextual Keywords
        AbstractKeyword,
        AsKeyword,
        AssertsKeyword,
        AnyKeyword,
        AsyncKeyword,
        AwaitKeyword,
        BooleanKeyword,
        ConstructorKeyword,
        DeclareKeyword,
        GetKeyword,
        InferKeyword,
        IsKeyword,
        KeyOfKeyword,
        ModuleKeyword,
        NamespaceKeyword,
        NeverKeyword,
        ReadonlyKeyword,
        RequireKeyword,
        NumberKeyword,
        ObjectKeyword,
        SetKeyword,
        StringKeyword,
        SymbolKeyword,
        TypeKeyword,
        UndefinedKeyword,
        UniqueKeyword,
        UnknownKeyword,
        FromKeyword,
        GlobalKeyword,
        BigIntKeyword,
        OverrideKeyword,
        OfKeyword, // for-of loop

        // Tokens with text
        IdentifierToken,
        NumericLiteralToken,
        BigIntLiteralToken,
        StringLiteralToken,
        RegularExpressionLiteralToken,
        TemplateHeadToken, // `template part ${
        TemplateMiddleToken, // } template part ${
        TemplateTailToken, // } template part`
        NoSubstitutionTemplateToken, // `template string`

        // Trivia
        EndOfLineTrivia,
        WhitespaceTrivia,
        SingleLineCommentTrivia,
        MultiLineCommentTrivia,
        ShebangTrivia,
        ConflictMarkerTrivia,

        // Special Tokens
        EndOfFileToken,
        BadToken,

        // Nodes - Declarations
        SourceFile,
        FunctionDeclaration,
        MethodDeclaration,
        ConstructorDeclaration,
        GetAccessorDeclaration,
        SetAccessorDeclaration,
        ClassDeclaration,
        InterfaceDeclaration,
        TypeAliasDeclaration,
        EnumDeclaration,
        ModuleDeclaration,
        ImportDeclaration,
        ImportEqualsDeclaration,
        ExportDeclaration,
        ExportAssignment,
        VariableStatement,
        VariableDeclaration,
        VariableDeclarator,
        BindingElement,
        PropertyAssignment,
        ShorthandPropertyAssignment,
        SpreadAssignment,
        EnumMember,
        Parameter,
        TypeParameter,
        Decorator,
        HeritageClause,
        PropertySignature,
        PropertyDeclaration,
        MethodSignature,
        CallSignature,
        ConstructSignature,
        IndexSignature,

        // Nodes - Statements
        Block,
        EmptyStatement,
        ExpressionStatement,
        IfStatement,
        DoStatement,
        WhileStatement,
        ForStatement,
        ForInStatement,
        ForOfStatement,
        ContinueStatement,
        BreakStatement,
        ReturnStatement,
        WithStatement,
        SwitchStatement,
        LabeledStatement,
        ThrowStatement,
        TryStatement,
        DebuggerStatement,

        // Nodes - Clauses & Parts
        CaseBlock,
        CaseClause,
        DefaultClause,
        CatchClause,
        FinallyClause,
        ModuleBlock,

        // Nodes - Expressions
        ArrayLiteralExpression,
        ObjectLiteralExpression,
        PropertyAccessExpression,
        ElementAccessExpression,
        CallExpression,
        NewExpression,
        TaggedTemplateExpression,
        TypeAssertionExpression,
        ParenthesizedExpression,
        FunctionExpression,
        ArrowFunction,
        DeleteExpression,
        TypeOfExpression,
        VoidExpression,
        AwaitExpression,
        PrefixUnaryExpression,
        PostfixUnaryExpression,
        BinaryExpression,
        ConditionalExpression,
        TemplateExpression,
        YieldExpression,
        SpreadElement,
        ClassExpression,
        OmittedExpression,
        ExpressionWithTypeArguments,
        AsExpression,
        NonNullExpression,
        MetaProperty,
        SyntheticExpression,
        ThisExpression,
        SuperExpression,
// dynamic import()
        Identifier, // simple identifier usage
        QualifiedName,

        // Nodes - Types
        TypeReference,
        TypePredicate,
        TypeQuery,
        TypeLiteral,
        ArrayType,
        TupleType,
        OptionalType,
        RestType,
        UnionType,
        IntersectionType,
        ConditionalType,
        InferType,
        ParenthesizedType,
        ThisType,
        TypeOperator,
        IndexedAccessType,
        MappedType,
        LiteralType,
        ImportType,

        // Predefined Types
        AnyKeywordType,
        UnknownKeywordType,
        NumberKeywordType,
        BigIntKeywordType,
        ObjectKeywordType,
        BooleanKeywordType,
        StringKeywordType,
        SymbolKeywordType,
        VoidKeywordType,
        UndefinedKeywordType,
        NullKeywordType,
        NeverKeywordType,

        // JSX
        JsxElement,
        JsxSelfClosingElement,
        JsxOpeningElement,
        JsxClosingElement,
        JsxFragment,
        JsxText,
        JsxOpeningFragment,
        JsxClosingFragment,
        JsxAttribute,
        JsxSpreadAttribute,
        JsxExpression,

        // Others
        NamedImports,
        NamedExports,
        ImportSpecifier,
        ExportSpecifier,
        ExternalModuleReference,
        TemplateSpan,
        SemicolonClassElement,
    }
}

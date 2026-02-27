using System.Linq;
using Microsoft.CodeAnalysis.TypeScript;
using Microsoft.CodeAnalysis.TypeScript.Syntax;
using Xunit;

namespace Microsoft.CodeAnalysis.TypeScript.UnitTests.Parser
{
    public class OperatorTests
    {
        [Fact]
        public void ParseBitwiseOperators()
        {
            var code = "a & b | c ^ d;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());

            // Precedence: & (5) > ^ (4) > | (3)
            // a & b | c ^ d -> (a & b) | (c ^ d)
            // Actually:
            // & binds tightest. ^ next. | last.
            // (a & b) | (c ^ d) ?? No.
            // Precedence:
            // 5: &
            // 4: ^
            // 3: |
            // a & b -> expr1
            // expr1 | c ^ d
            // c ^ d -> expr2
            // expr1 | expr2
            // Wait.
            // a & b (5)
            // result | (3) ...
            // c ^ d (4)
            // So: (a & b) | (c ^ d)

            var orExpr = Assert.IsType<BinaryExpressionSyntax>(stmt.Expression);
            Assert.Equal(SyntaxKind.BitwiseOrExpression, orExpr.Kind);

            var leftOr = Assert.IsType<BinaryExpressionSyntax>(orExpr.Left);
            Assert.Equal(SyntaxKind.BitwiseAndExpression, leftOr.Kind); // a & b

            var rightOr = Assert.IsType<BinaryExpressionSyntax>(orExpr.Right);
            Assert.Equal(SyntaxKind.ExclusiveOrExpression, rightOr.Kind); // c ^ d
        }

        [Fact]
        public void ParseShiftOperators()
        {
            var code = "a << 1 >> 2 >>> 3;";
            // << (8), >> (8), >>> (8) -> Left associative
            // ((a << 1) >> 2) >>> 3
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());

            var unsignedRight = Assert.IsType<BinaryExpressionSyntax>(stmt.Expression);
            Assert.Equal(SyntaxKind.UnsignedRightShiftExpression, unsignedRight.Kind);

            var rightShift = Assert.IsType<BinaryExpressionSyntax>(unsignedRight.Left);
            Assert.Equal(SyntaxKind.RightShiftExpression, rightShift.Kind);

            var leftShift = Assert.IsType<BinaryExpressionSyntax>(rightShift.Left);
            Assert.Equal(SyntaxKind.LeftShiftExpression, leftShift.Kind);
        }

        [Fact]
        public void ParseCompoundAssignments()
        {
            var code = "x += y -= z *= 2;";
            // Right associative
            // x += (y -= (z *= 2))
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());

            var plusEq = Assert.IsType<BinaryExpressionSyntax>(stmt.Expression);
            Assert.Equal(SyntaxKind.AssignmentExpression, plusEq.Kind);
            Assert.Equal(SyntaxKind.PlusEqualsToken, plusEq.OperatorToken.Kind());

            var minusEq = Assert.IsType<BinaryExpressionSyntax>(plusEq.Right);
            Assert.Equal(SyntaxKind.AssignmentExpression, minusEq.Kind);
            Assert.Equal(SyntaxKind.MinusEqualsToken, minusEq.OperatorToken.Kind());

            var timesEq = Assert.IsType<BinaryExpressionSyntax>(minusEq.Right);
            Assert.Equal(SyntaxKind.AssignmentExpression, timesEq.Kind);
            Assert.Equal(SyntaxKind.AsteriskEqualsToken, timesEq.OperatorToken.Kind());
        }

        [Fact]
        public void ParseConditionalExpression()
        {
            var code = "a ? b : c;";
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());

            var cond = Assert.IsType<ConditionalExpressionSyntax>(stmt.Expression);
            Assert.IsType<IdentifierNameSyntax>(cond.Condition);
            Assert.IsType<IdentifierNameSyntax>(cond.WhenTrue);
            Assert.IsType<IdentifierNameSyntax>(cond.WhenFalse);
        }

        [Fact]
        public void ParseNestedConditionalExpression()
        {
            var code = "a ? b : c ? d : e;";
            // Right associative: a ? b : (c ? d : e)
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());

            var cond1 = Assert.IsType<ConditionalExpressionSyntax>(stmt.Expression);
            Assert.Equal("a", ((IdentifierNameSyntax)cond1.Condition).Identifier.Text);
            Assert.Equal("b", ((IdentifierNameSyntax)cond1.WhenTrue).Identifier.Text);

            var cond2 = Assert.IsType<ConditionalExpressionSyntax>(cond1.WhenFalse);
            Assert.Equal("c", ((IdentifierNameSyntax)cond2.Condition).Identifier.Text);
            Assert.Equal("d", ((IdentifierNameSyntax)cond2.WhenTrue).Identifier.Text);
            Assert.Equal("e", ((IdentifierNameSyntax)cond2.WhenFalse).Identifier.Text);
        }

        [Fact]
        public void ParseAssignmentAndConditional()
        {
            var code = "x = a ? b : c;";
            // x = (a ? b : c)
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());

            var assign = Assert.IsType<BinaryExpressionSyntax>(stmt.Expression);
            Assert.Equal(SyntaxKind.AssignmentExpression, assign.Kind);

            Assert.IsType<ConditionalExpressionSyntax>(assign.Right);
        }

        [Fact]
        public void ParseArrowFunctionWithConditional()
        {
            var code = "a ? b : c => d;";
            // a ? b : (c => d)
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());

            var cond = Assert.IsType<ConditionalExpressionSyntax>(stmt.Expression);
            Assert.Equal("a", ((IdentifierNameSyntax)cond.Condition).Identifier.Text);
            Assert.Equal("b", ((IdentifierNameSyntax)cond.WhenTrue).Identifier.Text);

            var arrow = Assert.IsType<ArrowFunctionExpressionSyntax>(cond.WhenFalse);
            Assert.IsType<ParameterListSyntax>(arrow.ParameterList);
        }

        [Fact]
        public void ParsePrecedenceShiftAdditive()
        {
            var code = "a + b << c;";
            // (a + b) << c
            // + (9), << (8)
            var tree = TypeScriptSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var stmt = Assert.IsType<ExpressionStatementSyntax>(root.Statements.First());

            var shift = Assert.IsType<BinaryExpressionSyntax>(stmt.Expression);
            Assert.Equal(SyntaxKind.LeftShiftExpression, shift.Kind);

            var add = Assert.IsType<BinaryExpressionSyntax>(shift.Left);
            Assert.Equal(SyntaxKind.AddExpression, add.Kind);
        }
    }
}

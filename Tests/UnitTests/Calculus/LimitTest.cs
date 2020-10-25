using AngouriMath;
using AngouriMath.Extensions;
using AngouriMath.Core;
using static AngouriMath.Entity.Number;
using Xunit;

namespace UnitTests.Algebra
{
    public sealed class Limits
    {
        void TestLimit(Entity expr, Entity where, ApproachFrom appr, Entity desiredOutput)
            => Assert.Equal(desiredOutput.Simplify(), expr.Limit("x", where, appr).Simplify());

        [Fact] public void Test1() => TestLimit("x", Real.PositiveInfinity, ApproachFrom.Left, Real.PositiveInfinity);
        [Fact] public void Test2() => TestLimit("-x", Real.PositiveInfinity, ApproachFrom.Left, Real.NegativeInfinity);
        [Fact] public void Test3() => TestLimit("x / (2x + 1)", Real.PositiveInfinity, ApproachFrom.Left, "1/2");
        [Fact] public void Test4() => TestLimit("x / (a * x + 1)", Real.PositiveInfinity, ApproachFrom.Left, "1/a");
        [Fact] public void Test5() => TestLimit("(x2 + x) / x", Real.PositiveInfinity, ApproachFrom.Left, Real.PositiveInfinity);
        [Fact] public void Test6() => TestLimit("(x2 + x) / (a * x)", Real.PositiveInfinity, ApproachFrom.Left, Real.PositiveInfinity / (Entity)"a");
        [Fact] public void Test7() => TestLimit("(x + x) / (a * x2)", Real.PositiveInfinity, ApproachFrom.Left, "0");
        [Fact] public void Test8() => TestLimit("(x^3 + a*x^2)/(b*x^2 + 1)", "a", ApproachFrom.Left,"2 a3 / (1 + a2 * b)");
        [Fact] public void Test9() => TestLimit("x / a", "1", ApproachFrom.Left, "1 / a");
        [Fact(Skip = "Under work")] public void Test10() => TestLimit("sin(x) / x", "0", ApproachFrom.Left, "1");
        [Fact(Skip = "Under work")] public void Test11() => TestLimit("a * sin(x) / x", "0", ApproachFrom.Left, "1");
        [Fact] public void Test12() => TestLimit("a x", Real.PositiveInfinity, ApproachFrom.Left, Real.PositiveInfinity * (Entity)"a");
        [Fact] public void Test13() => TestLimit("a x2 + b / x", Real.PositiveInfinity, ApproachFrom.Left, Real.PositiveInfinity * (Entity)"a");
        [Fact] public void Test14() => TestLimit("a x ^ (-2) + b / x", Real.PositiveInfinity, ApproachFrom.Left, "0");
        [Fact] public void Test15() => TestLimit("1 / (x^2 + x)", "-1", ApproachFrom.Left, Real.PositiveInfinity);
        [Fact] public void Test16() => TestLimit("1 / x", "0", ApproachFrom.Left, Real.NegativeInfinity);
        [Fact] public void Test17() => TestLimit("1 / x", "0", ApproachFrom.Right, Real.PositiveInfinity);
        [Fact] public void Test18() => TestLimit("ln(x)", "0", ApproachFrom.Right, Real.NegativeInfinity);
        [Fact] public void Test19() => TestLimit("ln(x)", "0", ApproachFrom.Left, Real.NegativeInfinity);
        [Fact] public void Test20() => TestLimit("ln(x)", Real.PositiveInfinity, ApproachFrom.Left, Real.PositiveInfinity);
        [Fact] public void Test21() => TestLimit("ln(x)", Real.NegativeInfinity, ApproachFrom.Right, Real.PositiveInfinity);
        [Fact] public void Test22() => TestLimit("ln(1 / (x - a))", "a", ApproachFrom.Right, Real.PositiveInfinity);
        [Fact] public void Test23() => TestLimit("log(b, x)", 0, ApproachFrom.Right, Real.NegativeInfinity);
        [Fact] public void Test24() => TestLimit("ln(x^4 - x^2)", Real.PositiveInfinity, ApproachFrom.Left, Real.PositiveInfinity);
        [Fact] public void Test25() => TestLimit("ln((x - 1) / (2x + 1))", Real.PositiveInfinity, ApproachFrom.Right, MathS.Ln(0.5));
        [Fact] public void Test26() => TestLimit("log(x, x^2)", 0, ApproachFrom.Right, 2);
        [Fact] public void Test27() => TestLimit("log(1/x, x^2)", 0, ApproachFrom.Right, -2);
        [Fact] public void Test28() => TestLimit("log(x, x^(-2))", 0, ApproachFrom.Right, -2);
        [Fact] public void Test29() => TestLimit("log(1/x, x^(-2))", 0, ApproachFrom.Right, 2);
        [Fact] public void Test30() => TestLimit("log(x, x)", 0, ApproachFrom.Right, 1);
        [Fact] public void Test31() => TestLimit("ln(ln((e^2*x + t) / (x + 1)))", Real.PositiveInfinity, ApproachFrom.Left, MathS.Ln(2));
        [Fact] public void Test32() => TestLimit("log((2x - 1)/(x + 1), (x - 1)/(2x - 1))", Real.PositiveInfinity, ApproachFrom.Left, -1);
        [Fact] public void Test33() => TestLimit("(x + 3) / (x + 4)", "+oo", ApproachFrom.BothSides, 1);

        [Theory]
        [InlineData("sin(x) / x", "1")]
        [InlineData("sin(2x) / x", "2")]
        [InlineData("sin(a x) / x", "a")]
        [InlineData("sin(a x) / (a x)", "1")]
        [InlineData("tan(x) / x", "1")]
        [InlineData("tan(2x) / x", "2")]
        [InlineData("tan(a x) / x", "a")]
        [InlineData("tan(a x) / (a x)", "1")]
        [InlineData("arcsin(x) / x", "1")]
        [InlineData("arcsin(2x) / x", "2")]
        [InlineData("arcsin(a x) / x", "a")]
        [InlineData("arcsin(a x) / (a x)", "1")]
        [InlineData("arctan(x) / x", "1")]
        [InlineData("arctan(2x) / x", "2")]
        [InlineData("arctan(a x) / x", "a")]
        [InlineData("arctan(a x) / (a x)", "1")]
        [InlineData("(tan(a x) - sin(b x)) / (a x)", "1 + -b / a")]
        [InlineData("sin(a^x - 1) / tan(b^x - 1)", "ln(a) / ln(b)")]
        [InlineData("(1 - cos(x)) / x2", "1/2")]
        [InlineData("((1 + x)^4 - 1) / x", "4")]
        [InlineData("((1 + x)^a - 1) / x", "a")]
        [InlineData("((1 + c x2 + b x)^a - 1)", "0")]
        [InlineData("sin(x - a * x) / tan(b * x - x)", "(a - 1) / (1 - b)")]
        [InlineData("sin(x2) / sin(x)", "0")]
        [InlineData("(sin(x) - tan(a x)) / (sin(b x) - tan(x))", "(a - 1) / (1 - b)")]
        [InlineData("sec(x)", "1")]
        [InlineData("a x * csc(b x)", "a / b")]
        [InlineData("(arccos(x) - pi / 2) / x", "-1")]
        [InlineData("(arccotan(x) - pi / 2) / x", "-1")]
        [InlineData("arccsc(1 / x)", "0")]
        [InlineData("arcsec(1 / x) - pi / 2", "0")]
        public void TestEquivalenceTableTo0(string input, string expected)
        {
            var limit = input.ToEntity();
            var exp = expected.Simplify();
            var actual = limit.Limit("x", 0).Simplify();
            Assert.Equal(exp, actual);
        }

        [Theory]
        [InlineData("(1 + 1/x)^x", "e")]
        [InlineData("(1 + 1/(2x))^x", "sqrt(e)")]
        [InlineData("(1 + 1/x)^(2x)", "e2")]
        [InlineData("(1 + 2/x)^x", "e2")]
        [InlineData("(1 + a/x)^x", "e^a")]
        [InlineData("(1 + a/x)^(b x)", "e^(a * b)")]
        [InlineData("(1 + a/(2x))^(b x + c x)", "e^(a * (b + c) / 2)")]
        public void TestEquivalenceTableToInfinity(string input, string expected)
        {
            var limit = input.ToEntity();
            var exp = expected.Simplify();
            var actual = limit.Limit("x", "+oo").Simplify();
            Assert.Equal(exp, actual);
        }

        [Fact]
        public void TestComplicated()
        {
            Entity subExpr = "(a * x2 + b x) / (c x2 - 3)";
            Entity expr = MathS.Sqrt(subExpr * 3 / MathS.Sin(subExpr) + MathS.Sin("d"));
            Entity.Variable x = nameof(x);
            Entity dest = Real.PositiveInfinity;
            var limit = expr.Limit(x, dest, ApproachFrom.Left);
            Assert.NotNull(limit);
            Assert.Equal("sqrt(a / c * 3 / sin(a / c) + sin(d))", limit?.Stringize());
        }
    }
}
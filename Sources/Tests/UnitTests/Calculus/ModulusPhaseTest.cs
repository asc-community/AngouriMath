//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// https://github.com/asc-community/AngouriMath/issues/1186: the limit of z / |z| answered 1
    /// whatever the phase of z. The derivative of a modulus was sgn(f) * f' whether or not f was
    /// real-valued, l'Hopital's rule read the wrong derivative, and three things behind it agreed
    /// with the wrong answer: a zero times a complex infinity evaluated to 0, a complex infinity
    /// did not count as infinite, and the descent's unread 0 / abs(0) was returned as NaN.
    /// </summary>
    public sealed class ModulusPhaseTest
    {
        private static readonly Variable x = MathS.Var("x");

        [Theory]
        [InlineData("(i/x)/abs(i/x)", "i")]
        [InlineData("(i/x)^2/abs(i/x)^2", "-1")]
        [InlineData("i * abs(x) / x", "i")]
        [InlineData("(3 + 4i) * x / abs((3 + 4i) * x)", "3/5 + 4/5 * i")]
        [InlineData("abs(x)/x", "1")]
        [InlineData("x/abs(x)", "1")]
        [InlineData("abs(sin(x))/x", "0")]
        [InlineData("abs(i*x)", "+oo")]
        [InlineData("abs(2 + i * x)", "+oo")]
        public void TheLimitAtPlusInfinityKeepsThePhase(string expr, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, expr.ToEntity().Limit(x, Real.PositiveInfinity));

        // (i / x) / |i / x| is i * |x| / x, which is -i on the way to -oo.
        [Theory]
        [InlineData("(i/x)/abs(i/x)", "-i")]
        [InlineData("(i/x)^2/abs(i/x)^2", "-1")]
        [InlineData("(3 + 4i) * x / abs((3 + 4i) * x)", "-3/5 - 4/5 * i")]
        [InlineData("abs(x)/x", "-1")]
        public void TheLimitAtMinusInfinityKeepsThePhase(string expr, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, expr.ToEntity().Limit(x, Real.NegativeInfinity));

        [Theory]
        [InlineData("(i/x)^2/abs(i/x)^2", "-1")]
        [InlineData("x^2/abs(x)^2", "1")]
        [InlineData("(2 + i * x) / abs(2 + i * x)", "1")]
        public void TheLimitAtZeroKeepsThePhase(string expr, string expected)
            => Assert.Equal(expected.ToEntity(), expr.ToEntity().Limit(x, 0));

        // x is real on its way to 0, and the two sides disagree.
        [Theory]
        [InlineData("x/abs(x)")]
        [InlineData("abs(x)/x")]
        public void ATwoSidedLimitThatDisagreesIsStillNaN(string expr)
            => Assert.Equal(Real.NaN, expr.ToEntity().Limit(x, 0));

        // |1 + i| is sqrt(2), which the number type only holds rounded, so the factor stays
        // inside the modulus and nothing reads the quotient; the answer is i, and declining is
        // legitimate where 1 was not. The second is the issue's Simplify case, which folded to
        // 0 through a candidate (2 + i * x) * abs(2 + i * x)^(-1) and a product of zero and a
        // complex infinity that evaluated to 0.
        [Theory]
        [InlineData("((1+i)/x)^2/abs((1+i)/x)^2")]
        [InlineData("(2 + i * x) / abs(2 + i * x)")]
        public void AQuotientWhoseModulusStaysUnreadIsDeclinedRatherThanAnswered(string expr)
        {
            var limit = expr.ToEntity().Limit(x, Real.PositiveInfinity);
            Assert.IsType<Limitf>(limit);
            Assert.IsType<Limitf>(limit.Simplify());
        }

        [Fact]
        public void ADecidedNaNAtInfinityIsKept()
        {
            Assert.Equal(Real.NaN, "sin(x)".ToEntity().Limit(x, Real.PositiveInfinity));
            Assert.Equal(Real.NaN, "2 * sin(x)".ToEntity().Limit(x, Real.PositiveInfinity));
        }

        [Theory]
        [InlineData("abs(i * x)", "sgn(x) provided not x = 0")]
        [InlineData("abs(x / i)", "sgn(x) provided not x = 0")]
        [InlineData("abs(x + 2)", "sgn(x + 2) provided not x + 2 = 0")]
        [InlineData("sgn(x + 2)", "0 provided not x + 2 = 0")]
        public void TheDerivativeOfARealValuedModulusIsTheSignRule(string expr, string expected)
            => Assert.Equal(expected.ToEntity(), expr.ToEntity().Differentiate(x));

        // |i / x| is 1 / |x|, whose derivative is -sgn(x) / x^2. The real-line formula gave
        // sgn(i / x) * (-i / x^2), which is +sgn(x) / x^2: the wrong sign, and the whole of the
        // wrong limit.
        [Theory]
        [InlineData("2", "-1/4")]
        [InlineData("-2", "1/4")]
        public void TheDerivativeOfTheModulusOfAnImaginaryQuotientHasTheRightSign(string at, string expected)
            => Assert.Equal(expected.ToEntity(), "abs(i / x)".ToEntity().Differentiate(x).Substitute(x, at.ToEntity()).Simplify());

        // A symbol is complex until something says otherwise, and off the real line neither
        // sgn(f) * f' nor 0 is the derivative. Not answering is the honest answer.
        [Theory]
        [InlineData("abs(2 + i * x)")]
        [InlineData("abs((1 + i) * x)")]
        [InlineData("abs(a * x)")]
        [InlineData("sgn(2 + i * x)")]
        [InlineData("sgn(a * x)")]
        public void TheDerivativeOfAModulusNotShownRealIsLeftUnevaluated(string expr)
            => Assert.IsType<Derivativef>(expr.ToEntity().Differentiate(x));

        [Theory]
        [InlineData("abs(i * x)", "abs(x)")]
        [InlineData("abs(x * i)", "abs(x)")]
        [InlineData("abs(x / i)", "abs(x)")]
        [InlineData("abs(i)", "1")]
        [InlineData("abs(3 + 4i)", "5")]
        public void AModulusTakesANumericFactorOutAsItsModulus(string expr, string expected)
            => Assert.Equal(expected.ToEntity(), expr.ToEntity().Simplify());

        // 5 * abs(x) is no smaller than abs((3 + 4i) * x), so Simplify keeps whichever it saw
        // first; the rewrite itself is what is pinned, and the limit above is what it is for.
        [Theory]
        [InlineData("(3 + 4i) * x", "5 * abs(x)")]
        [InlineData("x * (3 + 4i)", "5 * abs(x)")]
        [InlineData("(3 + 4i) / x", "5 / abs(x)")]
        [InlineData("x / (3 + 4i)", "abs(x) / 5")]
        [InlineData("i / x", "1 / abs(x)")]
        public void ANumericFactorWithAnExactModulusComesOut(string argument, string expected)
        {
            Assert.True(AngouriMath.Functions.TreeAnalyzer.TryTakeNumericModulusOut(argument.ToEntity(), out var rewritten));
            Assert.Equal(expected.ToEntity(), rewritten);
        }

        [Theory]
        [InlineData("(1 + i) * x")]
        [InlineData("3 * x")]
        [InlineData("-2 * x")]
        [InlineData("a * x")]
        public void AFactorWithoutAnExactPhasedModulusStays(string argument)
            => Assert.False(AngouriMath.Functions.TreeAnalyzer.TryTakeNumericModulusOut(argument.ToEntity(), out _));

        // A factor whose modulus is not exact stays inside; 1.414... * abs(x) is not the value.
        [Fact]
        public void AFactorWithAnInexactModulusStaysInside()
        {
            var simplified = "abs((1 + i) * x)".ToEntity().Simplify();
            Assert.IsType<Absf>(simplified);
            Assert.IsType<Mulf>(((Absf)simplified).Argument);
        }

        [Theory]
        [InlineData("0 * (i * +oo)")]
        [InlineData("(2 + i * +oo) * 0")]
        [InlineData("+oo * 0")]
        public void ZeroTimesAComplexInfinityIsNaN(string expr)
            => Assert.Equal(Real.NaN, expr.ToEntity().Evaled);

        [Fact]
        public void AnImaginaryUnitTimesInfinityIsStillAComplexInfinity()
            => Assert.Equal(Complex.Create(0, Real.PositiveInfinity), "i * +oo".ToEntity().Evaled);
    }
}

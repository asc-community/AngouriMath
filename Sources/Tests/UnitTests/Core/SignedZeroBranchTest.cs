//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A decimal's negative zero is an artefact of the exponent arithmetic -- <c>0E-100 * -0.43</c>
    /// is <c>-0E-102</c> -- and not a limit from below, so a number on the negative axis has
    /// argument <c>pi</c> whatever the sign of its zero imaginary part, and its powers, roots
    /// and logarithm are the principal ones. With the downcasting off <c>Complex.Create</c>
    /// keeps <c>-1.54 - 0i</c> a Complex, and the phase read <c>-pi</c> off the zero's sign:
    /// <c>(-1.54)^(-1/2)</c> was <c>+0.80 i</c> there and <c>-0.80 i</c> with the downcasting on,
    /// the setting deciding a branch, and a numerical check that turns the downcasting off called
    /// a right antiderivative wrong on the strength of it.
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1378"/>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class SignedZeroBranchTest
    {
        private static Complex Evaluated(string expression, bool downcasting)
            => MathS.Settings.DowncastingEnabled.As(downcasting, () => (Complex)expression.ToEntity().Evaled);

        private static void AssertClose(Complex expected, Complex actual, string what)
        {
            var re = Math.Abs((double)(expected.RealPart - actual.RealPart));
            var im = Math.Abs((double)(expected.ImaginaryPart - actual.ImaginaryPart));
            Assert.True(re < 1e-40 && im < 1e-40, $"{what}: expected {expected}, got {actual}");
        }

        /// <summary>The same value with the downcasting on and off, and it is the principal one.</summary>
        [Theory]
        [InlineData("(2*1.44*(1/sqrt(-5.286))^2-1)^(-1/2)")]
        [InlineData("((1/sqrt(-5.286))*(2*1.44*(1/sqrt(-5.286))^2-1)^(-1/2))^(-3)")]
        [InlineData("sqrt((1/sqrt(-2))^2 * 3)")]
        [InlineData("ln((1/sqrt(-2))^2)")]
        public void TheSettingDoesNotDecideTheBranch(string expression)
        {
            var on = Evaluated(expression, downcasting: true);
            var off = Evaluated(expression, downcasting: false);
            AssertClose(on, off, expression);
        }

        [Fact]
        public void ANegativeZeroImaginaryPartIsOnTheAxis()
        {
            MathS.Settings.DowncastingEnabled.As(false, () =>
            {
                var minusFour = Complex.Create(EDecimal.FromInt32(-4), EDecimal.NegativeZero);
                AssertClose(Complex.Create(0, 2), Complex.Sqrt(minusFour), "sqrt(-4 - 0i)");
                AssertClose(Complex.Create(0, 2), Complex.Pow(minusFour, Rational.Create(1, 2)), "(-4 - 0i)^(1/2)");
                AssertClose(Complex.Create(EDecimal.FromInt32(4).Log(MathS.Settings.DecimalPrecisionContext), MathS.DecimalConst.pi),
                    Complex.Ln(minusFour), "ln(-4 - 0i)");
            });
        }
    }
}

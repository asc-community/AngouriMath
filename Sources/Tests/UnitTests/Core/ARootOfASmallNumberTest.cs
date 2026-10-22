//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The search for an exact root rounds its candidates to six digits. A root below 1e-6 was the integer 0, which was
    /// accepted because 0 ^ q is within 1e-6 of a base below it; and a rounded candidate was returned as the real root
    /// even where its power was not the base. It searches at the size of one, by an exact power of ten, accepts a
    /// rational root only where its power is the base at the working precision, and returns a real root as computed.
    /// References from mpmath.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/2065">#2065</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class ARootOfASmallNumberTest
    {
        private static void AssertClose(string reference, EDecimal actual, string relativeTolerance = "1E-40")
        {
            var expected = EDecimal.FromString(reference);
            var error = actual.Subtract(expected, EContext.ForPrecision(200)).Abs().Divide(expected.Abs(), EContext.ForPrecision(10));
            Assert.True(error.LessThan(EDecimal.FromString(relativeTolerance)), $"{actual} where it is {expected}");
        }

        [Theory]
        [InlineData("sqrt(2/10^20)", "1.41421356237309504880168872420969807856967187537694807317668E-10")]
        [InlineData("(3/10^30)^(1/3)", "1.44224957030740838232163831078010958839186925349935057754642E-10")]
        // The real root, as (-8)^(1/3) is -2.
        [InlineData("(-2/10^21)^(1/3)", "-1.25992104989487316476721060727822835057025146470150798008198E-7")]
        [InlineData("(10^12 + 1/2)^(1/2)", "1000000.00000024999999999996875000000000781249999999755859375")]
        public void IsItsValue(string expression, string reference)
            => AssertClose(reference, Assert.IsType<Real>(expression.EvalNumerical()).EDecimal);

        [Theory]
        [InlineData("sqrt(1/10^20)", "1/10000000000")]
        [InlineData("(-1/10^30)^(1/3)", "-1/10000000000")]
        [InlineData("(16/10^40)^(1/4)", "1/5000000000")]
        [InlineData("sqrt(10^30)", "1000000000000000")]
        [InlineData("(-8)^(1/3)", "-2")]
        [InlineData("(27/8)^(2/3)", "9/4")]
        public void AnExactRootIsExact(string expression, string root)
            => Assert.Equal(root.ToEntity(), expression.EvalNumerical());

        [Fact]
        public void TheRootOfASmallImaginaryIsComplex()
        {
            var root = Assert.IsType<Complex>("sqrt(i/10^20)".EvalNumerical());
            AssertClose("7.07106781186547524400844362104849039284835937688474036588340E-11", root.RealPart.EDecimal);
            AssertClose("7.07106781186547524400844362104849039284835937688474036588340E-11", root.ImaginaryPart.EDecimal);
        }

        [Fact]
        public void ARootOffTheRealLineKeepsItsImaginaryPart()
        {
            // It was 1: the candidate rounded to six digits lost 5e-9 i. To twelve digits, because the modulus,
            // 1 + 5e-17, is read as 1 within 1e-16 (https://github.com/asc-community/AngouriMath/issues/1944).
            var root = Assert.IsType<Complex>("(1 + i/10^8)^(1/2)".EvalNumerical());
            AssertClose("4.9999999999999999375000000000000027343749999999998388671875E-9", root.ImaginaryPart.EDecimal, "1E-12");
        }
    }
}

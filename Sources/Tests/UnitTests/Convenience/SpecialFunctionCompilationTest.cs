//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;
using NumericsComplex = System.Numerics.Complex;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// The special functions compile, through <c>Compile</c> and <c>Compile&lt;TIn, TOut&gt;</c>
    /// alike, in double precision and in agreement with the interpreter: its branch cuts, and its
    /// values on the axes.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1607">#1607</a>
    /// </summary>
    [Trait("Area", "Convenience")]
    public sealed class SpecialFunctionCompilationTest
    {
        public static readonly TheoryData<string, double, double> Points = new()
        {
            // Near the origin, where erf is its Taylor series, and the others their own series.
            { "erf(x)", 0.3, 0.2 }, { "erf(x)", 2.5, 0 }, { "erf(x)", -1.7, 0 }, { "erf(x)", 1.5, -2.5 },
            { "erf(x)", -3, 4 }, { "erf(x)", 0, 3 }, { "erf(x)", 20, 30 },
            { "erfc(x)", 0.3, 0.2 }, { "erfc(x)", 2.5, 0 }, { "erfc(x)", -1.7, 0 }, { "erfc(x)", 1.5, -2.5 },
            { "erfc(x)", 5, 1 }, { "erfc(x)", 0, 3 },
            { "erfi(x)", 0.3, 0.2 }, { "erfi(x)", 2.5, 0 }, { "erfi(x)", -1.7, 0 }, { "erfi(x)", 1.5, -2.5 },
            // Ei, by its series, by E1's continued fraction, by the asymptotic series, and either side
            // of its cut, where it is i pi more or less than on the axis.
            { "Ei(x)", 0.3, 0.2 }, { "Ei(x)", 2.5, 0 }, { "Ei(x)", -1.7, 0 }, { "Ei(x)", -3, 4 },
            { "Ei(x)", 12, 0 }, { "Ei(x)", -5, 1e-9 }, { "Ei(x)", -5, -1e-9 }, { "Ei(x)", 45, 10 },
            { "Ei(x)", -50, 0 }, { "Ei(x)", 0, 3 },
            { "li(x)", 2.5, 0 }, { "li(x)", 0.4, 0 }, { "li(x)", -2, 0 }, { "li(x)", 1.5, -2.5 }, { "li(x)", 1e6, 0 },
            { "Si(x)", 0.3, 0.2 }, { "Si(x)", 2.5, 0 }, { "Si(x)", -1.7, 0 }, { "Si(x)", 1.5, -2.5 },
            { "Si(x)", 20, 3 }, { "Si(x)", 0, 3 }, { "Si(x)", 50, 0 },
            { "Ci(x)", 0.3, 0.2 }, { "Ci(x)", 2.5, 0 }, { "Ci(x)", -1.7, 0 }, { "Ci(x)", -5, 1e-9 },
            { "Ci(x)", -5, -1e-9 }, { "Ci(x)", 0, 3 }, { "Ci(x)", 0, -3 }, { "Ci(x)", 20, 3 },
            { "Shi(x)", 0.3, 0.2 }, { "Shi(x)", 2.5, 0 }, { "Shi(x)", -1.7, 0 }, { "Shi(x)", 3, 20 },
            { "Chi(x)", 0.3, 0.2 }, { "Chi(x)", 2.5, 0 }, { "Chi(x)", -1.7, 0 }, { "Chi(x)", -5, 1e-9 },
            { "Chi(x)", 3, 20 }, { "Chi(x)", 0, 3 },
        };

        private static NumericsComplex Interpreted(string expression, double re, double im)
        {
            // With the downcasting off, so that a small value is not read as zero.
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            return ((Entity.Number.Complex)expression.ToEntity().Substitute("x", Entity.Number.Complex.Create(re, im)).EvalNumerical()).ToNumerics();
        }

        private static void Close(NumericsComplex expected, NumericsComplex actual)
            => Assert.True((actual - expected).Magnitude <= 1e-12 * expected.Magnitude, $"expected {expected}, got {actual}");

        [Theory]
        [MemberData(nameof(Points))]
        public void TheCompiledFormAgreesWithTheInterpreter(string expression, double re, double im)
            => Close(Interpreted(expression, re, im), expression.ToEntity().Compile("x").Call(new NumericsComplex(re, im)));

        [Theory]
        [MemberData(nameof(Points))]
        public void TheExpressionTreeAgreesWithTheInterpreter(string expression, double re, double im)
            => Close(Interpreted(expression, re, im),
                expression.ToEntity().Compile<NumericsComplex, NumericsComplex>("x")(new NumericsComplex(re, im)));

        /// <summary>
        /// Over doubles, a value where the function is real, and NaN where it is not, as
        /// <see cref="System.Math.Sqrt"/> of a negative number is: <c>li</c>, <c>Ci</c> and <c>Chi</c>
        /// are <c>i pi</c> more than a real value left of 0.
        /// </summary>
        [Theory]
        [InlineData("erf(x)", 2.5)]
        [InlineData("erfc(x)", -1.7)]
        [InlineData("erfi(x)", 2.5)]
        [InlineData("Ei(x)", -1.7)]
        [InlineData("li(x)", 0.4)]
        [InlineData("Si(x)", -1.7)]
        [InlineData("Ci(x)", 2.5)]
        [InlineData("Shi(x)", -1.7)]
        [InlineData("Chi(x)", 2.5)]
        public void OverDoublesARealValueCompiles(string expression, double at)
        {
            var expected = Interpreted(expression, at, 0);
            Assert.Equal(0, expected.Imaginary);
            var compiled = expression.ToEntity().Compile<double, double>("x")(at);
            Assert.True(System.Math.Abs(compiled - expected.Real) <= 1e-12 * System.Math.Abs(expected.Real), $"expected {expected.Real}, got {compiled}");
        }

        /// <summary>
        /// A value too large for a double is infinite in its large part and keeps the part that is
        /// exact on the axis, as the interpreter's does. Halving <c>Ei(z) - Ei(-z)</c> as a complex
        /// number made that part NaN, since an infinity times the other part's zero is one.
        /// </summary>
        [Theory]
        [InlineData("Shi(x)", 1000, 0, double.PositiveInfinity, 0)]
        [InlineData("Shi(x)", -1000, 0, double.NegativeInfinity, 0)]
        [InlineData("Chi(x)", -1000, 0, double.PositiveInfinity, System.Math.PI)]
        [InlineData("Si(x)", 0, 1000, 0, double.PositiveInfinity)]
        [InlineData("Ci(x)", 0, 1000, double.PositiveInfinity, System.Math.PI / 2)]
        public void AnInfiniteValueKeepsItsExactPart(string expression, double re, double im, double expectedRe, double expectedIm)
        {
            var compiled = expression.ToEntity().Compile("x").Call(new NumericsComplex(re, im));
            Assert.Equal(expectedRe, compiled.Real);
            Assert.Equal(expectedIm, compiled.Imaginary);
            var interpreted = Interpreted(expression, re, im);
            Assert.Equal(expectedRe, interpreted.Real);
            Assert.Equal(expectedIm, interpreted.Imaginary, 15);
        }

        [Theory]
        [InlineData("li(x)", -2)]
        [InlineData("Ci(x)", -1.7)]
        [InlineData("Chi(x)", -1.7)]
        public void OverDoublesAValueOffTheRealLineIsNaN(string expression, double at)
            => Assert.True(double.IsNaN(expression.ToEntity().Compile<double, double>("x")(at)));
    }
}

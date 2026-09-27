//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// <c>erf</c>, <c>erfc</c> and <c>erfi</c>: parsed, printed, differentiated, and evaluated
    /// to the working precision anywhere in the complex plane.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1501">#1501</a>
    /// </summary>
    [Trait("Area", "Convenience")]
    public sealed class ErrorFunctionTest
    {
        /// <summary>
        /// Against mpmath at 200 digits, to 40 significant digits of each part. At 60, mpmath's own
        /// value for the imaginary part of <c>erf(12 + 8 i)</c> is wrong from the 23rd digit and
        /// that of <c>erf(15 + 10 i)</c> from the 9th; at 100 and above it agrees with itself, with
        /// the quadrature of the definition, and with this. The rows cross the
        /// line where the series gives way to the asymptotic series, <c>|z|^2</c> near 242 at the
        /// default hundred digits, on the real line, off it, and on the imaginary axis where the
        /// series cancels hardest. Evaluated with downcasting off, which would otherwise read
        /// <c>1 + 4e-37</c> as the integer 1.
        /// </summary>
        [Theory]
        [InlineData("erf(1)", "0.842700792949714869341220635082609259296066998", "0")]
        [InlineData("erf(1/2)", "0.520499877813046537682746653891964528736451576", "0")]
        [InlineData("erf(-2)", "-0.995322265018952734162069256367252928610891797", "0")]
        [InlineData("erf(1 + i)", "1.31615128169794764488027108024367036902770653", "0.19045346923783468628410886196916244243777731")]
        [InlineData("erf(2 - 3 * i)", "-20.8294614276145683891030884519811128744390357", "-8.68731827147016314442807875454187155305198965")]
        [InlineData("erf(3 * i)", "0", "1629.99462260156565106164795207627416277888991")]
        [InlineData("erf(12 + 8 * i)", "1.00000000000000000000000000000000000041109186", "-5.73217037548207257307226846436398125243395923e-37")]
        [InlineData("erf(15 + 10 * i)", "1", "-1.36521293163424652983495625172992278784959889e-56")]
        [InlineData("erf(-15 + 10 * i)", "-1", "-1.36521293163424652983495625172992278784959889e-56")]
        [InlineData("erf(20 * i)", "0", "1.47479753962878620244773315313183512459925928e+172")]
        [InlineData("erfc(10)", "2.08848758376254475700078629495778861156081812e-45", "0")]
        [InlineData("erfc(-1)", "1.842700792949714869341220635082609259296067", "0")]
        [InlineData("erfc(15 + 5 * i)", "4.3823936244741368125841545115653530204851946e-89", "2.25866204017490621858266035895561905217636073e-89")]
        [InlineData("erfc(2 + i)", "-0.00360634272565175091291182820541914235532928537", "0.0112590060288150250764009156316482248536651599")]
        [InlineData("erfi(1)", "1.65042575879754287602533772956136244389567987", "0")]
        [InlineData("erfi(10)", "1524307422708669699360546614726544062463812.07", "0")]
        [InlineData("erfi(2 + i)", "-5.04914370344703466954303695861414056555309108", "-0.536643565778565033991795559314192749442093869")]
        [InlineData("erfi(20)", "1.47479753962878620244773315313183512459925928e+172", "0")]
        public void AgreesWithMpmath(string expression, string real, string imaginary)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var value = MathS.FromString(expression, useCache: false).EvalNumerical();
            Close(EDecimal.FromString(real), value.RealPart.EDecimal);
            Close(EDecimal.FromString(imaginary), value.ImaginaryPart.EDecimal);
        }

        private static void Close(EDecimal expected, EDecimal actual)
        {
            var context = EContext.ForPrecision(60);
            var scale = expected.Abs().CompareTo(EDecimal.Create(1, -300)) > 0 ? expected.Abs() : EDecimal.Create(1, -300);
            var error = actual.Subtract(expected, context).Abs().Divide(scale, context);
            Assert.True(error.CompareTo(EDecimal.Create(1, -40)) < 0, $"expected {expected}, got {actual}");
        }

        [Theory]
        [InlineData("erf(0)", "0")]
        [InlineData("erfc(0)", "1")]
        [InlineData("erfi(0)", "0")]
        [InlineData("erf(+oo)", "1")]
        [InlineData("erf(-oo)", "-1")]
        [InlineData("erfc(+oo)", "0")]
        [InlineData("erfc(-oo)", "2")]
        [InlineData("erfi(+oo)", "+oo")]
        [InlineData("erfi(-oo)", "-oo")]
        public void ExactValues(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), expression.ToEntity().InnerSimplified);

        [Theory]
        [InlineData("erf(x)", "2 / sqrt(pi) * e ^ (-x ^ 2)")]
        [InlineData("erfc(x)", "-2 / sqrt(pi) * e ^ (-x ^ 2)")]
        [InlineData("erfi(x)", "2 / sqrt(pi) * e ^ (x ^ 2)")]
        [InlineData("erf(2 x)", "4 / sqrt(pi) * e ^ (-4 x ^ 2)")]
        public void TheDerivativeIsTheIntegrandOfTheDefinition(string expression, string derivative)
        {
            var difference = expression.ToEntity().Differentiate("x") - derivative.ToEntity();
            foreach (var point in new[] { "3/10", "-6/5", "2" })
                Assert.Equal(0, difference.Substitute("x", point.ToEntity()).EvalNumerical().Abs().EDecimal.ToDouble(), 12);
        }

        [Theory]
        [InlineData("erf(x)", "erf(x)", @"\operatorname{erf}\left(x\right)", "sympy.erf(x)")]
        [InlineData("erfc(x)", "erfc(x)", @"\operatorname{erfc}\left(x\right)", "sympy.erfc(x)")]
        [InlineData("erfi(x)", "erfi(x)", @"\operatorname{erfi}\left(x\right)", "sympy.erfi(x)")]
        public void PrintsAndReadsBack(string written, string printed, string latex, string sympy)
        {
            var entity = written.ToEntity();
            Assert.Equal(printed, entity.ToString());
            Assert.Equal(entity, printed.ToEntity());
            Assert.Equal(latex, entity.Latexize());
            Assert.Contains(sympy, MathS.ToSympyCode(entity));
        }

        [Fact]
        public void TheEntryPointsAgreeWithTheParser()
        {
            Assert.Equal("erf(x)".ToEntity(), MathS.Erf("x"));
            Assert.Equal("erfc(x)".ToEntity(), MathS.Erfc("x"));
            Assert.Equal("erfi(x)".ToEntity(), MathS.Erfi("x"));
        }

        /// <summary>Each is monotone on the real line, so an interval goes to the interval between its ends' values.</summary>
        [Theory]
        [InlineData("erf([0; 1])", "0", "erf(1)")]
        [InlineData("erfc([0; 1])", "erfc(1)", "1")]
        [InlineData("erfi([-1; 1])", "erfi(-1)", "erfi(1)")]
        public void ARealIntervalIsMappedByMonotonicity(string expression, string left, string right)
        {
            var image = Assert.IsType<Entity.Set.Interval>(expression.ToEntity().Evaled);
            Assert.Equal(left.ToEntity().Evaled, image.Left.Evaled);
            Assert.Equal(right.ToEntity().Evaled, image.Right.Evaled);
        }
    }
}

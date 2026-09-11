//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Integration by parts reads a quotient as the product it is, and cuts it at the logarithm
    /// or inverse trigonometric factor wherever that factor sits.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>arcsin(x)/(x^2*sqrt(1 - x^2))</c> had no antiderivative and
    /// <c>arcsin(x)*(1/(x^2*sqrt(1 - x^2)))</c> did — the same function, and the second is a
    /// product where the first is a quotient, which by parts never looked at. The companion
    /// defect is the split: <c>x*arcsin(x)/sqrt(1 - x^2)</c> needs the arcsine taken out of the
    /// middle, which is the same move
    /// <a href="https://github.com/asc-community/AngouriMath/pull/1271">#1271</a> made for a
    /// polynomial factor.
    /// </para>
    /// <para>
    /// <b>Both spellings are asserted in every case</b>, because that disagreement is the defect
    /// and it is invisible unless the same function is asked for twice.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ByPartsQuotientTest
    {
        /// <summary>
        /// Inside <c>(0, 1)</c>, where every integrand here is real — the arcsine and
        /// <c>sqrt(1 - x^2)</c> both want it, and so does the logarithm.
        /// </summary>
        private static readonly double[] Points = { 0.21, 0.35, 0.52, 0.71, 0.83 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 4,
                $"only {compared} of {Points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// The same integrand as a quotient and as the product it is. The second of each pair
        /// came out before this and the first did not.
        /// </summary>
        [Theory]
        [InlineData("arcsin(x)/(x^2*sqrt(1 - x^2))", "arcsin(x)*(1/(x^2*sqrt(1 - x^2)))")]
        [InlineData("x*arcsin(x)/sqrt(1 - x^2)", "arcsin(x)*(x/sqrt(1 - x^2))")]
        [InlineData("arctan(x)/x^2", "arctan(x)*(1/x^2)")]
        [InlineData("ln(x)/x^3", "ln(x)*(1/x^3)")]
        public void AQuotientAndTheProductItIs(string quotient, string product)
        {
            DifferentiatesBack(quotient);
            DifferentiatesBack(product);
        }

        /// <summary>
        /// The factor to differentiate taken out of the middle of a quotient, which needs both
        /// moves at once — read as a product, and cut somewhere other than the top node.
        /// </summary>
        [Theory]
        [InlineData("x*arcsin(x)/sqrt(1 - x^2)")]
        [InlineData("x*ln(x + sqrt(1 + x^2))/sqrt(1 + x^2)")]
        [InlineData("x*ln(x)/x^2")]
        public void TheFactorIsTakenOutOfTheMiddle(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A polynomial over something else is the polynomial times its reciprocal, and that is
        /// the product the polynomial case reads: <c>x/cos(x)^2</c> reached nothing where
        /// <c>x sec(x)^2</c> is one step of parts. The polynomial factors of the numerator are
        /// gathered wherever they sit, so <c>u sec(u)^2/tan(u)^2</c> is <c>u</c> against the rest.
        /// Not over an algebraic function, which is the rational integrator's or Euler's and has
        /// already been declined by both.
        /// </summary>
        [Theory]
        [InlineData("x/cos(x)^2", "x*sec(x)^2")]
        [InlineData("x/sin(x)^2", "x*csc(x)^2")]
        [InlineData("x*sin(x)^3/cos(x)^2", "x*sin(x)^3*sec(x)^2")]
        [InlineData("x*sec(x)^2/tan(x)^2", "x*csc(x)^2")]
        public void APolynomialOverTheBar(string quotient, string product)
        {
            DifferentiatesBack(quotient);
            DifferentiatesBack(product);
        }

        /// <summary>
        /// <c>ln|x|/x</c>, which a recorded verdict called unsolvable and named the logarithmic
        /// integral for. It is not: <c>Li(x)</c> is the antiderivative of <c>1/ln(x)</c>, and
        /// this one is <c>ln|x|^2/2</c>. What comes out is that up to a constant — on the
        /// negative axis <c>ln(x)</c> is <c>ln|x| + i*pi</c>, so the form differs by
        /// <c>pi^2/2</c> — and it differentiates back either side of zero.
        /// </summary>
        [Theory]
        [InlineData(0.37)]
        [InlineData(1.4)]
        [InlineData(-0.6)]
        [InlineData(-2.3)]
        public void TheLogarithmOverTheVariableIsElementary(double at)
        {
            var integrand = "ln(abs(x))/x".ToEntity();
            var integral = integrand.Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var got = integral.Substitute("C", 0).Differentiate("x").Substitute("x", at).EvalNumerical();
            var want = integrand.Substitute("x", at).EvalNumerical();
            var difference = Math.Abs((double)(got - want).RealPart)
                           + Math.Abs((double)(got - want).ImaginaryPart);
            Assert.True(difference / Math.Max(1.0, Math.Abs((double)want.RealPart)) < 1e-9,
                $"d/dx of the antiderivative of ln|x|/x is {got} at x = {at}, where it is {want}");
        }

        /// <summary>
        /// Two factors to differentiate is <b>not</b> this rule's, and the bound is what keeps it
        /// cheap rather than what keeps it tidy: with two, there is no reason to pick one, and
        /// what is left still holds the other, so the step has made nothing smaller.
        /// </summary>
        /// <remarks>
        /// Bounded by the integrator's own budget rather than by a wall clock, which measures the
        /// runner. Without the bound this integrand took 39 s to be declined where it takes about
        /// sixty milliseconds; here what is pinned is only that the verdict is not a wrong answer.
        /// </remarks>
        [Theory]
        [InlineData("x*arctan(x)*ln(x + sqrt(1 + x^2))/sqrt(1 + x^2)")]
        [InlineData("ln(x)*arctan(x)/x")]
        public void TwoFactorsToDifferentiateIsNotThisRules(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand);
        }

        /// <summary>
        /// What must not change: the products this already answered, and a quotient with no
        /// factor to differentiate first — which is every rational function, and the reason the
        /// rule asks for such a factor rather than trying every quotient.
        /// </summary>
        [Theory]
        [InlineData("x*ln(x)")]
        [InlineData("x*arctan(x)")]
        [InlineData("arcsin(x)")]
        [InlineData("x^2*ln(x)")]
        [InlineData("ln(x)/x^2")]
        [InlineData("x/(x^2 + 1)")]
        [InlineData("1/(x^2 + 1)")]
        [InlineData("x^3/(x + 1)")]
        [InlineData("e^x*sin(x)")]
        public void TheNeighboursAreUntouched(string integrand) => DifferentiatesBack(integrand);
    }
}

//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Globalization;
using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Rows of Rubi's independent suites that were declined where the pieces they come apart into
    /// were answered: Hearn's rational function over <c>P^2 - 2Q^2</c> of the fourteenth degree,
    /// Hebisch's <c>e^u/u du</c> under <c>u = x + e^x</c>, Welz's linears beside the root of
    /// <c>x^2 +- i</c> and the nested root that is two of them, Timofeev's symbolic power below a
    /// nested root, and Bronstein's partial fractions in <c>ln(x)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class SeamsOfTheIndependentSuitesIntegralTest
    {
        [Theory]
        // Hearn 365: the logarithms over Q(sqrt(2)), past the twelfth degree.
        [InlineData("(3+3*x-4*x^2-4*x^3-7*x^6+4*x^7+10*x^8+7*x^13)/(1+2*x-x^2-4*x^3-2*x^4-2*x^7-2*x^8+x^14)", "-2.7 -0.8 -0.3 0.2 0.5 1.1 2.3")]
        // Hebisch 15: Ei(x + e^x).
        [InlineData("(1+e^x)*e^(x+e^x)/(x+e^x)", "-2.3 -1.1 -0.2 0.4 1.3 2.1")]
        // Welz 44, 47 and 48, and a linear beside a root of a quadratic off the real line.
        [InlineData("1/((1+x)^2*sqrt(2)*sqrt(-i+x^2))+1/((1+x)^2*sqrt(2)*sqrt(i+x^2))", "-3.1 -1.6 -0.7 0.3 1.2 2.8")]
        [InlineData("sqrt(x^2+sqrt(1+x^4))/((1+x)^2*sqrt(1+x^4))", "-3.1 -1.6 -0.7 0.3 1.2 2.8")]
        [InlineData("sqrt(x^2+sqrt(1+x^4))/((1+x)*sqrt(1+x^4))", "-3.1 -1.6 -0.7 0.3 1.2 2.8")]
        [InlineData("x/((2+x)^2*sqrt(x^2+3*i))", "-3.1 -1.6 -0.7 0.3 1.2 2.8")]
        // Timofeev 448, with n pinned to a whole number so that both sides of zero are real.
        [InlineData("1/((1+x^(2*n))*(-x^2+(1+x^(2*n))^(1/n))^(1/2))", "-2.1 -0.9 -0.4 0.3 0.8 1.7")]
        // Bronstein 22: li(x) and two logarithmic derivatives.
        [InlineData("(-x^2-ln(x)+2*ln(x)^2)/(-x^2*ln(x)+ln(x)^3)", "0.2 0.4 0.8 1.3 2.1 3.7")]
        public void DifferentiatesBackToTheIntegrand(string integrand, string points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("n", 2);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in points.Split(' ').Select(point => double.Parse(point, CultureInfo.InvariantCulture)))
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        [Theory]
        // Past the twelfth degree with residues in a field of degree past two, still declined, as
        // the residues' field is seen modulo a prime: the squarefree decomposition of their
        // resultant over the integers was a minute.
        [InlineData("x/(x^14+3*x+1)")]
        [InlineData("(x^3+1)/(x^15+2*x^2+7)")]
        [InlineData("1/((x^7-x-1)^2-2*(x+x^2)^2)")]
        public void AHighDegreeWithResiduesPastAQuadraticFieldIsDeclined(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());

        [Theory]
        // Answered before as they are now, by the routes that answered them: a power of x below
        // the root off the real line, and a polynomial beside the nested root.
        [InlineData("1/(x*sqrt(x^2+i))", "arctan")]
        [InlineData("sqrt(x^2+sqrt(1+x^4))*x/sqrt(1+x^4)", "x ^ 2) ^ 2")]
        public void ASolvedNeighbourKeepsItsForm(string integrand, string written)
        {
            var text = integrand.ToEntity().Integrate("x").Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.Contains(written, text);
        }
    }
}

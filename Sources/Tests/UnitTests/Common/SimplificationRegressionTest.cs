//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Regression tests for simplification, factoring and integration.
    /// Each test names the issue it locks down, so a future refactor that
    /// reintroduces the bug fails loudly.
    /// </summary>
    public sealed class SimplificationRegressionTest
    {
        // https://github.com/asc-community/AngouriMath/issues/205
        // Radicands were never reduced to a square-free part, so like radicals could
        // never be collected: sqrt(12) + sqrt(27) stayed as written instead of
        // 5*sqrt(3). Pulling perfect powers out from under the root makes the terms
        // share a radical, and the existing collection rules take it from there.
        [Theory]
        [InlineData("sqrt(12) + sqrt(27)", "5 * sqrt(3)")]
        [InlineData("sqrt(8) + sqrt(2)", "3 * sqrt(2)")]
        [InlineData("sqrt(18) + sqrt(8)", "5 * sqrt(2)")]
        [InlineData("sqrt(50) - sqrt(2)", "4 * sqrt(2)")]
        [InlineData("sqrt(20) + sqrt(45) + sqrt(5)", "6 * sqrt(5)")]
        [InlineData("cbrt(54) + cbrt(2)", "4 * cbrt(2)")]
        public void Issue205_LikeRadicalsCollect(string input, string expected)
        {
            var simplified = input.ToEntity().Simplify();
            // Compared by difference, not by form: the collected result comes out as
            // `sqrt(3) * 5` where the expectation reads `5 * sqrt(3)`.
            Assert.Equal(Entity.Number.Integer.Create(0),
                (simplified - expected.ToEntity()).Simplify());
            // ...but it must genuinely have collected into a single term, which is the
            // point of the issue.
            Assert.DoesNotContain("+", simplified.Stringize());
            Assert.DoesNotContain("-", simplified.Stringize());
        }

        // A standalone surd is a separate matter: Simplify picks between candidate
        // forms by a complexity metric, and by that metric `sqrt(12)` beats
        // `2 * sqrt(3)`, so the reduced form is generated but not selected. Changing
        // that is a change to the metric, not to this rule. Pinning the current
        // behaviour so the distinction is deliberate rather than accidental.
        [Theory]
        [InlineData("sqrt(12)")]
        [InlineData("sqrt(18)")]
        public void Issue205_StandaloneSurdsKeepTheShorterForm(string input) =>
            Assert.Equal(input.ToEntity(), input.ToEntity().Simplify());

        // Whatever form comes out, it must still be the same number.
        [Theory]
        [InlineData("sqrt(8)")]
        [InlineData("sqrt(12) + sqrt(27)")]
        [InlineData("sqrt(1000)")]
        [InlineData("cbrt(54)")]
        [InlineData("sqrt(2)")]     // already square-free, must not change
        [InlineData("sqrt(4)")]     // perfect square
        [InlineData("1 / sqrt(8)")] // negative exponent
        public void Issue205_ReductionPreservesValue(string input)
        {
            var before = input.ToEntity().EvalNumerical().RealPart.EDecimal.ToDouble();
            var after = input.ToEntity().Simplify().EvalNumerical().RealPart.EDecimal.ToDouble();
            Assert.Equal(before, after, 10);
        }

        [Theory]
        [InlineData("sqrt(2)")]
        [InlineData("sqrt(3)")]
        [InlineData("sqrt(6)")]
        public void Issue205_SquareFreeRadicalsAreLeftAlone(string input) =>
            Assert.Equal(input.ToEntity(), input.ToEntity().Simplify());

        // https://github.com/asc-community/AngouriMath/issues/403
        // Fractions over a common denominator were cross-multiplied into d*d and left to
        // the simplifier to cancel back down. Since the rule calls Simplify on both halves
        // from inside Simplify's own pass, each further fraction squared the denominator
        // before anything cancelled: two terms took 25 ms, three never returned.
        [Theory]
        [InlineData("1 / (x + y + z) * dx + 1 / (x + y + z) * dy + 1 / (x + y + z) * dz", "(dx + dy + dz) / (x + y + z)")]
        [InlineData("a / (x + y) + b / (x + y) + c / (x + y)", "(a + b + c) / (x + y)")]
        [InlineData("a / (x + y + z) + b / (x + y + z) + c / (x + y + z) + d / (x + y + z)", "(a + b + c + d) / (x + y + z)")]
        public void Issue403_FractionsOverACommonDenominatorCollapse(string input, string expected) =>
            Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        // The general case still brings unlike denominators together, which needs the
        // numerator fully simplified to reach 0.
        [Fact]
        public void Issue403_UnlikeDenominatorsStillCombine() =>
            Assert.Equal("0 provided not (b - 1) * (1 - b) = 0".ToEntity(),
                "(a - 1) / (1 - b) - (1 - a) / (b - 1)".ToEntity().Simplify());

        // Three separate defects sat on top of each other in the integrator,
        // so each gets its own test below.
        //
        // 1. SolveAsPolynomialTerm, SolveLogarithmic and SolveBySubstitution recursed
        //    without handing on `integrateByParts`, so a solver re-enabled integration by
        //    parts one level below the call that had switched it off. By parts calls back
        //    into the dispatcher, so that is a cycle: x * ln(x) rode it into a stack
        //    overflow, and sin(x)^2 and cos(x)^2 spun past 20 seconds.
        [Theory]
        [InlineData("x * ln(x)")]
        [InlineData("sin(x) ^ 2")]
        [InlineData("cos(x) ^ 2")]
        public void IntegrationTerminates(string integrand)
        {
            var task = System.Threading.Tasks.Task.Run(() => integrand.ToEntity().Integrate("x"));
            Assert.True(task.Wait(System.TimeSpan.FromSeconds(30)),
                $"integrating {integrand} did not terminate");
        }

        // 2. Substitution candidates were not required to depend on x. `ln(e)`, picked out
        //    of any integrand carrying a logarithm, has derivative 0, and dividing by it
        //    gave a NaN that contained no x -- so it passed the "substitution succeeded"
        //    test and was returned as the antiderivative.
        [Theory]
        [InlineData("x * ln(x)")]
        [InlineData("ln(x)")]
        [InlineData("x * e ^ x")]
        public void AntiderivativesAreFreeOfNaN(string integrand) =>
            Assert.DoesNotContain("NaN", integrand.ToEntity().Integrate("x").Stringize());

        // 3. For a logarithm times a polynomial, by parts differentiated the polynomial,
        //    which leaves the logarithm to integrate and puts the original integral back
        //    in front of the solver. Differentiating the logarithm terminates instead.
        [Theory]
        [InlineData("x * ln(x)")]
        [InlineData("x ^ 2 * ln(x)")]
        [InlineData("ln(x) * x")]
        // and the orderings that already worked must keep working
        [InlineData("x * e ^ x")]
        [InlineData("x * sin(x)")]
        [InlineData("x ^ 2 * e ^ x")]
        public void IntegrationByPartsAnswersAreCorrect(string integrand)
        {
            var antiderivative = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", antiderivative.Stringize());
            // Differentiating back must return the integrand. The residual may carry the
            // domain conditions of the antiderivative, which are not what is under test.
            var residual = (antiderivative.Differentiate("x") - integrand.ToEntity()).Simplify();
            while (residual is Entity.Providedf(var inner, _)) residual = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), residual);
        }

        // https://github.com/asc-community/AngouriMath/issues/531
        // The factoring rules only ever looked at two adjacent terms of the sum tree, so a
        // sum of four was left half-factored at a*(c + d) + b*c + b*d.
        [Theory]
        [InlineData("a * c + a * d + b * c + b * d", "(a + b) * (c + d)")]
        [InlineData("a * b + a * c + a * d", "a * (b + c + d)")]
        [InlineData("x * a + x * b + y * a + y * b + z * a + z * b", "(a + b) * (x + y + z)")]
        public void Issue531_TermsCollectOverTheWholeSum(string input, string expected) =>
            Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        // Whatever form comes out, it has to be the same expression.
        [Theory]
        [InlineData("a * c + a * d + b * c + b * d")]
        [InlineData("x * a + x * b + y * a + y * b + z * a + z * b")]
        [InlineData("a * b + a * c + a * d")]
        public void Issue531_CollectionPreservesValue(string input)
        {
            var difference = (input.ToEntity() - input.ToEntity().Simplify()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        // https://github.com/asc-community/AngouriMath/issues/178
        // Factorize left its own workings in the answer: the difference-of-squares rule
        // halved the exponents without checking they were even, so it fired again on the
        // linear factors it had just produced and turned x^2 - y^2 into
        // (sqrt(x) - sqrt(y)) * (sqrt(x) + sqrt(y)) * (x^1 + y^1). Nothing simplified the
        // result afterwards either, which is where the x^1 and the sqrt(4) survived.
        [Theory]
        [InlineData("4 * x ^ 2 - 4 * y ^ 2", "4 * (x - y) * (x + y)")]
        [InlineData("x ^ 2 - y ^ 2", "(x - y) * (x + y)")]
        [InlineData("x ^ 2 - 4", "(x - 2) * (x + 2)")]
        [InlineData("x ^ 4 - y ^ 4", "(x - y) * (x + y) * (x ^ 2 + y ^ 2)")]
        // Compared as printed text: what is under test is the form of the answer, and
        // the tree differs from the parsed expectation only in how the product associates.
        public void Issue178_FactorizeLeavesNoResidue(string input, string expected) =>
            Assert.Equal(expected, input.ToEntity().Factorize().Stringize());

        // Odd exponents must not be halved -- that is what introduced the radicals.
        [Fact]
        public void Issue178_OddPowersAreNotSplit() =>
            Assert.Equal("x ^ 3 - y ^ 5".ToEntity(), "x ^ 3 - y ^ 5".ToEntity().Factorize());

        [Theory]
        [InlineData("4 * x ^ 2 - 4 * y ^ 2")]
        [InlineData("x ^ 4 - y ^ 4")]
        [InlineData("x ^ 6 - y ^ 6")]
        [InlineData("x ^ 2 - 4")]
        public void Issue178_FactorizationPreservesValue(string input)
        {
            var difference = (input.ToEntity() - input.ToEntity().Factorize()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        // Dividing the integrand by a derivative that is zero gives NaN, and NaN
        // contains no x, which is exactly the test for a successful ubstitution --
        // so it was returned as the answer. The first guard caught the cases where
        // the derivative is visibly zero; these two are only zero after simplification,
        // since d/dx (sin(x)^2 + cos(x)^2) is written as 2sin(x)cos(x) - 2cos(x)sin(x).
        [Theory]
        [InlineData("sin(x) ^ 2 + cos(x) ^ 2")]
        [InlineData("x / (x + 1) + 1 / (x + 1)")]
        [InlineData("x * ln(x)")]
        public void AntiderivativesNeverContainNaN(string integrand) =>
            Assert.DoesNotContain("NaN", integrand.ToEntity().Integrate("x").Stringize());

        // A negative factor under the line was taken out by multiplying by it and inverting
        // what was left, so a / (-b * c) came back as -(b * (c / a)) -- the reciprocal of the
        // right answer. Simplify keeps whichever candidate is shortest, so the inverted form
        // won wherever it was smaller, which is wherever the numerator was 1.
        //
        // Checked numerically as well as symbolically: an inverted answer differs from the
        // original by a factor, and subtracting the two can simplify to something that only
        // looks like zero if the same wrong rule fires on the difference.
        [Theory]
        [InlineData("(1/x) / (-1 - 1/x)", 2.0, -1.0 / 3)]
        [InlineData("(1/x) / (-1 - 1/x^2)", 2.0, -0.4)]
        [InlineData("1 / (x * (-1 - 1/x))", 2.0, -1.0 / 3)]
        [InlineData("(1/x) / (-1 - y^2/x^2)", 2.0, -0.4)]
        [InlineData("1 / (-2 * x)", 2.0, -0.25)]
        public void ANegativeFactorUnderTheLineStaysUnderIt(string input, double at, double expected)
        {
            var simplified = input.ToEntity().Simplify();
            var value = simplified.Substitute("x", at).Substitute("y", 1).EvalNumerical();
            Assert.True(System.Math.Abs(((Entity.Number.Complex)value).RealPart.EDecimal.ToDouble() - expected) < 1e-9,
                $"{input} simplified to {simplified.Stringize()}, which is {value.Stringize()} at x = {at}");
        }
    }
}

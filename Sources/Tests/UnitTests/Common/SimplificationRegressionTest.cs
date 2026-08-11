//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using System.Linq;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Regression tests for simplification, factoring and integration.
    /// Each test names the issue it locks down, so a future refactor that
    /// reintroduces the bug fails loudly.
    /// </summary>
    [Trait("Area", "Common")]
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

        // https://github.com/asc-community/AngouriMath/issues/281
        // A standalone surd used to keep the form it was written in, because Simplify
        // picks between candidates by a complexity metric and by that metric `sqrt(12)`
        // beats `2 * sqrt(3)`. The metric is the wrong judge here: the reduced radicand
        // is the canonical form every other system prints -- sympy, Mathematica and
        // Maxima all answer 2*sqrt(3) -- and it is what makes two surds comparable
        // without simplifying their difference. So the reduction now happens where the
        // power itself is built rather than being offered as a candidate.
        [Theory]
        [InlineData("sqrt(12)", "2 * sqrt(3)")]
        [InlineData("sqrt(18)", "3 * sqrt(2)")]
        [InlineData("sqrt(1/2)", "sqrt(2) / 2")]
        [InlineData("(2 ^ 3 * 5 ^ 7) ^ (1/3)", "50 * 5 ^ (1/3)")]
        [InlineData("12 ^ (5/2)", "288 * sqrt(3)")]
        // A negative exponent is the same question asked of the reciprocal, and it has to give
        // the same answer: 1/sqrt(2) and sqrt(1/2) are one number and printed differently only
        // because one of them was declined.
        [InlineData("1 / sqrt(2)", "sqrt(2) / 2")]
        [InlineData("2 ^ (-1/2)", "sqrt(2) / 2")]
        [InlineData("1 / sqrt(12)", "sqrt(3) / 6")]
        [InlineData("1 / sqrt(8)", "sqrt(2) / 4")]
        [InlineData("1 / cbrt(2)", "4 ^ (1/3) / 2")]
        [InlineData("x / sqrt(2)", "x * sqrt(2) / 2")]
        [InlineData("8 ^ (-1/3)", "1/2")]
        public void Issue281_PerfectPowersComeOutFromUnderTheRoot(string input, string expected) =>
            Assert.Equal(Entity.Number.Integer.Create(0),
                (input.ToEntity().Simplify() - expected.ToEntity()).Simplify());

        // A negative radicand is not reduced: the extraction would have to choose a branch,
        // and which branch a negative base takes is already settled elsewhere -- the odd
        // roots take the real one, so (-8)^(1/3) is -2 and not 1 + i*sqrt(3). Reducing the
        // radicand under an even root would be picking that same choice by accident.
        [Fact]
        public void Issue281_NegativeRadicandsAreNotReduced() =>
            Assert.Equal("sqrt(-12)", "(-12) ^ (1/2)".ToEntity().Simplify().Stringize());

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

        // https://github.com/asc-community/AngouriMath/issues/55
        // A quotient of polynomials only ever reached lowest terms where the common
        // factor was one the pattern rules could see written out. In one variable the
        // factoring rules found most of them; in several there was nothing at all, so
        // (x + y)^2 / ((x - y)(x + y)) came back long-divided as
        // 1 + (2y^2 + 2xy)/(x^2 - y^2) -- still carrying the factor it should have lost.
        [Theory]
        [InlineData("(x ^ 2 + 2 * x * y + y ^ 2) / (x ^ 2 - y ^ 2)", "(x + y) / (x - y)")]
        [InlineData("(x ^ 2 - 2 * x * y + y ^ 2) / (x ^ 2 - y ^ 2)", "(x - y) / (x + y)")]
        [InlineData("(a ^ 2 - b ^ 2) / (a ^ 2 + 2 * a * b + b ^ 2)", "(a - b) / (a + b)")]
        [InlineData("(x ^ 2 * y - y ^ 3) / (x * y + y ^ 2)", "x - y")]
        [InlineData("(x ^ 2 * y ^ 2 - 1) / (x * y - 1)", "x * y + 1")]
        [InlineData("(x ^ 3 - y ^ 3) / (x ^ 2 - y ^ 2)", "(x ^ 2 + x * y + y ^ 2) / (x + y)")]
        public void Issue55_MultivariateQuotientsReachLowestTerms(string input, string expected)
        {
            var simplified = input.ToEntity().Simplify();
            var bare = simplified;
            while (bare is Entity.Providedf(var inner, _)) bare = inner;

            // Equal as expressions, wherever both are defined...
            var difference = (bare - expected.ToEntity()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);

            // ...and no bigger than the reduced form, which is what the issue is about:
            // the answer it complains of is the same number too, just not in lowest terms.
            Assert.True(bare.Nodes.Count() <= expected.ToEntity().Nodes.Count(), bare.Stringize());
        }

        // Cancelling widens the domain -- at x = -y the quotient was 0/0 where the
        // reduced form is 0 -- so the condition has to travel with the answer.
        [Fact]
        public void Issue55_CancellationKeepsTheDomainCondition()
        {
            var simplified = "(x ^ 2 + 2 * x * y + y ^ 2) / (x ^ 2 - y ^ 2)".ToEntity().Simplify();
            var atRemovedFactor = simplified.Substitute("x", 1).Substitute("y", -1).Evaled;
            Assert.Equal(MathS.NaN, atRemovedFactor);
        }

        // A common divisor that is not there must not be found. These are all coprime,
        // and the point of the check is that nothing is cancelled out of them.
        [Theory]
        [InlineData("(x + y) / (x - y)")]
        [InlineData("(x ^ 2 + y ^ 2) / (x + y)")]
        [InlineData("(x + 1) / (y + 1)")]
        public void Issue55_CoprimeQuotientsAreLeftAlone(string input) =>
            Assert.Equal(input.ToEntity(), input.ToEntity().Simplify());

        // The strongest thing that can be said about a computed divisor is that the
        // quotient still takes the same values. Sampled at whole points, where a divisor
        // that did not really divide shows up as a different number.
        [Theory]
        [InlineData("(x ^ 2 + 2 * x * y + y ^ 2) / (x ^ 2 - y ^ 2)")]
        [InlineData("(x ^ 3 - y ^ 3) / (x ^ 2 - y ^ 2)")]
        [InlineData("(a ^ 2 - b ^ 2) / (a ^ 2 + 2 * a * b + b ^ 2)")]
        [InlineData("(x ^ 4 - y ^ 4) / (x ^ 2 - 2 * x * y + y ^ 2)")]
        [InlineData("(x ^ 3 + x ^ 2 * y - x * y ^ 2 - y ^ 3) / (x ^ 2 - y ^ 2)")]
        [InlineData("(x ^ 2 * y ^ 2 - 1) / (x * y - 1)")]
        [InlineData("(6 * x ^ 2 - 6 * y ^ 2) / (4 * x + 4 * y)")]
        public void Issue55_CancellationPreservesValue(string input)
        {
            var original = input.ToEntity();
            var simplified = original.Simplify();
            var variables = original.Vars.OrderBy(variable => variable.Name).ToArray();
            foreach (var (first, second) in new[] { (3, 5), (7, 2), (-4, 9), (11, -6) })
            {
                Entity Bind(Entity expr) =>
                    expr.Substitute(variables[0], first).Substitute(variables[1], second);
                Assert.Equal(Bind(original).EvalNumerical(), Bind(simplified).EvalNumerical());
            }
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

        // https://github.com/asc-community/AngouriMath/issues/569
        // https://github.com/asc-community/AngouriMath/issues/179
        // The trigonometric tables were only ever read forwards, so sin(pi/4) was sqrt(2)/2
        // but arcsin(sqrt(2)/2) stayed as written. arctan was the exception, and only at
        // +-1, which is why arctan(1) already answered pi/4.
        [Theory]
        [InlineData("arcsin(1/2)", "pi / 6")]
        [InlineData("arcsin(sqrt(2) / 2)", "pi / 4")]
        [InlineData("arcsin(sqrt(3) / 2)", "pi / 3")]
        [InlineData("arcsin(1)", "pi / 2")]
        [InlineData("arcsin(0)", "0")]
        [InlineData("arccos(1/2)", "pi / 3")]
        [InlineData("arccos(0)", "pi / 2")]
        [InlineData("arccos(1)", "0")]
        [InlineData("arctan(sqrt(3))", "pi / 3")]
        [InlineData("arctan(sqrt(3) / 3)", "pi / 6")]
        [InlineData("arccotan(1)", "pi / 4")]
        [InlineData("arccotan(sqrt(3))", "pi / 6")]
        // The 5ths, 8ths, 10ths and 12ths of the circle, which are in the forward table too
        [InlineData("arcsin((sqrt(5) - 1) / 4)", "pi / 10")]
        [InlineData("arcsin(sqrt(2 - sqrt(2)) / 2)", "pi / 8")]
        [InlineData("arcsin((sqrt(6) - sqrt(2)) / 4)", "pi / 12")]
        [InlineData("arctan(2 - sqrt(3))", "pi / 12")]
        [InlineData("arctan(sqrt(2) - 1)", "pi / 8")]
        [InlineData("arctan(sqrt(5 - 2 * sqrt(5)))", "pi / 5")]
        // Both functions are odd, and arccos/arccotan take the complement
        [InlineData("arcsin(-1/2)", "-pi / 6")]
        [InlineData("arctan(-sqrt(3))", "-pi / 3")]
        [InlineData("arccos(-1/2)", "2 * pi / 3")]
        [InlineData("arccos(-1)", "pi")]
        public void Issue569_InverseTrigonometryReadsTheTableBackwards(string input, string expected) =>
            Assert.Equal(Entity.Number.Integer.Create(0),
                (input.ToEntity().Simplify() - expected.ToEntity()).Simplify());

        // A value that is merely near a table entry is not that entry, and one that is not in
        // the table at all has no closed form to give. `arcsin(0.4999999)` in particular must
        // not come back pi/6: the double comparison is a sieve, not the answer.
        [Theory]
        [InlineData("arcsin(4999999/10000000)", "arcsin")]
        [InlineData("arcsin(1/3)", "arcsin")]
        [InlineData("arctan(2)", "arctan")]
        [InlineData("arcsin(x)", "arcsin")]
        [InlineData("arcsin(2)", "arcsin")]           // outside the real branch
        public void Issue569_NonTableValuesAreLeftAlone(string input, string stillThere)
        {
            var simplified = input.ToEntity().Simplify().Stringize();
            Assert.Contains(stillThere, simplified);
            Assert.DoesNotContain("pi", simplified);
        }

        // And the angle that comes back must be the angle that was asked for.
        [Theory]
        [InlineData("arcsin(1/2)")]
        [InlineData("arccos(1/2)")]
        [InlineData("arctan(sqrt(3))")]
        [InlineData("arccotan(1)")]
        [InlineData("arcsin(-sqrt(2) / 2)")]
        [InlineData("arccos(-sqrt(3) / 2)")]
        public void Issue569_RecognisedAnglesKeepTheirValue(string input)
        {
            var before = input.ToEntity().EvalNumerical().RealPart.EDecimal.ToDouble();
            var after = input.ToEntity().Simplify().EvalNumerical().RealPart.EDecimal.ToDouble();
            Assert.Equal(before, after, 12);
        }

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

        // Cancelling a quotient of factorials can leave a sum -- (x + 1)! / x! is x + 1 --
        // and SmartExpandOver, which is documented not to take a sum, was handed the result
        // and threw AngouriBugException out of a public method.
        // https://github.com/asc-community/AngouriMath/issues/817
        //
        // Asserted as a value rather than as a string: expanding must not change what the
        // expression is worth, and the point it is checked at is one where the factorials
        // are defined and the quotient is a whole number.
        [Theory]
        [InlineData("(x + 1)! / x!", 4, 5)]
        [InlineData("(x + 2)! / x!", 4, 30)]
        [InlineData("(x + 3)! / x!", 4, 210)]
        [InlineData("x! / (x + 1)!", 4, 0.2)]
        [InlineData("(x + 1)! / x! + y", 4, 9)]
        [InlineData("((x + 1)! / x!) * (a + b)", 4, 40)]
        public void ExpandKeepsTheValueOfAQuotientOfFactorials(string input, int at, double expected)
        {
            var expanded = input.ToEntity().Expand();
            var value = expanded.Substitute("x", at).Substitute("y", 4).Substitute("a", 3).Substitute("b", 5).EvalNumerical();
            var real = ((Entity.Number.Complex)value).RealPart.EDecimal.ToDouble();
            Assert.True(System.Math.Abs(real - expected) < 1e-9,
                $"{input} expanded to {expanded.Stringize()}, which is {value.Stringize()} at x = {at} rather than {expected}");
        }

        // The cancellation is what makes the expansion possible at all, so the answer should
        // no longer mention a factorial where the two cancel completely.
        [Theory]
        [InlineData("(x + 1)! / x!")]
        [InlineData("(x + 2)! / x!")]
        [InlineData("(x + y + 1)! / (x + y)!")]
        public void ExpandCancelsAQuotientOfFactorialsRatherThanLeavingIt(string input)
            => Assert.DoesNotContain(input.ToEntity().Expand().Nodes, node => node is Entity.Factorialf);

        // https://github.com/asc-community/AngouriMath/issues/876
        // There was one excluded-middle rule and it matched the negation on the left operand
        // only. `or` is commutative, so the same proposition had two answers depending on
        // which side it was written: `not (x < 0) or (x < 0)` was True while
        // `(x < 0) or not (x < 0)` was left as written. A bare variable hid it, because the
        // boolean minimiser reduces those whichever way round they are — it takes a
        // comparison, which the minimiser does not treat as an atom, to see it.
        [Theory]
        [InlineData("not (x < 0) or (x < 0)")]
        [InlineData("(x < 0) or not (x < 0)")]
        [InlineData("not (x > 0) or (x > 0)")]
        [InlineData("(x > 0) or not (x > 0)")]
        [InlineData("not (x <= 0) or (x <= 0)")]
        [InlineData("(x <= 0) or not (x <= 0)")]
        [InlineData("not (a = b) or (a = b)")]
        [InlineData("(a = b) or not (a = b)")]
        [InlineData("not (x in RR) or (x in RR)")]
        [InlineData("(x in RR) or not (x in RR)")]
        [InlineData("not p or p")]
        [InlineData("p or not p")]
        public void ExcludedMiddleHoldsWhicheverOperandCarriesTheNegation(string input)
            => Assert.Equal(Entity.Boolean.True, input.ToEntity().Simplify());
    }
}

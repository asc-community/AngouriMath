//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Algebra.Polynomials
{
    /// <summary>
    /// The multivariate polynomial greatest common divisor, and the cancellation it drives —
    /// <a href="https://github.com/asc-community/AngouriMath/issues/55">#55</a>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The divisor itself is checked against <see cref="PolynomialGcd"/> directly, because the
    /// only thing the public surface shows of one is the condition a cancellation carries, and
    /// half the surface — a zero argument, a constant one, the ceilings — is not reachable
    /// from a quotient at all.
    /// </para>
    /// <para>
    /// The cancellation is checked through <see cref="RewriteRules.PolynomialGcdCancellation"/>
    /// rather than through <c>Simplify</c>: that rule set is the gcd's only caller, so a
    /// failure names it rather than the dozen other rule sets a simplification runs, and
    /// nothing else gets to rearrange the answer before it is read.
    /// </para>
    /// </remarks>
    [Trait("Area", "Algebra")]
    public sealed class MultivariateGcdTest
    {
        #region Reading polynomials in

        static Dictionary<Variable, int> Index(string[] variables)
        {
            var index = new Dictionary<Variable, int>(variables.Length);
            for (var i = 0; i < variables.Length; i++)
                index[(Variable)variables[i]] = i;
            return index;
        }

        static Variable[] Vars(string[] names)
        {
            var variables = new Variable[names.Length];
            for (var i = 0; i < names.Length; i++)
                variables[i] = (Variable)names[i];
            return variables;
        }

        static MultivariatePolynomial Polynomial(string source, params string[] variables)
        {
            var parsed = MultivariatePolynomial.TryParse(source.ToEntity(), Index(variables));
            Assert.True(parsed is not null, $"{source} was not read as a polynomial");
            return parsed!;
        }

        static MultivariatePolynomial? Gcd(MultivariatePolynomial left, MultivariatePolynomial right, int variableCount)
        {
            var order = new int[variableCount];
            for (var i = 0; i < order.Length; i++)
                order[i] = i;
            return PolynomialGcd.Gcd(left, right, order, 0);
        }

        #endregion

        #region The defining property

        /// <summary>
        /// The two halves of "greatest common divisor": it divides both, and what is left over
        /// shares nothing further. Together those say it is the greatest one, so nothing else
        /// has to be assumed about it.
        /// </summary>
        /// <remarks>
        /// The quotients are multiplied back with the operands the other way round from the
        /// check the implementation makes, and the expected divisor every caller passes is
        /// built by reading a product in rather than by dividing anything — asking the
        /// division to confirm its own quotient would only be asking it to agree with itself.
        /// </remarks>
        static void AssertIsGreatestCommonDivisor(
            MultivariatePolynomial left, MultivariatePolynomial right,
            MultivariatePolynomial divisor, int variableCount)
        {
            var leftQuotient = left.DivideExact(divisor);
            var rightQuotient = right.DivideExact(divisor);
            Assert.True(leftQuotient is not null, "the divisor does not divide the first argument");
            Assert.True(rightQuotient is not null, "the divisor does not divide the second argument");
            Assert.True(divisor.Multiply(leftQuotient!)?.SameAs(left) is true,
                "the first quotient does not multiply back");
            Assert.True(divisor.Multiply(rightQuotient!)?.SameAs(right) is true,
                "the second quotient does not multiply back");

            var leftOver = Gcd(leftQuotient!, rightQuotient!, variableCount);
            Assert.True(leftOver is not null, "the gcd of the two quotients declined");
            Assert.True(leftOver!.IsConstant, "the two quotients still share a factor");
        }

        /// <summary>
        /// <paramref name="expected"/> is what the gcd answers, and it really is the gcd. Both
        /// halves are needed: the first would pass an answer that is systematically too small
        /// if the expectation were computed the same way, and the second would pass any answer
        /// at all if the same blindness lost the factor left over.
        /// </summary>
        static void AssertGcd(string expected, string left, string right, params string[] variables)
        {
            var first = Polynomial(left, variables);
            var second = Polynomial(right, variables);
            var divisor = Gcd(first, second, variables.Length);
            Assert.True(divisor is not null, $"gcd({left}, {right}) declined");
            Assert.True(divisor!.SameAs(Polynomial(expected, variables)),
                $"gcd({left}, {right}) is {divisor.ToEntity(Vars(variables)).Stringize()}, expected {expected}");
            AssertIsGreatestCommonDivisor(first, second, divisor, variables.Length);
        }

        #endregion

        #region Divisors computed by hand

        [Theory]
        // One variable.
        [InlineData("x - 1", "x ^ 2 - 1", "x ^ 2 - 2 * x + 1", "x")]
        [InlineData("x ^ 2 + x + 1", "x ^ 3 - 1", "x ^ 4 + x ^ 2 + 1", "x")]
        // Two variables: #55's own example and its neighbours.
        [InlineData("x + y", "x ^ 2 + 2 * x * y + y ^ 2", "x ^ 2 - y ^ 2", "x", "y")]
        [InlineData("x - y", "x ^ 3 - y ^ 3", "x ^ 2 - y ^ 2", "x", "y")]
        // Three variables, the common factor carrying all three.
        [InlineData("x + y + z", "(x + y + z) * (x - y)", "(x + y + z) * (y - z)", "x", "y", "z")]
        [InlineData("x * y - z", "(x * y - z) * (x + z)", "(x * y - z) * (y + z)", "x", "y", "z")]
        // Four variables.
        [InlineData("x + y + z + w", "(x + y + z + w) * (x - y)", "(x + y + z + w) * (z - w)", "x", "y", "z", "w")]
        [InlineData("x * y - z * w", "(x * y - z * w) * (x + w)", "(x * y - z * w) * (y + z)", "x", "y", "z", "w")]
        // One argument divides the other, so one quotient comes out 1.
        [InlineData("x ^ 2 - y ^ 2", "x ^ 2 - y ^ 2", "(x ^ 2 - y ^ 2) * (x + 2 * y)", "x", "y")]
        [InlineData("(x + y) * (x + z)", "(x + y) * (x + z) * (y + z)", "(x + y) * (x + z)", "x", "y", "z")]
        // The common factor lives in the content -- the part free of the main variable -- so
        // it is only found once the content has been split off and recursed into.
        [InlineData("x * y + y", "2 * x * y + 2 * y", "3 * x * y + 3 * y", "x", "y")]
        [InlineData("x * y + y", "x ^ 2 * y - y", "x ^ 2 * y * z + x * y * z", "x", "y", "z")]
        [InlineData("y * z + z", "(y * z + z) * (x ^ 2 + 1)", "(y * z + z) * (x - 1)", "x", "y", "z")]
        // Only one variable is shared, so it is the only one a common factor may carry.
        [InlineData("y + 1", "(y + 1) * (x + 2)", "(y + 1) * (z + 3)", "x", "y", "z")]
        [InlineData("y ^ 2 - 1", "(y ^ 2 - 1) * (x ^ 2 + x + 1)", "(y ^ 2 - 1) * (z ^ 3 + 2)", "x", "y", "z")]
        public void KnownDivisor(string expected, string left, string right, params string[] variables) =>
            AssertGcd(expected, left, right, variables);

        [Theory]
        // Coprime: the greatest common divisor is a unit, and over Q that is written 1.
        [InlineData("x + y", "x - y", "x", "y")]
        [InlineData("x ^ 2 + y ^ 2", "x + y", "x", "y")]
        [InlineData("x * y + 1", "x * y - 1", "x", "y")]
        [InlineData("x + y + z", "x + y - z", "x", "y", "z")]
        [InlineData("(x + y) * (x + z)", "(x - y) * (x - z)", "x", "y", "z")]
        // No variable in common: the two lie in rings meeting only in the constants.
        [InlineData("x + 1", "y + 1", "x", "y")]
        [InlineData("x ^ 3 - 1", "y ^ 3 - 1", "x", "y")]
        public void CoprimeArgumentsGiveOne(string left, string right, params string[] variables) =>
            AssertGcd("1", left, right, variables);

        #endregion

        #region The construction-based sweep

        /// <summary>
        /// Pairwise coprime and irreducible, each primitive over the integers with a positive
        /// leading coefficient in the lexicographic order the gcd normalizes to. That is what
        /// lets an expected answer below be written as a plain product: by Gauss's lemma a
        /// product of such polynomials is again one, so it is already its own normal form and
        /// the assertion can be equality rather than equality up to a unit.
        /// </summary>
        static string[] Factors => new[]
        {
            "a + b",              //  0
            "a - b",              //  1
            "a + c",              //  2
            "a - d",              //  3
            "b + c + 1",          //  4
            "b - d",              //  5
            "c + d + 2",          //  6
            "a * b - 1",          //  7
            "a * c + 1",          //  8
            "b * c - d",          //  9
            "a + b + c + d",      // 10
            "a ^ 2 + b * c + d",  // 11
        };

        static string[] SweepVariables => new[] { "a", "b", "c", "d" };

        /// <summary>
        /// Pairs of index lists into <see cref="Factors"/>, and the indices their two products
        /// share. Written out rather than generated, so that the case list is the same on
        /// every machine and every run.
        /// </summary>
        static IEnumerable<(int[] Left, int[] Right, int[] Shared)> Pairs()
        {
            yield return (new[] { 0 }, new[] { 1 }, new int[0]);
            yield return (new[] { 1 }, new[] { 2 }, new int[0]);
            yield return (new[] { 7 }, new[] { 0 }, new int[0]);
            yield return (new[] { 4 }, new[] { 7 }, new int[0]);
            yield return (new[] { 9 }, new[] { 6 }, new int[0]);
            yield return (new[] { 10 }, new[] { 11 }, new int[0]);
            yield return (new[] { 11 }, new[] { 0 }, new int[0]);
            yield return (new[] { 0, 1 }, new[] { 1, 2 }, new[] { 1 });
            yield return (new[] { 0, 4 }, new[] { 2, 4 }, new[] { 4 });
            yield return (new[] { 7, 10 }, new[] { 10, 11 }, new[] { 10 });
            yield return (new[] { 0, 1, 2 }, new[] { 1, 2, 4 }, new[] { 1, 2 });
        }

        /// <summary>
        /// The factors a common multiplier is built from. One of them is a repeated factor, so
        /// that the divisor to be found is not square-free.
        /// </summary>
        static IEnumerable<int[]> Multipliers()
        {
            yield return new[] { 0 };
            yield return new[] { 2 };
            yield return new[] { 7 };
            yield return new[] { 9 };
            yield return new[] { 10 };
            yield return new[] { 11 };
            yield return new[] { 0, 2 };
            yield return new[] { 1, 1 };
        }

        static string Product(IEnumerable<int> indices)
        {
            string? product = null;
            foreach (var index in indices)
                product = product is null ? $"({Factors[index]})" : $"{product} * ({Factors[index]})";
            return product ?? "1";
        }

        static List<int> Joined(IEnumerable<int> first, IEnumerable<int> second)
        {
            var joined = new List<int>(first);
            joined.AddRange(second);
            return joined;
        }

        public static IEnumerable<object[]> Sweep()
        {
            foreach (var (left, right, shared) in Pairs())
                foreach (var multiplier in Multipliers())
                    yield return new object[]
                    {
                        Product(Joined(shared, multiplier)),
                        Product(Joined(left, multiplier)),
                        Product(Joined(right, multiplier))
                    };
        }

        /// <summary>
        /// <c>gcd(a g, b g) = g gcd(a, b)</c>, on products built from the table above. This is
        /// the case that does not depend on anyone having guessed which inputs are hard: a
        /// factor the algorithm loses, or one it invents, shows up here whatever shape it has.
        /// </summary>
        [Theory]
        [MemberData(nameof(Sweep))]
        public void GcdIsMultiplicativeOverACommonFactor(string expected, string left, string right) =>
            AssertGcd(expected, left, right, SweepVariables);

        /// <summary>The multiset intersection: a repeated factor is shared as often as both have it.</summary>
        static List<int> SharedFactors(IEnumerable<int> left, IReadOnlyList<int> right)
        {
            var remaining = new List<int>(right);
            var shared = new List<int>();
            foreach (var factor in left)
                if (remaining.Remove(factor))
                    shared.Add(factor);
            return shared;
        }

        static int[] Draw(Random random)
        {
            var drawn = new int[random.Next(1, 3)];
            for (var i = 0; i < drawn.Length; i++)
                drawn[i] = random.Next(Factors.Length);
            return drawn;
        }

        /// <summary>
        /// The same property over far more triples from the same table. The seed is fixed, so
        /// the case list is identical on every machine and every run — a generated case that
        /// cannot be reproduced is not a finding, and #746's determinism condition rules a
        /// clock-seeded generator out.
        /// </summary>
        /// <remarks>
        /// Seven of these three thousand are declined rather than answered, and the count is
        /// asserted rather than ignored: <see cref="TheTermCeilingIsReachedByAnIntermediate"/>
        /// is what reaches the ceiling and why the inputs that do it are so small. The bound
        /// is an upper one, so finding fewer never fails.
        /// </remarks>
        [Fact]
        public void GcdIsMultiplicativeOverManyDrawnTriples()
        {
            var random = new Random(20260813);
            var declined = 0;
            for (var trial = 0; trial < 3000; trial++)
            {
                var left = Draw(random);
                var right = Draw(random);
                var multiplier = Draw(random);
                var first = Polynomial(Product(Joined(left, multiplier)), SweepVariables);
                var second = Polynomial(Product(Joined(right, multiplier)), SweepVariables);
                var expected = Polynomial(Product(Joined(SharedFactors(left, right), multiplier)), SweepVariables);
                var divisor = Gcd(first, second, SweepVariables.Length);
                if (divisor is null)
                {
                    declined++;
                    continue;
                }
                Assert.True(divisor.SameAs(expected), $"trial {trial}: the divisor is not the expected one");
                AssertIsGreatestCommonDivisor(first, second, divisor, SweepVariables.Length);
            }
            Assert.True(declined <= 7, $"{declined} of 3000 triples were declined");
        }

        #endregion

        #region Coefficient growth

        /// <summary>
        /// Knuth's degree-8 pair (<i>TAOCP</i> vol. 2, §4.6.1), the standing example of a
        /// remainder sequence whose coefficients explode: taken as a plain pseudo-remainder
        /// sequence it reaches a coefficient near 10^35, and the subresultant divisions are
        /// what hold it to the size of the subresultants. The two are coprime, so the whole
        /// sequence is walked before the answer is known — there is no early exit to hide in.
        /// </summary>
        static string KnuthU => "x ^ 8 + x ^ 6 - 3 * x ^ 4 - 3 * x ^ 3 + 8 * x ^ 2 + 2 * x - 5";

        static string KnuthV => "3 * x ^ 6 + 5 * x ^ 4 - 4 * x ^ 2 - 9 * x + 21";

        [Fact]
        public void KnuthsDegreeEightPairIsCoprime() => AssertGcd("1", KnuthU, KnuthV, "x");

        /// <summary>
        /// The same sequence with a factor to find at the end of it, and a clock on it. The
        /// bound is loose on purpose: it is there to catch a change of algorithm, not to
        /// measure this machine. At degree 8 an implementation that had dropped back to a
        /// plain pseudo-remainder sequence would still come in well inside it, since a
        /// coefficient of 10^35 is four machine words; what that change really costs shows up
        /// in the multivariate sweep above, where the intermediates stop fitting under the
        /// term ceiling at all and the answer becomes a refusal.
        /// </summary>
        [Fact]
        public void KnuthsDegreeEightPairWithACommonFactor()
        {
            var clock = Stopwatch.StartNew();
            AssertGcd("x ^ 2 + 1", $"({KnuthU}) * (x ^ 2 + 1)", $"({KnuthV}) * (x ^ 2 + 1)", "x");
            Assert.True(clock.Elapsed.TotalSeconds < 20, $"took {clock.Elapsed}");
        }

        #endregion

        #region Normalisation

        /// <summary>
        /// A greatest common divisor is only defined up to a unit and over Q every nonzero
        /// rational is one, so a representative has to be chosen: whole coprime coefficients
        /// with a positive leading one. Asserted on the polynomial rather than on a printed
        /// form, since it is the choice of representative that is under test and a string
        /// would also be asserting the printer.
        /// </summary>
        [Theory]
        [InlineData("x + y", "(-x - y) * (x - y)", "(-x - y) * (x + 2 * y)")]
        [InlineData("x + y", "(x / 2 + y / 2) * (x - y)", "(x / 2 + y / 2) * (x + 2 * y)")]
        [InlineData("x + y", "(-3 * x - 3 * y) * (x - y)", "(-3 * x - 3 * y) * (x + 2 * y)")]
        [InlineData("2 * x - y", "(x / 3 - y / 6) * (x + y)", "(x / 3 - y / 6) * (x + 2 * y)")]
        public void TheDivisorIsNormalized(string expected, string left, string right) =>
            AssertGcd(expected, left, right, "x", "y");

        /// <summary>
        /// Which of the two signs "positive leading coefficient" picks depends on which
        /// monomial leads, and that is the lexicographic order on the variables as the caller
        /// listed them — so the same divisor comes back with either sign depending on that
        /// list. Pinned because it is a convention rather than a consequence, and because
        /// <see cref="PolynomialGcd.TryCancel"/> builds the list by sorting the names: with w
        /// sorting first, w z leads and x y - z w is returned negated.
        /// </summary>
        [Fact]
        public void TheDivisorsSignFollowsTheVariableOrder()
        {
            AssertGcd("z * w - x * y", "(x * y - z * w) * (x + w)", "(x * y - z * w) * (y + z)",
                "w", "x", "y", "z");
            AssertGcd("x * y - z * w", "(x * y - z * w) * (x + w)", "(x * y - z * w) * (y + z)",
                "x", "y", "z", "w");
        }

        #endregion

        #region Zero, constants, and the ceilings

        /// <summary>
        /// Everything divides zero, so the gcd of zero and anything is that thing normalized.
        /// Answering 1 here, or declining, would be wrong rather than merely unhelpful.
        /// </summary>
        [Fact]
        public void ZeroArguments()
        {
            var zero = MultivariatePolynomial.Zero(2);
            var polynomial = Polynomial("2 * x + 2 * y", "x", "y");
            var expected = Polynomial("x + y", "x", "y");

            Assert.True(Gcd(zero, polynomial, 2)?.SameAs(expected) is true);
            Assert.True(Gcd(polynomial, zero, 2)?.SameAs(expected) is true);
            // gcd(0, 0) is 0: it is the only common divisor that every common divisor divides.
            Assert.True(Gcd(zero, zero, 2)?.IsZero is true);
        }

        [Fact]
        public void ConstantArguments()
        {
            var polynomial = Polynomial("x ^ 2 - y ^ 2", "x", "y");
            var five = MultivariatePolynomial.Constant(2, ERational.FromInt32(5));
            var half = MultivariatePolynomial.Constant(2, ERational.Create(EInteger.One, EInteger.FromInt32(2)));
            var one = MultivariatePolynomial.One(2);

            // Every nonzero rational is a unit over Q, so a constant argument leaves 1.
            Assert.True(Gcd(five, polynomial, 2)?.SameAs(one) is true);
            Assert.True(Gcd(polynomial, half, 2)?.SameAs(one) is true);
            Assert.True(Gcd(five, half, 2)?.SameAs(one) is true);
        }

        [Fact]
        public void EqualArgumentsGiveThemselves() =>
            AssertGcd("x ^ 2 - y ^ 2", "2 * x ^ 2 - 2 * y ^ 2", "3 * x ^ 2 - 3 * y ^ 2", "x", "y");

        /// <summary>
        /// A ninth variable has no byte in the packed exponent vector, and the shift that
        /// would address it is negative — which the language turns into a shift by 56, the
        /// first variable's byte. Read that way <c>x_1 - x_9</c> is the zero polynomial, so
        /// the parse is refused instead. Both callers check the count themselves, which is why
        /// this never reached anybody; the refusal is at the door so the next one need not.
        /// </summary>
        [Fact]
        public void MoreVariablesThanFitAreRefused()
        {
            var eight = new[] { "x_1", "x_2", "x_3", "x_4", "x_5", "x_6", "x_7", "x_8" };
            var nine = new[] { "x_1", "x_2", "x_3", "x_4", "x_5", "x_6", "x_7", "x_8", "x_9" };
            Assert.Equal(MultivariatePolynomial.MaxVariables, eight.Length);

            Assert.NotNull(MultivariatePolynomial.TryParse("x_1 - x_8".ToEntity(), Index(eight)));
            Assert.Null(MultivariatePolynomial.TryParse("x_1 - x_9".ToEntity(), Index(nine)));
            // Refused for the whole parse, not only where the ninth variable is mentioned: a
            // polynomial in the first eight would still carry a variable count of nine into
            // every monomial multiplication, which reads the same aliased byte.
            Assert.Null(MultivariatePolynomial.TryParse("x_1 + x_2".ToEntity(), Index(nine)));
        }

        /// <summary>
        /// Half of what a byte holds is the most one exponent may carry if the sum of two is
        /// to stay in the byte, so the cap is 127 and going past it is a refusal, not a wrap.
        /// </summary>
        [Fact]
        public void DegreesPastTheCapAreRefused()
        {
            var x = Polynomial("x", "x");
            Assert.NotNull(x.Power(MultivariatePolynomial.MaxDegree));
            Assert.Null(x.Power(MultivariatePolynomial.MaxDegree + 1));
            Assert.Null(MultivariatePolynomial.TryParse("x ^ 130".ToEntity(), Index(new[] { "x" })));

            // The cap is really on the sum, and neither of these is past it on its own.
            var high = x.Power(100);
            Assert.NotNull(high);
            Assert.Null(high!.Multiply(high));
            Assert.Null(high.ShiftedBy(0, 100));
            Assert.NotNull(high.ShiftedBy(0, 27));
        }

        [Fact]
        public void TermCountsPastTheCeilingAreRefused()
        {
            var variables = new[] { "a", "b", "c", "d", "e" };
            // (a + b + c + d + e)^8 has C(12, 4) = 495 monomials, just inside the ceiling.
            var large = Polynomial("(a + b + c + d + e) ^ 8", variables);
            Assert.Equal(495, large.TermCount);
            Assert.True(large.TermCount <= MultivariatePolynomial.MaxTerms);

            Assert.Null(large.Multiply(large));
            // The ninth power has 1287 and is stopped while it is built, not after.
            Assert.Null(MultivariatePolynomial.TryParse("(a + b + c + d + e) ^ 9".ToEntity(), Index(variables)));
        }

        /// <summary>
        /// The ceiling that is actually reached in practice, and it is not reached by the
        /// input. These two have 19 and 29 terms and share <c>a + b + c + d</c>, but a
        /// multivariate pseudo-remainder multiplies through by a leading coefficient that is
        /// itself a polynomial, and three steps in, the product of a 25-term and a 159-term
        /// intermediate goes past 512. The subresultant divisions bound the coefficients, not
        /// the number of monomials, so nothing in the algorithm prevents this; what prevents a
        /// wrong answer is that the step declines.
        /// </summary>
        /// <remarks>
        /// Pinned as a refusal, which is a legitimate answer. Should the ceiling be raised, or
        /// the sequence learn to keep its intermediates primitive, this becomes an answer and
        /// the test should be changed to assert that answer deliberately.
        /// </remarks>
        [Fact]
        public void TheTermCeilingIsReachedByAnIntermediate()
        {
            var variables = new[] { "a", "b", "c", "d" };
            var left = Polynomial("(b + c + 1) * (a + b) * (a + b + c + d)", variables);
            var right = Polynomial("(a ^ 2 + b * c + d) * (a + b + c + d) * (a + b + c + d)", variables);
            Assert.Equal(19, left.TermCount);
            Assert.Equal(29, right.TermCount);

            // a + b + c + d divides both, and is not found.
            var common = Polynomial("a + b + c + d", variables);
            Assert.NotNull(left.DivideExact(common));
            Assert.NotNull(right.DivideExact(common));
            Assert.Null(Gcd(left, right, variables.Length));
        }

        #endregion

        #region TryCancel, end to end

        /// <summary>
        /// The cancelled numerator and denominator together with the divisor named by the
        /// condition, or <see langword="null"/> where the rule set declined — in which case it
        /// must have left the expression exactly as it was.
        /// </summary>
        static (Entity Numerator, Entity Denominator, Entity Divisor)? Cancelled(Entity quotient)
        {
            var rewritten = RewriteRules.PolynomialGcdCancellation.ApplyOnce(quotient);
            if (rewritten is Providedf(Divf(var top, var bottom), Notf(Equalsf(var divisor, var zero))))
            {
                Assert.Equal(Integer.Create(0), zero);
                return (top, bottom, divisor);
            }
            Assert.Equal(quotient, rewritten);
            return null;
        }

        /// <summary>Asserts that <paramref name="expression"/> is the zero polynomial.</summary>
        static void AssertIsZero(Entity expression, string because)
        {
            var difference = expression.Expand();
            if (!Integer.Create(0).Equals(difference))
            {
                difference = difference.Simplify();
                while (difference is Providedf(var inner, _))
                    difference = inner;
            }
            Assert.True(Integer.Create(0).Equals(difference), $"{because}; left over: {difference.Stringize()}");
        }

        /// <summary>
        /// The same property as <see cref="AssertIsGreatestCommonDivisor"/>, one layer up and
        /// on expressions rather than on packed monomials — so that a defect in the packing
        /// could not hide behind a check that used the packing to make it.
        /// </summary>
        [Theory]
        [InlineData("x ^ 2 + 2 * x * y + y ^ 2", "x ^ 2 - y ^ 2")]
        [InlineData("x ^ 3 - y ^ 3", "x ^ 2 - y ^ 2")]
        [InlineData("(x * y - z) * (x + z)", "(x * y - z) * (y + z)")]
        [InlineData("2 * x * y + 2 * y", "3 * x * y + 3 * y")]
        [InlineData("6 * x ^ 2 - 6 * y ^ 2", "4 * x + 4 * y")]
        public void ACancellationMultipliesBackToWhatItCameFrom(string numerator, string denominator)
        {
            var cancelled = Cancelled(numerator.ToEntity() / denominator.ToEntity());
            Assert.True(cancelled.HasValue, $"({numerator}) / ({denominator}) was not cancelled");
            var (top, bottom, divisor) = cancelled!.Value;
            AssertIsZero(top * divisor - numerator.ToEntity(), "the divisor does not divide the numerator");
            AssertIsZero(bottom * divisor - denominator.ToEntity(), "the divisor does not divide the denominator");
            Assert.False(Cancelled(top / bottom).HasValue, "the reduced quotient still cancels");
        }

        /// <summary>
        /// <c>(x^2 + 2xy + y^2) / (x^2 - y^2)</c> is <c>(x + y) / (x - y)</c> only away from
        /// <c>x + y = 0</c>, where the original is <c>0/0</c> and the reduced form is
        /// definite. Dropping the condition would widen the domain and claim a value where
        /// there is none, so the answer is asserted node by node rather than as a string.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/55">#55</a>.
        /// </summary>
        [Fact]
        public void TheCancelledFactorTravelsAsACondition()
        {
            var rewritten = RewriteRules.PolynomialGcdCancellation.ApplyOnce(
                "(x ^ 2 + 2 * x * y + y ^ 2) / (x ^ 2 - y ^ 2)".ToEntity());
            var provided = Assert.IsType<Providedf>(rewritten);
            var quotient = Assert.IsType<Divf>(provided.Expression);
            Assert.Equal("x + y".ToEntity(), quotient.Dividend);
            Assert.Equal("x - y".ToEntity(), quotient.Divisor);
            var negated = Assert.IsType<Notf>(provided.Predicate);
            var equality = Assert.IsType<Equalsf>(negated.Argument);
            Assert.Equal("x + y".ToEntity(), equality.Left);
            Assert.Equal(Integer.Create(0), equality.Right);
        }

        /// <summary>And the condition has to bite where the cancelled factor vanishes.</summary>
        [Fact]
        public void TheConditionMakesTheCancelledPointsUndefined()
        {
            var rewritten = RewriteRules.PolynomialGcdCancellation.ApplyOnce(
                "(x ^ 2 + 2 * x * y + y ^ 2) / (x ^ 2 - y ^ 2)".ToEntity());
            Assert.Equal(MathS.NaN, rewritten.Substitute("x", 1).Substitute("y", -1).Evaled);
            Assert.Equal(Integer.Create(3), rewritten.Substitute("x", 2).Substitute("y", 1).Evaled);
        }

        [Theory]
        // Already coprime, so there is nothing to do and no condition to invent.
        [InlineData("(x + y) / (x - y)")]
        [InlineData("(x ^ 2 + y ^ 2) / (x + y)")]
        [InlineData("(x + y + z) / (x + y - z)")]
        // Zero and constants: nothing to have in common.
        [InlineData("0 / (x + y)")]
        [InlineData("(x + y) / 0")]
        [InlineData("0 / 0")]
        [InlineData("7 / (x + y)")]
        [InlineData("(x ^ 2 - y ^ 2) / 7")]
        // No variable in common.
        [InlineData("(x + 1) / (y + 1)")]
        [InlineData("(x ^ 2 - 1) / (y ^ 2 - 1)")]
        // Nine variables, one more than a packed exponent vector holds. Named x_1..x_9 rather
        // than a..i because i is the imaginary unit, and a quotient carrying that would be
        // refused for not being a polynomial -- the same answer for the wrong reason.
        [InlineData("((x_1 + x_2) * (x_3 + x_4)) / ((x_1 + x_2) * (x_5 + x_6 + x_7 + x_8 + x_9))")]
        // Degree past the cap of 127, and more terms than the ceiling; in both the common
        // factor is plainly there.
        [InlineData("((x ^ 130 - y ^ 130) * (x + y)) / ((x ^ 130 - y ^ 130) * (x - y))")]
        [InlineData("((x + y + z + v + w) ^ 10 * (x + y)) / ((x + y + z + v + w) ^ 10 * (x - y))")]
        // Not a polynomial over Q: a transcendental factor, a genuine 1/x, a fractional power.
        [InlineData("(sin(x) * (x + y)) / ((x + y) * (x - y))")]
        [InlineData("((1 / x + y) * (x + y)) / ((x + y) * (x - y))")]
        [InlineData("((x + y) ^ 0.5 * (x + y)) / ((x + y) * (x - y))")]
        public void RefusedRatherThanAnsweredWrongly(string quotient) =>
            Assert.False(Cancelled(quotient.ToEntity()).HasValue, $"{quotient} was cancelled");

        /// <summary>
        /// The complexity bound is on the input's node count, and it is there because this runs
        /// on every quotient the simplifier builds rather than because a larger one would be
        /// wrong. So it is a refusal, and the same quotient written compactly is cancelled.
        /// </summary>
        [Fact]
        public void AQuotientPastTheComplexityBoundIsLeftAlone()
        {
            Entity numerator = "x + y".ToEntity();
            Entity denominator = "x - y".ToEntity();
            for (var i = 0; i < 40; i++)
            {
                numerator *= $"x + {i} * y + 1".ToEntity();
                denominator *= $"x + {i} * y + 1".ToEntity();
            }
            Assert.True(numerator.Complexity + denominator.Complexity > 256);
            Assert.False(Cancelled(numerator / denominator).HasValue);
        }

        #endregion
    }
}

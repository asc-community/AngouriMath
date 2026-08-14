//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Functions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Algebra.Polynomials
{
    /// <summary>
    /// The resultant and the discriminant, checked against two oracles that know nothing
    /// about how the library computes them: the product over the differences of the roots,
    /// and the determinant of the Sylvester matrix taken by cofactor expansion.
    /// </summary>
    /// <remarks>
    /// The oracles are the point of this file. A resultant is easy to compute in a way that
    /// is wrong only in its sign, or only on an argument free of the main variable, and a
    /// test that reproduced the implementation's own reasoning would agree with it in every
    /// one of those cases. Both oracles here are definitions rather than algorithms, and the
    /// second is deliberately the slowest way of taking a determinant there is — which is
    /// what a test is the right place for.
    /// </remarks>
    [Trait("Area", "Algebra")]
    public sealed class PolynomialResultantTest
    {
        private static readonly int[] NoOtherVariables = Array.Empty<int>();

        private static ERational Rational(int numerator, int denominator = 1)
            => ERational.Create(EInteger.FromInt32(numerator), EInteger.FromInt32(denominator));

        private static ERational[] Rationals(IReadOnlyList<int> values)
        {
            var result = new ERational[values.Count];
            for (var i = 0; i < values.Count; i++)
                result[i] = Rational(values[i]);
            return result;
        }

        private static ERational Raise(ERational value, int power)
        {
            var result = ERational.One;
            for (var i = 0; i < power; i++)
                result = result.Multiply(value).ToLowestTerms();
            return result;
        }

        private static Variable[] Vars(params string[] names)
            => names.Select(name => (Variable)name).ToArray();

        /// <summary>One variable, coefficients from the constant term upwards.</summary>
        private static MultivariatePolynomial Univariate(IReadOnlyList<ERational> coefficients)
        {
            var result = MultivariatePolynomial.Zero(1);
            for (var power = 0; power < coefficients.Count; power++)
                if (!coefficients[power].IsZero)
                    result = result.Add(
                        MultivariatePolynomial.Constant(1, coefficients[power]).ShiftedBy(0, power)!);
            return result;
        }

        private static MultivariatePolynomial Parse(string expression, params string[] variables)
        {
            var indices = new Dictionary<Variable, int>(variables.Length);
            for (var i = 0; i < variables.Length; i++)
                indices[(Variable)variables[i]] = i;
            return MultivariatePolynomial.TryParse(MathS.FromString(expression), indices)
                ?? throw new InvalidOperationException($"'{expression}' did not read as a polynomial");
        }

        private static void AssertConstant(ERational expected, MultivariatePolynomial? actual, string what)
        {
            Assert.NotNull(actual);
            Assert.True(actual!.SameAs(MultivariatePolynomial.Constant(actual.VariableCount, expected)),
                $"{what}: expected {expected}, got {actual.ToEntity(Vars("x"))}");
        }

        private static void AssertSame(
            MultivariatePolynomial expected, MultivariatePolynomial? actual, Variable[] variables, string what)
        {
            Assert.NotNull(actual);
            Assert.True(actual!.SameAs(expected),
                $"{what}: expected {expected.ToEntity(variables)}, got {actual.ToEntity(variables)}");
        }

        // ---------------------------------------------------------------------------------
        // The oracles.
        // ---------------------------------------------------------------------------------

        /// <summary>
        /// <c>Res(f, g) = lc(f)^deg g * lc(g)^deg f * prod over i, j of (a_i - b_j)</c>, the
        /// property that makes the resultant worth having in the first place.
        /// </summary>
        private static ERational ResultantFromRoots(
            int leadingLeft, IReadOnlyList<int> leftRoots, int leadingRight, IReadOnlyList<int> rightRoots)
        {
            var result = Raise(Rational(leadingLeft), rightRoots.Count)
                .Multiply(Raise(Rational(leadingRight), leftRoots.Count));
            foreach (var left in leftRoots)
                foreach (var right in rightRoots)
                    result = result.Multiply(Rational(left - right));
            return result.ToLowestTerms();
        }

        /// <summary>
        /// <c>disc(f) = lc(f)^(2n - 2) * prod over i &lt; j of (a_i - a_j)^2</c>.
        /// </summary>
        private static ERational DiscriminantFromRoots(int leading, IReadOnlyList<int> roots)
        {
            var result = Raise(Rational(leading), 2 * roots.Count - 2);
            for (var i = 0; i < roots.Count; i++)
                for (var j = i + 1; j < roots.Count; j++)
                    result = result.Multiply(Raise(Rational(roots[i] - roots[j]), 2));
            return result.ToLowestTerms();
        }

        /// <summary>
        /// The determinant of the Sylvester matrix by cofactor expansion. Both coefficient
        /// lists run from the constant term upwards and end in a nonzero entry.
        /// </summary>
        private static ERational SylvesterResultant(
            IReadOnlyList<ERational> left, IReadOnlyList<ERational> right)
        {
            var leftDegree = left.Count - 1;
            var rightDegree = right.Count - 1;
            var size = leftDegree + rightDegree;
            var matrix = new ERational[size][];
            for (var row = 0; row < size; row++)
            {
                matrix[row] = new ERational[size];
                for (var column = 0; column < size; column++)
                    matrix[row][column] = ERational.Zero;
            }
            for (var row = 0; row < rightDegree; row++)
                for (var power = 0; power <= leftDegree; power++)
                    matrix[row][row + leftDegree - power] = left[power];
            for (var row = 0; row < leftDegree; row++)
                for (var power = 0; power <= rightDegree; power++)
                    matrix[rightDegree + row][row + rightDegree - power] = right[power];
            return Determinant(matrix);
        }

        private static ERational Determinant(ERational[][] matrix)
        {
            var size = matrix.Length;
            if (size == 0)
                return ERational.One;
            if (size == 1)
                return matrix[0][0];
            var total = ERational.Zero;
            for (var column = 0; column < size; column++)
            {
                if (matrix[0][column].IsZero)
                    continue;
                var minor = new ERational[size - 1][];
                for (var row = 1; row < size; row++)
                {
                    minor[row - 1] = new ERational[size - 1];
                    var target = 0;
                    for (var other = 0; other < size; other++)
                        if (other != column)
                            minor[row - 1][target++] = matrix[row][other];
                }
                var term = matrix[0][column].Multiply(Determinant(minor));
                total = column % 2 == 0 ? total.Add(term) : total.Subtract(term);
            }
            return total.ToLowestTerms();
        }

        /// <summary>Multiplied out from <c>leading * prod (x - root)</c>, constant term first.</summary>
        private static ERational[] FromRoots(int leading, IReadOnlyList<int> roots)
        {
            var coefficients = new[] { Rational(leading) };
            foreach (var root in roots)
            {
                var next = new ERational[coefficients.Length + 1];
                for (var i = 0; i < next.Length; i++)
                    next[i] = ERational.Zero;
                for (var i = 0; i < coefficients.Length; i++)
                {
                    next[i + 1] = next[i + 1].Add(coefficients[i]);
                    next[i] = next[i].Subtract(coefficients[i].Multiply(Rational(root)));
                }
                coefficients = next;
            }
            return coefficients;
        }

        // ---------------------------------------------------------------------------------
        // Oracle 1 — the product over the differences of the roots.
        // ---------------------------------------------------------------------------------

        public static IEnumerable<object[]> RootCases => new[]
        {
            // Two monic linears, the smallest case in which the sign can be wrong.
            new object[] { 1, new[] { 2 }, 1, new[] { 5 } },
            new object[] { 1, new[] { 1, 2 }, 1, new[] { 3, 4, 5 } },
            new object[] { 1, new[] { 3, 4, 5 }, 1, new[] { 1, 2 } },
            // Non-monic on both sides, and a negative leading coefficient.
            new object[] { 3, new[] { 1, -2 }, -2, new[] { 0, 5, 5 } },
            new object[] { -1, new[] { 7 }, 4, new[] { -3, -3, -3, 2 } },
            // A shared root, so the resultant has to come out exactly zero.
            new object[] { 2, new[] { 1, 3 }, 5, new[] { 3, -1 } },
            new object[] { 1, new[] { -4 }, 6, new[] { -4 } },
            // A repeated root on one side only.
            new object[] { 1, new[] { 2, 2, 2 }, 1, new[] { 0, 1 } },
            // Degree zero against a positive degree, both ways round, and two constants.
            new object[] { 7, Array.Empty<int>(), 1, new[] { 1, 2, 3 } },
            new object[] { 1, new[] { 1, 2, 3 }, 7, Array.Empty<int>() },
            new object[] { 5, Array.Empty<int>(), 3, Array.Empty<int>() },
        };

        [Theory]
        [MemberData(nameof(RootCases))]
        public void ResultantIsTheProductOverTheDifferencesOfTheRoots(
            int leadingLeft, int[] leftRoots, int leadingRight, int[] rightRoots)
        {
            var left = Univariate(FromRoots(leadingLeft, leftRoots));
            var right = Univariate(FromRoots(leadingRight, rightRoots));
            AssertConstant(
                ResultantFromRoots(leadingLeft, leftRoots, leadingRight, rightRoots),
                PolynomialResultant.Resultant(left, right, 0, NoOtherVariables),
                "Res(f, g)");
        }

        [Theory]
        [MemberData(nameof(RootCases))]
        public void SwappingTheArgumentsTurnsTheSignAroundExactlyWhenBothDegreesAreOdd(
            int leadingLeft, int[] leftRoots, int leadingRight, int[] rightRoots)
        {
            var left = Univariate(FromRoots(leadingLeft, leftRoots));
            var right = Univariate(FromRoots(leadingRight, rightRoots));
            var forwards = ResultantFromRoots(leadingLeft, leftRoots, leadingRight, rightRoots);
            var expected = leftRoots.Length * rightRoots.Length % 2 == 0 ? forwards : forwards.Negate();
            AssertConstant(expected,
                PolynomialResultant.Resultant(right, left, 0, NoOtherVariables), "Res(g, f)");
        }

        // ---------------------------------------------------------------------------------
        // Oracle 2 — the Sylvester determinant.
        // ---------------------------------------------------------------------------------

        public static IEnumerable<object[]> CoefficientCases => new[]
        {
            // Constant term first throughout.
            new object[] { new[] { 1, 1 }, new[] { -1, 1 } },
            new object[] { new[] { -1, 0, 1 }, new[] { 2, 3 } },
            new object[] { new[] { 5, 0, 0, 2 }, new[] { 1, 0, 7 } },
            // Gaps, so that the elimination meets a zero where it wants a pivot.
            new object[] { new[] { 1, 0, 0, 0, 1 }, new[] { 0, 1, 0, 1 } },
            new object[] { new[] { 0, 0, 1 }, new[] { 0, 0, 0, 1 } },
            // The degree of the remainder falling by more than one, which is where a
            // remainder-sequence implementation needs its correction factor.
            new object[] { new[] { 1, 0, 0, 0, 1 }, new[] { 1, 0, 1 } },
            new object[] { new[] { 3, 0, 0, 0, 0, 1 }, new[] { -1, 0, 0, 4 } },
            // Negative and non-monic.
            new object[] { new[] { -7, 2, -5, 3 }, new[] { 4, -4, 1 } },
            new object[] { new[] { 6, -5, 1 }, new[] { -6, 11, -6, 1 } },
            // Sharing a factor.
            new object[] { new[] { -1, 0, 1 }, new[] { -1, -1, 1, 1 } },
            // An odd number of row interchanges in the elimination, which is the one place
            // the sign of the determinant can go missing without any other case noticing.
            // These were searched for rather than guessed: one interchange, then two, then
            // three, each with a resultant that is not zero.
            new object[] { new[] { -1, 0, 1 }, new[] { 0, 1 } },
            new object[] { new[] { -1, -1, -1 }, new[] { 2, 2 } },
            new object[] { new[] { -2, -2, -2, -2 }, new[] { -2, -2, -2, -1 } },
            new object[] { new[] { -2, -2, -2, -2, -2 }, new[] { -2, -2, -2, -2 } },
            // One side, then the other, then both free of the variable.
            new object[] { new[] { 4 }, new[] { 1, 2, 3 } },
            new object[] { new[] { 1, 2, 3 }, new[] { 4 } },
            new object[] { new[] { 4 }, new[] { 9 } },
        };

        [Theory]
        [MemberData(nameof(CoefficientCases))]
        public void ResultantIsTheSylvesterDeterminant(int[] left, int[] right)
        {
            var leftCoefficients = Rationals(left);
            var rightCoefficients = Rationals(right);
            var first = Univariate(leftCoefficients);
            var second = Univariate(rightCoefficients);
            AssertConstant(SylvesterResultant(leftCoefficients, rightCoefficients),
                PolynomialResultant.Resultant(first, second, 0, NoOtherVariables), "Res(f, g)");
            AssertConstant(SylvesterResultant(rightCoefficients, leftCoefficients),
                PolynomialResultant.Resultant(second, first, 0, NoOtherVariables), "Res(g, f)");
        }

        [Fact]
        public void RationalCoefficientsAgreeWithTheSylvesterDeterminant()
        {
            var left = new[] { Rational(1, 3), Rational(-5, 2), ERational.Zero, Rational(7, 4) };
            var right = new[] { Rational(-2, 9), Rational(1, 6), Rational(3, 5) };
            AssertConstant(SylvesterResultant(left, right),
                PolynomialResultant.Resultant(Univariate(left), Univariate(right), 0, NoOtherVariables),
                "Res(f, g) over Q");
        }

        // ---------------------------------------------------------------------------------
        // Where the resultant vanishes.
        // ---------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(CoefficientCases))]
        public void ResultantVanishesExactlyWhereTheGcdHasPositiveDegree(int[] left, int[] right)
        {
            var first = Univariate(Rationals(left));
            var second = Univariate(Rationals(right));
            var resultant = PolynomialResultant.Resultant(first, second, 0, NoOtherVariables);
            Assert.NotNull(resultant);
            var gcd = PolynomialGcd.Gcd(first, second, new[] { 0 }, 0);
            Assert.NotNull(gcd);
            Assert.Equal(gcd!.DegreeIn(0) > 0, resultant!.IsZero);
        }

        // ---------------------------------------------------------------------------------
        // The conventions on degenerate arguments. Measured against SymPy 1.14 rather than
        // recalled: there resultant(0, x^2 - 1, x), resultant(3, 0, x) and resultant(0, 0, x)
        // are all 0, resultant(3, 5, x) is 1, and resultant(x^3 - 1, 5, x) is 125.
        // ---------------------------------------------------------------------------------

        [Fact]
        public void AZeroArgumentGivesZeroWhateverTheOtherSideIs()
        {
            var zero = Univariate(Array.Empty<ERational>());
            var quadratic = Univariate(Rationals(new[] { -1, 0, 1 }));
            var constant = Univariate(Rationals(new[] { 3 }));
            AssertConstant(ERational.Zero,
                PolynomialResultant.Resultant(zero, quadratic, 0, NoOtherVariables), "Res(0, f)");
            AssertConstant(ERational.Zero,
                PolynomialResultant.Resultant(quadratic, zero, 0, NoOtherVariables), "Res(f, 0)");
            AssertConstant(ERational.Zero,
                PolynomialResultant.Resultant(zero, constant, 0, NoOtherVariables), "Res(0, c)");
            AssertConstant(ERational.Zero,
                PolynomialResultant.Resultant(constant, zero, 0, NoOtherVariables), "Res(c, 0)");
            AssertConstant(ERational.Zero,
                PolynomialResultant.Resultant(zero, zero, 0, NoOtherVariables), "Res(0, 0)");
        }

        [Fact]
        public void AConstantArgumentIsRaisedToTheDegreeOfTheOther()
        {
            var cubic = Univariate(Rationals(new[] { -1, 0, 0, 1 }));
            var five = Univariate(Rationals(new[] { 5 }));
            var three = Univariate(Rationals(new[] { 3 }));
            AssertConstant(Rational(125),
                PolynomialResultant.Resultant(five, cubic, 0, NoOtherVariables), "Res(5, f)");
            AssertConstant(Rational(125),
                PolynomialResultant.Resultant(cubic, five, 0, NoOtherVariables), "Res(f, 5)");
            AssertConstant(ERational.One,
                PolynomialResultant.Resultant(three, five, 0, NoOtherVariables), "Res(3, 5)");
        }

        // ---------------------------------------------------------------------------------
        // Discriminants.
        // ---------------------------------------------------------------------------------

        [Fact]
        public void DiscriminantOfTheGeneralQuadraticIsBSquaredMinusFourAC()
        {
            var names = Vars("a", "b", "c", "x");
            var quadratic = Parse("a * x ^ 2 + b * x + c", "a", "b", "c", "x");
            AssertSame(Parse("b ^ 2 - 4 * a * c", "a", "b", "c", "x"),
                PolynomialResultant.Discriminant(quadratic, 3, new[] { 0, 1, 2 }), names, "disc(a x^2 + b x + c)");
        }

        [Fact]
        public void DiscriminantOfTheDepressedCubicIsMinusFourPCubedMinusTwentySevenQSquared()
        {
            var names = Vars("p", "q", "x");
            var cubic = Parse("x ^ 3 + p * x + q", "p", "q", "x");
            AssertSame(Parse("-4 * p ^ 3 - 27 * q ^ 2", "p", "q", "x"),
                PolynomialResultant.Discriminant(cubic, 2, new[] { 0, 1 }), names, "disc(x^3 + p x + q)");
        }

        public static IEnumerable<object[]> DiscriminantRootCases => new[]
        {
            new object[] { 1, new[] { 1, 2 } },
            new object[] { 3, new[] { -1, 4 } },
            new object[] { 1, new[] { 0, 1, 2 } },
            new object[] { -2, new[] { 5, -3, 1 } },
            new object[] { 1, new[] { 1, 2, 3, 4 } },
            new object[] { 2, new[] { -2, 0, 3, 7 } },
            // Repeated roots, where the discriminant is zero.
            new object[] { 1, new[] { 1, 1, 3 } },
            new object[] { 4, new[] { 2, 2 } },
            new object[] { 1, new[] { 0, 1, 2, 3, 4 } },
        };

        [Theory]
        [MemberData(nameof(DiscriminantRootCases))]
        public void DiscriminantIsTheSquaredProductOverTheDifferencesOfTheRoots(int leading, int[] roots)
            => AssertConstant(DiscriminantFromRoots(leading, roots),
                PolynomialResultant.Discriminant(Univariate(FromRoots(leading, roots)), 0, NoOtherVariables),
                "disc(f)");

        [Fact]
        public void DiscriminantOfALinearPolynomialIsOneAndOfAConstantIsZero()
        {
            // f' is a nonzero constant for a linear f, so Res(f, f') is lc(f) and dividing it
            // out leaves 1. For a constant f the derivative is zero and so is Res(f, 0),
            // which is what SymPy answers for discriminant(5, x) as well.
            AssertConstant(ERational.One,
                PolynomialResultant.Discriminant(
                    Univariate(Rationals(new[] { 7, -3 })), 0, NoOtherVariables), "disc(a x + b)");
            AssertConstant(ERational.Zero,
                PolynomialResultant.Discriminant(
                    Univariate(Rationals(new[] { 5 })), 0, NoOtherVariables), "disc(c)");
            AssertConstant(ERational.Zero,
                PolynomialResultant.Discriminant(
                    Univariate(Array.Empty<ERational>()), 0, NoOtherVariables), "disc(0)");
        }

        [Fact]
        public void ANonMonicCubicAgreesWithSympy()
        {
            // SymPy 1.14, f = 2 x^3 - 3 x + 1: resultant(f, diff(f, x), x) is -216 and
            // discriminant(f, x) is 108, so (-1)^(n (n - 1) / 2) is -1 at n = 3.
            var cubic = Univariate(Rationals(new[] { 1, -3, 0, 2 }));
            var derivative = Univariate(Rationals(new[] { -3, 0, 6 }));
            AssertConstant(Rational(-216),
                PolynomialResultant.Resultant(cubic, derivative, 0, NoOtherVariables), "Res(f, f')");
            AssertConstant(Rational(108),
                PolynomialResultant.Discriminant(cubic, 0, NoOtherVariables), "disc(f)");
        }

        [Fact]
        public void TheDerivativeTheDiscriminantUsesIsTheDerivative()
        {
            var poly = Parse("3 * x ^ 4 * y ^ 2 - 5 * x * y + 7 * y + 2", "x", "y");
            AssertSame(Parse("12 * x ^ 3 * y ^ 2 - 5 * y", "x", "y"),
                poly.DerivativeIn(0), Vars("x", "y"), "d/dx");
            AssertSame(Parse("6 * x ^ 4 * y - 5 * x + 7", "x", "y"),
                poly.DerivativeIn(1), Vars("x", "y"), "d/dy");
        }

        // ---------------------------------------------------------------------------------
        // Genuinely several variables.
        // ---------------------------------------------------------------------------------

        [Fact]
        public void EliminatingYBetweenACircleAndALineLeavesTheXCoordinatesOfTheIntersections()
        {
            // x^2 + y^2 = 1 meets x + y = 1 at (1, 0) and (0, 1), so the eliminant vanishes
            // at x = 0 and x = 1 and nowhere else. By hand: substituting y = 1 - x into the
            // circle gives x^2 + (1 - x)^2 - 1, which is 2 x^2 - 2 x.
            var names = Vars("x", "y");
            var circle = Parse("x ^ 2 + y ^ 2 - 1", "x", "y");
            var line = Parse("x + y - 1", "x", "y");
            var expected = Parse("2 * x ^ 2 - 2 * x", "x", "y");
            AssertSame(expected, PolynomialResultant.Resultant(circle, line, 1, new[] { 0 }),
                names, "Res(circle, line)");
            // deg f * deg g is even here, so the two orders agree.
            AssertSame(expected, PolynomialResultant.Resultant(line, circle, 1, new[] { 0 }),
                names, "Res(line, circle)");
        }

        [Fact]
        public void EliminatingYBetweenACircleAndAHyperbolaIsTheHandComputedQuartic()
        {
            // x^2 + y^2 = 4 and x y = 1: substituting y = 1 / x and clearing the denominator
            // gives x^4 - 4 x^2 + 1.
            var circle = Parse("x ^ 2 + y ^ 2 - 4", "x", "y");
            var hyperbola = Parse("x * y - 1", "x", "y");
            AssertSame(Parse("x ^ 4 - 4 * x ^ 2 + 1", "x", "y"),
                PolynomialResultant.Resultant(circle, hyperbola, 1, new[] { 0 }),
                Vars("x", "y"), "Res(circle, hyperbola)");
        }

        [Fact]
        public void AFactorFreeOfTheMainVariableComesOutAsAPowerOfItself()
        {
            // Res(c f, g) = c^deg g Res(f, g) and Res(f, c g) = c^deg f Res(f, g), which is
            // what taking the content out before the elimination relies on. The two degrees
            // are deliberately different, since equal ones would hide the two exponents
            // being the wrong way round.
            var names = Vars("x", "y");
            var f = Parse("x ^ 3 + 3 * x + 2", "x", "y");
            var g = Parse("2 * x ^ 2 - x + 5", "x", "y");
            Assert.Equal(3, f.DegreeIn(0));
            Assert.Equal(2, g.DegreeIn(0));

            var plain = PolynomialResultant.Resultant(f, g, 0, new[] { 1 });
            Assert.NotNull(plain);
            foreach (var content in new[] { Parse("y", "x", "y"), Parse("y + 1", "x", "y") })
            {
                AssertSame(plain!.Multiply(content.Power(2)!)!,
                    PolynomialResultant.Resultant(f.Multiply(content)!, g, 0, new[] { 1 }),
                    names, "Res(c f, g)");
                AssertSame(plain.Multiply(content.Power(3)!)!,
                    PolynomialResultant.Resultant(f, g.Multiply(content)!, 0, new[] { 1 }),
                    names, "Res(f, c g)");
                // Both sides at once: c^deg g from the left and c^deg f from the right.
                AssertSame(plain.Multiply(content.Power(5)!)!,
                    PolynomialResultant.Resultant(f.Multiply(content)!, g.Multiply(content)!, 0, new[] { 1 }),
                    names, "Res(c f, c g)");
            }
        }

        [Fact]
        public void TheDiscriminantOfAQuadraticInOneVariableSeesTheOther()
        {
            // y x^2 + (y + 1) x + 3 read in x, so b^2 - 4 a c is (y + 1)^2 - 12 y.
            var poly = Parse("y * x ^ 2 + (y + 1) * x + 3", "x", "y");
            AssertSame(Parse("y ^ 2 - 10 * y + 1", "x", "y"),
                PolynomialResultant.Discriminant(poly, 0, new[] { 1 }), Vars("x", "y"), "disc in x");
        }

        // ---------------------------------------------------------------------------------
        // Refusal.
        // ---------------------------------------------------------------------------------

        [Fact]
        public void InputTooLargeForTheEliminationIsRefusedRatherThanAttempted()
        {
            // deg f + deg g of 42, one past the ceiling on size.
            var coefficients = new ERational[22];
            for (var power = 0; power < coefficients.Length; power++)
                coefficients[power] = Rational(power % 5 + 1);
            var poly = Univariate(coefficients);
            Assert.Null(PolynomialResultant.Resultant(poly, poly, 0, NoOtherVariables));
            Assert.Null(PolynomialResultant.Discriminant(poly, 0, NoOtherVariables));
        }

        [Fact]
        public void TheLargestAdmittedEliminationStillAnswers()
        {
            // deg f + deg g of exactly the ceiling, so that the refusal above reads as a
            // bound rather than as a description of everything past a handful of terms.
            var leftRoots = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            var rightRoots = new[] { 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40 };
            AssertConstant(
                ResultantFromRoots(1, leftRoots, 1, rightRoots),
                PolynomialResultant.Resultant(
                    Univariate(FromRoots(1, leftRoots)), Univariate(FromRoots(1, rightRoots)),
                    0, NoOtherVariables),
                "Res(f, g) at the ceiling");
        }

        /// <summary>
        /// Three variables, every coefficient in the main one carrying two monomials in the
        /// other two, at a Sylvester size the ceiling admits. The elimination is refused all
        /// the same, on the budget rather than on the size — which is the whole point of
        /// there being two bounds: the cost of an elimination is not readable off its degrees.
        /// https://github.com/asc-community/AngouriMath/issues/921
        /// </summary>
        [Fact]
        public void AnEliminationPastTheWorkBudgetIsRefusedThoughItsSizeIsAdmitted()
        {
            var left = Wide(20, 2, seed: 1);
            var right = Wide(20, 2, seed: 2);
            Assert.Equal(20, left.DegreeIn(0));
            Assert.Equal(20, right.DegreeIn(0));
            Assert.Null(PolynomialResultant.Resultant(left, right, 0, new[] { 1, 2 }));
        }

        /// <summary>
        /// A polynomial of the given degree in variable 0, each of whose coefficients carries
        /// <paramref name="width"/> distinct monomials in variables 1 and 2.
        /// </summary>
        private static MultivariatePolynomial Wide(int degree, int width, int seed)
        {
            var shape = new[] { (A: 0, B: 0), (A: 1, B: 0), (A: 0, B: 1), (A: 1, B: 1) };
            var result = MultivariatePolynomial.Zero(3);
            for (var power = 0; power <= degree; power++)
                for (var term = 0; term < width; term++)
                {
                    var value = Rational(1 + (seed * 7 + power * 3 + term * 5) % 11);
                    var monomial = MultivariatePolynomial.Constant(3, value).ShiftedBy(0, power)
                        ?.ShiftedBy(1, shape[term % shape.Length].A)
                        ?.ShiftedBy(2, shape[term % shape.Length].B);
                    Assert.NotNull(monomial);
                    result = result.Add(monomial!);
                }
            return result;
        }
    }
}

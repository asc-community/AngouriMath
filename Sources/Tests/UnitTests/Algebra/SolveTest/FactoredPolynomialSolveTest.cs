//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// A polynomial equation that has a factor the solver can answer exactly must be
    /// answered through that factor, rather than by handing the whole polynomial to the
    /// general formula for its degree -- or, above degree four, to the numeric solver.
    /// https://github.com/asc-community/AngouriMath/issues/272
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class FactoredPolynomialSolveTest
    {
        private static Entity.Set.FiniteSet Roots(string equation) =>
            (Entity.Set.FiniteSet)equation.ToEntity().Solve("x");

        private static void AssertRootsAre(string equation, params string[] expected)
        {
            var roots = Roots(equation);
            foreach (var root in expected)
                Assert.Contains(root.ToEntity().InnerSimplified, roots);
            Assert.Equal(expected.Length, roots.Count);
        }

        /// <summary>
        /// The clearest statement of the defect, and the one that does not depend on how a
        /// radical is spelled: an equation whose roots are all real must not be answered
        /// with expressions built out of complex numbers. Cardano's formula reaches the
        /// real roots of x^3 - x^2 - 3x + 3 through (26 + 18i)^(1/3).
        /// </summary>
        private static void AssertNoImaginaryPartsInTheWorking(string equation)
        {
            foreach (var root in Roots(equation))
                foreach (var node in root.Nodes)
                    if (node is Entity.Number.Complex and not Entity.Number.Real)
                        Assert.True(false,
                            $"{equation} has only real roots, but {root.Stringize()} reaches one " +
                            $"through {node.Stringize()}");
        }

        /// <summary>A root that is a decimal is a root the solver gave up on.</summary>
        private static void AssertEveryRootIsExact(string equation)
        {
            foreach (var root in Roots(equation))
                foreach (var node in root.Nodes)
                    if (node is Entity.Number.Real and not Entity.Number.Rational)
                        Assert.True(false,
                            $"{equation} was answered with the decimal {node.Stringize()} " +
                            $"inside {root.Stringize()}");
        }

        // Handed the factors outright, the solver still expanded them into a cubic and ran
        // Cardano over it, so the answer to an equation whose roots are 1 and +-sqrt(3) came
        // back as two nested cube roots of 26 + 18i.
        [Fact]
        public void AProductIsSolvedThroughItsFactors() =>
            AssertRootsAre("(x - 1) * (x ^ 2 - 3) = 0", "1", "sqrt(3)", "-sqrt(3)");

        [Fact]
        public void AProductOfSeveralFactorsIsSolvedThroughThemToo() =>
            AssertRootsAre("(x - 1) * (x - 2) * (x + 2) * (x ^ 2 - 3) = 0",
                "1", "2", "-2", "sqrt(3)", "-sqrt(3)");

        // The same equation written out. A rational root is there to be found, and dividing
        // it out leaves a quadratic that is answered exactly.
        [Fact]
        public void AnExpandedCubicWithARationalRootIsSplitAtIt() =>
            AssertRootsAre("x ^ 3 - x ^ 2 - 3 * x + 3 = 0", "1", "sqrt(3)", "-sqrt(3)");

        // Above degree four there is no general formula, so the whole equation used to fall
        // through to the numeric solver -- which returned decimals, one of them carrying a
        // spurious -2.3e-15i.
        [Fact]
        public void AQuinticThatSplitsIntoSolvableFactorsIsSolvedExactly() =>
            AssertRootsAre("x ^ 5 - x ^ 4 - 7 * x ^ 3 + 7 * x ^ 2 + 12 * x - 12 = 0",
                "1", "2", "-2", "sqrt(3)", "-sqrt(3)");

        [Theory]
        [InlineData("(x - 1) * (x ^ 2 - 3) = 0")]
        [InlineData("x ^ 3 - x ^ 2 - 3 * x + 3 = 0")]
        [InlineData("x ^ 3 - 6 * x ^ 2 + 11 * x - 6 = 0")]
        public void RealRootsAreNotReachedThroughComplexNumbers(string equation) =>
            AssertNoImaginaryPartsInTheWorking(equation);

        [Theory]
        [InlineData("x ^ 3 - x ^ 2 - 3 * x + 3 = 0")]
        [InlineData("x ^ 5 - x ^ 4 - 7 * x ^ 3 + 7 * x ^ 2 + 12 * x - 12 = 0")]
        [InlineData("(x - 1) * (x - 2) * (x + 2) * (x ^ 2 - 3) = 0")]
        public void RootsThatCanBeWrittenExactlyAre(string equation) =>
            AssertEveryRootIsExact(equation);

        private static bool IsNear(Entity root, double re, double im)
        {
            if (root.Vars.Any())
                return false;
            var value = root.EvalNumerical();
            return System.Math.Abs(value.RealPart.EDecimal.ToDouble() - re) < 1e-9
                && System.Math.Abs(value.ImaginaryPart.EDecimal.ToDouble() - im) < 1e-9;
        }

        /// <summary>
        /// Worse than an unreadable answer: an incomplete one. Above degree four the whole
        /// equation went to the numeric solver, which searches for real roots, so
        /// (x - 1)(x^2 - 2)(x^2 + x + 1) = 0 came back as { 1, sqrt(2), -sqrt(2) } -- the
        /// two roots of its x^2 + x + 1 factor were missing, and a set that is short of two
        /// roots does not look any different from one that is not.
        /// Splitting off the rational root leaves a quartic, which is solved whole.
        /// </summary>
        [Theory]
        [InlineData("x ^ 5 - 2 * x ^ 3 - x ^ 2 + 2 = 0")]
        [InlineData("(x - 1) * (x ^ 2 - 2) * (x ^ 2 + x + 1) = 0")]
        public void AQuinticKeepsTheRootsOfItsIrreducibleFactor(string equation)
        {
            var roots = Roots(equation);
            var sqrt2 = System.Math.Sqrt(2);
            var halfSqrt3 = System.Math.Sqrt(3) / 2;
            foreach (var (re, im) in new[] { (1.0, 0.0), (sqrt2, 0.0), (-sqrt2, 0.0),
                                             (-0.5, halfSqrt3), (-0.5, -halfSqrt3) })
                Assert.Contains(roots, root => IsNear(root, re, im));
            Assert.Equal(5, roots.Count);
        }

        // What must not change. A polynomial with no rational root has nothing to split off,
        // and its roots are what they always were.
        [Fact]
        public void APolynomialWithoutARationalRootIsUnaffected() =>
            AssertRootsAre("x ^ 2 - 2 = 0", "sqrt(2)", "-sqrt(2)");

        // A two-term polynomial is answered whole by inverting x^n = c, which gives its
        // roots as a + bi. Splitting off the rational ones instead leaves the others to be
        // dug out of the quotient, and x^3 - 8 comes back as (-2 -+ sqrt(-12))/2 rather
        // than as (-1/2 +- i*sqrt(3)/2)*2. Nothing is gained, so those are left alone.
        [Theory]
        [InlineData("x ^ 3 - 8 = 0")]
        [InlineData("x ^ 6 - 1 = 0")]
        public void ABinomialKeepsTheAnswerInversionGivesIt(string equation)
        {
            foreach (var root in Roots(equation))
                foreach (var node in root.Nodes)
                    if (node is Entity.Powf(Entity.Number.Real { IsNegative: true } radicand, Entity.Number.Rational))
                        Assert.True(false,
                            $"{equation} was answered through a root of the negative " +
                            $"{radicand.Stringize()}, in {root.Stringize()}");
        }

        // Repeated factors are one root, not several.
        [Fact]
        public void ARepeatedFactorContributesItsRootOnce() =>
            AssertRootsAre("(x - 1) ^ 2 * (x + 3) = 0", "1", "-3");

        // A factor free of the variable has no roots to contribute, and must not make the
        // equation look unsolvable either.
        [Fact]
        public void AConstantFactorContributesNothing() =>
            AssertRootsAre("5 * (x ^ 2 - 3) = 0", "sqrt(3)", "-sqrt(3)");

        // Solving factor by factor must not smuggle in a value that is not a root of the
        // whole equation. Here x = 0 zeroes the first factor but makes the second undefined.
        [Fact]
        public void AFactorsRootThatTheWholeEquationRejectsIsDropped()
        {
            var roots = Roots("x * (1 / x) = 0");
            Assert.DoesNotContain(Entity.Number.Integer.Create(0), roots);
        }
    }
}

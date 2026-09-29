//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// The Iverson bracket <c>iverson(P)</c>: <c>1</c> where a statement holds and <c>0</c> where
    /// it does not, which is how a count is written as a sum.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1478">#1478</a>
    /// </summary>
    [Trait("Area", "Discrete")]
    public sealed class IversonTest
    {
        [Theory]
        [InlineData("iverson(3 divides 12)", "1")]
        [InlineData("iverson(5 divides 12)", "0")]
        [InlineData("iverson(2 > 3)", "0")]
        [InlineData("iverson(pi > 3)", "1")]
        [InlineData("iverson(1/2 in ZZ)", "0")]
        [InlineData("iverson(True) + iverson(False)", "1")]
        public void ADecidedStatementIsOneOrZero(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        [Theory]
        [InlineData("iverson(x > 0)")]
        [InlineData("iverson(n divides 12)")]
        public void AnUndecidedStatementKeepsTheNode(string input)
            => Assert.IsType<Entity.Iversonf>(input.ToEntity().Simplify());

        /// <summary>
        /// A number is not a statement: <c>1</c> is not true, as a connective reads it.
        /// </summary>
        [Theory]
        [InlineData("iverson(1)")]
        [InlineData("iverson(0)")]
        public void ANumberIsNotAStatement(string input)
            => Assert.IsType<Entity.Iversonf>(input.ToEntity().Simplify());

        /// <summary>
        /// A statement with no truth value is <c>NaN</c>, and so is its bracket: the order
        /// comparison <c>i > 0</c> has no answer on the complex plane.
        /// </summary>
        [Fact]
        public void AStatementWithNoTruthValueHasNoBracket()
            => Assert.True("iverson(i > 0)".ToEntity().Simplify().IsNaN);

        [Fact]
        public void SubstitutingDecidesIt()
        {
            Assert.Equal("1".ToEntity(), "iverson(x > 0)".ToEntity().Substitute("x", 2).Simplify());
            Assert.Equal("0".ToEntity(), "iverson(x > 0)".ToEntity().Substitute("x", -2).Simplify());
        }

        /// <summary>
        /// A count is a sum of brackets. Sullivan and Mackey's Ex 8.3.18 draws four balls from
        /// three red, three blue and three green, and counts the outcomes as triples
        /// <c>(r, b, g)</c>: there are 12, where the book says 15.
        /// </summary>
        [Theory]
        [InlineData("sum(iverson(k divides 12), k, 1, 12)", "6")]
        [InlineData("sum(sum(sum(iverson(r + b + g = 4), g, 0, 3), b, 0, 3), r, 0, 3)", "12")]
        public void ACountIsASumOfBrackets(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        /// <summary>
        /// A sum of a bracket is a count, computed in closed form however long the range: the
        /// whole numbers of an interval, and the members of a residue class, with a negation and a
        /// disjunction counted by inclusion and exclusion.
        /// </summary>
        [Theory]
        [InlineData("sum(iverson(k >= 3), k, 1, 10)", "8")]
        [InlineData("sum(iverson(k > 5/2 and k < 7), k, 1, 10)", "4")]
        [InlineData("sum(iverson(2 k + 1 <= 11), k, 0, 1000)", "6")]
        [InlineData("sum(iverson(k = 7/2), k, 0, 10)", "0")]
        [InlineData("sum(iverson(k = 4), k, 0, 3)", "0")]
        [InlineData("sum(iverson(2 divides k), k, 1, 1000)", "500")]
        [InlineData("sum(iverson(2 divides k or 3 divides k), k, 1, 1000)", "667")]
        [InlineData("sum(iverson(3 divides 2 k + 1), k, 0, 99)", "33")]
        [InlineData("sum(iverson(2 divides k and 3 divides k + 1), k, 0, 59)", "10")]
        [InlineData("sum(iverson(2 divides k and 2 divides k + 1), k, 0, 99)", "0")]
        [InlineData("sum(iverson(k in [2; 5]), k, 0, 10)", "4")]
        [InlineData("sum(iverson(k in (2; +oo)), k, 0, 10)", "8")]
        [InlineData("sum(iverson(k + 1/2 in ZZ), k, 0, 9)", "0")]
        [InlineData("sum(iverson(k = 3 and 3 divides 12), k, 0, 9)", "1")]
        // Sullivan and Mackey's §8.7.4 Try 1: the numbers below 100 that are multiples of neither
        // 2 nor 5.
        [InlineData("sum(iverson(not 2 divides k and not 5 divides k), k, 1, 99)", "40")]
        // A perfect power is counted by the roots of the ends, and powers of two exponents are
        // the powers of their least common multiple. Ex 8.7.5: the numbers up to 1000 that are
        // not squares, cubes or fourth powers; §8.7.4 Try 2: below 1000, neither squares, cubes
        // nor fifth powers.
        [InlineData("sum(iverson(exists m in ZZ : m^2 = k), k, 1, 1000)", "31")]
        [InlineData("sum(iverson((exists m in ZZ+ : m^2 = k) and (exists m in ZZ+ : m^3 = k)), k, 1, 1000)", "3")]
        [InlineData("sum(iverson(not (exists m in ZZ+ : m^2 = k) and not (exists m in ZZ+ : m^3 = k) and not (exists m in ZZ+ : m^4 = k)), k, 1, 1000)", "962")]
        [InlineData("sum(iverson(not (exists m in ZZ+ : m^2 = k) and not (exists m in ZZ+ : m^3 = k) and not (exists m in ZZ+ : m^5 = k)), k, 1, 999)", "960")]
        // Ex 8.5.3: 20 identical coins among three pirates, binomial(22, 2) ways.
        [InlineData("sum(sum(sum(iverson(a + b + c = 20), c, 0, 20), b, 0, 20), a, 0, 20)", "231")]
        public void ACountIsComputedInClosedForm(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        /// <summary>
        /// A symbolic range is counted with floors, by inclusion and exclusion:
        /// <c>n - floor(n/2) - floor(n/5) + floor(n/10)</c>, and nothing below an empty range.
        /// </summary>
        [Theory]
        [InlineData(99, 40)]
        [InlineData(1000, 400)]
        [InlineData(10, 4)]
        [InlineData(0, 0)]
        [InlineData(-5, 0)]
        public void ASymbolicRangeIsCountedWithFloors(int n, int expected)
        {
            var count = "sum(iverson(not 2 divides k and not 5 divides k), k, 1, n)".ToEntity().Simplify();
            Assert.DoesNotContain(count.Nodes, node => node is Entity.Summationf);
            Assert.Equal(Entity.Number.Integer.Create(expected), count.Substitute("n", n).Simplify());
        }

        /// <summary>
        /// A sum over an index an equation fixes is a bracket of the point, so a nested sum is
        /// counted from the inside with the outer index a symbol: for each <c>r</c>, the triples
        /// of Ex 8.3.18 with that many red balls.
        /// </summary>
        [Theory]
        [InlineData("0", "3")]
        [InlineData("1", "4")]
        [InlineData("3", "2")]
        [InlineData("5", "0")]
        [InlineData("1/2", "0")]
        public void ANestedSumIsCountedFromTheInside(string r, string expected)
        {
            var count = "sum(sum(iverson(r + b + g = 4), g, 0, 3), b, 0, 3)".ToEntity().Simplify();
            Assert.DoesNotContain(count.Nodes, node => node is Entity.Summationf);
            Assert.Equal(expected.ToEntity(), count.Substitute("r", r.ToEntity()).Simplify());
        }

        /// <summary>
        /// Whether a linear function of the index is whole depends only on its constant term.
        /// </summary>
        [Theory]
        [InlineData("3", "5")]
        [InlineData("1/3", "0")]
        public void WholenessOfALinearFunctionIsAFactor(string m, string expected)
        {
            var count = "sum(iverson(k + m in ZZ), k, 1, 5)".ToEntity().Simplify();
            Assert.DoesNotContain(count.Nodes, node => node is Entity.Summationf);
            Assert.Equal(expected.ToEntity(), count.Substitute("m", m.ToEntity()).Simplify());
        }

        /// <summary>
        /// A point counts once where it is whole and in the range, and a point that is not a
        /// number says so with a bracket of its own.
        /// </summary>
        [Theory]
        [InlineData("5", "1")]
        [InlineData("11", "0")]
        [InlineData("5/2", "0")]
        public void ASymbolicPointCountsWhereItIsWholeAndInRange(string m, string expected)
        {
            var count = "sum(iverson(k = m), k, 1, 10)".ToEntity().Simplify();
            Assert.DoesNotContain(count.Nodes, node => node is Entity.Summationf);
            Assert.Equal(expected.ToEntity(), count.Substitute("m", m.ToEntity()).Simplify());
        }

        [Theory]
        [InlineData("iverson(x > 0)", "iverson(x > 0)")]
        [InlineData("iverson(n divides 12) + 1", "iverson(n divides 12) + 1")]
        public void ThePrintedFormIsTheFunction(string input, string expected)
            => Assert.Equal(expected, input.ToEntity().Stringize());

        [Theory]
        [InlineData("iverson(x > 0)")]
        [InlineData("2 iverson(x > 0 and y > 0)")]
        public void ThePrintedFormParsesBackToTheSameExpression(string input)
            => Assert.Equal(input.ToEntity(), input.ToEntity().Stringize().ToEntity());

        /// <summary>
        /// The double bracket, which no vector, matrix or interval prints as.
        /// </summary>
        [Fact]
        public void TheLatexIsTheDoubleBracket()
            => Assert.Equal(@"[\![x > 0]\!]", "iverson(x > 0)".ToEntity().Latexize());

        [Fact]
        public void TheSympyCodeIsAPiecewise()
            => Assert.Contains("sympy.Piecewise((1, x > 0), (0, True))", MathS.ToSympyCode("iverson(x > 0)".ToEntity()));

        [Fact]
        public void TheEntryPointIsTheNode()
            => Assert.Equal("iverson(x > 0)".ToEntity(), MathS.Iverson("x > 0"));
    }
}

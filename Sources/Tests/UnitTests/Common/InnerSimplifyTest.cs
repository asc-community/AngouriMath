using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace UnitTests.Common
{
    public class InnerSimplifyTest
    {
        [Theory(Skip = "Moved to the 1.2.2 milestone, see issue here https://github.com/asc-community/AngouriMath/issues/263")]
        [InlineData("3 ^ 100")]
        [InlineData("(-3) ^ 100")]
        [InlineData("0.01 ^ 100")]
        public void ShouldNotChangeTest(string expr)
        {
            var expected = expr.ToEntity();
            var actual = expr.ToEntity().InnerSimplified;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a provided b", "a provided b")]
        [InlineData("a provided (b provided c)", "a provided c and b")]
        [InlineData("(a provided (b provided c)) + 1 + x + (y provided h)", "(a + 1 + x + y) provided (c and b and h)")]
        [InlineData("a provided 0 = 0", "a")]
        [InlineData("a provided 0 = 1", "NaN")]
        [InlineData("a provided b provided c provided d", "a provided d and (c and b)")]
        [InlineData(@"[ { sqrt(3), sqrt(5) }; sqrt(10) ]", @"{ [ sqrt(3); sqrt(10) ], [ sqrt(5); sqrt(10) ] }")]
        [InlineData(@"[ sqrt(3); { sqrt(5), sqrt(10) } ]", @"{ [ sqrt(3); sqrt(5) ], [ sqrt(3); sqrt(10) ] }")]
        [InlineData(@"[ { sqrt(2), sqrt(3) }; { sqrt(5), sqrt(10) } ]", @"{ [ sqrt(2); sqrt(5) ], [ sqrt(3); sqrt(5) ], [ sqrt(2); sqrt(10) ], [ sqrt(3); sqrt(10) ] }")]
        [InlineData(@"piecewise((a provided true),  (b provided false), (c provided false))",    "a")]
        [InlineData(@"piecewise((a provided true),  (b provided true),  (c provided false))",    "a")]
        [InlineData(@"piecewise((a provided true),  (b provided false), (c provided true))",     "a")]
        [InlineData(@"piecewise((a provided false), (b provided false), (c provided false))",    "NaN")]
        [InlineData(@"piecewise((a provided false), (b provided true),  (c provided false))",    "b")]
        [InlineData(@"piecewise((a provided false), (b provided false), (c provided false), d)", "d")]
        [InlineData(@"piecewise((a provided false), (c provided f),     d)",                     "piecewise((c provided f), d)")]
        [InlineData(@"piecewise((a provided false), (c provided f),     (g provided false), d)",         "piecewise((c provided f), d)")]
        [InlineData(@"piecewise((a provided false), (c provided true),  (g provided k),     d)", "c")]
        [InlineData(@"piecewise((a provided h),     (c provided true),  (g provided k),     d)", "piecewise((a provided h), c)")]

        [InlineData("a and true", "a")]
        [InlineData("a and false", "false")]
        [InlineData("true and a", "a")]
        [InlineData("false and a", "false")]

        [InlineData("a or true", "true")]
        [InlineData("a or false", "a")]
        [InlineData("true or a", "true")]
        [InlineData("false or a", "a")]

        [InlineData("a xor true", "not a")]
        [InlineData("a xor false", "a")]
        [InlineData("true xor a", "not a")]
        [InlineData("false xor a", "a")]

        [InlineData("a implies true", "true")]
        [InlineData("a implies false", "not a")]
        [InlineData("true implies a", "a")]
        [InlineData("false implies a", "true")]
        public void ShouldChangeTo(string from, string to)
        {
            var expected = to.ToEntity().Replace(c => c == "NaN" ? MathS.NaN : c);
            var fromEntity = from.ToEntity();
            var actualInnerSimplified = fromEntity.InnerSimplified;
            var actualInnerEvaled = from.ToEntity().Evaled;
            Assert.Equal(expected, actualInnerSimplified);
            Assert.Equal(expected, actualInnerEvaled);
        }

        [Theory, CombinatorialData]
        public void PiecewiseInnerSimplifyOneArgumentFunction(
            [CombinatorialValues("arcsin", "arccos", "sin", "cos", 
            "tan", "cotan", "arctan", "arccotan", "sec", "cosec",
            "arcsec", "arccosec")]
            string func)
        {
            Entity initial = @$"{func}(piecewise(a provided b, c provided d, e provided f))";
            Entity expected = @$"piecewise(({func}(a) provided b), ({func}(c) provided d), ({func}(e) provided f))";
            var actual = initial.InnerSimplified;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("", " + 3")]
        [InlineData("", " - 3")]
        [InlineData("", " * 3")]
        [InlineData("", " / 3")]
        [InlineData("log(", ", 3)")]
        [InlineData("log(", ", 4)")]
        [InlineData("log(3, ", ")")]
        [InlineData("log(4, ", ")")]
        public void PiecewiseInnerSimplifyTwoArgumentFunction(string before, string after)
        {
            Entity initial = @$"{before}piecewise((a provided b), (c provided d), (e provided f)){after}";
            Entity expected = @$"piecewise(({before}a{after} provided b), ({before}c{after} provided d), ({before}e{after} provided f))";
            var actual = initial.InnerSimplified;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("", " + ")]
        [InlineData("", " - ")]
        [InlineData("", " * ")]
        [InlineData("", " / ")]
        [InlineData("", " ^ ")]
        [InlineData("log", ", ")]
        public void PiecewiseAndPiecewise(string before, string inBetween)
        {
            var rawInitial = @$"{before}(piecewise(a provided b, c provided d) {inBetween} piecewise(k provided f, g provided j))";
            var rawExpected = @$"piecewise({before}(a {inBetween} k) provided b and f, {before}(a {inBetween} g) provided b and j, {before}(c {inBetween} k) provided d and f, {before}(c {inBetween} g) provided d and j)";
            var initial = (Entity)rawInitial;
            var expected = (Entity)rawExpected;
            var actual = initial.InnerSimplified;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("[ 1 , 3 , 5 ] * 3", "[ 3 , 9 , 15 ]")]
        [InlineData("3 * [ 1 , 3 , 5 ]", "[ 3 , 9 , 15 ]")]
        [InlineData("[ 1 , 3 , 5 ] + 3", "[ 4 , 6 , 8 ]")]
        [InlineData("3 + [ 1 , 3 , 5 ]", "[ 4 , 6 , 8 ]")]
        [InlineData("[ 1 , 3 , 5 ] - 3", "[ -2 , 0 , 2 ]")]
        [InlineData("3 - [ 1 , 3 , 5 ]", "[ 2 , 0 , -2 ]")]
        [InlineData("[ 6 , 3 , 18 ] / 3", "[ 2 , 1 , 6 ]")]
        [InlineData("[ [ 1, 0, 0 ] , [ 0, 1, 0 ] , [ 0, 0, 1 ] ] * [ 6 , 3 , 18 ]", "[ 6 , 3 , 18 ]")]
        [InlineData("[ 1 ]", "1")]
        [InlineData("[ [1] ]", "1")]
        [InlineData("[ 1, 2, 3 ]T * [ a , b , c ]", "a + 2b + 3c")]
        [InlineData("[[1, 0], [0, 1]] ^ 12", "[[1, 0], [0, 1]]")]
        [InlineData("(|[3, 4]|)", "5")]
        [InlineData("(|[2, 3, 6]|)", "7")]
        public void TestMatrices(string before, string after)
            => ShouldChangeTo(before, after);
    }
}



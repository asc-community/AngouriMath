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
        public void ShouldChangeTo(string from, string to)
        {
            var expected = to.ToEntity().Replace(c => c == "NaN" ? MathS.NaN : c);
            var actualInnerSimplified = from.ToEntity().InnerSimplified;
            var actualInnerEvaled = from.ToEntity().Evaled;
            Assert.Equal(expected, actualInnerSimplified);
            Assert.Equal(expected, actualInnerEvaled);
        }

        [Theory, CombinatorialData]
        public void PiecewiseInnerSimplifyOneArgumentFunction(
            [CombinatorialValues("sin", "cos", "arcsin", "arccos", 
            "tan", "cotan", "arctan", "arccotan", "sec", "cosec",
            "arcsec, arccosec")]
            string func)
        {
            Entity initial = @$"{func}(piecewise((a, b), (c, d), (e, f)))";
            Entity expected = @$"piecewise(({func}(a), b), ({func}(c), d), ({func}(e), f))";
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
            Entity initial = @$"{before}piecewise((a, b), (c, d), (e, f)){after}";
            Entity expected = @$"piecewise(({before}a{after}, b), ({before}c{after}, d), ({before}e{after}, f))";
            var actual = initial.InnerSimplified;
            Assert.Equal(expected, actual);
        }
    }
}


//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using System.Linq;
using Xunit;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Common
{
    public class InnerSimplifyTest
    {
        [Theory(Skip = "Moved to the 1.2.2 milestone, see issue here https://github.com/asc-community/AngouriMath/issues/263")]
        [InlineData("3 ^ 100")]
        [InlineData("(-3) ^ 100")]
        [InlineData("0.01 ^ 100")]
        [InlineData("integral((4x^2+5x-4)/((5x-2)(4x^2+2)), x)")]
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

        [Fact] public void PiecewiseIntegrate1() =>
            "piecewise(2 provided a, 3)".Integrate("x")
            .ShouldBe("piecewise(2x provided a, 3x)");

        [Fact] public void PiecewiseIntegrate2() =>
            "piecewise(2 provided a, 3)".Integrate("d")
            .ShouldBe("piecewise(2d provided a, 3d)");

        [Fact] public void PiecewiseIntegrate3() =>
            "piecewise(x provided a, 1/x)".Integrate("x")
            .ShouldBe("piecewise(x ^ 2 / 2 provided a, ln(x))");

        [Fact] public void PiecewiseDerivative1() =>
            "piecewise(x provided a, 1/x)".Differentiate("x")
            .ShouldBe("piecewise(1 provided a, ((-1)) / ((x) ^ (2)))");

        // although this one will be collapsed into 0 with #327 implemented
        [Fact] public void PiecewiseDerivative2() =>
            "piecewise(x provided a, 1/x)".Differentiate("y")
            .ShouldBe("piecewise(0 provided a, 0)");

        [Fact] public void PiecewiseLimit1() =>
            "piecewise(sin(a x) / (b x) provided a, sin(b x) / sin(a x))".Limit("x", "0")
            .ShouldBe("piecewise(a / b provided a, b / a)");




        [Fact] public void PiecewiseIntegrate1NodeEvaled() =>
            "integral(piecewise(2 provided a, 3), x)".ToEntity().Evaled
            .ShouldBe("piecewise(2x provided a, 3x)");

        [Fact] public void PiecewiseIntegrate2NodeEvaled() =>
            "integral(piecewise(2 provided a, 3), d)".ToEntity().Evaled
            .ShouldBe("piecewise(2d provided a, 3d)");

        [Fact] public void PiecewiseIntegrate3NodeEvaled() =>
            "integral(piecewise(x provided a, 1/x), x)".ToEntity().Evaled
            .ShouldBe("piecewise(x ^ 2 / 2 provided a, ln(x))".ToEntity().Evaled);

        [Fact] public void PiecewiseDerivative1NodeEvaled() =>
            "derivative(piecewise(x provided a, 1/x), x)".ToEntity().Evaled
            .ShouldBe("piecewise(1 provided a, ((-1)) / ((x) ^ (2)))");

        // although this one will be collapsed into 0 with #327 implemented
        [Fact] public void PiecewiseDerivative2NodeEvaled() =>
            "derivative(piecewise(x provided a, 1/x), y)".ToEntity().Evaled
            .ShouldBe("piecewise(0 provided a, 0)");

        [Fact] public void PiecewiseLimit1NodeEvaled() =>
            "limit(piecewise(sin(a x) / (b x) provided a, sin(b x) / sin(a x)), x, 0)".ToEntity().Evaled
            .ShouldBe("piecewise(a / b provided a, b / a)");




        [Fact] public void PiecewiseIntegrate1NodeInnerSimplified() =>
            "integral(piecewise(2 provided a, 3), x)".ToEntity().InnerSimplified
            .ShouldBe("piecewise(2x provided a, 3x)");

        [Fact] public void PiecewiseIntegrate2NodeInnerSimplified() =>
            "integral(piecewise(2 provided a, 3), d)".ToEntity().InnerSimplified
            .ShouldBe("piecewise(2d provided a, 3d)");

        [Fact] public void PiecewiseIntegrate3NodeInnerSimplified() =>
            "integral(piecewise(x provided a, 1/x), x)".ToEntity().InnerSimplified
            .ShouldBe("piecewise(x ^ 2 / 2 provided a, ln(x))");

        [Fact] public void PiecewiseDerivative1NodeInnerSimplified() =>
            "derivative(piecewise(x provided a, 1/x), x)".ToEntity().InnerSimplified
            .ShouldBe("piecewise(1 provided a, ((-1)) / ((x) ^ (2)))");

        // although this one will be collapsed into 0 with #327 implemented
        [Fact] public void PiecewiseDerivative2NodeInnerSimplified() =>
            "derivative(piecewise(x provided a, 1/x), y)".ToEntity().InnerSimplified
            .ShouldBe("piecewise(0 provided a, 0)");

        [Fact] public void PiecewiseLimit1NodeInnerSimplified() =>
            "limit(piecewise(sin(a x) / (b x) provided a, sin(b x) / sin(a x)), x, 0)".ToEntity().InnerSimplified
            .ShouldBe("piecewise(a / b provided a, b / a)");

        [Theory]
        [InlineData("pi", "1")]
        [InlineData("pi / 2", "1 / 2")]
        [InlineData("2pi", "2 * 1")]
        [InlineData("pi * 2", "1 * 2")]
        [InlineData("pi + pi / 2", "1 + 1 / 2")]
        [InlineData("pi / 2 + pi / 3", "1 / 2 + 1 / 3")]
        [InlineData("pi / 2 - 2 pi", "1 / 2 - 2 * 1")]
        [InlineData("pi / 2 - pi * 2", "1 / 2 - 1 * 2")]
        [InlineData("1 / (2 / pi)", "1 / (2 / 1)")]
        [InlineData("1 / (2 / pi) + pi / 2", "1 / (2 / 1) + 1 / 2")]
        [InlineData("1 / (2 / (3 / (4 / pi)))", "1 / (2 / (3 / (4 / 1)))")]
        [InlineData("pi * pi", "1 * pi")]
        public void TestDivideByEntityStrict(string inputRaw, string expectedRaw)
        {
            Entity input = inputRaw;
            var actual = MathS.UnsafeAndInternal.DivideByEntityStrict(input, "pi");
            actual.ShouldBeNotNull().ShouldBe(expectedRaw);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("1 + pi")]
        [InlineData("1 / pi")]
        [InlineData("2 - pi")]
        [InlineData("pi ^ 2")]
        [InlineData("sin(pi)")]
        public void TestDivideByEntityStrictShouldBeNull(string inputRaw)
        {
            Entity input = inputRaw;
            MathS.UnsafeAndInternal.DivideByEntityStrict(input, "pi").ShouldBeNull();
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("1/3", 1)]
        [InlineData("1/6", 1)]
        [InlineData("2/3", 1)]
        [InlineData("1/2 + 1/3", 2)]
        [InlineData("19", 1)]
        [InlineData("3/2 + 1/3", 2)]
        [InlineData("3/2 + 1/3 + 4/17", -1 /* current algo cannot take this */)]
        [InlineData("3/2 + 1/3 + 1/19", -1)]
        [InlineData("37/17", 2)]
        public void TestRepresentRational(string ratRaw, int countOfForms)
        {
            var rat = (Rational)ratRaw.ToEntity().InnerSimplified;

            var forms = new[] { "1/2", "1/3", "1/6", "1/17" }.Select(c => (Rational)c.ToEntity().InnerSimplified);

            var repr = MathS.UnsafeAndInternal.RepresentRational(rat, forms);

            if (countOfForms is -1)
                repr.ShouldBeNull();
            else
            {
                Rational res = 0;
                foreach (var (coef, form) in repr.ShouldBeNotNull().ShouldCountTo(countOfForms))
                    res += coef * form;
                res.ShouldBe(rat);
            }
        }
        
        [Theory]
        [InlineData("pi / 2")]
        [InlineData("-pi / 2")]
        [InlineData("-5pi / 2")]
        [InlineData("-9pi / 2")]
        [InlineData("9pi / 2")]
        [InlineData("5pi / 2")]
        public void TanShouldBeNaN(string angle)
            => MathS.Tan(angle).Evaled.ShouldBe(MathS.NaN);
        
        [Theory]
        [InlineData("pi / 2")]
        [InlineData("-pi / 2")]
        [InlineData("-5pi / 2")]
        [InlineData("-9pi / 2")]
        [InlineData("9pi / 2")]
        [InlineData("5pi / 2")]
        public void SecShouldBeNaN(string angle)
            => MathS.Sec(angle).Evaled.ShouldBe(MathS.NaN);
        
        [Theory]
        [InlineData("pi")]
        [InlineData("-pi")]
        [InlineData("-5pi")]
        [InlineData("-9pi")]
        [InlineData("9pi")]
        [InlineData("5pi")]
        public void CotanShouldBeNaN(string angle)
            => MathS.Cotan(angle).Evaled.ShouldBe(MathS.NaN);
        
        [Theory]
        [InlineData("pi")]
        [InlineData("-pi")]
        [InlineData("-5pi")]
        [InlineData("-9pi")]
        [InlineData("9pi")]
        [InlineData("5pi")]
        public void CosecShouldBeNaN(string angle)
            => MathS.Cosec(angle).Evaled.ShouldBe(MathS.NaN);

        [Theory]
        [InlineData("1 / 0 > 0")]
        [InlineData("1 / 0 < 0")]
        [InlineData("1 / 0 >= 0")]
        [InlineData("1 / 0 <= 0")]
        [InlineData("0 < 1 / 0")]
        [InlineData("0 > 1 / 0")]
        [InlineData("0 <= 1 / 0")]
        [InlineData("0 >= 1 / 0")]
        [InlineData("i > i")]
        [InlineData("i < i")]
        [InlineData("i >= (i + 1)")]
        [InlineData("i <= (i + 1)")]
        public void InequalityShouldBeNaN(string expr)
            => expr.ToEntity().Evaled.ShouldBe(MathS.NaN);

        [Theory]
        [InlineData("1 / 0 > 0")]
        [InlineData("1 / 0 < 0")]
        [InlineData("1 / 0 >= 0")]
        [InlineData("1 / 0 <= 0")]
        [InlineData("0 < 1 / 0")]
        [InlineData("0 > 1 / 0")]
        [InlineData("0 <= 1 / 0")]
        [InlineData("0 >= 1 / 0")]
        [InlineData("i > i")]
        [InlineData("i < i")]
        [InlineData("i >= -i")]
        [InlineData("i <= -i")]
        public void InequalityShouldBeKeptInnerSimplify(string expr)
            => expr.ToEntity().InnerSimplified.ShouldBe(expr);
    }
}



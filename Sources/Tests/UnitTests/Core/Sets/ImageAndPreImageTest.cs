//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// The image and the pre-image of a set under an expression: item 6 of the #1409 docket
    /// (Sullivan and Mackey, §7.3), the half that needs no pair. <c>image(f(x), x in A)</c> is
    /// <c>{ f(x) : x in A }</c>, an indexed union of singletons; <c>preimage(f(x), x in A, Y)</c>
    /// is <c>{ x in A : f(x) in Y }</c>, solved where the membership is read.
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1409"/>
    /// </summary>
    [Trait("Area", "Sets")]
    public sealed class ImageAndPreImageTest
    {
        private static Entity Evaluated(string expression) => expression.ToEntity().Evaled;

        /// <summary>Ex 7.3.2's table as an expression over a listed set; Ex 7.3.3, Fahrenheit on <c>(0, 100)</c>; a square on an interval through zero.</summary>
        [Theory]
        [InlineData("image(x^2, x in {1, 2, 3})", "{1, 4, 9}")]
        [InlineData("image(2 x + 1, x in {0, 1, 2})", "{1, 3, 5}")]
        [InlineData("image(9 c / 5 + 32, c in (0; 100))", "(32; 212)")]
        [InlineData("image(x^2, x in [-1; 2])", "[0; 4]")]
        [InlineData("image(e^x, x in [0; 1])", "[1; e]")]
        [InlineData("image(x^2, x in {})", "{}")]
        public void AnImageIsListedOrAnInterval(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, Evaluated(expression));

        /// <summary>
        /// A quotient of polynomials the name occurs in more than once, by its critical points and
        /// its limits. Ex 7.3.4, <c>x^2/(1 + x^2)</c> on <c>RR</c>, is <c>[0, 1)</c>: <c>0</c> is
        /// taken at the turning point and <c>1</c> only approached. Prob 7.8.8,
        /// <c>(2x - 1)/(2x (1 - x))</c> on <c>(0, 1)</c>, is every real. A turning point takes the
        /// value an open end only approaches, so <c>x^3 - 3x</c> on <c>(-2, 2)</c> is closed.
        /// </summary>
        [Theory]
        [InlineData("image(x^2/(1 + x^2), x in RR)", "[0; 1)")]
        [InlineData("image((2 x - 1)/(2 x (1 - x)), x in (0; 1))", "(-oo; +oo)")]
        [InlineData("image(x^2 - 2 x, x in [0; 3])", "[-1; 3]")]
        [InlineData("image(x^3 - 3 x, x in (-2; 2))", "[-2; 2]")]
        [InlineData("image(x^3 - 3 x, x in RR)", "(-oo; +oo)")]
        [InlineData("image(x^4 - 2 x^2, x in RR)", "[-1; +oo)")]
        [InlineData("image((x^2 + 1)/x, x in (0; +oo))", "[2; +oo)")]
        [InlineData("image(x/(1 + x^2), x in RR)", "[-1/2; 1/2]")]
        public void AQuotientOfPolynomialsHasItsImageByCalculus(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, Evaluated(expression));

        /// <summary>
        /// Across a pole or a removed point the image is a union, compared at points: §7.3.5
        /// Try 1, <c>x/(1 + x)</c> on <c>RR \ {-1}</c>, is <c>RR \ {1}</c>.
        /// </summary>
        [Theory]
        [InlineData("image(x/(1 + x), x in RR \\ {-1})", "-1 0 1/2 999/1000 1001/1000 2", "1")]
        [InlineData("image(1/x - x, x in RR \\ {0})", "-2 0 1 5", "")]
        [InlineData("image(1/(x^2 - 1), x in RR \\ {-1, 1})", "-2 -1 1/2 3", "-1/2 0")]
        public void AnImageAcrossAPoleIsAUnion(string expression, string inside, string outside)
        {
            var image = (Set)Evaluated(expression);
            foreach (var (points, expected) in new[] { (inside, true), (outside, false) })
                foreach (var point in points.Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
                {
                    Assert.True(image.TryContains(point.ToEntity(), out var contains), $"{image} does not decide {point}");
                    Assert.Equal(expected, contains);
                }
        }

        /// <summary>
        /// Membership in an image is membership in what it evaluates to: <c>1</c> is approached by
        /// <c>x^2/(1 + x^2)</c> and never taken.
        /// </summary>
        [Fact]
        public void MembershipInAnImageIsDecided()
        {
            Assert.Equal(Boolean.True, Evaluated("1/2 in image(x^2/(1 + x^2), x in RR)"));
            Assert.Equal(Boolean.False, Evaluated("1 in image(x^2/(1 + x^2), x in RR)"));
            Assert.Equal(Boolean.False, Evaluated("1/2 in image(9 c / 5 + 32, c in (0; 100))"));
            Assert.Equal(Boolean.True, Evaluated("100 in image(9 c / 5 + 32, c in (0; 100))"));
        }

        /// <summary>
        /// An image the calculus cannot read stays the family it is: a function that is not a
        /// quotient of polynomials, a derivative of degree five whose zeros the solver does not
        /// list, and a symbolic coefficient.
        /// </summary>
        [Theory]
        [InlineData("image(x sin(x), x in RR)")]
        [InlineData("image(x^6 + x, x in RR)")]
        [InlineData("image(a x^2 + x, x in RR)")]
        public void AnImageTheCalculusCannotReadStaysAnObject(string expression)
        {
            var image = expression.ToEntity();
            Assert.Equal(image, image.Evaled);
        }

        /// <summary>Ex 7.3.10: the pre-images of <c>x^2</c>; §7.3.5 rows over the integers.</summary>
        [Theory]
        [InlineData("preimage(x^2, x in RR, {1})", "{-1, 1}")]
        [InlineData("preimage(x^2, x in RR, (-oo; 0))", "{}")]
        [InlineData("preimage(x^2, x in ZZ, {1, 4})", "{-2, -1, 1, 2}")]
        [InlineData("preimage(2 x, x in ZZ+, [1; 5])", "{1, 2}")]
        [InlineData("preimage(x + 1, x in {0, 1, 2, 3}, {2, 3})", "{1, 2}")]
        public void APreImageIsSolved(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, Evaluated(expression));

        /// <summary>The pre-image of <c>(0, 1)</c> under <c>x^2</c> is <c>(-1, 0) ∪ (0, 1)</c>, not the book's <c>(-1, 1)</c>: compared at points.</summary>
        [Fact]
        public void ThePreImageOfAnOpenIntervalExcludesZero()
        {
            var preimage = (Set)Evaluated("preimage(x^2, x in RR, (0; 1))");
            foreach (var (at, inside) in new[] { (-0.5, true), (0.5, true), (0.0, false), (1.0, false), (-1.5, false) })
            {
                Assert.True(preimage.TryContains((Entity)at, out var contains), $"{preimage} does not decide {at}");
                Assert.Equal(inside, contains);
            }
        }

        /// <summary>A pre-image the solver cannot read stays the set builder it is, and answers membership itself.</summary>
        [Fact]
        public void AnUnreadPreImageStaysWritten()
        {
            var preimage = "preimage(x^2, x in RR, ZZ)".ToEntity();
            Assert.Equal("{ x in RR : x^2 in ZZ }".ToEntity(), preimage);
            Assert.Equal(preimage, preimage.Evaled);
            Assert.Equal(Boolean.True, Evaluated("2 in preimage(x^2, x in RR, ZZ)"));
            Assert.Equal(Boolean.False, Evaluated("1/2 in preimage(x^2, x in RR, ZZ)"));
        }
    }
}

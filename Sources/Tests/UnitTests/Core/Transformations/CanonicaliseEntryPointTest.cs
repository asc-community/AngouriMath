//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The two canonicalisers reached from <see cref="Entity"/>, where a caller will look for
    /// them, rather than only through the transformation layer.
    /// https://github.com/asc-community/AngouriMath/issues/746
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class CanonicaliseEntryPointTest
    {
        /// <summary>
        /// What the method is for: equality of the forms is a real test of equality of the
        /// expressions, which comparing simplified forms is not.
        /// </summary>
        [Theory]
        [InlineData("x + y", "y + x")]
        [InlineData("x * y * a", "a * y * x")]
        [InlineData("(x + y) + a", "x + (y + a)")]
        [InlineData("1/2 - x", "-x + 1/2")]
        public void CanonicaliseMakesTwoWritingsOneTree(string left, string right)
            => Assert.Equal(left.ToEntity().Canonicalise(), right.ToEntity().Canonicalise());

        [Theory]
        [InlineData("x + y", "x * y")]
        [InlineData("x - y", "y - x")]
        public void AndDoesNotConflateWhatDiffers(string left, string right)
            => Assert.NotEqual(left.ToEntity().Canonicalise(), right.ToEntity().Canonicalise());

        /// <summary>It is a form, so applying it twice changes nothing.</summary>
        [Theory]
        [InlineData("1/2 - x")]
        [InlineData("sin(x) + cos(x) + 1/3")]
        [InlineData("(x + y) * (a - 1/2)")]
        public void CanonicaliseIsAFixedPoint(string expression)
        {
            var once = expression.ToEntity().Canonicalise();
            Assert.Equal(once, once.Canonicalise());
        }

        /// <summary>The rational form decides equality on the sublanguage where it can.</summary>
        [Theory]
        [InlineData("1/x + 1/y", "(x + y) / (x * y)")]
        [InlineData("2 * x / (4 * y)", "x / (2 * y)")]
        [InlineData("x / y + 1", "(x + y) / y")]
        public void TheRationalFormDecidesEquality(string left, string right)
        {
            var one = left.ToEntity().CanonicaliseAsRationalFunction();
            var two = right.ToEntity().CanonicaliseAsRationalFunction();
            Assert.NotNull(one);
            Assert.Equal(one, two);
        }

        /// <summary>
        /// And answers nothing where it cannot, which is the boundary being in the signature
        /// rather than in a comment.
        /// </summary>
        [Theory]
        [InlineData("sin(x) / x")]
        [InlineData("sqrt(x)")]
        [InlineData("e ^ x")]
        public void TheRationalFormRefusesWhatIsNotOne(string expression)
            => Assert.Null(expression.ToEntity().CanonicaliseAsRationalFunction());

        /// <summary>
        /// A removable singularity is not removed: the quotient is undefined where the
        /// polynomial is not, so they are not the same function and the form must not say
        /// they are.
        /// </summary>
        [Fact]
        public void ACancelledFactorKeepsItsCondition()
        {
            var cancelled = "x / x".ToEntity().CanonicaliseAsRationalFunction();
            Assert.True(cancelled is Entity.Providedf);
            Assert.NotEqual("1".ToEntity().CanonicaliseAsRationalFunction(), cancelled);
        }

        /// <summary>
        /// Neither is applied by anything: asking to simplify must not start returning
        /// canonicalised trees behind the caller's back.
        /// </summary>
        [Fact]
        public void NothingRunsThemByDefault()
        {
            Assert.Equal("y + x".ToEntity(), "y + x".ToEntity().InnerSimplified);
            Assert.Equal("1 / x + 1 / y".ToEntity(), "1/x + 1/y".ToEntity().Simplify());
        }
    }
}

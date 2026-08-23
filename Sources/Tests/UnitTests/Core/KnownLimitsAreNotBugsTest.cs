//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System.Linq;
using AngouriMath.Core.Exceptions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A capability the library does not have must say so, and must not ask to be reported.
    /// </summary>
    /// <remarks>
    /// These used to throw through <c>FutureReleaseException.Raised(feature, plannedVersion)</c>,
    /// which turned itself into an <see cref="AngouriBugException"/> — "please report about it
    /// to the official repository" — once the named version had shipped. Every site named
    /// 1.2, 1.2.1 or 1.3, all of which shipped long ago, so twelve known gaps were inviting
    /// bug reports about work nobody had started. An unbuilt feature is not a bug.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class KnownLimitsAreNotBugsTest
    {
        /// <summary>
        /// Degree is no longer the gap; an irreducible factor of degree four or more is.
        /// The sign table answers a polynomial inequality of any degree whose real roots it
        /// can establish completely, and the number of real roots of an irreducible factor is
        /// read off its discriminant — which decides it up to degree three and does not at
        /// four, where a positive discriminant means four real roots or none. So
        /// <c>x^4 + 1</c> and <c>x^4 - 2</c>, both irreducible over Q, are still refused,
        /// and the three cases that used to be listed here are now answered — see
        /// <c>SolveInequality.AHigherDegreePolynomialInequalityIsAnswered</c>.
        /// </summary>
        [Theory]
        [InlineData("x ^ 4 + 1 > 0")]
        [InlineData("x ^ 4 - 2 > 0")]
        [InlineData("x ^ 4 + x + 1 > 0")]
        public void AHigherDegreeInequalitySaysItIsUnsupported(string statement)
        {
            var thrown = Assert.Throws<NotSufficientlySupportedException>(
                () => ((Entity)statement).Solve("x"));
            Assert.DoesNotContain("please report", thrown.Message);
            Assert.Contains("degree", thrown.Message);
        }

        /// <summary>The degrees that are supported must keep working.</summary>
        [Theory]
        [InlineData("x - 1 > 0")]
        [InlineData("x ^ 2 - 1 > 0")]
        [InlineData("x ^ 2 - 2 > 0")]
        [InlineData("x ^ 3 - x > 0")]
        [InlineData("x ^ 4 - 5 * x ^ 2 + 4 > 0")]
        [InlineData("x ^ 6 - 1 >= 0")]
        public void TheDegreesThatAreSupportedStillAnswer(string statement)
        {
            var solutions = ((Entity)statement).Solve("x");
            Assert.DoesNotContain("solve(", solutions.Stringize());
        }

        /// <summary>
        /// The type that turned a known gap into a bug report is gone, so nothing can
        /// reintroduce the behaviour by calling it.
        /// </summary>
        [Fact]
        public void NothingCanAskForAFutureRelease()
        {
            var exceptions = typeof(AngouriBugException).Assembly
                .GetTypes()
                .Select(type => type.FullName)
                .ToList();
            Assert.DoesNotContain("AngouriMath.Core.Exceptions.FutureReleaseException", exceptions);
        }
    }
}

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

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// <c>forall b in B : exists a in A : f(a) = b</c> -- surjectivity onto <c>B</c>, Def 7.4.1 of the
    /// reference (Sullivan and Mackey) -- is the statement that <c>B</c> lies in the image of
    /// <c>A</c> under <c>f</c>, and is decided where the image evaluates: listed over a listed
    /// <c>A</c>, an interval by interval arithmetic. The cube is onto the reals because
    /// <c>(-oo; +oo)^3</c> is <c>(-oo; +oo)</c>, which needed <c>(-oo)^3</c> to be <c>-oo</c>
    /// rather than <c>NaN</c>. <see href="https://github.com/asc-community/AngouriMath/issues/1409"/>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class SurjectivityTest
    {
        [Theory]
        [InlineData("forall b in RR : exists a in RR : a^3 = b", "True")]
        [InlineData("forall b in RR : exists a in RR : 2 a + 1 = b", "True")]
        [InlineData("forall b in RR : exists a in RR : a^2 = b", "False")]
        [InlineData("forall b in [0; +oo) : exists a in RR : a^2 = b", "True")]
        [InlineData("forall b in RR : exists a in RR : e^a = b", "False")]
        [InlineData("forall b in (0; +oo) : exists a in RR : e^a = b", "True")]
        [InlineData("forall b in ZZ : exists a in ZZ : 2 a = b", "False")]
        [InlineData("forall b in {1, 4, 9} : exists a in {1, 2, 3} : a^2 = b", "True")]
        [InlineData("forall b in {1, 4, 9, 16} : exists a in {1, 2, 3} : a^2 = b", "False")]
        public void SurjectivityIsTheImageCoveringTheCodomain(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Evaled);

        /// <summary>The extended reals' whole powers: by parity for <c>-oo</c>, and <c>0</c> for a negative exponent. Each was <c>NaN</c>.</summary>
        [Theory]
        [InlineData("(-oo)^3", "-oo")]
        [InlineData("(-oo)^2", "+oo")]
        [InlineData("(-oo)^(-1)", "0")]
        [InlineData("(+oo)^3", "+oo")]
        [InlineData("(-oo; +oo)^3", "(-oo; +oo)")]
        [InlineData("image(x^3, x in RR)", "(-oo; +oo)")]
        public void AnInfiniteBaseHasItsWholePowers(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Evaled);
    }
}

//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// The inverse hyperbolic functions are area functions, not arc functions, so arcsinh
    /// is not another spelling of arsinh -- it is not a name for anything. It used to be
    /// read as a product of an undeclared variable and its argument and nothing was said
    /// about it. No issue is filed for this.
    /// </summary>
    [Trait("Area", "Convenience")]
    public sealed class ArcHyperbolicRefusedTest
    {
        [Theory]
        [InlineData("arcsinh(x)", "arsinh")]
        [InlineData("arccosh(x)", "arcosh")]
        [InlineData("arctanh(x)", "artanh")]
        [InlineData("arccotanh(x)", "arcotanh")]
        [InlineData("arcsech(x)", "arsech")]
        [InlineData("arccosech(x)", "arcosech")]
        public void AnArcSpellingIsRefusedAndTheAreaOneNamed(string written, string correct)
        {
            var thrown = Assert.Throws<UnrecognizedFunctionParseException>(() => written.ToEntity());
            Assert.Contains("area functions, not arc functions", thrown.Message);
            Assert.Contains(correct, thrown.Message);
        }

        // The spellings that do name something are untouched, as is everything that merely
        // begins the same way and is an ordinary product.
        [Theory]
        [InlineData("arsinh(x)")]
        [InlineData("asinh(x)")]
        [InlineData("arcosh(x)")]
        [InlineData("artanh(x)")]
        [InlineData("arcotanh(x)")]
        [InlineData("arcoth(x)")]
        [InlineData("arsech(x)")]
        [InlineData("arcsch(x)")]
        [InlineData("arcsin(x)")]
        [InlineData("arccos(x)")]
        [InlineData("arctan(x)")]
        [InlineData("arccotan(x)")]
        [InlineData("arcsec(x)")]
        [InlineData("arccosec(x)")]
        [InlineData("arcs(x)")]
        [InlineData("arcsinhh(x)")]
        [InlineData("arc * x")]
        public void EverythingElseParsesAsItDid(string written) => written.ToEntity();
    }
}

//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    [Trait("Area", "Core")]
    public sealed class Domains
    {
        [Theory]
        [InlineData("domain(sqrt(-3), RR)")]
        [InlineData("domain(sqrt(3), ZZ)")]
        [InlineData("domain(sqrt(3), QQ)")]
        [InlineData("1 / 0")]
        [InlineData("domain(true and false, CC)")]
        [InlineData("domain(domain(sqrt(-3), RR) + 3, CC)")]
        [InlineData("domain(sqrt(4 / 9), ZZ)")]
        [InlineData("domain(1 / 2, ZZ)")]
        public void CheckNaN(string expr)
            => Assert.Equal(MathS.NaN, expr.EvalNumerical());

        /// <summary>
        /// A rewritten node keeps the codomain the original carried.
        /// https://github.com/asc-community/AngouriMath/issues/955 -- `Replace` rebuilds every
        /// node on the path to a change, and the rebuilt node used to start from its type's
        /// default, so a domain constraint silently disappeared and the expression began
        /// answering where it had refused.
        /// </summary>
        [Fact]
        public void ARewrittenNodeKeepsItsCodomain()
        {
            var original = "domain(sqrt(x), ZZ)".ToEntity();
            Assert.Equal(Domain.Integer, original.Codomain);

            var rewritten = original.Replace(node =>
                node is Entity.Variable { Name: "x" } ? "y".ToEntity() : node);
            Assert.Equal(Domain.Integer, rewritten.Codomain);
        }

        /// <summary>
        /// The same through `Substitute`, which is what a caller actually reaches for and is
        /// built on `Replace`. sqrt(4/9) is 2/3, which is not an integer, so this must refuse.
        /// </summary>
        [Fact]
        public void SubstitutingIntoADomainKeepsTheConstraint()
            => Assert.Equal(MathS.NaN,
                "domain(sqrt(x), ZZ)".ToEntity().Substitute("x", "4/9".ToEntity()).EvalNumerical());

        /// <summary>And a constraint deeper than one level survives too.</summary>
        [Fact]
        public void ANestedRewriteKeepsTheCodomain()
        {
            var rewritten = "domain(sin(x) + 1, ZZ)".ToEntity()
                .Replace(node => node is Entity.Variable { Name: "x" } ? "y".ToEntity() : node);
            Assert.Equal(Domain.Integer, rewritten.Codomain);
        }

        [Theory]
        [InlineData("domain(sqrt(4), RR)")]
        [InlineData("domain(sqrt(4), QQ)")]
        [InlineData("domain(sqrt(4), ZZ)")]
        [InlineData("domain(sqrt(4 / 9), QQ)")]
        [InlineData("domain(sqrt(-3), CC)")]
        [InlineData("domain(3 / 5, CC)")]
        [InlineData("domain(3 / 5, RR)")]
        [InlineData("domain(3 / 5, QQ)")]
        public void CheckNotNaN(string expr)
            => Assert.NotEqual(MathS.NaN, expr.EvalNumerical());
    }
}

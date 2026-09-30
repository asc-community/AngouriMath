//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// What a binder's body says about the name it binds stays inside the binder: it is required
    /// at every value the name ranges over, and never left free outside, nor left out.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1632">#1632</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class BinderConditionTest
    {
        private const string LogarithmsOverRoots = "sum(ln(x - w), w in { w : w^3 + w + 1 = 0 })";

        /// <summary>
        /// The product rule leaves <c>0 * sum(...)</c>, which keeps the sum's condition: that was
        /// <c>not x - w = 0</c> with <c>w</c> free, and the derivative could not be evaluated.
        /// </summary>
        [Fact]
        public void ADerivativeKeepsNoBoundName()
        {
            var derivative = ("2 * " + LogarithmsOverRoots).ToEntity().Differentiate("x");
            Assert.DoesNotContain(derivative.FreeVariables, v => v.Name == "w");
            Assert.True(derivative.Substitute("x", 0.3).EvalNumerical() is Number.Complex);
        }

        /// <summary>
        /// Over the roots of <c>p</c>, "<c>x - w != 0</c> at every root" is the resultant
        /// <c>p(x) != 0</c>. With <c>p = w^2 - 1</c> that is false at the roots 1 and -1 and true
        /// elsewhere, and it mentions <c>x</c> alone. At 0 too: the zeroth power in the resultant
        /// was written as a power, which is 1 only where its base is not zero.
        /// </summary>
        [Theory]
        [InlineData("1", false)]
        [InlineData("-1", false)]
        [InlineData("2", true)]
        [InlineData("1/2", true)]
        [InlineData("0", true)]
        public void OverRootsTheConditionIsTheResultant(string at, bool defined)
        {
            var condition = "sum(ln(x - w), w in { w : w^2 - 1 = 0 })".ToEntity().DomainCondition;
            Assert.Equal(new[] { "x" }, condition.Vars.Select(v => v.Name).ToArray());
            Assert.Equal(defined ? MathS.Boolean.True : MathS.Boolean.False, condition.Substitute("x", at.ToEntity()).Simplify());
        }

        /// <summary>
        /// The root polynomial itself as the condition: every term is undefined, so the sum is
        /// defined nowhere, and the resultant of <c>p</c> with itself is zero.
        /// </summary>
        [Fact]
        public void ASumOfTermsUndefinedAtEveryRootIsDefinedNowhere()
            => Assert.Equal(MathS.Boolean.False,
                "sum(1/(w^3 + w + 1), w in { w : w^3 + w + 1 = 0 })".ToEntity().DomainCondition);

        /// <summary>
        /// Not left out: <c>0 * sum(1/(k - x), k, 1, n)</c> is undefined wherever the sum is, and a
        /// plain 0 would give it a value at <c>x = 1</c>.
        /// </summary>
        [Fact]
        public void AZeroTimesASumKeepsTheSumsCondition()
        {
            var product = "0 * sum(1/(k - x), k, 1, n)".ToEntity().InnerSimplified;
            Assert.IsType<Providedf>(product);
            Assert.DoesNotContain(product.FreeVariables, v => v.Name == "k");
        }

        /// <summary>A few whole numbers are the conjunction over them, and none is true.</summary>
        [Theory]
        [InlineData("sum(1/(k - x), k, 1, 3)", "2", false)]
        [InlineData("sum(1/(k - x), k, 1, 3)", "5", true)]
        [InlineData("sum(1/(k - x), k, 3, 1)", "2", true)]
        public void AFiniteRangeIsTheConjunctionOverIt(string sum, string at, bool defined)
        {
            var condition = sum.ToEntity().DomainCondition;
            Assert.DoesNotContain(condition.Vars, v => v.Name == "k");
            Assert.Equal(defined ? MathS.Boolean.True : MathS.Boolean.False, condition.Substitute("x", at.ToEntity()).Simplify());
        }

        /// <summary>
        /// A set builder admits the members its predicate is defined at: its condition on its own
        /// name is whom it admits, not where the set is defined.
        /// </summary>
        [Fact]
        public void ASetBuildersOwnConditionIsMembership()
            => Assert.DoesNotContain("{ w : 1/w > a }".ToEntity().DomainCondition.Vars, v => v.Name == "w");
    }
}

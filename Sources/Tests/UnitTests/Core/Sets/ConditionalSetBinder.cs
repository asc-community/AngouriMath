//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using System.Linq;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// A conditional set's bound variable is renamed in <see cref="Entity.DirectChildren"/>, so that
    /// the <c>x</c> inside <c>{ x : x &gt; 0 }</c> is not read as the same <c>x</c> that may be free
    /// outside it.
    /// </summary>
    /// <remarks>
    /// https://github.com/asc-community/AngouriMath/issues/891 — the replacement name used to be
    /// four lowercase letters read off the predicate's hash code, and it went through
    /// <c>MathS.Var</c>, which parses. Four lowercase letters can spell <c>true</c>, which parses to
    /// a boolean and not a variable, so the conversion threw; and they can spell a variable the
    /// predicate already uses, which captures it silently. Both are properties of the name, so both
    /// are tested here as properties of the name.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class ConditionalSetBinder
    {
        private static Variable[] BinderOf(ConditionalSet set) =>
            set.DirectChildren[0].Vars.Where(variable => variable.Name.StartsWith("%")).ToArray();

        [Theory]
        [InlineData("x", "x > 0")]
        [InlineData("y", "y < 0 and y > -2")]
        [InlineData("x", "x = 0")]
        [InlineData("x", "x ^ 5 - x - 1 = 0")]
        public void TheBinderIsRenamedToATemporary(string variable, string predicate)
        {
            var set = new ConditionalSet(variable, predicate);
            Assert.Single(BinderOf(set));
            Assert.DoesNotContain((Variable)variable, set.DirectChildren[0].Vars);
        }

        /// <summary>
        /// A free variable of the predicate is left alone: only the binder is renamed.
        /// </summary>
        [Fact]
        public void AFreeVariableIsUntouched()
        {
            var set = new ConditionalSet("x", "x > a");
            Assert.Contains((Variable)"a", set.DirectChildren[0].Vars);
            Assert.DoesNotContain((Variable)"x", set.DirectChildren[0].Vars);
        }

        /// <summary>
        /// The name a nested set has already taken is not taken again, which is what stops the outer
        /// binder from capturing the inner one. Reachable only because the inner set's renamed
        /// predicate is what the outer set is built over.
        /// </summary>
        [Fact]
        public void ANestedBinderDoesNotReuseTheInnerName()
        {
            var inner = new ConditionalSet("y", "y > 0").DirectChildren[0];
            var outer = new ConditionalSet("x", inner & (Entity)"x > 0");
            var names = outer.DirectChildren[0].Vars
                .Select(variable => variable.Name)
                .Where(name => name.StartsWith("%"))
                .Distinct()
                .ToArray();
            Assert.Equal(2, names.Length);
        }

        /// <summary>
        /// Why a temporary cannot collide with anything a caller wrote, and the reason a name that
        /// <em>can</em> be parsed is the wrong choice here: the parser does not read this one, so no
        /// input can produce a variable of the same name.
        /// </summary>
        [Fact]
        public void ATemporaryIsNotSomethingTheParserProduces() =>
            Assert.Throws<UnhandledParseException>(() => "%1".ToEntity());

        /// <summary>
        /// The rename is there to make two sets that differ only in the name of the bound variable
        /// one set, and it still does.
        /// </summary>
        [Fact]
        public void TwoSetsDifferingOnlyInTheirBinderAgree() =>
            Assert.Equal(new ConditionalSet("x", "x > 0"), new ConditionalSet("y", "y > 0"));
    }
}

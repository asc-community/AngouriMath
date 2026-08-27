//
// Copyright (c) 2019-2026 Angouri.
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
    /// <summary>
    /// <see cref="Entity.DomainConditionIn(Domain)"/> — the domain of definition asked for a
    /// stated reading rather than for whichever one each node happens to carry.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/721">#721</a>
    /// </summary>
    /// <remarks>
    /// The nested rows are the ones this exists for. Every node that has two answers already
    /// wrote them both, selected by its own <c>Codomain</c>; what could not be done was to ask a
    /// <b>tree</b>, because <c>WithCodomain</c> replaces the root's reading and leaves the
    /// children on theirs.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class DomainConditionInAReadingTest
    {
        [Theory]
        // A node with two answers, asked for each.
        [InlineData("arcsin(x)", "True", "abs(x) <= 1")]
        [InlineData("arcsec(x)", "not x = 0", "abs(x) >= 1")]
        [InlineData("log(b, x)", "not b = 0 and not b = 1 and not x = 0", "b > 0 and not b = 1 and x > 0")]
        // An even root leaves the real line for a negative base; an odd one does not.
        [InlineData("sqrt(x)", "True", "x >= 0")]
        [InlineData("x ^ (1/4)", "True", "x >= 0")]
        [InlineData("x ^ (1/3)", "True", "True")]
        [InlineData("x ^ (2/3)", "True", "True")]
        // Nested: the reading has to reach past the root, which is the whole point.
        [InlineData("arcsin(x) + arcsin(y)", "True", "abs(x) <= 1 and abs(y) <= 1")]
        [InlineData("arcsin(x) / y", "not y = 0", "not y = 0 and abs(x) <= 1")]
        [InlineData("sqrt(x) + arcsin(y)", "True", "x >= 0 and abs(y) <= 1")]
        // Nothing to say either way.
        [InlineData("x + y", "True", "True")]
        [InlineData("x / y", "not y = 0", "not y = 0")]
        public void AReadingIsAnswered(string source, string overComplex, string overReal)
        {
            var expr = source.ToEntity();
            Assert.Equal(overComplex, expr.DomainConditionIn(Domain.Complex).Stringize());
            Assert.Equal(overReal, expr.DomainConditionIn(Domain.Real).Stringize());
        }

        /// <summary>
        /// A symbolic exponent is left with the condition it had rather than guessed at: too
        /// strict is a wrong answer here, since this is what a rewrite consults before firing.
        /// </summary>
        [Fact]
        public void ASymbolicExponentIsNotGuessedAt()
        {
            var expr = "x ^ y".ToEntity();
            Assert.Equal(
                expr.DomainConditionIn(Domain.Complex).Stringize(),
                expr.DomainConditionIn(Domain.Real).Stringize());
        }

        /// <summary>
        /// The reading narrows and never widens, so a variable declared over the integers is not
        /// made real by being asked a question about the reals.
        /// </summary>
        [Fact]
        public void ANarrowerCodomainIsLeftAlone()
        {
            var narrow = MathS.Var("n").WithCodomain(Domain.Integer);
            Assert.Equal(Domain.Integer, narrow.Codomain);
            Assert.Equal(Domain.Integer, narrow.WithCodomain(Domain.Integer).Codomain);
            Assert.Equal("True", narrow.DomainConditionIn(Domain.Real).Stringize());
        }

        /// <summary>
        /// The property is unchanged: it answers for the codomain the expression carries, which
        /// is what every existing caller reads.
        /// </summary>
        [Theory]
        [InlineData("arcsin(x)")]
        [InlineData("sqrt(x)")]
        [InlineData("log(b, x)")]
        [InlineData("arcsin(x) + arcsin(y)")]
        public void ThePropertyStillAnswersForTheExpressionsOwnReading(string source)
        {
            var expr = source.ToEntity();
            Assert.Equal(Domain.Complex, expr.Codomain);
            Assert.Equal(expr.DomainCondition.Stringize(), expr.DomainConditionIn(Domain.Complex).Stringize());
        }
    }
}

//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Differentiating an <see cref="Entity.Application"/> — <c>apply(f, x)</c> and its
    /// several-argument forms.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The two-argument case overflowed the stack.</b> An argument that <em>is</em> the
    /// variable has nothing for the chain rule to do — its own derivative is one, so its term is
    /// the derivative node the expansion is inside. That was special-cased for an application of
    /// exactly one argument, whose comment says it exists to avoid the overflow, and the case was
    /// written as "the only argument is the variable" rather than "an argument is". With two,
    /// <c>d/dx apply(f, x, y)</c> expanded to <c>derivative(apply(f, x, y), x) * 1 + ...</c>,
    /// which is not a <c>Derivativef</c> but contains one — so the "did the derivative come back
    /// unresolved" test read it as progress and simplified its way back to the same node.
    /// </para>
    /// <para>
    /// A stack overflow is not a failing test: it takes the process down and the run reports
    /// nothing. These therefore assert the answer rather than merely that a call returns, and if
    /// the guard is lost the suite aborts rather than going red — which is the loud signal, but
    /// worth knowing about when one appears.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ApplicationDerivativeTest
    {
        /// <summary>
        /// The shape that overflowed, at every arity. The answer is the unresolved derivative,
        /// which is the honest one: nothing is known about <c>f</c>.
        /// </summary>
        [Theory]
        [InlineData("apply(f, x, y)", "derivative(apply(f, x, y), x)")]
        [InlineData("apply(f, x, y, z)", "derivative(apply(f, x, y, z), x)")]
        [InlineData("apply(f, y, x)", "derivative(apply(f, y, x), x)")]
        public void AnArgumentThatIsTheVariableLeavesTheDerivativeUnresolved(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Differentiate("x"));

        /// <summary>The one-argument case, which always worked, and must go on working.</summary>
        [Fact]
        public void OneArgumentIsUnchanged()
            => Assert.Equal("derivative(apply(f, x), x)".ToEntity(),
                "apply(f, x)".ToEntity().Differentiate("x"));

        /// <summary>
        /// Nothing varies with a name the application does not mention, so the derivative is
        /// zero rather than an unresolved node.
        /// </summary>
        [Theory]
        [InlineData("apply(f, x, y)", "z")]
        [InlineData("apply(f, x)", "y")]
        public void AVariableTheApplicationDoesNotMentionGivesZero(string input, string variable)
            => Assert.Equal(Entity.Number.Integer.Create(0), input.ToEntity().Differentiate(variable));

        /// <summary>
        /// Where no argument <em>is</em> the variable the chain rule still runs, which is what
        /// the guard must not cost: each argument contributes a partial times its own derivative.
        /// </summary>
        [Fact]
        public void TheChainRuleStillRunsWhereNoArgumentIsTheVariable()
        {
            var derivative = "apply(f, sin(x), y)".ToEntity().Differentiate("x");
            // The partial with respect to the first argument survives, and it is multiplied by
            // that argument's own derivative rather than by one.
            Assert.Contains("cos(x)", derivative.Stringize());
            Assert.Contains("derivative(apply(f, sin(x), y), sin(x))", derivative.Stringize());
        }

        /// <summary>
        /// The product rule over two applications, which is the shape
        /// <a href="https://github.com/asc-community/AngouriMath/issues/286">#286</a> asks for —
        /// written with <c>apply</c>, since <c>f(x)</c> still parses as a product.
        /// </summary>
        [Fact]
        public void TheProductRuleOverTwoApplications()
        {
            var derivative = "apply(f, x) * apply(g, x)".ToEntity().Differentiate("x").Stringize();
            Assert.Contains("derivative(apply(f, x), x)", derivative);
            Assert.Contains("derivative(apply(g, x), x)", derivative);
            Assert.Contains("apply(f, x)", derivative);
            Assert.Contains("apply(g, x)", derivative);
        }

        /// <summary>
        /// Repeated differentiation terminates too — the second pass asks the same question of
        /// the node the first one produced.
        /// </summary>
        [Fact]
        public void DifferentiatingTwiceTerminates()
        {
            var twice = "apply(f, x, y)".ToEntity().Differentiate("x").Differentiate("x");
            Assert.Contains("derivative(", twice.Stringize());
        }

        /// <summary>
        /// The neighbouring passes over the same node, none of which should recurse either.
        /// </summary>
        [Theory]
        [InlineData("apply(f, x, y)")]
        [InlineData("derivative(apply(f, x, y), x)")]
        public void TheUsualPassesTerminate(string input)
        {
            var entity = input.ToEntity();
            Assert.NotNull(entity.InnerSimplified);
            Assert.NotNull(entity.Simplify());
            Assert.NotNull(entity.Evaled);
        }
    }
}

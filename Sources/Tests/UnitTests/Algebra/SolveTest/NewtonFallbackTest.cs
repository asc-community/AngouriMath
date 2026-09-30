//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// Newton's method is the last resort of <see cref="Entity.Solve"/>, and it compiles the
    /// expression and its derivative. Where the compiler has no form for a node -- <c>floor(x)</c>,
    /// or the unevaluated <c>derivative(max(x, 1), x)</c> that <c>max</c> differentiates to --
    /// the solver threw <see cref="UncompilableNodeException"/> out of the public method, in 2.5.0
    /// as on master.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1603">#1603</a>
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class NewtonFallbackTest
    {
        /// <summary>No analytical route answers these, and the search cannot run on them.</summary>
        [Theory]
        [InlineData("floor(x) = x/2 + 1/3")]
        [InlineData("ceil(x) = x/3 + 1/7")]
        [InlineData("round(x) = x/2 + 1/5")]
        [InlineData("max(x, 1) = 2*x")]
        [InlineData("max(x, 0) = x")]
        [InlineData("min(x, 2) = x^2 + 1/5")]
        public void AnEquationTheCompilerHasNoFormForIsLeftUnsolved(string equation)
            => Assert.IsType<Entity.Set.ConditionalSet>(equation.ToEntity().Solve("x"));

        /// <summary>One the compiler has a form for is still searched, and its roots kept.</summary>
        [Fact]
        public void AnEquationTheCompilerHasAFormForIsStillSearched()
            => Assert.IsType<Entity.Set.FiniteSet>("sin(x) = x/3".ToEntity().Solve("x"));

        /// <summary>
        /// Asked for Newton's method by name, the caller is told why it cannot run, as before:
        /// only the fallback declines.
        /// </summary>
        [Fact]
        public void NewtonsMethodAskedByNameStillSaysWhy()
            => Assert.Throws<UncompilableNodeException>(() => "floor(x) - x/2 - 1/3".ToEntity().SolveNt("x"));
    }
}

//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// Newton's method is the last resort of <see cref="Entity.Solve"/>, and it compiles the
    /// expression and its derivative. Where the compiler had no form for a node -- <c>floor(x)</c>,
    /// or the unevaluated <c>derivative(max(x, 1), x)</c> that <c>max</c> differentiated to -- the
    /// solver threw <see cref="UncompilableNodeException"/> out of the public method, in 2.5.0 as on
    /// master. The floors, the rounding and the extremes compile now, and <c>max</c> and <c>min</c>
    /// differentiate; what still has no compiled form is declined, and so is a set of roots that
    /// are not isolated.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1603">#1603</a>
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class NewtonFallbackTest
    {
        /// <summary>
        /// Each root checked by hand: <c>floor(x) = x/2 + 1/3</c> holds only at 4/3, where the
        /// floor is 1; <c>ceil(x) = x/3 + 1/7</c> only at -3/7; <c>round(x) = x/2 + 1/5</c> only at
        /// -2/5; <c>min(x, 2) = x^2 + 1/5</c> at the two roots of <c>x^2 - x + 1/5</c>, both below 2.
        /// </summary>
        [Theory]
        [InlineData("floor(x) = x/2 + 1/3", new[] { 4.0 / 3 })]
        [InlineData("ceil(x) = x/3 + 1/7", new[] { -3.0 / 7 })]
        [InlineData("round(x) = x/2 + 1/5", new[] { -2.0 / 5 })]
        [InlineData("max(x, 1) = 2*x", new[] { 0.5 })]
        [InlineData("min(x, 2) = x^2 + 1/5", new[] { 0.27639320225002103, 0.72360679774997897 })]
        public void TheFloorsAndTheExtremesAreSearched(string equation, double[] expected)
        {
            var roots = Assert.IsType<Entity.Set.FiniteSet>(equation.ToEntity().Solve("x"));
            Assert.Equal(expected.Length, roots.Count);
            foreach (var root in roots)
                Assert.Contains(expected, e => System.Math.Abs(e - root.EvalNumerical().RealPart.EDecimal.ToDouble()) < 1e-9);
        }

        /// <summary>No analytical route answers these, and the compiler still has no form for them.</summary>
        [Theory]
        [InlineData("erf(x) = x/2 + 1/7")]
        [InlineData("Si(x) = x/2 + 1/7")]
        public void AnEquationTheCompilerHasNoFormForIsLeftUnsolved(string equation)
            => Assert.IsType<Entity.Set.ConditionalSet>(equation.ToEntity().Solve("x"));

        /// <summary>
        /// Each holds on the whole of [0, +oo), and the search's grid converged to fifty-one points
        /// of [0, 10] as though they were the set.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1420">#1420</a>
        /// </summary>
        [Theory]
        [InlineData("abs(x) - x = 0")]
        [InlineData("abs(x) = x")]
        [InlineData("max(x, 0) = x")]
        public void AContinuumIsLeftUnsolved(string equation)
            => Assert.IsType<Entity.Set.ConditionalSet>(equation.ToEntity().Solve("x"));

        /// <summary>One the compiler has a form for is still searched, and its isolated roots kept.</summary>
        [Fact]
        public void AnEquationTheCompilerHasAFormForIsStillSearched()
            => Assert.IsType<Entity.Set.FiniteSet>("sin(x) = x/3".ToEntity().Solve("x"));

        /// <summary>
        /// Asked for Newton's method by name, the caller is told why it cannot run, as before:
        /// only the solver's fallback declines.
        /// </summary>
        [Fact]
        public void NewtonsMethodAskedByNameStillSaysWhy()
            => Assert.Throws<UncompilableNodeException>(() => "erf(x) - x/2 - 1/7".ToEntity().SolveNt("x"));

        /// <summary>
        /// The compiled floors, rounding and extremes against the interpreter, off the real line
        /// too: the floors and the rounding are taken componentwise, and an extreme of two
        /// numbers that are not both real has no value.
        /// </summary>
        [Theory]
        [InlineData("floor(x)", 2.7, 0.5)]
        [InlineData("floor(x)", -2.5, -1.25)]
        [InlineData("ceil(x)", 2.2, -0.5)]
        [InlineData("round(x)", 2.5, 3.5)]
        [InlineData("round(x)", -1.5, 0.5)]
        [InlineData("max(x, 1)", 0.5, 0)]
        [InlineData("max(x, 1)", 3, 0)]
        [InlineData("min(x, 1)", 0.5, 0)]
        [InlineData("min(x^2, 2)", 3, 0)]
        public void TheCompiledFormAgreesWithTheInterpreter(string expression, double re, double im)
        {
            var compiled = expression.ToEntity().Compile("x").Call(new System.Numerics.Complex(re, im));
            var interpreted = expression.ToEntity().Substitute("x", Entity.Number.Complex.Create(re, im)).EvalNumerical();
            Assert.Equal((double)interpreted.RealPart, compiled.Real, 12);
            Assert.Equal((double)interpreted.ImaginaryPart, compiled.Imaginary, 12);
        }

        /// <summary>
        /// And through <c>Compile&lt;TIn, TOut&gt;</c>, the compiler to .NET expression trees, over
        /// doubles and over complex numbers.
        /// </summary>
        [Theory]
        [InlineData("floor(x)", 2.7)]
        [InlineData("floor(x)", -2.5)]
        [InlineData("ceil(x)", 2.2)]
        [InlineData("round(x)", 2.5)]
        [InlineData("round(x)", -1.5)]
        [InlineData("max(x, 1)", 0.5)]
        [InlineData("min(x^2, 2)", 3)]
        public void TheExpressionTreeAgreesWithTheInterpreterOverDoubles(string expression, double at)
        {
            var compiled = expression.ToEntity().Compile<double, double>("x")(at);
            var interpreted = expression.ToEntity().Substitute("x", at).EvalNumerical();
            Assert.Equal((double)interpreted.RealPart, compiled, 12);
        }

        [Theory]
        [InlineData("floor(x)", 2.7, 0.5)]
        [InlineData("ceil(x)", 2.2, -0.5)]
        [InlineData("round(x)", 2.5, 3.5)]
        [InlineData("max(x, 1)", 3, 0)]
        public void TheExpressionTreeAgreesWithTheInterpreterOverComplexNumbers(string expression, double re, double im)
        {
            var compiled = expression.ToEntity().Compile<System.Numerics.Complex, System.Numerics.Complex>("x")(new System.Numerics.Complex(re, im));
            var interpreted = expression.ToEntity().Substitute("x", Entity.Number.Complex.Create(re, im)).EvalNumerical();
            Assert.Equal((double)interpreted.RealPart, compiled.Real, 12);
            Assert.Equal((double)interpreted.ImaginaryPart, compiled.Imaginary, 12);
        }

        [Fact]
        public void AnExtremeOfNumbersNotBothRealHasNoCompiledValue()
            => Assert.True(double.IsNaN("max(x, 1)".ToEntity().Compile("x").Call(new System.Numerics.Complex(2, 1)).Real));

        /// <summary>
        /// <c>max</c> and <c>min</c> differentiate where their arguments are real, as <c>|f|</c> does;
        /// off the real line the derivative is left as written.
        /// </summary>
        [Theory]
        [InlineData("max(x, 1)", 3, 1)]
        [InlineData("max(x, 1)", 0, 0)]
        [InlineData("min(x, 1)", 3, 0)]
        [InlineData("min(x, 1)", 0, 1)]
        [InlineData("min(x^2, 2)", 1, 2)]
        public void TheExtremesDifferentiate(string expression, double at, double slope)
            => Assert.Equal(slope, (double)expression.ToEntity().Differentiate("x").Substitute("x", at).EvalNumerical().RealPart, 12);

        [Fact]
        public void AnExtremeOffTheRealLineIsNotDifferentiated()
            => Assert.Contains("derivative", "max(x, i)".ToEntity().Differentiate("x").Stringize());
    }
}

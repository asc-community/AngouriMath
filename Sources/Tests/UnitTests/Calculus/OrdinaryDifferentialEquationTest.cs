//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The first-order linear ordinary differential equation.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/241">#241</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every solution here is checked by <b>putting it back into the equation it came from</b> —
    /// substituting the answer for the unknown and its derivative for the unknown's derivative,
    /// then evaluating the residual at sampled points and for two values of the arbitrary
    /// constant. A general solution is a family, so an answer that satisfies the equation for one
    /// constant and not another is wrong in a way its printed form does not show.
    /// </para>
    /// <para>
    /// Numerically rather than symbolically, because the residual routinely simplifies to
    /// <c>0 provided ...</c> rather than to <c>0</c>, and a symbolic comparison would then have
    /// to decide what to make of the condition. At a point, either it vanishes or it does not.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class OrdinaryDifferentialEquationTest
    {
        private static readonly Entity Unknown = "apply(y, x)".ToEntity();
        private static readonly Entity UnknownDerivative = "derivative(apply(y, x), x)".ToEntity();

        private static void Satisfies(string equation)
        {
            var solution = MathS.SolveOde(equation.ToEntity(), "y", "x");
            Assert.NotNull(solution);

            var residual = equation.ToEntity()
                .Substitute(UnknownDerivative, solution!.Differentiate("x"))
                .Substitute(Unknown, solution)
                .Simplify();

            var compared = 0;
            foreach (var at in new[] { 0.4, 1.3, 2.2, 3.1 })
                foreach (var constant in new[] { 0.0, 1.5 })
                {
                    var here = residual;
                    // The arbitrary constant is whatever unique name the solver gave it, so it is
                    // found rather than named: pinning `C_1` would make this test about how the
                    // constant is spelled.
                    foreach (var free in residual.Vars.Where(v => v.Name.StartsWith("C")))
                        here = here.Substitute(free, constant);
                    var value = here.Substitute("x", at).EvalNumerical();
                    if (value.IsNaN)
                        continue;
                    compared++;
                    Assert.True(Math.Abs((double)value.RealPart) < 1e-9,
                        $"{solution} does not satisfy {equation}: the residual is {value} "
                        + $"at x = {at} with the constant {constant}");
                }

            Assert.True(compared >= 4,
                $"only {compared} points were comparable for {equation}, so this asserts little");
        }

        /// <summary>
        /// The homogeneous and the separable cases, where the integrating factor is an
        /// exponential.
        /// </summary>
        [Theory]
        [InlineData("derivative(apply(y, x), x)")]
        [InlineData("derivative(apply(y, x), x) - apply(y, x)")]
        [InlineData("derivative(apply(y, x), x) + apply(y, x)")]
        [InlineData("derivative(apply(y, x), x) - 3*apply(y, x)")]
        [InlineData("derivative(apply(y, x), x) - apply(y, x) * x")]
        [InlineData("derivative(apply(y, x), x) - x")]
        public void HomogeneousAndSeparable(string equation) => Satisfies(equation);

        /// <summary>With a right-hand side, so the second integral does real work.</summary>
        [Theory]
        [InlineData("derivative(apply(y, x), x) + apply(y, x) - 1")]
        [InlineData("derivative(apply(y, x), x) + 2*x*apply(y, x) - x")]
        [InlineData("derivative(apply(y, x), x) - apply(y, x) - e^x")]
        [InlineData("derivative(apply(y, x), x) + apply(y, x) - sin(x)")]
        public void WithARightHandSide(string equation) => Satisfies(equation);

        /// <summary>
        /// Where the integrating factor is a power rather than an exponential, because the
        /// coefficient integrates to a logarithm. <c>y' + y/x = 1</c> is the textbook first
        /// example and was the last of these to work.
        /// </summary>
        [Theory]
        [InlineData("derivative(apply(y, x), x) + apply(y, x)/x - 1")]
        [InlineData("derivative(apply(y, x), x) + 2*apply(y, x)/x - x")]
        [InlineData("derivative(apply(y, x), x) + apply(y, x)/x - x^2")]
        [InlineData("x * derivative(apply(y, x), x) + apply(y, x) - x")]
        public void WhereTheFactorIsAPower(string equation) => Satisfies(equation);

        /// <summary>
        /// An equation and the same equation multiplied through by <c>x</c> are one equation, and
        /// must come back with one answer. They did not: the multiplied form was solved while the
        /// divided form was declined, because differentiating <c>y/x</c> gives <c>x/x^2</c> under
        /// a domain condition and neither the unsimplified quotient nor the condition would
        /// integrate.
        /// </summary>
        [Fact]
        public void TheSameEquationWrittenTwoWaysHasOneSolution()
        {
            var divided = MathS.SolveOde(
                "derivative(apply(y, x), x) + apply(y, x)/x - 1".ToEntity(), "y", "x");
            var multiplied = MathS.SolveOde(
                "x * derivative(apply(y, x), x) + apply(y, x) - x".ToEntity(), "y", "x");

            Assert.NotNull(divided);
            Assert.NotNull(multiplied);
            Assert.Equal(multiplied!.Simplify(), divided!.Simplify());
        }

        /// <summary>
        /// What it declines. The first four are not linear in the unknown and its derivative, so
        /// the method does not apply at all; the last is linear and declined because
        /// <c>int e^(x^2) dx</c> has no closed form, which is a fact about the integral rather
        /// than about this solver.
        /// </summary>
        /// <remarks>
        /// Declining is the right answer to each. The alternative — reading a nonlinear equation
        /// as though it were linear, by taking coefficients that do not exist — is a wrong answer
        /// that looks entirely plausible, which is why the solver reassembles what it read and
        /// compares it with what it was given.
        /// </remarks>
        [Theory]
        [InlineData("derivative(apply(y, x), x) * apply(y, x) - 1")]
        [InlineData("derivative(apply(y, x), x) + apply(y, x)^2")]
        [InlineData("sin(derivative(apply(y, x), x)) - 1")]
        [InlineData("derivative(apply(y, x), x)^2 - apply(y, x)")]
        [InlineData("derivative(apply(y, x), x) + apply(y, x) - e^(x^2)")]
        public void WhatItDeclines(string equation)
            => Assert.Null(MathS.SolveOde(equation.ToEntity(), "y", "x"));

        /// <summary>
        /// An equation with no derivative in it is algebraic, and answering it here would be
        /// answering a different question from the one asked.
        /// </summary>
        [Fact]
        public void AnAlgebraicEquationIsNotAnOde()
            => Assert.Null(MathS.SolveOde("apply(y, x) - x".ToEntity(), "y", "x"));

        /// <summary>
        /// The answer the issue's own worked example gives, asserted as a form rather than only
        /// as a residual: a solution off by a constant factor still satisfies a homogeneous
        /// equation.
        /// </summary>
        [Fact]
        public void TheGeneralSolutionCarriesOneArbitraryConstant()
        {
            var solution = MathS.SolveOde(
                "derivative(apply(y, x), x) + apply(y, x) - 1".ToEntity(), "y", "x");
            Assert.NotNull(solution);
            var constants = solution!.Vars.Where(v => v.Name.StartsWith("C")).ToList();
            Assert.Single(constants);
            // y = 1 + C e^(-x): at the constant zero it is the particular solution y = 1.
            //
            // Read at a point rather than compared as a tree. The answer comes back as
            // `1 provided not e ^ x = 0`, a condition that is never false for real x but is
            // carried anyway, and asserting the tree would be asserting that noise. Whether a
            // vacuous condition should survive simplification is a question about Providedf
            // rather than about this solver.
            var particular = solution.Substitute(constants[0], 0);
            foreach (var at in new[] { 0.5, 2.0, 3.5 })
                Assert.True(
                    Math.Abs((double)(particular.Substitute("x", at).EvalNumerical() - 1).RealPart) < 1e-9,
                    $"the particular solution is {particular}, which is not 1 at x = {at}");
        }
    }
}

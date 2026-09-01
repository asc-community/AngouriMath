//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Transformations;
using static AngouriMath.Entity;

namespace AngouriMath.Functions.Algebra
{
    /// <summary>
    /// The first-order linear ordinary differential equation, solved by its integrating factor.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/241">#241</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// For <c>y' + f(x) y = g(x)</c>, multiplying through by <c>m = e^(int f dx)</c> makes the left
    /// side the derivative of <c>m y</c> — which is the whole method, since integrating both sides
    /// then gives <c>m y = int (m g) dx</c>. Nothing here is a heuristic: where the two integrals
    /// come out, the answer is exact, and where either does not this declines.
    /// </para>
    /// <para>
    /// <b>The unknown is an <see cref="Application"/>.</b> A bare <see cref="Variable"/> cannot
    /// stand for a function of <c>x</c>: <c>derivative(y, x)</c> is <c>0</c>, because <c>y</c> does
    /// not contain <c>x</c> and the library is right about that. Written <c>apply(y, x)</c> it is
    /// an application that does not reduce, and its derivative stays symbolic — which is exactly
    /// what an equation about an unknown function needs.
    /// </para>
    /// <para>
    /// <b>Linearity is checked, not assumed.</b> The coefficients are read off by differentiating
    /// with respect to the unknown and its derivative, which is only valid if the equation really
    /// is linear in them — so the reading is put back together and compared with what it came
    /// from. An equation that is not linear fails that comparison and is declined rather than
    /// answered wrongly.
    /// </para>
    /// </remarks>
    internal static class OrdinaryDifferentialEquation
    {
        /// <summary>
        /// The general solution of <paramref name="equation"/> read as equal to zero, or
        /// <see langword="null"/> where this method does not apply.
        /// </summary>
        /// <param name="equation">
        /// An expression in <c>apply(<paramref name="function"/>, <paramref name="variable"/>)</c>
        /// and its derivative, equal to zero.
        /// </param>
        /// <param name="function">The name of the unknown function.</param>
        /// <param name="variable">The name it is a function of.</param>
        internal static Entity? SolveFirstOrderLinear(
            Entity equation, Variable function, Variable variable)
        {
            var unknown = new Application(function, LList.Of<Entity>(variable));
            var derivative = new Derivativef(unknown, variable, 1);

            // Stand-ins, so that the coefficients can be read by differentiating with respect to
            // them. Unique against the whole equation, since `y` and `x` are the caller's names
            // and anything else in it is the caller's too.
            var atDerivative = Variable.CreateUnique(equation, "yPrime");
            var atUnknown = Variable.CreateUnique(equation, "yValue");
            var linear = equation
                .Substitute(derivative, atDerivative)
                .Substitute(unknown, atUnknown);

            // Nothing to solve if the equation never mentions the unknown's derivative: that is
            // an algebraic equation, and answering it here would be answering a different
            // question from the one asked.
            if (!linear.ContainsNode(atDerivative))
                return null;

            var coefficientOfDerivative = Coefficient(linear.Differentiate(atDerivative));
            var coefficientOfUnknown = Coefficient(linear.Differentiate(atUnknown));
            var rest = Coefficient(linear
                .Substitute(atDerivative, Number.Integer.Zero)
                .Substitute(atUnknown, Number.Integer.Zero));

            // The check that makes the three readings above legitimate. If the equation is linear
            // in the two, this reassembles it exactly; if it is not -- y' * y, or y ^ 2, or
            // sin(y') -- the difference does not vanish and this declines.
            var reassembled = coefficientOfDerivative * atDerivative
                + coefficientOfUnknown * atUnknown
                + rest;
            if (Coefficient(reassembled - linear) != Number.Integer.Zero)
                return null;

            // And a coefficient that still mentions either stand-in means the equation was not
            // linear after all, in a way the subtraction above can miss when the simplifier
            // cannot decide it.
            foreach (var coefficient in new[] { coefficientOfDerivative, coefficientOfUnknown, rest })
                if (coefficient.ContainsNode(atDerivative) || coefficient.ContainsNode(atUnknown))
                    return null;

            if (coefficientOfDerivative.Evaled == Number.Integer.Zero)
                return null;

            // y' + f y = g, having divided through.
            var f = (coefficientOfUnknown / coefficientOfDerivative).InnerSimplified;
            var g = (-rest / coefficientOfDerivative).InnerSimplified;

            if (Antiderivative(f, variable) is not { } integralOfF)
                return null;
            var factor = ExponentialOf(integralOfF);

            if (Antiderivative((factor * g).InnerSimplified, variable) is not { } integralOfFactorTimesG)
                return null;

            // The one constant of integration the answer keeps, and it belongs to this integral
            // rather than to the factor: a constant in the exponent of the factor would multiply
            // the whole solution by a constant, which is the same family written less clearly.
            var constant = Variable.CreateUnique(equation + integralOfFactorTimesG + factor, "C");
            return ((integralOfFactorTimesG + constant) / factor).InnerSimplified;
        }

        /// <summary>
        /// A coefficient read off the equation, simplified and with its domain condition set
        /// aside.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both halves are needed and both were found by measuring rather than expected.
        /// Differentiating <c>y / x</c> with respect to <c>y</c> gives <c>x / x^2</c>, which is
        /// <c>1/x</c> and is not written that way, and the integrator has no antiderivative for
        /// the unsimplified form — so <c>y' + y/x = 1</c> was declined while
        /// <c>x y' + y = x</c>, the same equation multiplied through, was solved.
        /// </para>
        /// <para>
        /// And the quotient rule attaches a <see cref="Providedf"/>: the coefficient came back as
        /// <c>1 provided not x^2 = 0</c>. A condition travelling into the integrand stops it
        /// integrating, and it says nothing the answer does not already say — the solution of an
        /// equation with <c>y/x</c> in it is undefined at zero however it is derived, and the
        /// answers here carry that condition of their own accord.
        /// </para>
        /// </remarks>
        private static Entity Coefficient(Entity expression)
        {
            var simplified = expression.Simplify();
            while (simplified is Providedf(var inner, _))
                simplified = inner;
            return simplified;
        }

        /// <summary>
        /// <c>e</c> to the power of <paramref name="exponent"/>, written without the exponential
        /// where the exponent is a logarithm.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is not tidying, it is the difference between answering and not. The integrating
        /// factor for <c>y' + y/x = 1</c> is <c>e^(int dx/x)</c>, which is <c>e^ln(x)</c> — and
        /// <b><c>e^ln(x)</c> is not simplified to <c>x</c> by anything in the library</b>, neither
        /// by <c>InnerSimplified</c> nor by <c>Simplify</c>. The factor then stays an exponential
        /// of a logarithm, the second integral has no antiderivative for it, and the whole
        /// equation is declined. Measured, which is how it was found: every step but that one
        /// came out.
        /// </para>
        /// <para>
        /// Done here rather than as a simplification rule because a rule for <c>e^ln(a) = a</c>
        /// reaches every expression in the library and wants deciding on its own terms. Filed
        /// separately; this is the local reading, which is sound because the exponent was built
        /// here and is known to be an antiderivative.
        /// </para>
        /// </remarks>
        private static Entity ExponentialOf(Entity exponent)
            => exponent switch
            {
                // e^ln(u) is u.
                Logf(var logBase, var antilog) when logBase == MathS.e => antilog,
                // e^(k ln u) is u^k, which is what an antiderivative of k/x gives.
                Mulf(var scale, Logf(var logBase, var antilog)) when logBase == MathS.e
                    => MathS.Pow(antilog, scale).InnerSimplified,
                Mulf(Logf(var logBase, var antilog), var scale) when logBase == MathS.e
                    => MathS.Pow(antilog, scale).InnerSimplified,
                _ => MathS.Pow(MathS.e, exponent).InnerSimplified,
            };

        /// <summary>
        /// The antiderivative with <b>no</b> constant of integration, or <see langword="null"/>
        /// where the integral did not come out.
        /// </summary>
        /// <remarks>
        /// <see cref="Entity.Integrate(Variable)"/> appends one, which is right for a caller
        /// asking for an antiderivative and wrong here twice over: the integrating factor would
        /// carry an arbitrary constant into an exponent, and the answer would end up with two
        /// constants standing for one family of solutions.
        /// </remarks>
        private static Entity? Antiderivative(Entity expression, Variable variable)
            => Transformation.Integration(variable).Apply(expression).Output is { } antiderivative
                && !antiderivative.Nodes.Any(node => node is Integralf)
                ? antiderivative.InnerSimplified
                : null;
    }
}

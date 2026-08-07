//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using AngouriMath.Functions.Algebra;
using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Integrates indefinitely the given expression over the `x` variable, if can.
        /// May return an unresolved <see cref="Integralf"/> node.
        /// </summary>
        /// <param name="x">Over which variable to integrate</param>
        /// <returns>
        /// An integrated expression. It might remain the same or be transformed into nodes with no integrals.
        /// </returns>
        /// <remarks>
        /// The antiderivative is inner-simplified before it is returned, as the definite
        /// overload below has always done. Without it the answer kept branches whose guard
        /// was a comparison of two literals -- <c>1/sqrt(x^2 - 1)</c> came back as a
        /// three-branch piecewise on <c>1 * 1 ^ 2</c>, two of them dead -- and a caller that
        /// inspects the answer rather than evaluating it saw all three. `Piecewise` already
        /// drops a branch whose guard is decidably false; nothing was asking it to.
        /// https://github.com/asc-community/AngouriMath/issues/772
        /// </remarks>
        public Entity Integrate(Variable x) =>
            Integration.ComputeIndefiniteIntegral(InnerSimplified, x)?.InnerSimplified is { } antiderivative
            ? antiderivative + (antiderivative.VarsAndConsts.Contains("C") ? Variable.CreateUnique(antiderivative, "C") : "C")
            : new Integralf(this, x, null);
        /// <summary>
        /// Integrates definitely the given expression over the `x` variable, if can.
        /// May return an unresolved <see cref="Integralf"/> node.
        /// </summary>
        /// <param name="x">Over which variable to integrate</param>
        /// <param name="from">The lower bound for integrating</param>
        /// <param name="to">The upper bound for integrating</param>
        /// <returns>
        /// An integrated expression. It might remain the same or be transformed into nodes with no integrals.
        /// </returns>
        public Entity Integrate(Variable x, Entity from, Entity to) =>
            Integration.ComputeIndefiniteIntegral(InnerSimplified, x)?.InnerSimplified is { } antiderivative
            ? antiderivative.Substitute(x, to) - antiderivative.Substitute(x, from)
            : new Integralf(this, x, (from, to));
    }
}

namespace AngouriMath.Functions.Algebra
{
    internal static partial class Integration
    {
        /// <summary>
        /// Brings the two powers of one base in a product together, so that an integrand
        /// which is a power in disguise is recognised as one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This has to happen on every recursive call rather than once on the way in. The
        /// shape is produced by the integrator itself -- distributing a product over a sum
        /// turns <c>sin(x)^4 * (5 - 6*sin(x)^2)</c> into a term <c>sin(x)^4 * (-6) * sin(x)^2</c>
        /// -- so normalising only the caller's input would miss every case the integrator
        /// generates for itself. https://github.com/asc-community/AngouriMath/issues/781
        /// </para>
        /// <para>
        /// Deliberately not followed by <c>InnerSimplified</c>: that rewrites <c>x^(-2)</c>
        /// back into <c>1/x^2</c>, and <see cref="IndefiniteIntegralSolver.SolveAsPolynomialTerm"/>
        /// rewrites a <c>1/x^n</c> it is handed into <c>Pow(x, -n)</c>, so the pair recurse
        /// into each other until the stack runs out. The gathering folds the exponents it
        /// builds and leaves the rest of the tree alone.
        /// </para>
        /// </remarks>
        private static Entity Normalized(Entity expr) =>
            expr.Replace(Patterns.GatherPowersOfOneBase);

        /// <summary>Does not add the constant of integration because this is called recursively.</summary>
        internal static Entity? ComputeIndefiniteIntegral(Entity expr, Entity.Variable x, bool integrateByParts = true)
        {
            expr = Normalized(expr);
            if (!expr.ContainsNode(x)) return expr * x; // base case, handle here
            if ((IntegralPatterns.TryStandardIntegrals(expr, x)) is { } answer) return answer;
            // The flag has to be handed on. Every one of these recurses, and a solver that
            // dropped it re-enabled integration by parts one level below the call that
            // switched it off -- which is a cycle, since by parts calls back into here.
            // `x * ln(x)` went round it until the stack ran out.
            if ((answer = IndefiniteIntegralSolver.SolveAsPolynomialTerm(expr, x, integrateByParts)) is { }) return answer;
            if ((answer = IndefiniteIntegralSolver.SolveLogarithmic(expr, x, integrateByParts)) is { }) return answer;
            if ((answer = IndefiniteIntegralSolver.SolveBySubstitution(expr, x, integrateByParts)) is { }) return answer;
            if ((answer = IndefiniteIntegralSolver.SolveByPartialFractions(expr, x, integrateByParts)) is { }) return answer;
            // Linearity comes before integration by parts, because it decomposes the problem
            // into strictly simpler ones where by parts searches. It cannot cost an answer:
            // splitting returns null unless *every* term integrates, so a sum that only comes
            // out whole still falls through to by parts below.
            //
            // A product with a sum in it -- sin(a+f*x)^4 * (5 - 6*sin(a+f*x)^2) -- reaches
            // this as a product, so only the expansion here finds the two terms it is. Behind
            // by parts it never did: the search spent the whole budget first and the caller
            // waited over 20 seconds to be told nothing, where the split answers in 0.16.
            // https://github.com/asc-community/AngouriMath/issues/779
            //
            // Expansion is bounded by MaxExpansionTermCount, which returns null rather than
            // building the terms, so putting it earlier cannot blow the tree up either.
            if ((answer = IndefiniteIntegralSolver.SolveBySplittingSum(expr, x, integrateByParts)) is { }) return answer;
            if (integrateByParts && (answer = IndefiniteIntegralSolver.SolveIntegratingByParts(expr, x)) is { }) return answer;
            return null;
        }
        /// <summary>
        /// Returns the approximate numeric value of a definite integral of a function. Only works for one-variable functions.
        /// Accuracy is limited to the number specified (default is 100).
        /// See more at <see cref="MathS.Compute.DefiniteIntegral(Entity, Entity.Variable, EDecimal, EDecimal)"/>
        /// </summary>
        /// <param name="expr">Expression to integrate</param>
        /// <param name="x">Variable to integrate over</param>
        /// <param name="from">The complex lower bound for integrating</param>
        /// <param name="to">The complex upper bound for integrating</param>
        /// <param name="accuracy">Accuracy (for now, number of iterations)</param>
        internal static Complex IntegrateNumerically(Entity expr, Entity.Variable x, Complex from, Complex to, int accuracy = 100)
        {
            System.Numerics.Complex res = 0;
            var cfunc = expr.Compile(x);
            for (int i = 0; i <= accuracy; i++)
            {
                var share = ((EDecimal)i) / accuracy;
                var tmp = Complex.Create(from.RealPart.EDecimal * share + to.RealPart.EDecimal * (1 - share), from.ImaginaryPart.EDecimal * share + to.ImaginaryPart.EDecimal * (1 - share));
                res += cfunc.Substitute(tmp.ToNumerics());
            }
            return res.ToNumber() / (accuracy + 1) * (to - from);
        }
    }
}

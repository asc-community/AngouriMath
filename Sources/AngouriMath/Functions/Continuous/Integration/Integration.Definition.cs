//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Convenience;
using AngouriMath.Core.Transformations;
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
            Transformation.Integration(x).Apply(this).Output is { } antiderivative
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
            Transformation.Integration(x).Apply(this).Output is { } antiderivative
            ? antiderivative.Substitute(x, to) - antiderivative.Substitute(x, from)
            : new Integralf(this, x, (from, to));

        /// <summary>
        /// Integrates numerically over <paramref name="x"/> between two bounds, without
        /// looking for an antiderivative. Only works for one-variable functions.
        /// </summary>
        /// <param name="x">The variable to integrate over.</param>
        /// <param name="from">The lower bound.</param>
        /// <param name="to">The upper bound.</param>
        /// <param name="stepCount">How many steps to take; more is more accurate and slower.</param>
        /// <returns>The value of the integral.</returns>
        /// <remarks>
        /// This is the instance method the obsolete <c>MathS.Compute.DefiniteIntegral</c>
        /// said to use and which did not exist — every other member of that group had a
        /// counterpart on <see cref="Entity"/> and this one did not, so removing the group
        /// would have taken numeric definite integration with it.
        /// </remarks>
        public Number.Complex DefiniteIntegral(Variable x, Number.Complex from, Number.Complex to, int stepCount = 100)
            => Integration.IntegrateNumerically(this, x, from, to, stepCount);

        /// <summary>
        /// Integrates numerically over <paramref name="x"/> between two real bounds.
        /// </summary>
        /// <param name="x">The variable to integrate over.</param>
        /// <param name="from">The lower bound.</param>
        /// <param name="to">The upper bound.</param>
        /// <param name="stepCount">How many steps to take; more is more accurate and slower.</param>
        /// <returns>The value of the integral.</returns>
        public Number.Complex DefiniteIntegral(Variable x, EDecimal from, EDecimal to, int stepCount = 100)
            => Integration.IntegrateNumerically(this, x,
                Number.Complex.Create(from, 0), Number.Complex.Create(to, 0), stepCount);
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

        /// <summary>
        /// The integrals already answered under the settings in force, so that the same question
        /// is not worked out again.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Why an integral is asked for twice at all.</b> Two layers of it. Within one call the
        /// solvers overlap — substitution, partial fractions, splitting a sum and integration by
        /// parts each decompose the integrand differently and produce pieces that coincide — and
        /// across calls <c>Simplify</c> asks about one integral through every
        /// rewritten candidate it generates. Traced on <c>sin(x)/(x^2 + 1)^2</c>, which has no
        /// elementary antiderivative and so runs the search to exhaustion, that is <b>562
        /// top-level calls for 3 distinct integrands</b>, one of them asked 500 times; on
        /// <c>e^x/(x^2 + 1)^2</c> a single call enters the integrator <b>5330 times for 23</b>.
        /// </para>
        /// <para>
        /// <b>The answers are held across calls and not only within one</b>, which is where the
        /// repetition is: discarding them at the end of each top-level call leaves the 562 to be
        /// paid in full. Measured on those three integrands, keeping them takes a
        /// <c>Simplify</c> from 115, 86 and 51 seconds to about 1.2, 1.5 and 0.2, and a single
        /// cold <c>Integrate</c> of <c>sqrt(tan(x))</c> from 155 ms to 79.
        /// https://github.com/asc-community/AngouriMath/issues/1156
        /// </para>
        /// <para>
        /// <b>Why it is sound to hold them.</b> An answer depends on the ambient settings —
        /// <see cref="MathS.Settings.Codomain"/> decides whether the logarithms carry an
        /// <c>abs</c>, <c>MaxExpansionTermCount</c> bounds the by-parts recursion — and a setting
        /// is scoped to a flow rather than to a thread, so "the settings have not changed" is not
        /// something a per-thread cache can assume. <see cref="SettingsState"/> answers it
        /// exactly, by comparing what every setting reads as rather than by counting changes —
        /// which matters, because the library opens and closes thousands of balanced scopes while
        /// simplifying and a change count is therefore never still.
        /// </para>
        /// </remarks>
        [System.ThreadStatic] private static Dictionary<(Entity, Entity.Variable, bool), Entity?>? answered;

        /// <summary>The settings <see cref="answered"/> was filled under.</summary>
        [System.ThreadStatic] private static object?[]? answeredUnder;

        /// <summary>
        /// How many answers to hold before starting over. Comfortably above what one call needs
        /// — the worst measured is 23 distinct integrands within a call and 3 across the
        /// candidates of one <c>Simplify</c> — and low enough that a process integrating
        /// different things all day does not accumulate them.
        /// </summary>
        private const int MostAnswersKept = 4096;

        /// <summary>Does not add the constant of integration because this is called recursively.</summary>
        internal static Entity? ComputeIndefiniteIntegral(Entity expr, Entity.Variable x, bool integrateByParts = true)
        {
            expr = Normalized(expr);

            if (answered is null || answeredUnder is null || !SettingsState.StillHolds(answeredUnder))
            {
                // A fresh dictionary rather than Clear(), because this flow may have inherited
                // the reference from the one that started it, and emptying it in place would
                // empty that one's too.
                answered = new();
                answeredUnder = SettingsState.Snapshot();
            }

            // Held as locals over the recursive call, which reaches this method again and may
            // replace both fields on the way. Writing the answer into whatever the fields hold
            // afterwards would put it in a dictionary stamped with different settings.
            var into = answered;
            var stamp = answeredUnder;

            var key = (expr, x, integrateByParts);
            if (into.TryGetValue(key, out var already))
                return already;

            var computed = ComputeIndefiniteIntegralUncached(expr, x, integrateByParts);

            // Only kept if those settings still hold. Working the answer out runs simplification,
            // which opens scopes of its own; one still open means this answer was computed under
            // settings the stamp does not describe.
            if (SettingsState.StillHolds(stamp))
            {
                // Emptied rather than grown without end. Nothing here expires on its own — the
                // settings holding still is the whole condition for keeping an answer — so a
                // long-lived process integrating a stream of different expressions would hold
                // every one of them for ever. What this is for is being asked the same question
                // again, which needs the recent answers and not all of them.
                if (into.Count >= MostAnswersKept)
                    into.Clear();
                into[key] = computed;
            }
            return computed;
        }

        private static Entity? ComputeIndefiniteIntegralUncached(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!expr.ContainsNode(x)) return expr * x; // base case, handle here
            if ((IntegralPatterns.TryStandardIntegrals(expr, x)) is { } answer) return answer;
            // The flag has to be handed on. Every one of these recurses, and a solver that
            // dropped it re-enabled integration by parts one level below the call that
            // switched it off -- which is a cycle, since by parts calls back into here.
            // `x * ln(x)` went round it until the stack ran out.
            if ((answer = IndefiniteIntegralSolver.SolveAsPolynomialTerm(expr, x, integrateByParts)) is { }) return answer;
            if ((answer = IndefiniteIntegralSolver.SolveLogarithmic(expr, x, integrateByParts)) is { }) return answer;
            if ((answer = IndefiniteIntegralSolver.SolveBySubstitution(expr, x, integrateByParts)) is { }) return answer;
            // After the general substitution rather than inside it, because the general one
            // divides by du/dx and asks what is left, and that question loses the shape here:
            // sqrt(tan(x)) over the derivative of sqrt(tan(x)) simplifies to sin(2x), in which
            // the substitution is no longer visible. This one rewrites rather than divides.
            if ((answer = IndefiniteIntegralSolver.SolveByTangentSubstitution(expr, x, integrateByParts)) is { }) return answer;
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
        /// See more at <see cref="Entity.DefiniteIntegral(Entity.Variable, EDecimal, EDecimal, int)"/>
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

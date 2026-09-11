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
            // An integrand that can jump -- a piecewise whose conditions mention x, a floor or a
            // ceiling of x -- never goes through an antiderivative between two bounds: the rules
            // integrate it with the case or the step taken for a constant, which is right
            // between two jumps and wrong across one. (x - floor(x))^2 from 0 to 4 came back as
            // 0 that way, where it is 4/3, and the tent map over [0, 1] as 1, where it is 1/2.
            // It is split at the jumps instead, and where that cannot be done it is left as
            // written. See BreakpointIntegration.
            Functions.Algebra.BreakpointIntegration.HasABreak(this, x)
            ? Functions.Algebra.BreakpointIntegration.Split(this, x, from, to) ?? new Integralf(this, x, (from, to))
            : Transformation.Integration(x).Apply(this).Output is { } antiderivative
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

        /// <summary>
        /// How deep the recursive descent may go before it declines to go further.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Without this the descent has **no bound at all**, and a stack overflow is not an
        /// exception a caller can handle: it takes the process down, so anything the process had
        /// not finished is lost. Found by <c>work/intbench</c> against Rubi's independent test
        /// suites, where the run died with <c>SIGABRT</c> partway through; the trace was thousands
        /// of frames alternating <see cref="ComputeIndefiniteIntegral"/> with
        /// <see cref="IndefiniteIntegralSolver.SolveBySubstitution"/>.
        /// https://github.com/asc-community/AngouriMath/issues/1232
        /// </para>
        /// <para>
        /// <b>Why the memo above does not already stop it, which is the part worth stating.</b>
        /// <c>SolveBySubstitution</c> names its new variable with
        /// <c>Variable.CreateUnique</c>, so every level integrates with respect to a *fresh*
        /// variable. The key <c>(expr, x, integrateByParts)</c> therefore differs at every level
        /// even when the level is the same problem renamed, and <see cref="answered"/> can never
        /// fire on a cycle. Neither could a set of shapes already visited, for the same reason —
        /// the shapes are alpha-equivalent rather than equal. A depth bound does not care what
        /// the levels are called.
        /// </para>
        /// <para>
        /// <b>The number, which is measured rather than picked.</b> Instrumenting the descent over
        /// twenty-three integrands chosen from the hard end of the corpus — the ones that take
        /// substitutions, by-parts chains and partial fractions — the deepest any *answered*
        /// integral reaches is <b>13</b>, for <c>x^5*cosh(x)</c>. Next are <c>x^2*sqrt(5-x^2)</c>
        /// at 8 and <c>e^(x^(1/3))</c> at 7; everything else sits at 4 or less. 32 is therefore
        /// about two and a half times the deepest real descent seen.
        /// </para>
        /// <para>
        /// <b>Why not far more, since a bound only has to stop the overflow.</b> Because this
        /// descent branches: the remark on <see cref="answered"/> above records a single call
        /// entering the integrator 5,330 times for 23 distinct integrands. Depth that is never
        /// legitimately used is still searched before it is abandoned, so a bound of 64 stopped
        /// the crash and left the run crawling. A bound has to be tight enough to be a bound.
        /// </para>
        /// <para>
        /// Declining is a legitimate answer here and a wrong one is not: an unevaluated
        /// <c>integral(...)</c> says "I could not settle this", which is true, where an aborted
        /// process says nothing at all.
        /// </para>
        /// </remarks>
        private const int DeepestDescent = 32;

        /// <summary>How deep the current descent is. Per thread, like <see cref="answered"/>.</summary>
        [System.ThreadStatic] private static int descentDepth;

        /// <summary>
        /// Whether the integrand now being solved is the one the caller asked for, rather than
        /// one a rule produced on the way to answering something else.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For a rule that is the right answer to a question and a distraction as an offer. A
        /// rule which answers a sub-integral that used to come back <see langword="null"/> does
        /// not only add its own work: it lets the search that asked stop failing and carry on,
        /// which is where the cost turns up — on integrands the rule never fires on.
        /// </para>
        /// <para>
        /// Measured on the secant power reduction: <c>sec(x)^6*tan(x)^3</c> is declined in 637 ms
        /// without it and does not return in 400 s with it, while the rule itself fires exactly
        /// twice and terminates both times. The work is not the rule's; the continuation is.
        /// https://github.com/asc-community/AngouriMath/issues/1265
        /// </para>
        /// <para>
        /// One is the top: the depth is incremented before the solvers run, so a rule consulted
        /// about the caller's own integrand sees exactly one.
        /// </para>
        /// </remarks>
        internal static bool AnsweringTheQuestionAsked => descentDepth == 1;

        /// <summary>
        /// Whether anything in the current top-level call gave up on <see cref="DeepestDescent"/>
        /// rather than on the mathematics. A <c>null</c> produced that way must not be cached as
        /// "this cannot be integrated", because the same key may well be answerable when it is
        /// not reached from so deep.
        /// </summary>
        [System.ThreadStatic] private static bool descentTruncated;

        /// <summary>
        /// The integrals this thread is part-way through, so that asking for one again while it is
        /// still being worked out is recognised as a cycle rather than followed round again.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Keyed on the integrand with its variable renamed to <see cref="CycleVariable"/>, which
        /// is the whole point. <see cref="IndefiniteIntegralSolver.SolveBySubstitution"/> names
        /// each new variable with <c>Variable.CreateUnique</c>, so a level and the level it came
        /// from are alpha-equivalent rather than equal, and a set keyed on the integrand as
        /// written would never see the same entry twice. Neither does <see cref="answered"/>, and
        /// for the same reason — which is why the memo could not stop this.
        /// </para>
        /// <para>
        /// This is what makes <see cref="DeepestDescent"/> a backstop instead of the mechanism.
        /// The bound alone stops the process dying, and it is not enough on its own: the descent
        /// branches, so 32 levels of a cycle is still an enormous search, and one Rubi integrand
        /// took over ten minutes under the bound where <c>v2.4.0</c> passed the whole section it
        /// is in within a minute. Declining at the first repeat costs nothing and answers at once.
        /// </para>
        /// </remarks>
        [System.ThreadStatic] private static HashSet<(Entity, bool)>? inProgress;

        /// <summary>The name every in-progress integrand's variable is rewritten to.</summary>
        [ConstantField] private static readonly Entity.Variable CycleVariable = Entity.Variable.CreateVariableOrConstant("__integration_cycle_var");

        /// <summary>Does not add the constant of integration because this is called recursively.</summary>
        internal static Entity? ComputeIndefiniteIntegral(Entity expr, Entity.Variable x, bool integrateByParts = true)
        {
            if (descentDepth >= DeepestDescent)
            {
                descentTruncated = true;
                return null;
            }
            if (descentDepth == 0)
            {
                descentTruncated = false;
                inProgress?.Clear();
            }

            // Renamed so that the same integral under two different variable names is one entry.
            var cycleKey = (expr.Substitute(x, CycleVariable), integrateByParts);
            var open = inProgress ??= new HashSet<(Entity, bool)>();
            if (!open.Add(cycleKey))
            {
                // Already on the stack: following it again cannot reach an answer this call has
                // not already tried, so declining is both correct and immediate.
                descentTruncated = true;
                return null;
            }

            descentDepth++;
            try
            {
                return ComputeIndefiniteIntegralBounded(expr, x, integrateByParts);
            }
            finally
            {
                open.Remove(cycleKey);
                descentDepth--;
            }
        }

        private static Entity? ComputeIndefiniteIntegralBounded(Entity expr, Entity.Variable x, bool integrateByParts)
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
            // A null that came of running out of descent, rather than of the mathematics, is not
            // an answer about this integrand and must not be remembered as one — the same key
            // reached from less deep may well be answerable. A non-null answer is kept whatever
            // happened elsewhere, since an antiderivative that was found is correct regardless of
            // how deep the search that found it went.
            if (SettingsState.StillHolds(stamp) && (computed is not null || !descentTruncated))
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
            // An integrand *already written* as a sum is split here, before any search. Every
            // rule below does speculative work on the whole sum first, and on a polynomial that
            // costs everything: the terms of `1 + 2x + ... + 25x^24` integrate in under a
            // millisecond between them, and the sum they add up to took 20 seconds.
            //
            //     degree               4     8     12     16     20     24
            //     was              64ms  384ms  705ms  708ms  2.6s   20.1s
            //     is               69ms   69ms  141ms  213ms  138ms  0.16s
            //
            // **Only a sum, and deliberately not a product with a sum in it.** Expanding one is a
            // rewrite, and a rewrite this early takes answers away from the rules that give
            // better ones: `x * (x^2 + 1)^3` is `(x^2 + 1)^4/8` by substitution and a written-out
            // degree-8 polynomial by expanding, both right and only one worth reading. Three
            // tests pinned exactly that and caught this when the split was put in front of the
            // substitution wholesale. The expanding call stays where it was, below.
            if (expr is Entity.Sumf or Entity.Minusf
                && (answer = IndefiniteIntegralSolver.SolveBySplittingSum(expr, x, integrateByParts)) is { })
                return answer;
            if ((answer = IndefiniteIntegralSolver.SolveLogarithmic(expr, x, integrateByParts)) is { }) return answer;
            if ((answer = IndefiniteIntegralSolver.SolveBySubstitution(expr, x, integrateByParts)) is { }) return answer;
            // After the general substitution rather than inside it, because the general one
            // divides by du/dx and asks what is left, and that question loses the shape here:
            // sqrt(tan(x)) over the derivative of sqrt(tan(x)) simplifies to sin(2x), in which
            // the substitution is no longer visible. This one rewrites rather than divides.
            // Before the tangent substitution, which would also take a power of the secant and
            // answer it as a rational function of tan(x) -- correct, and a good deal longer than
            // the reduction gives. Rubi's own ordering puts the reduction first for the same
            // reason.
            if ((answer = IndefiniteIntegralSolver.SolveBySecantPowerReduction(expr, x)) is { }) return answer;
            if ((answer = IndefiniteIntegralSolver.SolveByTangentSubstitution(expr, x, integrateByParts)) is { }) return answer;
            if ((answer = IndefiniteIntegralSolver.SolveByPartialFractions(expr, x, integrateByParts)) is { }) return answer;
            // Linearity again, and this time with the expansion: a product with a sum in it --
            // sin(a+f*x)^4 * (5 - 6*sin(a+f*x)^2) -- reaches this as a product, so only the
            // expansion finds the two terms it is. Behind by parts it never did: the search spent
            // the whole budget first and the caller waited over 20 seconds to be told nothing,
            // where the split answers in 0.16.
            // https://github.com/asc-community/AngouriMath/issues/779
            //
            // It cannot cost an answer: splitting returns null unless *every* term integrates, so
            // a sum that only comes out whole still falls through to by parts below. Expansion is
            // bounded by MaxExpansionTermCount, which returns null rather than building the terms.
            if ((answer = IndefiniteIntegralSolver.SolveBySplittingSum(expr, x, integrateByParts)) is { }) return answer;
            // The half-angle substitution goes *after* linearity, and that is not a preference.
            // It fires on anything built from sines and cosines, and it answers `cos(x) + 1` with
            // a correct expression in tan(x/2) some forty characters long where splitting the sum
            // answers `sin(x) + x`. Both are antiderivatives; only one is an answer anybody wants,
            // and the F# wrapper's test pinned the readable one. So everything that decomposes the
            // problem into pieces the ordinary rules know gets first refusal, and this sees only
            // what is left — which is the quotients it was added for, since linearity declines a
            // quotient and partial fractions declines one that is not a ratio of polynomials.
            if ((answer = IndefiniteIntegralSolver.SolveByHalfAngleSubstitution(expr, x, integrateByParts)) is { }) return answer;
            // Beside the half-angle one and for the same reason: it rewrites the integrand into a
            // rational function, so it wants everything that answers a problem in its own terms to
            // have declined first.
            if ((answer = IndefiniteIntegralSolver.SolveByLinearRadicalSubstitution(expr, x, integrateByParts)) is { }) return answer;
            // Product-to-sum among the rewrites rather than before them, because a product of
            // trigonometric functions of *equal* arguments is a power and wants a different tool;
            // this only fires where the arguments differ, which is exactly what every substitution
            // above declines for want of an inner function to substitute for.
            if ((answer = IndefiniteIntegralSolver.SolveByProductToSum(expr, x, integrateByParts)) is { }) return answer;
            // The exponential substitution beside the other rewrites. It is also what integrates
            // the hyperbolic functions, which are not nodes here but quotients of exponentials.
            if ((answer = IndefiniteIntegralSolver.SolveByExponentialSubstitution(expr, x, integrateByParts)) is { }) return answer;
            // A quotient by an exponential is the product with its reciprocal, and only that
            // spelling reaches integration by parts. After the substitution above rather than
            // before it, since a quotient that is rational in e^(k x) is that rule's to answer
            // whole and comes out in better shape for it.
            if ((answer = IndefiniteIntegralSolver.SolveByDividingByAnExponential(expr, x, integrateByParts)) is { }) return answer;
            // Last of the rewrites, because it is the only one that fires on an integrand nothing
            // is wrong with -- it clears a parameter rather than a shape -- so everything that
            // can answer the problem as written gets to try first.
            if ((answer = IndefiniteIntegralSolver.SolveByScalingTheVariable(expr, x, integrateByParts)) is { }) return answer;
            // After every rule that can answer a power in its own terms, because expanding one
            // throws away whatever structure it had: (1 + x^2)^2 is answered as a power and only
            // wants writing out if that fails.
            if ((answer = IndefiniteIntegralSolver.SolveByExpandingAPower(expr, x, integrateByParts)) is { }) return answer;
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

//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Core;
using AngouriMath.Core.Multithreading;
using AngouriMath.Core.Transformations;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    using static AngouriMath.Entity.Set;
    using static Entity;
    using static Entity.Number;
    internal static class Simplificator
    {
        internal static Entity PickSimplest(Entity one, Entity another)
            => one.SimplifiedRate < another.SimplifiedRate ? one : another;

        /// <summary>See more details in <see cref="Entity.Simplify(int)"/></summary>
        internal static Entity Simplify(Entity expr, int level)
            => Simplified(RewriteRecording.Current, Alternate(expr, level).First());

        /// <summary>
        /// <see cref="Entity.InnerSimplified"/>, and the whole-expression step it takes written
        /// down where somebody is recording.
        /// </summary>
        /// <remarks>
        /// A derivation is a chain of whole expressions, and a rewrite pass only ever records the
        /// subexpressions it changed -- so the passes below have to say what they turned into or
        /// the chain has a hole in it wherever the simplifier tidied up. Free when nobody is
        /// listening: one ambient read per stage, against a tree walk per stage.
        /// See <see cref="RewriteRecording.PathFrom(Entity, Entity)"/>.
        /// </remarks>
        private static Entity Simplified(RewriteRecording? recording, Entity expression)
        {
            if (recording is null)
                return expression.InnerSimplified;
            var mark = recording.Mark();
            var simplified = expression.InnerSimplified;
            if (simplified != expression)
                recording.Note(expression, simplified, null, nameof(Entity.InnerSimplified), mark);
            return simplified;
        }

        /// <summary>
        /// An expression the simplifier produced by something that is not a rewrite pass, written
        /// down for the same reason as <see cref="Simplified(RewriteRecording, Entity)"/>. The
        /// mark is taken with <see cref="RewriteRecording.Mark"/> before the work started.
        /// </summary>
        private static Entity Noted(RewriteRecording? recording, Entity before, Entity after, string name, int mark)
        {
            if (recording is not null && after != before)
                recording.Note(before, after, null, name, mark);
            return after;
        }

        /// <summary>
        /// The tidying pass every stage of <see cref="Alternate(Entity, int)"/> runs: get
        /// the quotients and the signs into their usual shape, put the operands in order,
        /// then collect like terms.
        /// </summary>
        /// <remarks>
        /// Composed once, statically, out of <see cref="RewriteRules"/> rather than written
        /// out as a chain of <c>Replace</c> calls -- so the sequence is a value that can be
        /// named, printed and tested, and so each stage of it is a registry entry other
        /// code can reach on its own. The passes and their order are unchanged.
        /// </remarks>
        [ConstantField]
        private static readonly Transformation simplifyChildren =
            Transformation.Rewriting(RewriteRules.InvertNegativePowers)
                .Then(Transformation.Rewriting(RewriteRules.InvertNegativeMultipliers))
                .Then(Transformation.Rewriting(RewriteRules.CanonicalOrder))
                .Then(Transformation.InnerSimplification)
                .Then(Transformation.Rewriting(RewriteRules.Common))
                .Then(Transformation.InnerSimplification);

        internal static Entity SimplifyChildren(Entity expr)
        {
            var recording = RewriteRecording.Current;
            if (recording is null)
                return simplifyChildren.ApplyOrKeep(expr);
            var mark = recording.Mark();
            var simplified = simplifyChildren.ApplyOrKeep(expr);
            // The chain above is six stages, two of which are inner simplifications that record
            // nothing of their own, so the composition is the smallest step a reader can be
            // handed here without the chain coming apart in the middle.
            return Noted(recording, expr, simplified, nameof(SimplifyChildren), mark);
        }

        /// <summary>Finds all alternative forms of an expression</summary>
        internal static IEnumerable<Entity> Alternate(Entity src, int level)
        {
            var recording = RewriteRecording.Current;
            if (src is FiniteSet ss)
            {
                var setMark = recording?.Mark() ?? 0;
                return new[] { Noted(recording, src, ss.Apply(ent => ent.Simplify()).InnerSimplified, "Elementwise", setMark) };
            }
            if (src is Number or Variable or Entity.Boolean)
                return new[] { src };
            var stage1 = Simplified(recording, src);

#if DEBUG
            if (MathS.Diagnostic.CatchOnSimplify.Value(stage1))
                throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

            if (stage1 is Number or Variable or Entity.Boolean)
                return new[] { stage1 };

            // List of criteria for expr's complexity
            var history = new SortedDictionary<double, HashSet<Entity>>();
            void AddHistory(Entity expr)
            {
#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(expr)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif
                void __IterAddHistory(Entity expr)
                {
                    var refexpr = Simplified(recording, expr.Rewrite(RewriteRules.CanonicalOrder));
                    var compl1 = refexpr.SimplifiedRate;
                    var compl2 = expr.SimplifiedRate;
                    var n = compl1 > compl2 ? expr : refexpr;
                    var ncompl = Math.Min(compl2, compl1);
                    if (history.TryGetValue(ncompl, out var ncomplList))
                        ncomplList.Add(n);
                    else 
                        history[ncompl] = new HashSet<Entity> { n };
                }
                __IterAddHistory(expr);
                __IterAddHistory(expr.Rewrite(RewriteRules.InvertNegativePowers));

                MultithreadingFunctional.ExitIfCancelled();
            }

            AddHistory(stage1);
            var res = stage1;

            // A boolean expression minimised properly, offered as one more candidate rather
            // than taken. The rewrite rules reach absorption and stop there, so
            // `a and b or a and not b` factored to `a and (b or not b)` and had no rule to
            // finish it. Quine-McCluskey covers that, excluded middle, non-contradiction and
            // every larger cover in one procedure.
            //
            // A candidate can only be returned where it is the shortest on offer, so this
            // cannot make any expression's answer longer -- which is also what settles
            // https://github.com/asc-community/AngouriMath/issues/769, where an `implies`
            // rewrite won at 12 nodes against the 16-node input because the 4-node minimal
            // form was never generated for it to lose to.
            // https://github.com/asc-community/AngouriMath/issues/768
            var minimiserMark = recording?.Mark() ?? 0;
            if (Functions.Boolean.Minimiser.Minimise(stage1) is { } minimised)
                AddHistory(Noted(recording, stage1, minimised, "Minimise", minimiserMark));

            for (int i = 0; i < Math.Abs(level); i++)
            {
                var sortLevel = i switch
                {
                    1 => TreeAnalyzer.SortLevel.MIDDLE_LEVEL,
                    2 => TreeAnalyzer.SortLevel.LOW_LEVEL,
                    _ => TreeAnalyzer.SortLevel.HIGH_LEVEL
                };
                // Clearing a surd out of a two-term denominator, before anything else has
                // rearranged the quotient. Taken rather than offered as a candidate: the
                // complexity metric ties on it -- 1/(sqrt(3)+5) and (sqrt(3)-5)/(-22) rate
                // the same -- and a tie is settled by whichever candidate came first, which
                // is an accident rather than a preference.
                // https://github.com/asc-community/AngouriMath/issues/205
                if (res.Nodes.Any(child => child is Divf))
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.RationalizeDenominator)));

                res = Simplified(recording, res.Rewrite(RewriteRules.CanonicalOrderAt(sortLevel)));
                if (res.Nodes.Any(child => child is Powf))
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.Power)));

                AddHistory(res = SimplifyChildren(res));

                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.InvertNegativePowers).Rewrite(RewriteRules.DivisionPreparing)));

                // Lowest terms, offered as one more candidate rather than taken. Cancelling
                // means multiplying out, and a quotient that cancels down to a polynomial
                // was often better off in the form it was already written in:
                // 3x^2 (u + 2)^2 / (3x^2) is (u + 2)^2, which the rules below reach as it
                // stands, and u^2 + 4u + 4 only once this has expanded it. The complexity
                // metric is what should decide between them.
                if (res.Nodes.Any(child => child is Divf))
                    AddHistory(Simplified(recording, res.Rewrite(RewriteRules.PolynomialGcdCancellation)));

                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.PolynomialLongDivision)));


                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.NormalTrigonometricForm)));
                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.CollapseMultipleFractions)));
                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.CommonDenominatorAt(sortLevel))));
                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.InvertNegativePowers).Rewrite(RewriteRules.DivisionPreparing)));
                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.Power)));
                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.Trigonometric)));
                AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.CollapseTrigonometricFunctions)));

                if (res.Nodes.Any(child => child is TrigonometricFunction))
                {
                    var res1 = Simplified(recording, res.Rewrite(RewriteRules.ExpandTrigonometric));
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.Trigonometric).Rewrite(RewriteRules.Common)));
                    AddHistory(res1);
                    res = PickSimplest(res, res1);
                    AddHistory(res = res.Rewrite(RewriteRules.CollapseTrigonometricFunctions).Rewrite(RewriteRules.Trigonometric));

                    // Multiple angles opened up, then gathered again by the ordinary rules.
                    // Offered as a candidate rather than taken: written out, sin(4x) is far
                    // longer than it started, and only worth it where the pieces cancel --
                    // which is what the complexity metric is for.
                    var expandedAngles = Simplified(recording, res
                        .Rewrite(RewriteRules.ExpandMultipleAngle)
                        .Rewrite(RewriteRules.NormalTrigonometricForm));
                    if (expandedAngles != res)
                    {
                        AddHistory(expandedAngles);
                        AddHistory(Simplified(recording, SimplifyChildren(expandedAngles)
                            .Rewrite(RewriteRules.Trigonometric)
                            .Rewrite(RewriteRules.Common)));
                    }
                }


                if (res.Nodes.Any(child => child is Statement))
                {
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.Boolean)));
                }


                if (res.Nodes.Any(child => child is ComparisonSign))
                {
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.InequalityEquality)));
                }

                if (res.Nodes.Any(child => child is Factorialf))
                {
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.ExpandFactorialDivisions)));
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.FactorizeFactorialMultiplications)));
                }


                if (res.Nodes.Any(child => child is Powf or Logf))
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.Power)));

                if (res.Nodes.Any(child => child is Set))
                {
                    var replaced = res.Rewrite(RewriteRules.SetOperator);

                    AddHistory(res = Simplified(recording, replaced));
                }


                if (res.Nodes.Any(child => child is Phif))
                    AddHistory(res = Simplified(recording, res.Rewrite(RewriteRules.PhiFunction)));

                Entity? possiblePoly = null;
                foreach (var var in res.Vars)
                {
                    var polyMark = recording?.Mark() ?? 0;
                    if (TryPolynomial(res, var, out var resPoly)
                        && (possiblePoly is null || resPoly.Complexity < possiblePoly.Complexity))
                        AddHistory(possiblePoly = Noted(recording, res, resPoly, "AsPolynomial", polyMark));
                }
                if (possiblePoly is { } && possiblePoly.Complexity < res.Complexity)
                    res = possiblePoly;

                // The factored form is offered as one more candidate rather than taken:
                // (x + 1)^2 is worth having over x^2 + 2x + 1, but x^100 - 1 factors into
                // something far longer than it started, and the complexity metric is what
                // should decide between them.
                foreach (var var in res.Vars)
                {
                    var factorMark = recording?.Mark() ?? 0;
                    if (PolynomialFactoring.TryFactor(res, var, out var factoredPoly))
                        AddHistory(Noted(recording, res, factoredPoly, "Factorize", factorMark));
                }


                AddHistory(res = res.Rewrite(RewriteRules.Common));


                AddHistory(res = res.Rewrite(RewriteRules.NumericNeat));

                /*
                This was intended to simplify expressions as polynomials over nodes, some kind of
                greatest common node and simplifying over it. However, the current algorithm does
                not solve this issue completely and yet too slow to be accepted.

                AddHistory(res = TreeAnalyzer.Factorize(res));
                */

                res = history[history.Keys.Min()].First();
            }
            if (level > 0) // if level < 0 we don't check whether expanded version is better
            {
                var expandMark = recording?.Mark() ?? 0;
                AddHistory(Noted(recording, res, res.Expand(), nameof(Entity.Expand), expandMark).Simplify(-level));
                var factorizeMark = recording?.Mark() ?? 0;
                // The **rule-based** factorisation, not `Entity.Factorize` -- which now also asks
                // the polynomial layer, and whose answers must not become candidates here. The
                // cost model prefers the expanded form (`x ^ 6 - 1` rates 12 expanded against 58
                // factored), so a factored candidate wins only where the metric is closest, and
                // those are exactly the places a factored answer is least wanted: `x^3/3 + x^2/2`
                // becomes `(3 + 2 * x) * x ^ 2 / 6`, an antiderivative in a form no one writes.
                // Offering the layer here is #746 tier 2's pluggable cost model, not this.
                // https://github.com/asc-community/AngouriMath/issues/1018
                AddHistory(Noted(recording, res,
                    Transformation.RuleBasedFactorizationAtLevel(2).ApplyOrKeep(res),
                    nameof(Entity.Factorize), factorizeMark).Simplify(-level));

                // A multiple angle written out is worth having only where the pieces then
                // cancel, so it has to be simplified in full before the metric can be
                // asked -- the same reason Expand and Factorize are re-simplified above,
                // and for the same reason it is a candidate rather than a step. The pass
                // inside the loop offers the opened form to the trigonometric rules only,
                // which settles an expression that is already one term; it cannot settle
                // one whose cancellation needs a common denominator, because the passes
                // that build one ran earlier in the loop, while the angles were still shut.
                // That is https://github.com/asc-community/AngouriMath/issues/557: the
                // reporter's second expression is 0, and reaches 0 through
                // 2 sin(t) cos(t) and not through sin(2t).
                var openedAngles = Simplified(recording, res
                    .Rewrite(RewriteRules.ExpandMultipleAngle)
                    .Rewrite(RewriteRules.NormalTrigonometricForm));
                if (openedAngles != res)
                {
                    // Expanded, and for the same reason res is expanded above: the
                    // cancellation only shows up once the products are multiplied out.
                    var openedMark = recording?.Mark() ?? 0;
                    AddHistory(Noted(recording, openedAngles, openedAngles.Expand(), nameof(Entity.Expand), openedMark).Simplify(-level));
                }
            }

            return history.Values.SelectMany(x => x);
        }

        /// <summary>
        /// Sorts an expression into a polynomial.
        /// See more at <see cref="MathS.TryPolynomial"/>
        /// </summary>
        internal static bool TryPolynomial(Entity expr, Variable variable,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out Entity? dst)
        {
            dst = null;
            var children = Sumf.LinearChildren(expr.Expand());
            var monomialsByPower = Algebra.AnalyticalSolving.PolynomialSolver.GatherMonomialInformation
                <EInteger, TreeAnalyzer.PrimitiveInteger>(children, variable);
            if (monomialsByPower == null)
                return false;
            var res = BuildPoly(monomialsByPower, variable);
            if (res is null)
                return false;
            dst = res;
            return true;
        }
        internal static Entity? BuildPoly(Dictionary<EInteger, Entity> monomialsByPower, Variable x)
        {
            var terms = new List<Entity>();
            foreach (var index in monomialsByPower.Keys.OrderByDescending(x => x))
            {
                var pair = new KeyValuePair<EInteger, Entity>(index, monomialsByPower[index]);
                if (pair.Key.IsZero)
                {
                    terms.Add(pair.Value.InnerSimplified);
                    continue;
                }

                var px = pair.Key.Equals(EInteger.One) ? x : MathS.Pow(x, pair.Key);
                terms.Add(pair.Value == 1 ? px : pair.Value.InnerSimplified * px);
            }

            if (terms.Count == 0)
                return null;
            var dst = terms[0];
            for (int i = 1; i < terms.Count; i++)
                if (terms[i] is Mulf(Real { IsNegative: true } r, var m))
                    dst -= -r * m;
                else
                    dst += terms[i];
            return dst.InnerSimplified;
        }

        internal static Entity ParaphraseInterval(Entity entity, Entity left, bool leftClosed, Entity right, bool rightClosed)
        {
            var leftCon = ConditionallyGreater(entity, left);
            if (leftClosed)
                leftCon |= entity.EqualTo(left);
            var rightCon = ConditionallyGreater(right, entity);
            if (rightClosed)
                rightCon |= entity.EqualTo(right);
            return leftCon & rightCon;
        }

        /// <summary>
        /// <paramref name="left"/> exceeds <paramref name="right"/>, stated as a comparison of
        /// their difference against zero — which is the form the inequality rules can decide.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="SimplifyChildren"/> alone is not enough to reach that form. It simplifies
        /// the operands and leaves the sum standing, so <c>a - a / 3</c> comes back as
        /// <c>a + -a / 3</c> — two terms in one variable, which no rule about a sign can read.
        /// <see cref="Transformation.RationalCanonicalization"/> collects them into
        /// <c>2/3 * a</c>, and a rational multiple of something is positive exactly when that
        /// something is, so <c>a in (a / 3; 3 * a)</c> is <c>a &gt; 0</c>.
        /// </para>
        /// <para>
        /// It was <c>a in (a / 2; 2 * a)</c> alone that answered before, and by coincidence
        /// rather than by this route: nothing collected the terms, and the answer came out of
        /// <see cref="Entity.Simplify(int)"/>'s candidate search happening to reach it for that
        /// one denominator. Three, four, five and six were all left as written.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1056">#1056</a>
        /// </para>
        /// <para>
        /// Then the comparison itself is normalised, and that is what makes the answer reachable
        /// rather than merely correct. <c>Simplify</c> prunes a candidate by
        /// <c>SimplifiedRate</c> at each step, and <c>2/3 * a &gt; 0 and 2 * a &gt; 0</c> rates 26
        /// where the membership it came from rates 25 — one point worse, so it was discarded
        /// before anything could reduce it to <c>a &gt; 0</c>, which rates 8. The <c>n = 2</c> case
        /// answered only because <c>1/2 * a &gt; 0 and a &gt; 0</c> happens to rate 24. Dividing
        /// out the positive factor here, with the rule set that already knows how, means the
        /// candidate is born at its best rate instead of having to survive on the way there.
        /// </para>
        /// <para>
        /// Two named transformations rather than a call back into <see cref="Entity.Simplify(int)"/>,
        /// deliberately: this runs inside a rewrite rule that <c>Simplify</c> itself applies, and
        /// the full simplifier here would be a cycle through the interval rule rather than a
        /// deeper answer.
        /// </para>
        /// </remarks>
        internal static Entity ConditionallyGreater(Entity left, Entity right)
            => decideSign.ApplyOrKeep(
                Transformation.RationalCanonicalization.ApplyOrKeep(SimplifyChildren(left - right)) > 0);

        /// <summary>
        /// Divides a decidably-signed numeric factor out of a comparison with zero, which is what
        /// takes <c>2/3 * a &gt; 0</c> to <c>a &gt; 0</c>. Held as a field so the chain is built
        /// once rather than per interval end.
        /// </summary>
        [ConstantField]
        private static readonly Transformation decideSign =
            Transformation.Rewriting(RewriteRules.InequalityEquality);

        /// <summary>
        /// Divides the given expression by the divisor.
        /// Requires a given node to exactly match the divisor,
        /// so no "smart" division can be applied.
        /// (e. g. pi / 2 divide by pi would work, but
        /// (2 a) / 2 won't be divided by 4a)
        /// </summary>
        /// <returns>The result if valid, null otherwise</returns>
        internal static Entity? DivideByEntityStrict(Entity expr, Entity divisor)
            => expr switch
            {
                var same when same == divisor => 1,
                Sumf(var left, var right) => 
                    DivideByEntityStrict(left, divisor) is { } l && 
                    DivideByEntityStrict(right, divisor) is { } r
                    ? l + r
                    : null,
                Minusf(var left, var right) => 
                    DivideByEntityStrict(left, divisor) is { } l && 
                    DivideByEntityStrict(right, divisor) is { } r
                    ? l - r
                    : null,
                Mulf(var left, var right) =>
                    DivideByEntityStrict(left, divisor) is { } l
                    ? l * right 
                    : DivideByEntityStrict(right, divisor) is { } r
                        ? left * r
                        : null,
                Divf(var left, var right) =>
                    DivideByEntityStrict(left, divisor) is { } l
                    ? l / right
                    : DivideByEntityStrict(right, divisor is Powf(var newDiv, Integer(-1)) ? newDiv : divisor.Pow(-1)) is { } r
                        ? left / r
                        : null,
                _ => null
            };

        /// <summary>
        /// If it can, it will find coefficients 
        /// [a_1, a_2, ..., a_n] such that for
        /// given rational forms [p_1, p_2, ..., p_n]
        /// it is true that 
        /// q = a_1 * p_1 + a_2 * p_2 + ... + a_n * p_n
        /// </summary>
        /// <returns>
        /// The sequence of pairs coef-form or
        /// null if it cannot find them
        /// </returns>
        internal static IEnumerable<(Integer coef, Rational form)>? RepresentRational(Rational q, IEnumerable<Rational> forms)
        {
            if (q.Denominator > 600)
                return null;
            var res = new List<(Integer coef, Rational form)>();
            foreach (var form in forms.OrderBy(c => -c.AsDouble()))
            {
                if (form > q)
                    continue;
                //if (q.Denominator % form.Denominator != 0)
                //    continue;
                /*
                 * a/b = k * c/d + e/f
                 * 
                 * We need to find such k (the result of "integer" division of rationals)
                 * and e, f such that e/f is the remainder of that division.
                 * 
                 * 1. Get the common denominator:
                 * (ad, cb) <- (a/b * bd, c/d * bd)
                 * 
                 * 2. Perform normal integer division
                 * We get ad = k * cb + e
                 * 
                 * 3. f = e / bd
                 * 
                 */

                var bd = q.Denominator * form.Denominator;
                var (ad, cb) = ((Integer)(q * bd), (Integer)(form * bd));
                var (k, e) = (ad.IntegerDiv(cb), ad % cb);
                var newQ = (Rational)(e / bd);

                if (q.Denominator % newQ.Denominator == 0)
                {
                    q = newQ;
                    res.Add((k, form));
                }
            }
            if (q.IsZero)
                return res;
            return null;
        }
    }
}
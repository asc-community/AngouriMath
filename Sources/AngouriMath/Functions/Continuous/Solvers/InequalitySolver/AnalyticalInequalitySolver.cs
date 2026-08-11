//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class AnalyticalInequalitySolver
    {
        /// <summary>
        /// Considers expr > 0
        /// </summary>
        internal static Set Solve(Entity expr, Variable x)
        {
            switch (expr)
            {
                case Providedf(var e, var predicate): return Solve(e, x).Filter(predicate, x);
                case Piecewise p: return EquationSolver.SolvePiecewise(p, x, Solve);
            }
            {
                if (MathS.Utils.TryGetPolyLinear(expr, x, out var a, out var b))
                {
                    a = a.InnerSimplified;
                    b = b.InnerSimplified;
                    var root = PolynomialSolver.SolveLinear(a, b).First();
                    if (root is Complex and not Real)
                        return Empty;
                    // a*x + b > 0 is x > -b/a for a positive a and x < -b/a for a negative one,
                    // and for a = 0 it is not an inequality in x at all -- it is b > 0, which
                    // holds everywhere or nowhere.
                    Set below = new Interval(Real.NegativeInfinity, false, root, false);
                    Set above = new Interval(root, false, Real.PositiveInfinity, false);
                    if (a is Real { IsNegative: true })
                        return below;
                    if (a is Real)
                        return above;
                    return BySignOf(a, x, whenPositive: above, whenNegative: below,
                                    whenZero: Everywhere(b, x));
                }
            }
            {
                if (MathS.Utils.TryGetPolyQuadratic(expr, x, out var a, out var b, out var c))
                {
                    a = a.InnerSimplified;
                    b = b.InnerSimplified;
                    c = c.InnerSimplified;
                    var roots = PolynomialSolver.SolveQuadratic(a, b, c);
                    var discriminant = (MathS.Sqr(b) - 4 * a * c).InnerSimplified;
                    // Read off the discriminant and not off the roots. Whether a root comes back
                    // as something `is Complex` depends on how far the radical simplified:
                    // sqrt(-4) is the literal 2i and sqrt(-12) is a product that is not one, so
                    // x^2 + 1 was recognised as never reaching zero and 3*x^2 + 1 was not.
                    // Where the leading coefficient vanishes there is no parabola: what is left
                    // is b*x + c, which the linear branch above answers -- unless b vanishes
                    // too, and then there is no x in it at all and the statement is a
                    // comparison of constants, true on the whole line or on none of it.
                    var degenerate = (b * x + c).InnerSimplified;
                    Set whenDegenerate = degenerate.ContainsNode(x)
                        ? Solve(degenerate, x)
                        : Everywhere(degenerate, x);
                    // No real root means the parabola never crosses zero, so it is above it
                    // everywhere or below it everywhere -- and which of those is the sign of the
                    // leading coefficient. Returning the empty set regardless answered
                    // x^2 + 1 > 0 with nothing, where it holds at every real x.
                    Set NeverZero() => ByLeadingSign(a, x, SpecialSet.Create(Domain.Real), Empty,
                                                     whenDegenerate);
                    if (discriminant.Evaled is Real { IsNegative: true }
                        || roots.Any(root => root is Complex and not Real))
                        return NeverZero();
                    roots = TreeAnalyzer.SortRealsAndNonReals(roots);
                    var (root1, root2, endpointCondition) = AscendingEndpoints(roots.First(), roots.Last());
                    // A parabola is above zero between its roots when it opens downwards and
                    // outside them when it opens upwards, and which of those it does is the
                    // sign of the leading coefficient. That test read `a is Real { IsNegative:
                    // true }`, which a symbol fails -- so every symbolic leading coefficient
                    // was answered as though it were positive, and a*x^2 - 1 < 0 came back with
                    // the complement of its solution set.
                    Set between = new Interval(root1, false, root2, false);
                    Set outside = new Interval(Real.NegativeInfinity, false, root1, false)
                        .Unite(new Interval(root2, false, Real.PositiveInfinity, false));
                    var crossingZero = ByLeadingSign(a, x, outside, between, whenDegenerate);
                    // The discriminant's sign is a second undecidable question, and an
                    // independent one: (x + 1)(x + 2) < a negates to a leading coefficient of
                    // -1, concrete and negative, while its discriminant 1 + 4a is symbolic. So
                    // this split is made on its own evidence rather than only where the leading
                    // coefficient is a symbol.
                    if (discriminant.Evaled is Real)
                        return Assuming(endpointCondition, crossingZero, x);
                    return Assuming(endpointCondition,
                        crossingZero.Filter(discriminant >= 0, x)
                            .Unite(NeverZero().Filter(discriminant < 0, x)),
                        x);
                }
            }
            throw new NotSufficientlySupportedException(
                "Only linear and quadratic polynomial inequalities are supported; "
                + "this one is of a higher degree");
        }

        /// <summary>
        /// The three answers a coefficient of unknown sign may have, joined into one set that
        /// is whichever of them the sign turns out to select.
        /// </summary>
        /// <remarks>
        /// The conditions are mutually exclusive and cover every case, so the union of the
        /// three conditional sets is the case split -- each is empty except the one whose
        /// condition holds. This is what a piecewise would say, said in a way that is still a
        /// <see cref="Set"/>: <see cref="Piecewise"/> is an <see cref="Entity"/>, so
        /// <see cref="Solve"/> cannot hand one back.
        /// <para/>
        /// Unlike the ordering of the endpoints, which has a closed form
        /// (<see cref="AscendingEndpoints"/>), this genuinely needs the split: lying between
        /// the roots and lying outside them differ topologically and not arithmetically, so no
        /// single interval covers both.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/762">#762</a>
        /// </remarks>
        private static Set BySignOf(Entity coefficient, Variable x,
                                    Set whenPositive, Set whenNegative, Set whenZero)
            => whenPositive.Filter(coefficient > 0, x)
                .Unite(whenNegative.Filter(coefficient < 0, x))
                .Unite(whenZero.Filter(coefficient.Equalizes(0), x));

        /// <summary>
        /// Whichever of the two the leading coefficient selects, read off directly where its
        /// sign is known and left as a case split where it is not, so that a concrete
        /// coefficient produces exactly the interval it always did.
        /// </summary>
        private static Set ByLeadingSign(Entity a, Variable x,
                                         Set whenPositive, Set whenNegative, Set whenZero)
            => a is Real { IsNegative: true } ? whenNegative
             : a is Real ? whenPositive
             : BySignOf(a, x, whenPositive, whenNegative, whenZero);

        /// <summary>
        /// The solutions of a statement that does not mention <paramref name="x"/> at all:
        /// every real number where it holds, and none where it does not.
        /// </summary>
        private static Set Everywhere(Entity constant, Variable x)
            => SpecialSet.Create(Domain.Real).Filter(constant > 0, x);

        /// <summary>
        /// The answer under a condition it needed, or the answer itself where there was none.
        /// </summary>
        private static Set Assuming(Entity? condition, Set answer, Variable x)
            => condition is null ? answer : answer.Filter(condition, x);

        /// <summary>
        /// The two roots as (smaller, larger).
        /// </summary>
        /// <remarks>
        /// <see cref="TreeAnalyzer.SortRealsAndNonReals"/> orders the roots it can compare
        /// and leaves the rest in the order the quadratic formula produced them, which is
        /// <c>(-b - sqrt(D))/(2a)</c> before <c>(-b + sqrt(D))/(2a)</c> -- ascending only
        /// while <c>a</c> is positive. Solving <c>expr &lt;= 0</c> negates the expression and
        /// so arrives here with a negative leading coefficient, which is how
        /// <c>(x - a)(x + a) &lt;= 0</c> came to be answered with an interval running from
        /// <c>|a|</c> to <c>-|a|</c>: empty, with the whole solution set lost.
        /// <para/>
        /// Written with <c>abs</c> rather than as a case split on the sign of the symbol,
        /// because the ordering has a closed form -- <c>min(p, q)</c> is
        /// <c>(p + q - |p - q|)/2</c> and <c>max(p, q)</c> is <c>(p + q + |p - q|)/2</c>. So
        /// the answer is the single interval <c>[-|a|; |a|]</c>, which is right for either
        /// sign of <c>a</c> and for <c>a = 0</c>, rather than three branches each holding on
        /// part of the line. It also keeps the result a <see cref="Set"/>: a piecewise is an
        /// <see cref="Entity"/> and not one, so returning it here is not open in any case.
        /// <para/>
        /// Where both roots are real numbers the sort above has already ordered them, and
        /// they are handed back untouched so that nothing a concrete coefficient produces
        /// changes shape.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/757">#757</a>
        /// </remarks>
        private static (Entity Lower, Entity Upper, Entity? Condition) AscendingEndpoints(Entity first, Entity second)
        {
            if (first is Real && second is Real)
                return (first, second, null);
            var spread = MathS.Abs(first - second);
            var (lower, lowerCondition) = WithoutCondition(((first + second - spread) / 2).Simplify());
            var (upper, upperCondition) = WithoutCondition(((first + second + spread) / 2).Simplify());
            return (lower, upper, Both(lowerCondition, upperCondition));
        }

        /// <summary>
        /// An endpoint split into its value and whatever the simplification had to assume to
        /// reach it, since a <see cref="Providedf"/> where an endpoint belongs makes the
        /// interval an <see cref="Entity"/> rather than a <see cref="Set"/>.
        /// </summary>
        /// <remarks>
        /// Ordering the roots of <c>a*x^2 - 1</c> cancels an <c>a</c> against itself, which
        /// holds only away from zero, so the condition is real and is carried into the answer
        /// rather than dropped from it -- by the same <see cref="Set.Filter"/> that carries the
        /// sign of the leading coefficient.
        /// </remarks>
        private static (Entity Value, Entity? Condition) WithoutCondition(Entity expr)
        {
            Entity? condition = null;
            while (expr is Providedf(var inner, var predicate))
            {
                condition = Both(condition, predicate);
                expr = inner;
            }
            return (expr, condition);
        }

        private static Entity? Both(Entity? left, Entity? right)
            => left is null ? right
             : right is null || left == right ? left
             : left & right;
    }
}

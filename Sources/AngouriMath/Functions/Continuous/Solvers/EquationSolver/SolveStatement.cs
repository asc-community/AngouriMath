//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using PeterO.Numbers;
using AngouriMath.Functions.Continuous.Solvers.SetSolver;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class StatementSolver
    {
        private static Entity Minus(Entity left, Entity right)
        {
            if (left.Evaled == 0)
                return -right;
            if (right.Evaled == 0)
                return left;
            return left - right;
        }

        /// <summary>
        /// Substitutes each root back into the equation it came from and drops the ones
        /// that do not satisfy it.
        /// </summary>
        /// <remarks>
        /// Several of the rewrites the solvers rely on widen the domain of the equation.
        /// <c>ln(a) + ln(b) = ln(a * b)</c> is the plainest: solving
        /// <c>ln(x) + ln(x+1) = 0</c> goes through <c>x^2 + x - 1 = 0</c> and hands back
        /// both of its roots, but at -1.618... the original is 2*pi*i, not 0. The
        /// individual rewrites cannot always carry a condition that survives the chain of
        /// substitutions that follows, so the answers are checked once, here, against the
        /// equation as the caller wrote it.
        /// </remarks>
        internal static Set WithoutSpuriousRoots(Set roots, Entity equation, Variable x)
            => roots is FiniteSet finite && finite.Any(root => IsSpurious(equation, x, root))
                ? finite.Where(root => !IsSpurious(equation, x, root)).ToSet()
                : roots;

        /// <summary>
        /// Answers the condition itself where a root denies the independence the calculus
        /// operators in the equation were evaluated under.
        /// </summary>
        /// <remarks>
        /// <c>derivative(y, x) + y - x</c> was answered <c>{ x }</c>: the derivative went to
        /// zero because <c>y</c> is not <c>x</c>, and the root then says that it is. Putting
        /// it back gives <c>derivative(x, x) + x - x</c>, which is 1 — so the set named a
        /// member that is not a root. A root free of that name is untouched, because nothing
        /// was assumed that it goes on to deny: <c>derivative(y * x, x) + y - 1</c> is still
        /// <c>{ 1/2 }</c>.
        ///
        /// The equation is not thereby unsatisfiable, so the empty set would be a second
        /// false claim in place of the first. What holds is the condition as written, and
        /// solving it needs a differential-equation solver this library does not have.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/964">#964</a>,
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a>
        /// </remarks>
        internal static Set UnsolvedWhereIndependenceIsDenied(Set roots, Entity condition, Variable x)
        {
            if (roots is not FiniteSet finite)
                return roots;
            var assumed = CalculusOperator.NamesAssumedFreeOf(condition, x);
            return assumed.Count > 0 && finite.Any(root => assumed.Any(root.ContainsNode))
                ? new ConditionalSet(x, condition)
                : roots;
        }

        /// <summary>
        /// How large the residual has to be next to the terms it is the sum of before it
        /// counts as evidence against a root.
        /// </summary>
        /// <remarks>
        /// A residual that is merely non-zero is not evidence. A root found numerically is
        /// only as accurate as the search that found it, and the terms of the equation at
        /// such a root cancel to within that accuracy rather than exactly. Judging the
        /// residual on its own size threw away every root of
        /// <c>1/210 - 17x/210 + 101x^2/210 - 247x^3/210 + x^4</c>, whose four roots come
        /// back as decimals and leave 5.2e-6 against terms of about 1.2e-2, and answered
        /// that quartic with the empty set.
        ///
        /// The two cases are far apart, which is what makes a threshold possible at all.
        /// A spurious root is not a near miss: it comes from a rewrite that widened the
        /// domain, so the residual is a whole quantity in its own right -- 2*pi*i for the
        /// logarithm sum, or the difference between -3 and 9 for an exponential. Over the
        /// equations measured, every genuine root sits at 4.5e-4 or below and most at
        /// 1e-15, while every spurious one sits between 1.5 and 2. This is a hundredth:
        /// twenty times above the worst genuine root and a hundred and fifty times below
        /// the mildest spurious one.
        /// </remarks>
        [ConstantField] private static readonly EDecimal RelativeResidualTolerance = EDecimal.FromString("0.01");

        /// <summary>
        /// Whether a root demonstrably fails the equation. Anything that cannot be
        /// evaluated to a number -- a root still carrying a parameter, say
        /// <c>pi + 2 * pi * n_1</c>, or one whose residual is not finite -- is kept, so
        /// that a root is only ever dropped on positive evidence against it.
        /// </summary>
        private static bool IsSpurious(Entity equation, Variable x, Entity root)
        {
            // A root that still mentions the variable it solves for is the equation
            // rearranged, not an answer to it: x = sqrt(-1 - x) says nothing about x. This
            // is decided before the parameter exemption below, which would otherwise keep
            // such a root unconditionally -- it carries a variable, so substituting it back
            // leaves an expression rather than a residual, and there is no evidence to drop
            // it on. That is how (x^2 + x + 1)^2 = 0 came to be answered with two of them.
            //
            // No equation reaches this today: the routes that produced such a root are
            // fixed at their source in AnalyticalEquationSolver, which is what makes those
            // equations answer correctly rather than emptily. It is kept because Invert
            // requires the variable to occur once and only one of its five callers checks
            // that, so the next caller to forget is caught here rather than in a bug report.
            // https://github.com/asc-community/AngouriMath/issues/744
            if (root.ContainsNode(x))
                return true;
            if (root.Vars.Any())
                return false;
            try
            {
                var substituted = equation.Substitute(x, root);
                if (substituted.Evaled is not Number.Complex residual || !residual.IsFinite)
                    return false;
                var size = residual.Abs().EDecimal;
                if (size.LessThan(MathS.Settings.PrecisionErrorCommon))
                    return false;
                return size.GreaterThan(LargestTerm(substituted).Multiply(RelativeResidualTolerance));
            }
            catch (Core.Exceptions.AngouriBugException) { throw; }
            catch (System.Exception) { return false; }
        }

        /// <summary>
        /// The largest of the terms the equation adds up to at the root, which is the scale
        /// the residual has to be read against. A single term is its own largest, so an
        /// equation that is not a sum is judged on the size of its residual alone -- there
        /// is nothing there for a root to have cancelled inexactly.
        /// </summary>
        private static EDecimal LargestTerm(Entity substituted)
        {
            var largest = EDecimal.Zero;
            foreach (var term in Sumf.LinearChildren(substituted))
                if (term.Evaled is Number.Complex value && value.IsFinite)
                    largest = EDecimal.Max(largest, value.Abs().EDecimal);
            return largest;
        }

        internal static Set Solve(Entity expr, Variable x)
            => expr switch
            {
                Equalsf(var left, var right) when left is Set || right is Set
                    => AnalyticalSetSolver.Solve(left, right, x),

                Equalsf(var left, var right) when left is not Set && right is not Set
                    => UnsolvedWhereIndependenceIsDenied(
                        WithoutSpuriousRoots(AnalyticalEquationSolver.Solve(left - right, x), left - right, x),
                        expr, x),

                Equalsf => Empty,

                Andf(var left, var right) => 
                    MathS.Intersection(Solve(left, x), Solve(right, x)),
                Orf(var left, var right) => 
                    MathS.Union(Solve(left, x), Solve(right, x)),
                Impliesf(var left, var right) => 
                    MathS.Union(MathS.SetSubtraction(expr.Codomain, Solve(left, x)), Solve(right, x)),

                // TODO: there should be universal set to subtract from when inverting
                Greaterf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(left, right), x),
                LessOrEqualf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(right, left), x)
                    .Unite(AnalyticalEquationSolver.Solve(Minus(left, right), x)),
                GreaterOrEqualf(var left, var right) => MathS.Union(AnalyticalInequalitySolver.Solve(Minus(left, right), x), AnalyticalEquationSolver.Solve(Minus(left, right), x)),

                Lessf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(right, left), x),

                Variable when expr == x => new FiniteSet(true),

                Inf(var var, Set set) when var == x => set,
                
                Providedf(var e, var predicate) => Solve(e, x).Filter(predicate, x),
                Piecewise p => EquationSolver.SolvePiecewise(p, x, Solve),

                // TODO: Although piecewise needed?
                _ => Set.Empty
            };
    }
}

//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Core.Transformations.Matching;
using static AngouriMath.Entity;

namespace AngouriMath.Functions.Algebra
{
    /// <summary>
    /// A rule of the integrator written as data: a pattern for a piece of the integrand, a
    /// condition on what it bound, and what that piece becomes -- after which the integrand, so
    /// rewritten, is asked again as the same question.
    /// </summary>
    /// <remarks>
    /// The pilot of <a href="https://github.com/asc-community/AngouriMath/issues/1486">#1486</a>:
    /// Rubi's rules are one line because the pattern matcher carries the shape -- which operands
    /// of a sum, in which order, with a coefficient or without one -- and the rule says only the
    /// mathematics. <see cref="MatchPattern"/> carries that shape here too, with
    /// <see cref="MatchPattern.Gathered{T}"/> for the order and <see cref="MatchPattern.Scaled"/>
    /// for the coefficient, so a rule written this way is its pattern, its condition and its
    /// result. What the condition and the result need of the integration variable they are given,
    /// since a pattern is built once and the variable is known only when it is asked.
    /// </remarks>
    internal sealed class IntegrandRule
    {
        /// <summary>One shape the rule reads, what it needs of it, and what it becomes.</summary>
        internal sealed record Case(MatchPattern Pattern, Func<Bindings, Variable, bool> When, Func<Bindings, Variable, Entity> Becomes);

        private readonly Case[] cases;
        private readonly bool belowTheBarOnly;
        private readonly Func<Entity, Entity>? afterwards;

        internal IntegrandRule(string name, bool belowTheBarOnly, Func<Entity, Entity>? afterwards, params Case[] cases)
        {
            Name = name;
            this.cases = cases;
            this.belowTheBarOnly = belowTheBarOnly;
            this.afterwards = afterwards;
        }

        /// <summary>What the rule says, for a trace or a report.</summary>
        internal string Name { get; }

        /// <summary>
        /// The integrand with every piece the rule reads rewritten -- each node by the first case
        /// that reads it -- asked again as the same question; null where nothing was read.
        /// </summary>
        internal Entity? Apply(Entity expr, Variable x, bool integrateByParts)
        {
            Entity above = Number.Integer.One, target = expr;
            if (belowTheBarOnly)
            {
                (above, target) = SingleQuotient.Of(SingleQuotient.Combine(expr));
                if (!target.ContainsNode(x))
                    return null;
            }
            var rewritten = target.Replace(node =>
            {
                if (!node.ContainsNode(x))
                    return node;
                foreach (var @case in cases)
                {
                    // Asked of every node below the bar, so the cheap test goes first: which kinds
                    // of node the pattern could match at all, before an enumerator is started on
                    // it -- derived from the pattern, so that no rule has to remember to say it.
                    if (!@case.Pattern.CouldMatchRoot(node))
                        continue;
                    foreach (var bound in @case.Pattern.Match(node, Bindings.Empty))
                        if (@case.When(bound, x))
                            return @case.Becomes(bound, x);
                }
                return node;
            });
            if (rewritten == target)
                return null;
            if (afterwards is not null)
                rewritten = afterwards(rewritten);
            var whole = belowTheBarOnly ? above / rewritten : rewritten;
            return Integration.ComputeAsTheSameQuestion(whole.InnerSimplified, x, integrateByParts);
        }
    }

    /// <summary>The integrator's rules that are written as data.</summary>
    internal static class IntegrandRules
    {
        /// <summary>
        /// <c>A cos(y) + i A sin(y)</c> below the bar is <c>A e^(i y)</c>, and
        /// <c>A cos(y) - i A sin(y)</c> is <c>A e^(-i y)</c>; see
        /// <see cref="IndefiniteIntegralSolver.SolveByWritingAnImaginarySumOfACosineAndASineAsAnExponential"/>.
        /// </summary>
        internal static IntegrandRule ImaginarySumOfACosineAndASine { get; } = new(
            "an imaginary sum of a cosine and a sine is an exponential",
            belowTheBarOnly: true,
            // A whole power of the product written, split, `(A e^(i y))^n` as `A^n e^(i n y)`: the
            // rule that distributes such powers takes a constant with a symbol in it and leaves a
            // number, and `cos(x)^2/(2 e^(i x))^3` was declined where `cos(x)^2/(a e^(i x))^3` was not.
            afterwards: SplitPowersOfProductsHoldingAnExponential,
            new IntegrandRule.Case(
                MatchPattern.Gathered<Sumf>("rest",
                    MatchPattern.Scaled("A", MatchPattern.Node<Cosf>(MatchPattern.Any("y"))),
                    MatchPattern.Scaled("B", MatchPattern.Node<Sinf>(MatchPattern.Any("y")))),
                When: (bound, x) => bound["rest"] == Number.Integer.Zero && bound["y"].ContainsNode(x)
                    && !bound["A"].ContainsNode(x) && !bound["B"].ContainsNode(x)
                    && ImaginaryUnitSign(bound["B"], bound["A"]) != 0,
                Becomes: (bound, _) => bound["A"] * MathS.Pow(MathS.e,
                    (ImaginaryUnitSign(bound["B"], bound["A"]) * MathS.i * bound["y"]).InnerSimplified)));

        /// <summary>
        /// <c>A + i A tan(z)</c> below the bar is <c>A e^(i z)/cos(z)</c>, and <c>A + i A cot(z)</c>
        /// is <c>i A e^(-i z)/sin(z)</c>, the imaginary unit either way round; see
        /// <see cref="IndefiniteIntegralSolver.SolveByWritingAnImaginaryTangentAsAnExponential"/>.
        /// <c>A</c> is every other term of the sum, which has to be free of the variable.
        /// </summary>
        internal static IntegrandRule ImaginaryTangent { get; } = new(
            "an imaginary tangent or cotangent is an exponential",
            belowTheBarOnly: true,
            afterwards: null,
            new IntegrandRule.Case(
                MatchPattern.Gathered<Sumf>("A", MatchPattern.Scaled("B", MatchPattern.Node<Tanf>(MatchPattern.Any("z")))),
                When: (bound, x) => ReadsAsAnImaginaryMultiple(bound, x),
                Becomes: (bound, _) => bound["A"] * MathS.Pow(MathS.e,
                    (ImaginaryUnitSign(bound["B"], bound["A"]) * MathS.i * bound["z"]).InnerSimplified) / MathS.Cos(bound["z"])),
            new IntegrandRule.Case(
                MatchPattern.Gathered<Sumf>("A", MatchPattern.Scaled("B", MatchPattern.Node<Cotanf>(MatchPattern.Any("z")))),
                When: (bound, x) => ReadsAsAnImaginaryMultiple(bound, x),
                Becomes: (bound, _) => ImaginaryUnitSign(bound["B"], bound["A"]) * MathS.i * bound["A"]
                    * MathS.Pow(MathS.e, (-ImaginaryUnitSign(bound["B"], bound["A"]) * MathS.i * bound["z"]).InnerSimplified)
                    / MathS.Sin(bound["z"])));

        private static bool ReadsAsAnImaginaryMultiple(Bindings bound, Variable x)
            => bound["z"].ContainsNode(x) && !bound["A"].ContainsNode(x) && !bound["B"].ContainsNode(x)
               && bound["A"] != Number.Integer.Zero && ImaginaryUnitSign(bound["B"], bound["A"]) != 0;

        /// <summary>
        /// One where <paramref name="b"/>/<paramref name="a"/> is <c>i</c>, minus one where it is
        /// <c>-i</c>, zero otherwise. Bare: <c>i a/a</c> simplifies to <c>i provided not a = 0</c>,
        /// and a condition is not a number to compare against.
        /// </summary>
        private static int ImaginaryUnitSign(Entity b, Entity a)
        {
            var ratio = PartialFractions.Bare((b / a).InnerSimplified);
            if (ratio.Evaled is not Number.Complex)
                ratio = PartialFractions.Bare(ratio.Simplify());
            return ratio.Evaled == MathS.i.Evaled ? 1 : ratio.Evaled == (-MathS.i).Evaled ? -1 : 0;
        }

        private static Entity SplitPowersOfProductsHoldingAnExponential(Entity expr)
            => expr.Replace(node =>
                node is Powf(Mulf(var left, var right), Number.Integer power)
                && (left is Powf(var leftBase, _) && leftBase == MathS.e || right is Powf(var rightBase, _) && rightBase == MathS.e)
                    ? MathS.Pow(left, power) * MathS.Pow(right, power)
                    : node);
    }
}

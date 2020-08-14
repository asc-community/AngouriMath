
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */



using static AngouriMath.Entity;
using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using AngouriMath.Functions;
using AngouriMath.Functions.Algebra;

namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>
        /// Attempt to find analytical roots of a custom equation
        /// </summary>
        /// <param name="x"></param>
        /// <returns>
        /// Returns Set. Work with it as with a list
        /// </returns>
        public Set SolveEquation(Variable x) => EquationSolver.Solve(this, x);

        /// <summary><para>This <see cref="Entity"/> MUST contain exactly ONE occurance of <paramref name="x"/>,
        /// otherwise this function won't work correctly.</para>
        /// 
        /// This function inverts an expression and returns a <see cref="Set"/>. Here, a represents <paramref name="value"/>.
        /// <list type="table">
        /// <item>x^2 = a ⇒ x = { sqrt(a), -sqrt(a) }</item>
        /// <item>sin(x) = a ⇒ x = { arcsin(a) + 2 pi n, pi - arcsin(a) + 2 pi n }</item>
        /// </list>
        /// </summary>
        /// <returns>A set of possible roots of the expression.</returns>
        internal IEnumerable<Entity> Invert(Entity value, Entity x) =>
            Invert_(value.InnerSimplify(), x).Where(el => el.IsFinite);
        /// <summary>Use <see cref="Invert(Entity, Entity)"/> instead which auto-simplifies <paramref name="value"/></summary>
        private protected abstract IEnumerable<Entity> Invert_(Entity value, Entity x);
        /// <summary>
        /// Returns true if <paramref name="a"/> is inside a rect with corners <paramref name="from"/>
        /// and <paramref name="to"/>, OR <paramref name="a"/> is an unevaluable expression
        /// </summary>
        private protected static bool EntityInBounds(Entity a, ComplexNumber from, ComplexNumber to)
        {
            if (!MathS.CanBeEvaluated(a))
                return true;
            var r = a.Eval();
            return r.Real >= from.Real &&
                   r.Imaginary >= from.Imaginary &&
                   r.Real <= to.Real &&
                   r.Imaginary <= to.Imaginary;
        }
        public partial record Number : Entity
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                throw new ArgumentException("This function must contain " + nameof(x), nameof(x));
        }
        public partial record Variable : Entity
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) => Enumerable.Repeat(this, 1);
        }
        public partial record Tensor : Entity
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) => Enumerable.Repeat(this, 1);
        }
        // Each function and operator processing
        public partial record Sumf
        {
            // x + a = value => x = value - a
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Augend.Contains(x) ? Augend.Invert(value - Addend, x) : Addend.Invert(value - Augend, x);
        }
        public partial record Minusf
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Subtrahend.Contains(x)
                // x - a = value => x = value + a
                ? Subtrahend.Invert(value + Minuend, x)
                // a - x = value => x = a - value
                : Minuend.Invert(value - Subtrahend, x);
        }
        public partial record Mulf
        {
            // x * a = value => x = value / a
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Multiplier.Contains(x)
                ? Multiplier.Invert(value / Multiplicand, x)
                : Multiplicand.Invert(value / Multiplier, x);
        }
        public partial record Divf
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Dividend.Contains(x)
                // x / a = value => x = a * value
                ? Dividend.Invert(value * Divisor, x)
                // a / x = value => x = a / value
                : Divisor.Invert(value / Dividend, x);
        }
        public partial record Powf
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Base.Contains(x)
                ? Exponent is IntegerNumber { Integer: var pow }
                  ? Number.GetAllRoots(1, pow)
                    .SelectMany(root => Base.Invert(root * MathS.Pow(value, 1 / Exponent), x))
                  : Base.Invert(MathS.Pow(value, 1 / Exponent), x)
                // a ^ x = value => x = log(a, value)
                : Exponent.Invert(MathS.Log(Base, value) + 2 * MathS.i * Variable.CreateUnique(this + value, "n") * MathS.pi, x);
        }
        // TODO: Consider case when sin(sin(x)) where double-mention of n occures
        public partial record Sinf
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                // sin(x) = value => x = arcsin(value) + 2pi * n
                Argument.Invert(MathS.Arcsin(value) + 2 * MathS.pi * Variable.CreateUnique(this + value, "n"), x)
                // sin(x) = value => x = pi - arcsin(value) + 2pi * n
                .Concat(Argument.Invert(MathS.pi - MathS.Arcsin(value) + 2 * MathS.pi * Variable.CreateUnique(this + value, "n"), x));
        }
        public partial record Cosf
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                // cos(x) = value => x = arccos(value) + 2pi * n
                Argument.Invert(MathS.Arccos(value) + 2 * MathS.pi * Variable.CreateUnique(this + value, "n"), x)
                // cos(x) = value => x = -arccos(value) + 2pi * n
                .Concat(Argument.Invert(-MathS.Arccos(value) + 2 * MathS.pi * Variable.CreateUnique(this + value, "n"), x));
        }
        public partial record Tanf
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                // tan(x) = value => x = arctan(value) + pi * n
                Argument.Invert(MathS.Arctan(value) + MathS.pi * Variable.CreateUnique(this + value, "n"), x);
        }
        public partial record Cotanf
        {
            // cotan(x) = value => x = arccotan(value) + pi * n
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Argument.Invert(MathS.Arccotan(value) + MathS.pi * Variable.CreateUnique(this + value, "n"), x);
        }

        public partial record Logf
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Base.Contains(x)
                // log(x, a) = value => x = a ^ value
                ? Base.Invert(MathS.Pow(Antilogarithm, value), x)
                // log(a, x) = value => a = x ^ value => x = a ^ (1 / value)
                : Antilogarithm.Invert(MathS.Pow(Base, 1 / value), x);
        }

        public partial record Arcsinf
        {
            private static readonly ComplexNumber From = ComplexNumber.Create(-MathS.DecimalConst.pi / 2, RealNumber.NegativeInfinity.Value);
            private static readonly ComplexNumber To = ComplexNumber.Create(MathS.DecimalConst.pi / 2, RealNumber.PositiveInfinity.Value);
            // arcsin(x) = value => x = sin(value)
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                EntityInBounds(value, From, To) ? Argument.Invert(MathS.Sin(value), x) : Enumerable.Empty<Entity>();
        }
        public partial record Arccosf
        {
            private static readonly ComplexNumber From = ComplexNumber.Create(0, RealNumber.NegativeInfinity.Value);
            private static readonly ComplexNumber To = ComplexNumber.Create(MathS.DecimalConst.pi, RealNumber.PositiveInfinity.Value);
            // arccos(x) = value => x = cos(value)
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                EntityInBounds(value, From, To) ? Argument.Invert(MathS.Cos(value), x) : Enumerable.Empty<Entity>();
        }
        public partial record Arctanf
        {
            private static readonly ComplexNumber From = ComplexNumber.Create(-MathS.DecimalConst.pi / 2, RealNumber.NegativeInfinity.Value);
            private static readonly ComplexNumber To = ComplexNumber.Create(MathS.DecimalConst.pi / 2, RealNumber.PositiveInfinity.Value);
            // arctan(x) = value => x = tan(value)
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                EntityInBounds(value, From, To) ? Argument.Invert(MathS.Tan(value), x) : Enumerable.Empty<Entity>();
        }
        public partial record Arccotanf
        {
            // TODO: Range should exclude Re(z) = 0
            private static readonly ComplexNumber From = ComplexNumber.Create(-MathS.DecimalConst.pi / 2, RealNumber.NegativeInfinity.Value);
            private static readonly ComplexNumber To = ComplexNumber.Create(MathS.DecimalConst.pi / 2, RealNumber.PositiveInfinity.Value);
            // arccotan(x) = value => x = cotan(value)
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                EntityInBounds(value, From, To) ? Argument.Invert(MathS.Cotan(value), x) : Enumerable.Empty<Entity>();
        }
        public partial record Factorialf
        {
            // TODO: Inverse of factorial not implemented yet
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Enumerable.Empty<Entity>();
        }

        public partial record Derivativef
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Expression.Contains(x)
                ? Expression.Invert(MathS.Integral(value, Var, Iterations), x)
                : Enumerable.Empty<Entity>();
        }

        public partial record Integralf
        {
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Expression.Contains(x)
                ? Expression.Invert(MathS.Derivative(value, Var, Iterations), x)
                : Enumerable.Empty<Entity>();
        }

        public partial record Limitf
        {
            // TODO: We can't just do a limit on the inverse function: https://math.stackexchange.com/q/3397326/627798
            private protected override IEnumerable<Entity> Invert_(Entity value, Entity x) =>
                Enumerable.Empty<Entity>();
        }
    }
}

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// Searches for a subtree containing `ent` and being minimal possible size.
        /// For example, for expr = MathS.Sqr(x) + 2 * (MathS.Sqr(x) + 3) the result
        /// will be MathS.Sqr(x) while for MathS.Sqr(x) + x the minimum subtree is x.
        /// Further, it will be used for solving with variable replacing, for example,
        /// there's no pattern for solving equation like sin(x)^2 + sin(x) + 1 = 0,
        /// but we can first solve t^2 + t + 1 = 0, and then root = sin(x).
        /// </summary>
        public static Entity GetMinimumSubtree(Entity expr, Variable x)
        {
            if (!expr.Vars.Contains(x))
                throw new ArgumentException($"{nameof(expr)} must contain {nameof(x)}", nameof(expr));

            // The idea is the following:
            // We must get a subtree that has more occurances than 1,
            // But at the same time it should cover all references to `ent`

            var xs = expr.Count(child => child == x);
            return
                expr
                .TakeWhile(e => e != x) // Requires Entity enumeration to be depth-first!!
                .Where(e => e.Vars.Contains(x))
                .Reverse() // e.g. when expr is sin((x+1)^2)+3, this step results in [x+1, (x+1)^2, sin((x+1)^2), sin((x+1)^2)+3]
                .FirstOrDefault(sub => expr.Count(child => child == sub) * sub.Count(child => child == x) == xs)
                // if `expr` contains 2 `sub`s and `sub` contains 3 `x`s, then there should be 6 `x`s in `expr` (6 == `xs`)
                ?? x;
        }
    }
}

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class AnalyticalSolver
    {
        private static Entity TryDowncast(Entity equation, Variable x, Entity root)
        {
            if (!MathS.CanBeEvaluated(root))
                return root;
            var preciseValue = root.Eval();
            var downcasted = MathS.Settings.FloatToRationalIterCount.As(20, () =>
                MathS.Settings.PrecisionErrorZeroRange.As(1e-7m, () =>
                    ComplexNumber.Create(preciseValue.Real, preciseValue.Imaginary)));
            var errorExpr = equation.Substitute(x, downcasted);
            if (!MathS.CanBeEvaluated(errorExpr))
                return root;
            var error = errorExpr.Eval();

            static bool ComplexRational(ComplexNumber a)
                => a.Real is RationalNumber && a.Imaginary is RationalNumber;

            var innerSimplified = root.InnerSimplify();

            return Number.IsZero(error) && ComplexRational(downcasted) ? downcasted : innerSimplified;
        }


        /// <summary>Equation solver</summary>
        internal static void Solve(Entity expr, Variable x, Set dst)
            => Solve(expr, x, dst, compensateSolving: false);
        internal static void Solve(Entity expr, Variable x, Set dst, bool compensateSolving)
        {
            if (expr == x)
            {
                dst.Add(0);
                return;
            }

            // Applies an attempt to downcast roots
            void DestinationAddRange(Set toAdd)
            {
                toAdd.FiniteApply(ent => TryDowncast(expr, x, ent));
                dst.AddRange(toAdd);
            }

            Set? res = PolynomialSolver.SolveAsPolynomial(expr, x);
            if (res is { })
            {
                res.FiniteApply(e => e.InnerSimplify());
                DestinationAddRange(res);
                return;
            }

            switch (expr)
            {
                case Mulf(var multiplier, var multiplicand):
                    Solve(multiplier, x, dst);
                    Solve(multiplicand, x, dst);
                    return;
                case Divf(var dividend, var divisor):
                    static bool IsSetNumeric(Set a) =>
                        a.Select(piece => piece.LowerBound().Item1).All(MathS.CanBeEvaluated);

                    var zeroNumerators = new Set();
                    Solve(dividend, x, zeroNumerators);
                    if (!IsSetNumeric(zeroNumerators))
                    {
                        dst.AddRange(zeroNumerators);
                        return;
                    }
                    var zeroDenominators = new Set();
                    Solve(divisor, x, zeroDenominators);
                    if (!IsSetNumeric(zeroDenominators))
                    {
                        dst.AddRange(zeroNumerators);
                        return;
                    }
                    dst.AddRange((Set)(zeroNumerators & !zeroDenominators));
                    return;
                case Powf(var @base, _):
                    Solve(@base, x, dst);
                    return;
                case Minusf(var subtrahend, var minuend):
                    if (!minuend.Vars.Contains(x) && compensateSolving)
                    {
                        if (subtrahend == x)
                        {
                            dst.Add(minuend);
                            return;
                        }
                        var subs = 0;
                        Entity? lastChild = null;
                        foreach (var child in subtrahend.DirectChildren)
                        {
                            if (child.Vars.Contains(x))
                            {
                                subs += 1;
                                lastChild = child;
                            }
                        }
                        if (subs != 1 || lastChild is null)
                            break;
                        var resInverted = subtrahend.Invert(minuend, lastChild);
                        foreach (var result in resInverted)
                            Solve(lastChild - result, x, dst, compensateSolving: true);
                        return;
                    }
                    break;
                case Function:
                    DestinationAddRange(expr.Invert(0, x).Aggregate(new Set(), (set, e) => { set.Add(e); return set; }));
                    return;
            }

            // if the replacement isn't one-variable one,
            // then solving over replacements is already useless,
            // so we skip this part and go to other solvers
            if (!compensateSolving)
            {
                // Here we generate a unique variable name
                var newVar =
                    expr.Vars
                    .OrderByDescending(v => v.Name)
                    .First().Name + "quack";
                // // //


                // Here we find all possible replacements
                var replacements = new List<(Entity, Entity)>
                {
                    (TreeAnalyzer.GetMinimumSubtree(expr, x), expr)
                };
                foreach (var alt in expr.Alternate(4).FiniteSet())
                {
                    if (!alt.Vars.Contains(x))
                        return; // in this case there is either 0 or +oo solutions
                    replacements.Add((TreeAnalyzer.GetMinimumSubtree(alt, x), alt));
                }
                // // //

                // Here we find one that has at least one solution

                foreach (var replacement in replacements)
                {
                    if (replacement.Item1 == x)
                        continue;
                    var solutions = replacement.Item2.Substitute(replacement.Item1, newVar).SolveEquation(newVar);
                    if (!solutions.IsEmpty())
                    {
                        var bestReplacement = replacement.Item1;

                        // Here we are trying to solve for this replacement
                        Set newDst = new Set();
                        foreach (var solution in solutions.FiniteSet())
                        {
                            var str = bestReplacement.ToString();
                            // TODO: make a smarter comparison than just comparison of complexities of two expressions
                            // The idea is  
                            // similarToPrevious = ((bestReplacement - solution) - expr).Simplify() == 0
                            // But Simplify costs us too much time
                            var similarToPrevious = (bestReplacement - solution).Complexity >= expr.Complexity;
                            if (!compensateSolving || !similarToPrevious)
                                Solve(bestReplacement - solution, x, newDst, compensateSolving: true);
                        }
                        DestinationAddRange(newDst);
                        if (!dst.IsEmpty())
                            break;
                        // // //
                    }
                }
                // // //
            }

            // if no replacement worked, try trigonometry solver
            if (dst.IsEmpty())
            {
                res = TrigonometricSolver.SolveLinear(expr, x);
                if (res != null)
                {
                    DestinationAddRange(res);
                    return;
                }
            }
            // // //

            // if no trigonometric rules helped, common denominator might help
            if (dst.IsEmpty())
            {
                res = CommonDenominatorSolver.Solve(expr, x);
                if (res != null)
                {
                    DestinationAddRange(res);
                    return;
                }
            }
            // // //


            // if we have fractioned polynomials
            if (dst.IsEmpty())
            {
                res = FractionedPolynoms.Solve(expr, x);
                if (res != null)
                {
                    DestinationAddRange(res);
                    return;
                }
            }
            // // //

            // TODO: Solve factorials (Needs Lambert W function)
            // https://mathoverflow.net/a/28977

            // if nothing has been found so far
            if (dst.IsEmpty() && MathS.Settings.AllowNewton)
                if (expr.Vars.Count == 1)
                    DestinationAddRange(expr.SolveNt(x));
        }
    }
}
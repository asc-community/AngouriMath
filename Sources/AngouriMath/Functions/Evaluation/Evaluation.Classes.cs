//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using System;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Variable
        {
            private protected override Entity IntrinsicCondition => true;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) => !isExact && ConstantList.TryGetValue(Name, out var value) ? value : this;
        }

        /// <summary>
        /// For two-argument nodes
        /// Used in InnerSimplify and InnerEval
        /// Allows to avoid looking over all the combinations with piecewise, tensor, finiteset
        /// </summary>
        /// <param name="left">
        /// Left argument
        /// </param>
        /// <param name="right">
        /// Right argument
        /// </param>
        /// <param name="operation">
        /// That is the main switch for the types. It must return null if no suitable couple of types is found,
        /// so that the method could move on to the matrix choice
        /// </param>
        /// <param name="defaultCtor">
        /// If no suitable case in switch found, it should return the default node, for example, for sum it would be
        /// <code>(a, b) => a + b</code>
        /// </param>
        /// <param name="isExact">
        /// Check if the number is exact and, if so, return it.
        /// </param>
        /// <param name="propagateSet">
        /// Set operations should not be applied on all pairs of elements when it cannot be simplified.
        /// </param>
        /// <param name="settlesNaN">
        /// Whether <paramref name="operation"/> is asked about a <c>NaN</c> operand instead of the
        /// result being <c>NaN</c> outright. A logical connective can settle one -- <c>false and u</c>
        /// is <c>false</c> whatever <c>u</c> is -- and arithmetic cannot, so this is off by default:
        /// <c>NaN * 0</c> must not become <c>0</c> just because a rule for a zero factor exists.
        /// https://github.com/asc-community/AngouriMath/issues/880
        /// </param>
        private Entity ExpandOnTwoArguments(
            Entity left,
            Entity right,
            Func<Entity, Entity, Entity?> operation,
            Func<Entity, Entity, Entity, Entity> defaultCtor,
            bool isExact,
            bool propagateSet = true,
            bool settlesNaN = false)
        {
            if (isExact && this.Evaled is (Number { IsExact: true } or Boolean) and var n)
                return n;
            left = left.InnerSimplified(isExact);
            right = right.InnerSimplified(isExact);
            if (left.IsNaN || right.IsNaN)
            {
                // A connective gets first refusal on an undefined operand, and hands back null where
                // it cannot settle the case, which is what falls through to NaN here. Its own table
                // is already the three-valued one: `and` reads (_, false) as false and (true, _) as
                // its right operand, so a NaN that genuinely decides nothing stays NaN by arriving
                // back out of the switch.
                if (settlesNaN && operation(left, right) is { } settled)
                    return settled;
                return MathS.NaN;
            }

            if (operation(left, right) is { } preRes)
                return preRes;

            Entity ops(Entity a, Entity b)
            {
                if (operation(a, b) is { } res)
                    return res;
                if (isExact && defaultCtor(this, a, b).Evaled is Number { IsExact: true } n)
                    return n;
                return defaultCtor(this, a, b);
            }

            return (left, right) switch
            {
                (Providedf a, Providedf b) => ops(a.Expression, b.Expression).Provided(a.Predicate & b.Predicate),
                (Providedf a, var b) => ExpandOnTwoArguments(a.Expression, b, operation, defaultCtor, isExact).Provided(a.Predicate),
                (var a, Providedf b) => ExpandOnTwoArguments(a, b.Expression, operation, defaultCtor, isExact).Provided(b.Predicate),
                (Piecewise a, Piecewise b) =>
                    MathS.Piecewise(

                        (a.Cases, b.Cases).EachForEach((c1, c2) =>
                        (
                        ExpandOnTwoArguments(c1.Expression, c2.Expression, operation, defaultCtor, isExact)
                        , (c1.Predicate & c2.Predicate).InnerSimplified).ToProvided()
                        )

                        ),
                (Piecewise a, var b) => a.ApplyToValues(a => ops(a, b)),
                (var a, Piecewise b) => b.ApplyToValues(b => ops(a, b)),
                (Matrix a, Matrix b) => a.InnerMatrix.Shape == b.InnerMatrix.Shape ? a.Elementwise(b, ops) : defaultCtor(this, left, right),
                (Matrix a, var b) => a.Elementwise(a => ops(a, b)),
                (var a, Matrix b) => b.Elementwise(b => ops(a, b)),
                _ => propagateSet ? (left, right) switch
                {
                    (FiniteSet a, FiniteSet b) => new FiniteSet((a, b).EachForEach().Select(s => ops(s.left, s.right))),
                    (FiniteSet a, var b) => a.Apply(a => ops(a, b)),
                    (var a, FiniteSet b) => b.Apply(b => ops(a, b)),
                    _ => defaultCtor(this, left, right)
                } : defaultCtor(this, left, right)
            };
        }

        private Entity ExpandOnOneArgument(Entity expr, Func<Entity, Entity?> operation, Func<Entity, Entity, Entity> defaultCtor, bool isExact,
            bool propagateSet = true)
        {
            if (isExact && this.Evaled is (Number { IsExact: true } or Boolean) and var n)
                return n;

            expr = expr.InnerSimplified(isExact);
            if (operation(expr) is { } notNull)
                return notNull;

            Entity ops(Entity a)
            {
                if (operation(a) is { } res)
                    return res;
                if (isExact && defaultCtor(this, a).Evaled is Number { IsExact: true } n)
                    return n;
                return defaultCtor(this, a);
            }

            return expr switch
            {
                Providedf p => ExpandOnOneArgument(p.Expression, operation, defaultCtor, isExact).Provided(p.Predicate),
                Piecewise p => p.ApplyToValues(ops),
                Matrix t => t.Elementwise(ops),
                _ => propagateSet ? expr switch
                {
                    FiniteSet s => s.Apply(ops),
                    _ => defaultCtor(this, expr)
                } : defaultCtor(this, expr)
            };
        }

        private Entity ExpandOnTwoAndTArguments<T>(Entity left, Entity right, T third, Func<Entity, Entity, T, Entity?> operation, Func<Entity, Entity, Entity, T, Entity> defaultCtor, bool isExact,
            bool propagateSet = true)
        {
            if (isExact && this.Evaled is (Number { IsExact: true } or Boolean) and var n)
                return n;

            left = left.InnerSimplified(isExact);
            right = right.InnerSimplified(isExact);
            if (operation(left, right, third) is { } preRes)
                return preRes;

            Entity ops(Entity a, Entity b)
            {
                if (operation(a, b, third) is { } res)
                    return res;
                if (isExact && defaultCtor(this, a, b, third).Evaled is Number { IsExact: true } n)
                    return n;
                return defaultCtor(this, a, b, third);
            }

            return (left, right, third) switch
            {
                (Providedf a, Providedf b, _) => ops(a.Expression, b.Expression).Provided(a.Predicate & b.Predicate),
                (Providedf a, var b, _) => ExpandOnTwoAndTArguments(a.Expression, b, third, operation, defaultCtor, isExact).Provided(a.Predicate),
                (var a, Providedf b, _) => ExpandOnTwoAndTArguments(a, b.Expression, third, operation, defaultCtor, isExact).Provided(b.Predicate),
                (Piecewise a, Piecewise b, _) =>
                    MathS.Piecewise(

                        (a.Cases, b.Cases).EachForEach((c1, c2) =>
                        (
                        ExpandOnTwoAndTArguments(c1.Expression, c2.Expression, third, operation, defaultCtor, isExact)
                        , (c1.Predicate & c2.Predicate).InnerSimplified).ToProvided()
                        )

                        ),
                (Piecewise a, var b, _) => a.ApplyToValues(a => ops(a, b)),
                (var a, Piecewise b, _) => b.ApplyToValues(b => ops(a, b)),
                (Matrix a, Matrix b, _) => a.InnerMatrix.Shape == b.InnerMatrix.Shape ? a.Elementwise(b, ops) : defaultCtor(this, left, right, third),
                (Matrix a, var b, _) => a.Elementwise(a => ops(a, b)),
                (var a, Matrix b, _) => b.Elementwise(b => ops(a, b)),
                _ => propagateSet ? (left, right, third) switch
                {
                    (FiniteSet a, FiniteSet b, _) => new FiniteSet((a, b).EachForEach().Select(s => ops(s.left, s.right))),
                    (FiniteSet a, var b, _) => a.Apply(a => ops(a, b)),
                    (var a, FiniteSet b, _) => b.Apply(b => ops(a, b)),
                    _ => defaultCtor(this, left, right, third)
                }
                : defaultCtor(this, left, right, third)
            };
        }
    }
}
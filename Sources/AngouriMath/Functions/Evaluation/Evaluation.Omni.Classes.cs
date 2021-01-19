/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Core.Sets;
using AngouriMath.Functions;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        partial record Set
        {
            partial record FiniteSet
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => Apply(el => el.Evaled);

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => Apply(el => el.InnerSimplified);
            }

            partial record Interval
            {
                private Entity IfEqualEndsThenCollapse()
                    => Left.Unpack1Eval() == Right.Unpack1Eval() ? 
                    (
                    LeftClosed && RightClosed ? new FiniteSet(Simplificator.PickSimplest(Left.Unpack1(), Right.Unpack1())) : Empty)
                     : this;

                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => New(Left.Unpack1Eval(), Right.Unpack1Eval()).IfEqualEndsThenCollapse();

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => New(Left.Unpack1Simplify(), Right.Unpack1Simplify()).IfEqualEndsThenCollapse();
            }

            partial record ConditionalSet
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => Simplificator.PickSimplest(New(Var, Predicate.Unpack1Eval()), InnerSimplified);

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                {
                    if (!Predicate.EvaluableBoolean)
                        return New(Var, Predicate.Unpack1Simplify());
                    // so it's either U or {} if the statement is always true or false respectively
                    return Predicate.EvalBoolean() ? Codomain : Set.Empty;
                }
            }

            partial record SpecialSet
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => this;

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => this;
            }

            partial record Unionf
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => InnerSimplified;

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => (Left, Right).Unpack2Simplify() switch
                    {
                        (FiniteSet setLeft, Set setRight) => SetOperators.UniteFiniteSetAndSet(setLeft, setRight),
                        (Set setLeft, FiniteSet setRight) => SetOperators.UniteFiniteSetAndSet(setRight, setLeft),
                        (Interval intLeft, Interval intRight) => SetOperators.UniteIntervalAndInterval(intLeft, intRight),
                        (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.UniteCSetAndCSet(csetLeft, csetRight),
                        (var left, var right) => New(left, right)
                    };
            }

            partial record Intersectionf
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => InnerSimplified;

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => (Left, Right).Unpack2Simplify() switch
                    {
                        (FiniteSet setLeft, Set setRight) => SetOperators.IntersectFiniteSetAndSet(setLeft, setRight),
                        (Set setLeft, FiniteSet setRight) => SetOperators.IntersectFiniteSetAndSet(setRight, setLeft),
                        (Interval intLeft, Interval intRight) => SetOperators.IntersectIntervalAndInterval(intLeft, intRight),
                        (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.IntersectCSetAndCSet(csetLeft, csetRight),
                        (var left, var right) => New(left, right)
                    };
            }

            partial record SetMinusf
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => InnerSimplified;

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => (Left, Right).Unpack2Simplify() switch
                    {
                        (Set setLeft, FiniteSet setRight) => SetOperators.SetSubtractSetAndFiniteSet(setLeft, setRight),
                        (Interval intLeft, Interval intRight) => SetOperators.SetSubtractIntervalAndInterval(intLeft, intRight),
                        (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.SetSubtractCSetAndCSet(csetLeft, csetRight),
                        (var left, var right) => New(left, right)
                    };
            }
        }

        partial record Providedf
        {
            private Providedf SwitchOverChildren(Entity expression, Entity predicate)
                => (expression, predicate) switch
                {
                    (Providedf exprProvided, Providedf predProvided) =>
                        New(exprProvided.Expression, exprProvided.Predicate & predProvided.Predicate & predProvided.Expression),
                    (var expr, Providedf predProvided) =>
                        New(expr, predProvided.Predicate & predProvided.Expression),
                    (Providedf exprProvided, var pred)
                        => New(exprProvided.Expression, pred & exprProvided.Predicate),
                    (var expr, var pred)
                        => New(expr, pred)
                };

            private static Entity DecideWithPredicate(Providedf expr)
            {
                if (expr.Predicate == Boolean.True)
                    return expr.Expression;
                if (expr.Predicate == Boolean.False)
                    return MathS.NaN;
                return expr;
            }

            /// <inheritdoc/>
            protected override Entity InnerEval()
            {
                var evaled = SwitchOverChildren(Expression.Evaled, Predicate.Evaled);
                return DecideWithPredicate(evaled);
            }

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                var simplified = SwitchOverChildren(Expression.InnerSimplified, Predicate.InnerSimplified);
                return DecideWithPredicate(simplified);
            }
        }
    }
}

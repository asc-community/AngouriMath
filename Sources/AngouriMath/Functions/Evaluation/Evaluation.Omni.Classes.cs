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
using System.Collections.Generic;
using System.Linq;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        partial record Set
        {
            // TODO:
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
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => ExpandOnTwoAndTArguments(Left.Evaled, Right.Evaled, (l: LeftClosed, r: RightClosed),
                        (a, b, lr) => (a, b, lr) switch
                        {
                            (var left, var right, _) when left == right => lr.l && lr.r ?
                            new FiniteSet(Simplificator.PickSimplest(a, b)) :
                            Empty,
                            _ => null
                        },
                        (@this, a, b, lr) => ((Interval)@this).New(a, lr.l, b, lr.r),
                        false);

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => ExpandOnTwoAndTArguments(Left.InnerSimplified, Right.InnerSimplified, (l: LeftClosed, r: RightClosed),
                        (a, b, lr) => (a, b, lr) switch
                        {
                            (var left, var right, _) when left.Evaled == right.Evaled => lr.l && lr.r ?
                            new FiniteSet(Simplificator.PickSimplest(left, right)) :
                            Empty,
                            _ => null
                        },
                        (@this, a, b, lr) => ((Interval)@this).New(a, lr.l, b, lr.r),
                        true);
            }

            partial record ConditionalSet
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => ExpandOnTwoAndTArguments(Var, Predicate.Evaled, this,
                        (@var, pred, @this) => 
                    Simplificator.PickSimplest(@this.New(@var, pred), @this),
                        (@this, @var, pred, _) => ((ConditionalSet)@this).New(@var, pred)
                        );

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => ExpandOnTwoAndTArguments(Var, Predicate.InnerSimplified, Codomain,
                        (a, b, cod) => (a, b, cod) switch
                        {
                            (var v, var pred, var codom) when pred.EvaluableBoolean && (bool)pred.EvalBoolean() => codom,
                            (var v, var pred, var codom) when pred.EvaluableBoolean && !(bool)pred.EvalBoolean() => Empty,
                            _ => null
                        },
                        (@this, @var, pred, _) => ((ConditionalSet)@this).New(@var, pred)
                        );
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
                    => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                        (a, b) => (a, b) switch
                        {
                            (FiniteSet setLeft, Set setRight) => SetOperators.UniteFiniteSetAndSet(setLeft, setRight),
                            (Set setLeft, FiniteSet setRight) => SetOperators.UniteFiniteSetAndSet(setRight, setLeft),
                            (Interval intLeft, Interval intRight) => SetOperators.UniteIntervalAndInterval(intLeft, intRight),
                            (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.UniteCSetAndCSet(csetLeft, csetRight),
                            _ => null
                        },
                        (@this, a, b) => ((Unionf)@this).New(a, b)
                        );
            }

            partial record Intersectionf
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => InnerSimplified;

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                        (a, b) => (a, b) switch
                        {
                            (FiniteSet setLeft, Set setRight) => SetOperators.IntersectFiniteSetAndSet(setLeft, setRight),
                            (Set setLeft, FiniteSet setRight) => SetOperators.IntersectFiniteSetAndSet(setRight, setLeft),
                            (Interval intLeft, Interval intRight) => SetOperators.IntersectIntervalAndInterval(intLeft, intRight),
                            (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.IntersectCSetAndCSet(csetLeft, csetRight),
                            _ => null
                        },
                        (@this, a, b) => ((Intersectionf)@this).New(a, b)
                        );
            }

            partial record SetMinusf
            {
                /// <inheritdoc/>
                protected override Entity InnerEval()
                    => InnerSimplified;

                /// <inheritdoc/>
                protected override Entity InnerSimplify()
                    => ExpandOnTwoArguments(Left.InnerSimplified, Right.InnerSimplified,
                        (a, b) => (a, b) switch
                        {
                            (Set setLeft, FiniteSet setRight) => SetOperators.SetSubtractSetAndFiniteSet(setLeft, setRight),
                            (Interval intLeft, Interval intRight) => SetOperators.SetSubtractIntervalAndInterval(intLeft, intRight),
                            (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.SetSubtractCSetAndCSet(csetLeft, csetRight),
                            _ => null
                        },
                        (@this, a, b) => ((SetMinusf)@this).New(a, b)
                        );
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

        partial record Piecewise
        {
            private Entity? ComputePiecewiseResultIfPossible()
            {
                foreach (var oneCase in Cases)
                {
                    if (oneCase.Predicate.Evaled is not Boolean)
                        return null;
                    if (oneCase.Predicate.Evaled == Boolean.True)
                        return oneCase.Expression;
                }
                return MathS.NaN;
            }

            private IEnumerable<Providedf> ProcessPiecewise(IEnumerable<Providedf> source)
            {
                var res = new List<Providedf>();
                foreach (var (@case, srcCase) in (Cases, source).Zip())
                    if (@case.Evaled == Boolean.True)
                    {
                        res.Add(srcCase);
                        break;
                    }
                    else if (@case.Evaled != Boolean.False)
                        res.Add(srcCase);
                return res;
            }

            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ComputePiecewiseResultIfPossible() is { } expr
                ?
                expr.Evaled
                :
                New(ProcessPiecewise(Cases.Select(c => c.New(c.Expression.Evaled, c.Predicate.Evaled))));

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ComputePiecewiseResultIfPossible() is { } expr 
                ?
                expr.InnerSimplified
                :
                New(ProcessPiecewise(Cases.Select(c => c.New(c.Expression.InnerSimplified, c.Predicate.InnerSimplified))));
        }
    }
}

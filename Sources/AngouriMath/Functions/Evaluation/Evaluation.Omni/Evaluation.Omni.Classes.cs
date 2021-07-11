/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using AngouriMath.Core.Sets;

namespace AngouriMath
{
    partial record Entity
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
                    if (@case.Predicate.Evaled == Boolean.True)
                    {
                        res.Add(srcCase);
                        break;
                    }
                    else if (@case.Predicate.Evaled != Boolean.False)
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


        partial record Matrix
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => IsScalar ? AsScalar().Evaled :
                Elementwise(e => e.Evaled);

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => IsScalar ? AsScalar().InnerSimplified :
                Elementwise(e => e.InnerSimplified);
        }
    }
}

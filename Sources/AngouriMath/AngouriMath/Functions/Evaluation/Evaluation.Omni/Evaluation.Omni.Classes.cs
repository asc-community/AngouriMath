//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Sets;
using System;

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

        partial record Application
        {
            private static Entity ApplyOthersIfNeeded(Entity outer, LList<Entity> arguments)
                => arguments switch
                {
                    LEmpty<Entity> => outer,
                    var nonEmpty => outer.Apply(nonEmpty)
                };

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ((Expression.InnerSimplified, Arguments.Map(arg => arg.InnerSimplified)) switch
                {
                    (var identifier, LEmpty<Entity>) => identifier,
                    (Application(var any, var argsInner), var argsOuter) => any.Apply(argsInner.Concat(argsOuter).ToLList()),

                    (Variable("sin"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Sin(), otherArgs),
                    (Variable("cos"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Cos(), otherArgs),
                    (Variable("tan"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Tan(), otherArgs),
                    (Variable("cotan" or "cot"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Cotan(), otherArgs),
                    (Variable("sec"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Sec(), otherArgs),
                    (Variable("cosec" or "csc"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Cosec(), otherArgs),
                    (Variable("arcsin" or "asin"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Arcsin(), otherArgs),
                    (Variable("arccos" or "acos"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Arccos(), otherArgs),
                    (Variable("arctan" or "atan"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Arctan(), otherArgs),
                    (Variable("arccotan" or "acotan" or "acot" or "arccot"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Arccotan(), otherArgs),
                    (Variable("arcsec" or "asec"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Arcsec(), otherArgs),
                    (Variable("arccosec" or "arccsc" or "acsc" or "acosec"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Arccosec(), otherArgs),

                    (Variable("sinh" or "sh"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Sinh(x), otherArgs),
                    (Variable("cosh" or "ch"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Cosh(x), otherArgs),
                    (Variable("tanh" or "th"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Tanh(x), otherArgs),
                    (Variable("cotanh" or "coth" or "cth"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Cotanh(x), otherArgs),
                    (Variable("sech" or "sch"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Sech(x), otherArgs),
                    (Variable("cosech" or "csch"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Cosech(x), otherArgs),
                    (Variable("asinh" or "arsinh" or "arsh"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Arsinh(x), otherArgs),
                    (Variable("acosh" or "arcosh" or "arch"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Arcosh(x), otherArgs),
                    (Variable("atanh" or "artanh" or "arth"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Artanh(x), otherArgs),
                    (Variable("acoth" or "arcoth" or "acotanh" or "arcotanh" or "arcth"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Arcotanh(x), otherArgs),
                    (Variable("asech" or "arsech" or "arsch"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Arsech(x), otherArgs),
                    (Variable("acosech" or "arcosech" or "arcsch" or "acsch"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Hyperbolic.Arcosech(x), otherArgs),

                    (Variable("gamma"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Gamma(x), otherArgs),
                    (Variable("phi"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.PhiFunction(), otherArgs),
                    (Variable("abs"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Abs(), otherArgs),
                    (Variable("sqrt"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Sqrt(x), otherArgs),
                    (Variable("cbrt"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Cbrt(x), otherArgs),
                    (Variable("sqr"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Sqr(x), otherArgs),
                    (Variable("signum" or "sign" or "sgn"), (var x, var otherArgs)) => ApplyOthersIfNeeded(x.Signum(), otherArgs),

                    (Variable("ln"), (var x, var otherArgs)) => ApplyOthersIfNeeded(MathS.Ln(x), otherArgs),
                    (Variable("log") v, (var x, LEmpty<Entity>) args) => New(v, args),
                    (Variable("log"), (var p, (var x, var otherArgs))) => ApplyOthersIfNeeded(MathS.Log(p, x), otherArgs),

                    (Variable("derivative") v, (var expr, LEmpty<Entity>) args) => New(v, args),
                    (Variable("derivative"), (var expr, (var x, var otherArgs))) => ApplyOthersIfNeeded(MathS.Derivative(expr, x), otherArgs),

                    (Variable("integral") v, (_, LEmpty<Entity>) args) => New(v, args),
                    (Variable("integral"), (var expr, (var x, var otherArgs))) => ApplyOthersIfNeeded(MathS.Integral(expr, x), otherArgs),

                    (Variable("limit") v,  ((_, LEmpty<Entity>) or (_, (_, LEmpty<Entity>))) and var args) => New(v, args),
                    (Variable("limit") v, (var expr, (var x, (var to, var otherArgs)))) => ApplyOthersIfNeeded(MathS.Limit(expr, x, to), otherArgs),

                    (Variable("limitleft") v, ((_, LEmpty<Entity>) or (_, (_, LEmpty<Entity>))) and var args) => New(v, args),
                    (Variable("limitleft") v, (var expr, (var x, (var to, var otherArgs)))) => ApplyOthersIfNeeded(MathS.Limit(expr, x, to, ApproachFrom.Left), otherArgs),

                    (Variable("limitright") v, ((_, LEmpty<Entity>) or (_, (_, LEmpty<Entity>))) and var args) => New(v, args),
                    (Variable("limitright") v, (var expr, (var x, (var to, var otherArgs)))) => ApplyOthersIfNeeded(MathS.Limit(expr, x, to, ApproachFrom.Right), otherArgs),

                    (Lambda(var x, var body), (var arg, var otherArgs)) => ApplyOthersIfNeeded(body.Substitute(x, arg), otherArgs),

                    (var exprSimplified, var argsSimplified) => New(exprSimplified, argsSimplified),
                }) switch
                {
                    var thisAgain when ReferenceEquals(thisAgain, this) => this,
                    var newOne => newOne.InnerSimplified
                };

            /// <inheritdoc/>
            protected override Entity InnerEval()
                => InnerSimplify();
        }

        partial record Lambda
        {
            private static LList<Entity>? ReduceArgList(LList<Entity> args, Variable toReduce)
                => args switch
                {
                    LEmpty<Entity> => null,
                    (var curr, LEmpty<Entity>) when curr == toReduce => LList<Entity>.Empty,
                    (_, LEmpty<Entity>) => null,
                    (var curr, var rest) when curr.FreeVariables.Contains(toReduce) => null,
                    (var curr, var rest) =>
                        (ReduceArgList(rest, toReduce) is { } list)
                        ? curr + list
                        : null
                };
            
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => (Parameter, Body.InnerSimplified) switch
                {
                    (var x1, Application(var expr, var args))
                        when !expr.FreeVariables.Contains(x1)
                        && ReduceArgList(args, x1) is { } newArgList
                            => newArgList switch
                            {
                                LEmpty<Entity> => expr,
                                var rest => new Application(expr, rest)
                            },
                    (var x, var body) when body != Body => new Lambda(x, body).InnerSimplified,
                    _ => this
                };
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => New(Parameter, Body.Evaled);
        }
    }
}

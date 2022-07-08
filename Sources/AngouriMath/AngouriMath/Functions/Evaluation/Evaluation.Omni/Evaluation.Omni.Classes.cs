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
                    LEmpty<Entity> or null => outer,
                    var nonEmpty => outer.Apply(nonEmpty)
                };

            internal static (Entity Applied, LList<Entity> RemainingArgs)? KeywordToApplied(string kw, LList<Entity> arguments, out bool badArgument, out bool applied)
                => (kw, arguments).Let(out badArgument, false).Let(out applied, true) switch
                {
                    (("sin"), (var x, var otherArgs)) => (x.Sin(), otherArgs),
                    (("cos"), (var x, var otherArgs)) => (x.Cos(), otherArgs),
                    (("tan"), (var x, var otherArgs)) => (x.Tan(), otherArgs),
                    (("cotan" or "cot"), (var x, var otherArgs)) => (x.Cotan(), otherArgs),
                    (("sec"), (var x, var otherArgs)) => (x.Sec(), otherArgs),
                    (("cosec" or "csc"), (var x, var otherArgs)) => (x.Cosec(), otherArgs),
                    (("arcsin" or "asin"), (var x, var otherArgs)) => (x.Arcsin(), otherArgs),
                    (("arccos" or "acos"), (var x, var otherArgs)) => (x.Arccos(), otherArgs),
                    (("arctan" or "atan"), (var x, var otherArgs)) => (x.Arctan(), otherArgs),
                    (("arccotan" or "acotan" or "acot" or "arccot"), (var x, var otherArgs)) => (x.Arccotan(), otherArgs),
                    (("arcsec" or "asec"), (var x, var otherArgs)) => (x.Arcsec(), otherArgs),
                    (("arccosec" or "arccsc" or "acsc" or "acosec"), (var x, var otherArgs)) => (x.Arccosec(), otherArgs),

                    (("sinh" or "sh"), (var x, var otherArgs)) => (MathS.Hyperbolic.Sinh(x), otherArgs),
                    (("cosh" or "ch"), (var x, var otherArgs)) => (MathS.Hyperbolic.Cosh(x), otherArgs),
                    (("tanh" or "th"), (var x, var otherArgs)) => (MathS.Hyperbolic.Tanh(x), otherArgs),
                    (("cotanh" or "coth" or "cth"), (var x, var otherArgs)) => (MathS.Hyperbolic.Cotanh(x), otherArgs),
                    (("sech" or "sch"), (var x, var otherArgs)) => (MathS.Hyperbolic.Sech(x), otherArgs),
                    (("cosech" or "csch"), (var x, var otherArgs)) => (MathS.Hyperbolic.Cosech(x), otherArgs),
                    (("asinh" or "arsinh" or "arsh"), (var x, var otherArgs)) => (MathS.Hyperbolic.Arsinh(x), otherArgs),
                    (("acosh" or "arcosh" or "arch"), (var x, var otherArgs)) => (MathS.Hyperbolic.Arcosh(x), otherArgs),
                    (("atanh" or "artanh" or "arth"), (var x, var otherArgs)) => (MathS.Hyperbolic.Artanh(x), otherArgs),
                    (("acoth" or "arcoth" or "acotanh" or "arcotanh" or "arcth"), (var x, var otherArgs)) => (MathS.Hyperbolic.Arcotanh(x), otherArgs),
                    (("asech" or "arsech" or "arsch"), (var x, var otherArgs)) => (MathS.Hyperbolic.Arsech(x), otherArgs),
                    (("acosech" or "arcosech" or "arcsch" or "acsch"), (var x, var otherArgs)) => (MathS.Hyperbolic.Arcosech(x), otherArgs),

                    (("gamma"), (var x, var otherArgs)) => (MathS.Gamma(x), otherArgs),
                    (("phi"), (var x, var otherArgs)) => (x.PhiFunction(), otherArgs),
                    (("abs"), (var x, var otherArgs)) => (x.Abs(), otherArgs),
                    (("sqrt"), (var x, var otherArgs)) => (MathS.Sqrt(x), otherArgs),
                    (("cbrt"), (var x, var otherArgs)) => (MathS.Cbrt(x), otherArgs),
                    (("sqr"), (var x, var otherArgs)) => (MathS.Sqr(x), otherArgs),
                    (("signum" or "sign" or "sgn"), (var x, var otherArgs)) => (x.Signum(), otherArgs),

                    (("ln"), (var x, var otherArgs)) => (MathS.Ln(x), otherArgs),
                    (("log"), (var x, LEmpty<Entity>) args) => (new Application(Variable.CreateVariableUnchecked("log"), args), LList<Entity>.Empty).Let(out applied, false),
                    (("log"), (var p, (var x, var otherArgs))) => (MathS.Log(p, x), otherArgs),

                    (("derivative"), (var expr, LEmpty<Entity>) args) => (new Application(Variable.CreateVariableUnchecked("derivative"), args), LList<Entity>.Empty).Let(out applied, false),
                    (("derivative"), (var expr, (var x, (Integer power, var otherArgs)))) => (MathS.Derivative(expr, x, (int)power), otherArgs),
                    (("derivative"), (var expr, (var x, (_, var otherArgs)))) => (MathS.Derivative(expr, x), otherArgs).Let(out badArgument, true),
                    (("derivative"), (var expr, (var x, var otherArgs))) => (MathS.Derivative(expr, x), otherArgs),

                    (("integral"), (_, LEmpty<Entity>) args) => (new Application(Variable.CreateVariableUnchecked("integral"), args), LList<Entity>.Empty).Let(out applied, false),
                    (("integral"), (var expr, (var x, (Integer power, var otherArgs)))) => (MathS.Integral(expr, x, (int)power), otherArgs),
                    (("integral"), (var expr, (var x, (_, var otherArgs)))) => (MathS.Integral(expr, x), otherArgs).Let(out badArgument, true),
                    (("integral"), (var expr, (var x, var otherArgs))) => (MathS.Integral(expr, x), otherArgs),

                    (("limit"),  ((_, LEmpty<Entity>) or (_, (_, LEmpty<Entity>))) and var args) => (new Application(Variable.CreateVariableUnchecked("limit"), args), LList<Entity>.Empty).Let(out applied, false),
                    (("limit"), (var expr, (var x, (var to, var otherArgs)))) => (MathS.Limit(expr, x, to), otherArgs),

                    (("limitleft"), ((_, LEmpty<Entity>) or (_, (_, LEmpty<Entity>))) and var args) => (new Application(Variable.CreateVariableUnchecked("limitleft"), args), LList<Entity>.Empty).Let(out applied, false),
                    (("limitleft"), (var expr, (var x, (var to, var otherArgs)))) => (MathS.Limit(expr, x, to, ApproachFrom.Left), otherArgs),

                    ("limitright", ((_, LEmpty<Entity>) or (_, (_, LEmpty<Entity>))) and var args) => (new Application(Variable.CreateVariableUnchecked("limitright"), args), LList<Entity>.Empty).Let(out applied, false),
                    ("limitright", (var expr, (var x, (var to, var otherArgs)))) => (MathS.Limit(expr, x, to, ApproachFrom.Right), otherArgs),
                    
                    ("domain", (var x, LEmpty<Entity>) args) => (new Application(Variable.CreateVariableUnchecked("domain"), args), LList<Entity>.Empty).Let(out applied, false),
                    ("domain", (var expr, (Set.SpecialSet ss, var otherArgs))) => (expr.WithCodomain(ss.ToDomain()), otherArgs),
                    ("domain", (var x, var _) args) => (new Application(Variable.CreateVariableUnchecked("domain"), args), LList<Entity>.Empty).Let(out badArgument, true).Let(out applied, false),
                    
                    _ => null
                };

            private (Entity Result, bool RerunAgain) InnerProcess(Entity expr, LList<Entity> allArgs)
                => (expr, allArgs) switch
                    {
                        (var identifier, LEmpty<Entity>)
                            => (identifier, false),
                            
                        (Application(var any, var argsInner), var argsOuter)
                            => (any.Apply(argsInner.Concat(argsOuter).ToLList()), true),

                        (Variable(var name), var arguments) when KeywordToApplied(name, arguments, out _, out var isApplied) is var (applied, args)
                            => (ApplyOthersIfNeeded(applied, args), isApplied),

                        (Lambda(var x, var body), (var arg, var otherArgs))
                            => (ApplyOthersIfNeeded(body.Substitute(x, arg), otherArgs), true),

                        (var exprSimplified, var argsSimplified)
                            => (New(exprSimplified, argsSimplified), false)
                    };

            /// <inheritdoc/>
            protected override Entity InnerSimplify()
            {
                var (expr, toRunOuter) = InnerProcess(Expression.InnerSimplified, Arguments.Map(arg => arg.InnerSimplified));
                if (toRunOuter)
                    return expr.InnerSimplified;
                return expr;
            }

            /// <inheritdoc/>
            protected override Entity InnerEval()
            {
                var (expr, toRunOuter) = InnerProcess(Expression.Evaled, Arguments.Map(arg => arg.Evaled));
                if (toRunOuter)
                    return expr.Evaled;
                return expr;
            }
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

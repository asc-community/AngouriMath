//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Sets;
using System;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Set
        {
            // TODO:
            partial record FiniteSet
            {
                private protected override Entity IntrinsicCondition => Boolean.True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => Apply(el => el.InnerSimplified(isExact));
            }

            partial record Interval
            {
                private protected override Entity IntrinsicCondition => Boolean.True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => ExpandOnTwoAndTArguments(Left, Right, (l: LeftClosed, r: RightClosed),
                        (a, b, lr) => (a, b, lr) switch
                        {
                            (var left, var right, _) when (isExact ? left.Evaled == right.Evaled : left == right) =>
                                lr.l && lr.r
                                ? new FiniteSet(Simplificator.PickSimplest(left, right))
                                : Empty,
                            _ => null
                        },
                        (@this, a, b, lr) => ((Interval)@this).New(a, lr.l, b, lr.r), isExact); // NOTE: Intervals propagate set unlike other set operations
            }

            partial record ConditionalSet
            {
                private protected override Entity IntrinsicCondition => Boolean.True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                {
                    // Deliberately not ExpandOnTwoAndTArguments: this node binds Var, and that
                    // helper lifts a Providedf out of an argument onto the whole expression.
                    // A predicate's condition is a statement about the bound variable, so
                    // lifting it puts Var outside its own binder -- `{ x : 1/x = 0 }` came back
                    // as `{ } provided not x = 0`, where the x named in the condition is no
                    // longer the x the set ranges over.
                    // https://github.com/asc-community/AngouriMath/issues/878

                    // A calculus operator taken over some other name reads this one as free of
                    // it -- derivative(y, x) is 0 because y is not x. Under a binder over y that
                    // reading is not available: y ranges over values here, and expressions in x
                    // are among them. Simplifying anyway settles a condition that was written to
                    // stay open: `{ y : derivative(y, x) + y - x = 0 }` became `{ y : y - x = 0 }`,
                    // which names x, and putting x back into the condition it came from gives 1.
                    // https://github.com/asc-community/AngouriMath/issues/964
                    if (Var is Variable bound && CalculusOperator.NamesAssumedFreeOf(Predicate, bound).Count > 0)
                        return this;

                    var predicate = Predicate.InnerSimplified(isExact);

                    // `expr provided a provided b` is what the rules build where two operands
                    // each need a condition, so the chain is read to the end rather than one
                    // layer down.
                    var condition = (Entity)Boolean.True;
                    var core = predicate;
                    while (core is Providedf(var inner, var predicated))
                    {
                        condition = condition == Boolean.True ? predicated : condition & predicated;
                        core = inner;
                    }

                    // Membership is the predicate holding, so anything short of True admits
                    // nothing: a predicate that is False where it is defined and undefined
                    // elsewhere -- which is what a condition on False says -- has no members.
                    // Where the predicate is True under a condition, the members are exactly
                    // the points the condition admits, and it becomes the predicate rather than
                    // escaping to the outside of the set.
                    return core.Evaled switch
                    {
                        Boolean(false) => Empty,
                        Boolean(true) when condition != Boolean.True => New(Var, condition),
                        // No set names every value a symbol could take, so a predicate that
                        // holds everywhere is left as written rather than asking for the set of
                        // Domain.Any, which does not exist.
                        Boolean(true) => Codomain is AngouriMath.Core.Domain.Any
                            ? New(Var, Boolean.True)
                            : SpecialSet.Create(Codomain),
                        _ => New(Var, predicate)
                    };
                }
            }

            partial record SpecialSet
            {
                private protected override Entity IntrinsicCondition => Boolean.True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => this;
            }

            partial record Unionf
            {
                private protected override Entity IntrinsicCondition => Boolean.True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => ExpandOnTwoArguments(Left, Right,
                        (a, b) => (a, b) switch
                        {
                            (FiniteSet setLeft, Set setRight) => SetOperators.UniteFiniteSetAndSet(setLeft, setRight),
                            (Set setLeft, FiniteSet setRight) => SetOperators.UniteFiniteSetAndSet(setRight, setLeft),
                            (Interval intLeft, Interval intRight) => SetOperators.UniteIntervalAndInterval(intLeft, intRight),
                            (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.UniteCSetAndCSet(csetLeft, csetRight),
                            _ => null
                        },
                        (@this, a, b) => ((Unionf)@this).New(a, b), isExact, propagateSet: false);
            }

            partial record Intersectionf
            {
                private protected override Entity IntrinsicCondition => Boolean.True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => ExpandOnTwoArguments(Left, Right,
                        (a, b) => (a, b) switch
                        {
                            (FiniteSet setLeft, Set setRight) => SetOperators.IntersectFiniteSetAndSet(setLeft, setRight),
                            (Set setLeft, FiniteSet setRight) => SetOperators.IntersectFiniteSetAndSet(setRight, setLeft),
                            (Interval intLeft, Interval intRight) => SetOperators.IntersectIntervalAndInterval(intLeft, intRight),
                            (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.IntersectCSetAndCSet(csetLeft, csetRight),
                            _ => null
                        },
                        (@this, a, b) => ((Intersectionf)@this).New(a, b), isExact, propagateSet: false);
            }

            partial record SetMinusf
            {
                private protected override Entity IntrinsicCondition => Boolean.True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => ExpandOnTwoArguments(Left, Right,
                        (a, b) => (a, b) switch
                        {
                            (Set setLeft, FiniteSet setRight) => SetOperators.SetSubtractSetAndFiniteSet(setLeft, setRight),
                            (Interval intLeft, Interval intRight) => SetOperators.SetSubtractIntervalAndInterval(intLeft, intRight),
                            (ConditionalSet csetLeft, ConditionalSet csetRight) => SetOperators.SetSubtractCSetAndCSet(csetLeft, csetRight),
                            _ => null
                        },
                        (@this, a, b) => ((SetMinusf)@this).New(a, b), isExact, propagateSet: false);
            }
        }

        partial record Providedf
        {
            private protected override Entity IntrinsicCondition => Predicate;
            private Entity Decide(Entity expr, Entity predicate)
            {
                if (predicate.Evaled == Boolean.True)
                    return expr;
                if (predicate.Evaled == Boolean.False || predicate.Evaled.IsNaN)
                    return MathS.NaN;
                return New(expr, predicate);
            }
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoArguments(Expression, Predicate,
                    (a, b) => (a, b) switch
                    {
                        (Providedf exprProvided, Providedf predProvided) =>
                            Decide(exprProvided.Expression, exprProvided.Predicate & predProvided.Predicate & predProvided.Expression),
                        (var expr, Providedf predProvided) => Decide(expr, predProvided.Predicate & predProvided.Expression),
                        (Providedf exprProvided, var pred) => Decide(exprProvided.Expression, pred & exprProvided.Predicate),
                        (var expr, var pred) => Decide(expr, pred),
                    },
                    (@this, a, b) => ((Providedf)@this).New(a, b), isExact);
        }

        partial record Piecewise
        {
            private protected override Entity IntrinsicCondition =>
                Cases.Aggregate((Entity?)null, (acc, curr) => acc is { } ? acc | curr.Predicate : curr.Predicate) ?? Boolean.False;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
            {
                foreach (var oneCase in Cases)
                {
                    if (oneCase.Predicate.Evaled is not Boolean) goto notYetDecidable;
                    if (oneCase.Predicate.Evaled == Boolean.True) return oneCase.Expression.InnerSimplified(isExact);
                }
                return MathS.NaN;
            notYetDecidable:
                var res = new List<Providedf>();
                foreach (var (@case, srcCase) in (Cases, Cases.Select(c => c.New(c.Expression.InnerSimplified(isExact), c.Predicate.InnerSimplified(isExact)))).Zip()) {
                    if (@case.Predicate.Evaled == Boolean.False) continue;
                    var toAdd = srcCase.Expression is Providedf(var inner, var pred) ? new Providedf(inner, (srcCase.Predicate & pred).InnerSimplified(isExact)) : srcCase;

                    // A piecewise takes its first matching case, and both rules below follow
                    // from that alone -- neither needs any predicate to be decidable, which
                    // is what makes them apply where the reduction above does not.
                    // https://github.com/asc-community/AngouriMath/issues/327

                    // A predicate that already guards an earlier case can never reach this
                    // one: wherever it holds, the earlier case is taken.
                    if (res.Any(seen => seen.Predicate == toAdd.Predicate))
                    {
                        // Not `continue` -- a decidably true predicate still ends the list,
                        // and skipping that check here would carry unreachable cases past it.
                        if (@case.Predicate.Evaled == Boolean.True) break;
                        continue;
                    }

                    // Two consecutive cases with one expression are one case guarded by
                    // either predicate. Consecutive is the whole condition: a case with a
                    // different expression sitting between them could be taken instead, and
                    // merging across it would change the value where its predicate holds.
                    if (res.Count > 0 && res[res.Count - 1].Expression == toAdd.Expression)
                        res[res.Count - 1] = new Providedf(toAdd.Expression,
                            (res[res.Count - 1].Predicate | toAdd.Predicate).InnerSimplified(isExact));
                    else
                        res.Add(toAdd);

                    if (@case.Predicate.Evaled == Boolean.True) break;
                }
                return New(res);
            }
        }


        partial record Matrix
        {
            private protected override Entity IntrinsicCondition => Boolean.True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => IsScalar ? AsScalar().InnerSimplified(isExact) :
                Elementwise(e => e.InnerSimplified(isExact));
        }

        partial record Application
        {
            private protected override Entity IntrinsicCondition => Boolean.True;
            private static Entity ApplyOthersIfNeeded(Entity outer, LList<Entity> arguments)
                => arguments switch
                {
                    LEmpty<Entity> => outer,
                    var nonEmpty => outer.Apply(nonEmpty)
                };

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ((Expression.InnerSimplified(isExact), Arguments.Map(arg => arg.InnerSimplified(isExact))) switch
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
                    var newOne => newOne.InnerSimplified(isExact)
                };
        }

        partial record Lambda
        {
            private protected override Entity IntrinsicCondition => Boolean.True;
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
            protected override Entity InnerSimplify(bool isExact)
                => (Parameter, Body.InnerSimplified(isExact)) switch
                {
                    (var x1, Application(var expr, var args))
                        when !expr.FreeVariables.Contains(x1)
                        && ReduceArgList(args, x1) is { } newArgList
                            => newArgList switch
                            {
                                LEmpty<Entity> => expr,
                                var rest => new Application(expr, rest)
                            },
                    (var x, var body) when body != Body => new Lambda(x, body).InnerSimplified(isExact),
                    _ => this
                };
        }
    }
}

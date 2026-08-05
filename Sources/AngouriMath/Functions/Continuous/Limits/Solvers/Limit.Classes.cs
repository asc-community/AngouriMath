//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    using AngouriMath.Core.Exceptions;
    using AngouriMath.Core.Multithreading;
    using Core;
    using System.Linq;
    using static Functions.Algebra.LimitFunctional;
    partial record Entity
    {
        partial record Number
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) => this;
        }

        partial record Variable
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side);
        }

        partial record Matrix
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                null; // TODO
        }

        partial record Sumf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (ComputeLimitImpl(this, x, dist, side) is { } lim)
                    return lim;
                var (augend, addend) =
                    (Augend.ComputeLimitDivideEtImpera(x, dist, side), Addend.ComputeLimitDivideEtImpera(x, dist, side)) switch
                    {
                        ({ } lim1, { } lim2) when IsDeterminate(New(lim1, lim2), x) => (lim1, lim2),
                        var (lim1, lim2) => (lim1 is { IsFinite: true } ? lim1 : Augend,
                                             lim2 is { IsFinite: true } ? lim2 : Addend)
                    };
                return ComputeLimitImpl(New(augend, addend), x, dist, side);
            }
        }

        partial record Minusf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (ComputeLimitImpl(this, x, dist, side) is { } lim)
                    return lim;
                var (minuend, subtrahend) =
                    (Minuend.ComputeLimitDivideEtImpera(x, dist, side), Subtrahend.ComputeLimitDivideEtImpera(x, dist, side)) switch
                    {
                        ({ } lim1, { } lim2) when IsDeterminate(New(lim1, lim2), x) => (lim1, lim2),
                        var (lim1, lim2) => (lim1 is { IsFinite: true } ? lim1 : Minuend,
                                             lim2 is { IsFinite: true } ? lim2 : Subtrahend)
                    };
                return ComputeLimitImpl(New(minuend, subtrahend), x, dist, side);
            }
        }

        partial record Mulf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (ComputeLimitImpl(this, x, dist, side) is { } lim)
                    return lim;
                else
                {
                    var (mp, md) =
                        (Multiplier.ComputeLimitDivideEtImpera(x, dist, side), Multiplicand.ComputeLimitDivideEtImpera(x, dist, side)) switch
                        {
                            ({ } lim1, { } lim2) when IsDeterminate(New(lim1, lim2), x) => (lim1, lim2),
                            ({ IsFinite: true } lim1, { IsFinite: true } lim2) => (lim1, lim2),
                            (_, { } l2) when !Multiplier.ContainsNode(x) => (Multiplier, l2),
                            ({ } l1, _) when !Multiplicand.ContainsNode(x) => (l1, Multiplicand),
                            ({ IsFinite: true } lim1, { } exp) => (lim1, exp),
                            ({ } bas, { IsFinite: true } lim2) => (bas, lim2),
                            _ => (Multiplier, Multiplicand)
                        };
                    return ComputeLimitImpl(New(mp, md), x, dist, side);
                }
            }
        }

        partial record Divf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (ComputeLimitImpl(this, x, dist, side) is { } lim)
                    return lim;
                else
                {

                    var (dividend, divisor) =
                        (Dividend.ComputeLimitDivideEtImpera(x, dist, side), Divisor.ComputeLimitDivideEtImpera(x, dist, side)) switch
                        {
                            ({ } lim1, { } lim2) when IsDeterminate(New(lim1, lim2), x) => (lim1, lim2),
                            ({ } lim1, { } lim2) when lim1.InnerSimplified.IsFinite && lim2.InnerSimplified.IsFinite && lim2.InnerSimplified != 0 => (lim1, lim2),
                            ({ IsFinite: true } lim1, { IsFinite: true } lim2) => (lim1, lim2),
                            (_, { } l2) when !Dividend.ContainsNode(x) => (Dividend, l2),
                            ({ } l1, _) when !Divisor.ContainsNode(x) => (l1, Divisor),
                            ({ IsFinite: true } lim1, { } exp) => (lim1, exp),
                            ({ } bas, { IsFinite: true } lim2) => (bas, lim2),
                            _ => (Dividend, Divisor)
                        };
                    var substituted = ComputeLimitImpl(New(dividend, divisor), x, dist, side);
                    if (substituted is { } found && found.Evaled != MathS.NaN)
                        return found;
                    // Putting the two parts' limits in place of the parts loses the one thing
                    // that decides a quotient whose divisor vanishes: 1 / 0 says nothing about
                    // which side the divisor vanishes from, and so comes back NaN. Only where
                    // nothing above answered, and whatever it did answer is kept if this finds
                    // nothing better.
                    return DivergesAtAVanishingDivisor(Dividend, Divisor, x, dist, side) ?? substituted;
                }
            }
        }

        partial record Modf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (Dividend.ComputeLimitDivideEtImpera(x, dist, side) is not { } dividend
                    || Divisor.ComputeLimitDivideEtImpera(x, dist, side) is not { } divisor)
                    return null;
                var substituted = New(dividend, divisor);
                if (substituted.Evaled is not Number { IsFinite: true } value)
                    return null;
                // The remainder is continuous except where the dividend reaches a non-zero
                // multiple of the divisor, and there it jumps: x % 3 tends to 3 approaching 3
                // from the left and to 0 from the right. Which of the two a one-sided limit
                // takes depends on the direction the dividend crosses in, not only on what it
                // tends to, so the jumps are left unanswered rather than answered with the
                // value at the point -- that value is one of the two one-sided limits and not
                // the other, and is no two-sided limit at all.
                //
                // Zero is not one of those points: the remainder takes the sign of the dividend,
                // so x % 3 is x on either side of 0 and passes through it continuously.
                if (value == 0 && dividend.Evaled != 0)
                    return null;
                return substituted;
            }
        }

        partial record Sinf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Cosf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Secantf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Cosecantf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Arcsecantf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                // Asked in front of the substitution, which for a diverging argument does answer
                // -- with arcsec(+oo), a right angle written as a function of an infinity rather
                // than as the right angle it is.
                if (InverseTrigonometryAtInfinity(this, Argument.ComputeLimitDivideEtImpera(x, dist, side)) is { } atInfinity)
                    return atInfinity;
                return ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                    : ComputeLimitImpl(New(
                        Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                        x, dist, side);
            }
        }

        partial record Arccosecantf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Tanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Cotanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Logf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (ComputeLimitImpl(this, x, dist, side) is { } lim)
                    return lim;
                else
                {
                    var @base = Base.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 && lim1 != 0 ? lim1 : Base;
                    var power = Antilogarithm.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 && lim2 != 0 ? lim2 : Antilogarithm;
                    MultithreadingFunctional.ExitIfCancelled();
                    return ComputeLimitImpl(New(@base, power), x, dist, side);
                }
            }
        }

        partial record Powf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(
                    (Base.ComputeLimitDivideEtImpera(x, dist, side), Exponent.ComputeLimitDivideEtImpera(x, dist, side))
                    switch {
                        ({ IsFinite: true } lim1, { IsFinite: true } lim2) => New(lim1, lim2),
                        (_, { } l2) when !Base.ContainsNode(x) => New(Base, l2),
                        ({ } l1, _) when !Exponent.ContainsNode(x) => New(l1, Exponent),
                        ({ IsFinite: true } lim1, { } exp) => New(lim1, exp),
                        ({ } bas, { IsFinite: true } lim2) => New(bas, lim2),
                        _ => New(Base, Exponent)
                    },
                    x, dist, side);
        }

        partial record Arcsinf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (InverseTrigonometryAtInfinity(this, Argument.ComputeLimitDivideEtImpera(x, dist, side)) is { } atInfinity)
                    return atInfinity;
                return ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                    : ComputeLimitImpl(New(
                        Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                        x, dist, side);
            }
        }

        partial record Arccosf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (InverseTrigonometryAtInfinity(this, Argument.ComputeLimitDivideEtImpera(x, dist, side)) is { } atInfinity)
                    return atInfinity;
                return ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                    : ComputeLimitImpl(New(
                        Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                        x, dist, side);
            }
        }

        partial record Arctanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Arccotanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Factorialf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Derivativef
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Expression.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Expression,
                    Var.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Var),
                    x, dist, side);
        }

        partial record Integralf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Expression.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Expression,
                    Var.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Var,
                    Range is var (from, to)
                    ? (from.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim3 ? lim3 : from,
                       to.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim4 ? lim4 : to)
                    : null),
                    x, dist, side);
        }

        partial record Limitf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Expression.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Expression,
                    Var.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Var,
                    Destination.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim3 ? lim3 : Destination,
                    ApproachFrom),
                x, dist, side);
        }

        partial record Signumf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                if (Argument.ComputeLimitDivideEtImpera(x, dist, side) is not { } argument)
                    return null;
                // The sign is constant on either side of zero, so wherever the argument tends
                // to anything but zero the limit is simply the sign of that -- including the
                // infinities, where it is 1 and -1. At zero it is the one place there is
                // nothing to say: the sign is 1 on one side and -1 on the other, and which one
                // a one-sided limit takes depends on the direction the argument approaches
                // from rather than only on what it tends to.
                //
                // Nothing is returned there rather than an unevaluated limit of this very
                // expression. That is what used to be here, and it does not merely fail to
                // answer -- the two-sided path compares its two one-sided results by evaluating
                // them, evaluating a limit computes it, and computing it arrives back here. The
                // recursion ends by overflowing the stack, which kills the process rather than
                // raising anything a caller could catch:
                // https://github.com/asc-community/AngouriMath/issues/704. Null says the same
                // thing to the caller, which hands back an unevaluated limit of its own.
                if (argument.Evaled is not Number value || value == 0 || value.Evaled == MathS.NaN)
                    return null;
                return new Signumf(argument);
            }
        }

        partial record Absf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
                => Argument.ComputeLimitDivideEtImpera(x, dist, side)?.Abs();
        }

        partial record Providedf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                var lim = Expression.ComputeLimitDivideEtImpera(x, dist, side);
                if (lim is null)
                    return null;
                return New(lim, Predicate);
            }
        }

        partial record Piecewise
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                // Close enough to the destination one case is the whole of the expression, so the
                // limit is that case's limit -- and which case that is is decided the same way
                // evaluation decides it, by the first predicate that holds once every earlier one
                // has failed. A limit of the cases with their predicates carried along was what
                // used to be built here, and it answers nothing: the predicates still speak about
                // x, which the limit has just got rid of.
                foreach (var (expression, predicate) in Cases)
                    switch (HoldsNear(predicate, x, dist, side))
                    {
                        case true: return ComputeLimit(expression, x, dist, side);
                        case false: continue;
                        // A case that may or may not hold near the destination leaves it open
                        // which expression the limit is of, and the ones after it unreachable.
                        default: return null;
                    }
                // Every case fails throughout the neighbourhood, so the expression is undefined
                // on the whole of the way in and there is nothing for it to tend to.
                return MathS.NaN;
            }
        }

        partial record Application
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
                => null;
        }

        partial record Lambda
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
                => null;
        }
    }
}

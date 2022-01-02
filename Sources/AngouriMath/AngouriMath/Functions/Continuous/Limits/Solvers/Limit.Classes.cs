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
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Augend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Augend,
                    Addend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Addend),
                    x, dist, side);
        }

        partial record Minusf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Subtrahend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Subtrahend,
                    Minuend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Minuend),
                    x, dist, side);
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
                            ({ } lim1, { } lim2) when lim1.InnerSimplified.IsFinite && lim2.InnerSimplified.IsFinite && lim2.InnerSimplified != 0 => (lim1, lim2),
                            ({ IsFinite: true } lim1, { IsFinite: true } lim2) => (lim1, lim2),
                            (_, { } l2) when !Dividend.ContainsNode(x) => (Dividend, l2),
                            ({ } l1, _) when !Divisor.ContainsNode(x) => (l1, Divisor),
                            ({ IsFinite: true } lim1, { } exp) => (lim1, exp),
                            ({ } bas, { IsFinite: true } lim2) => (bas, lim2),
                            _ => (Dividend, Divisor)
                        };
                    return ComputeLimitImpl(New(dividend, divisor), x, dist, side);
                }
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
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
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
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        partial record Arccosf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
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
                    Var.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Var),
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
            // TODO:
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
                => new Limitf(this, x, dist, side);
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
                var allLims = Cases.Select(c => (lim: c.Expression.ComputeLimitDivideEtImpera(x, dist, side), pred: c.Predicate));
                if (allLims.Select(c => c.lim is null).Any())
                    return null;
                return New(allLims.Select(
                    c => c.lim is not null ? 
                    new Providedf(c.lim, c.pred) : 
                    throw new AngouriBugException("It's been checked before")));
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

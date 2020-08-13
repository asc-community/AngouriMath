using AngouriMath.Core.Numerix;
using System.Linq;

namespace AngouriMath.Limits
{
    using static Entity;
    public enum ApproachFrom
    {
        BothSides,
        Left,
        Right,
    }
    internal static class LimitFunctional
    {
        private static Entity? SimplifyAndComputeLimitToInfinity(Entity expr, Var x)
        {
            expr = expr.Simplify();
            return ComputeLimitToInfinity(expr, x);
        }

        private static Entity? ComputeLimitToInfinity(Entity expr, Var x)
        {
            var substitutionResult = LimitSolvers.SolveBySubstitution(expr, x);
            if (substitutionResult is { }) return substitutionResult;

            var logarithmResult = LimitSolvers.SolveAsLogarithm(expr, x);
            if (logarithmResult is { }) return logarithmResult;

            var polynomialResult = LimitSolvers.SolveAsPolynomial(expr, x);
            if (polynomialResult is { }) return polynomialResult;

            var polynomialDivisionResult = LimitSolvers.SolvePolynomialDivision(expr, x);
            if (polynomialDivisionResult is { }) return polynomialDivisionResult;

            var logarithmDivisionResult = LimitSolvers.SolveAsLogarithmDivision(expr, x);
            if (logarithmDivisionResult is { }) return logarithmDivisionResult;

            return null;
        }

        public static Entity? ComputeLimit(Entity expr, Var x, Entity dist, ApproachFrom side = ApproachFrom.BothSides) => side switch
        {
            ApproachFrom.Left or ApproachFrom.Right => expr.ComputeLimitDivideEtImpera(x, dist, side),
            ApproachFrom.BothSides =>
                !dist.IsFinite
                // just compute limit with no check for left/right equality
                // here approach left will be ignored anyways, as dist is infinite number
                ? expr.ComputeLimitDivideEtImpera(x, dist, ApproachFrom.Left)
                : expr.ComputeLimitDivideEtImpera(x, dist, ApproachFrom.Left) is { } fromLeft
                  && expr.ComputeLimitDivideEtImpera(x, dist, ApproachFrom.Right) is { } fromRight
                ? fromLeft == fromRight ? fromLeft : RealNumber.NaN
                : null,
            _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(side), (int)side, typeof(ApproachFrom))
        };

        internal static Entity? ComputeLimitImpl(Entity expr, Var x, Entity dist, ApproachFrom side)
        {
            if (!expr.Vars.Contains(x)) return expr;

            switch (dist)
            {
                case RealNumber real when !real.IsFinite:
                    // avoid NaN values as non finite numbers
                    if (real == RealNumber.NaN)
                    {
                        return RealNumber.NaN;
                    }
                    // if x -> -oo just make -x -> +oo
                    if (real == RealNumber.NegativeInfinity)
                    {
                        expr = expr.Substitute(x, -x);
                    }
                    // compute limit for x -> +oo
                    return SimplifyAndComputeLimitToInfinity(expr, x);
                case ComplexNumber complex when !complex.IsFinite:
                    throw new Core.Exceptions.MathSException($"complex infinities are not supported in limits: lim({x} -> {dist}) {expr}");
            }
            // handle side from which the limit is approached
            expr = side switch
            {
                ApproachFrom.Left => expr.Substitute(x, dist - 1 / x),
                ApproachFrom.Right => expr.Substitute(x, dist + 1 / x),
                _ => throw new System.ArgumentOutOfRangeException(nameof(side), side,
                    $"Only {ApproachFrom.Left} and {ApproachFrom.Right} are supported.")
            };
            // lim(x -> 3-) x <=> lim(x -> 0+) 3 - x <=> lim(x -> +oo) 3 - 1 / x
            // lim(x -> 3+) x <=> lim(x -> 0+) 3 + x <=> lim(x -> +oo) 3 + 1 / x
            return SimplifyAndComputeLimitToInfinity(expr, x);
        }
    }
}
namespace AngouriMath
{
    using static Limits.LimitFunctional;
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Divide_and_rule"/>
        /// Divide and rule (Latin: divide et impera), or divide and conquer,
        /// in politics and sociology is gaining and maintaining power by
        /// breaking up larger concentrations of power into pieces that
        /// individually have less power than the one implementing the strategy.
        /// 
        /// In computer science, divide and conquer is an algorithm design paradigm
        /// based on multi-branched recursion. A divide-and-conquer algorithm works
        /// by recursively breaking down a problem into two or more sub-problems of
        /// the same or related type, until these become simple enough to be solved
        /// directly. The solutions to the sub-problems are then combined to give a
        /// solution to the original problem.
        /// </summary>
        // here we try to compute limit for each children and then merge them into limit of whole expression
        // theoretically, for cases such limit (x -> -1) 1 / (x + 1)
        // this method will return NaN, but thanks to replacement of x to an non-definite expression,
        // it is somehow compensated
        internal abstract Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side);
        public partial record Num
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) => this;
        }
        public partial record Var
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side);
        }
        public partial record Tensor
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                null; // TODO
        }
        public partial record Sumf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Sumf(
                    Augend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Augend,
                    Addend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Addend),
                    x, dist, side);
        }
        public partial record Minusf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Minusf(
                    Subtrahend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Subtrahend,
                    Minuend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Minuend),
                    x, dist, side);
        }
        public partial record Mulf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Mulf(
                    Multiplier.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Multiplier,
                    Multiplicand.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Multiplicand),
                    x, dist, side);
        }
        public partial record Divf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Divf(
                    Dividend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Dividend,
                    Divisor.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Divisor),
                    x, dist, side);
        }
        public partial record Sinf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Sinf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Cosf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Cosf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Tanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Tanf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Cotanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Cotanf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Logf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Logf(
                    Base.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Base,
                    Antilogarithm.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Antilogarithm),
                    x, dist, side);
        }
        public partial record Powf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Powf(
                    Base.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Base,
                    Exponent.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Exponent),
                    x, dist, side);
        }
        public partial record Arcsinf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Arcsinf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Arccosf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Arccosf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Arctanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Arctanf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Arccotanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Arccotanf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Factorialf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Factorialf(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }
        public partial record Derivativef
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Derivativef(
                    Expression.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Expression,
                    Variable.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Variable,
                    Iterations.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim3 ? lim3 : Iterations),
                    x, dist, side);
        }
        public partial record Integralf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Integralf(
                    Expression.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Expression,
                    Variable.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Variable,
                    Iterations.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim3 ? lim3 : Iterations),
                    x, dist, side);
        }
        public partial record Limitf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Var x, Entity dist, Limits.ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(new Integralf(
                    Expression.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Expression,
                    Variable.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Variable,
                    Destination.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim3 ? lim3 : Destination),
                    x, dist, side);
        }
    }
}
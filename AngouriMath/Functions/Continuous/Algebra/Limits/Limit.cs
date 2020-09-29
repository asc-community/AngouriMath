namespace AngouriMath.Functions.Algebra
{
    using Core;
    using static Entity;
    using static Entity.Number;
    internal static class LimitFunctional
    {
        private static Entity? SimplifyAndComputeLimitToInfinity(Entity expr, Variable x)
        {
            expr = expr.Simplify();
            return ComputeLimitToInfinity(expr, x);
        }

        private static Entity? ComputeLimitToInfinity(Entity expr, Variable x)
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

        public static Entity? ComputeLimit(Entity expr, Variable x, Entity dist, ApproachFrom side = ApproachFrom.BothSides) => side switch
        {
            ApproachFrom.Left or ApproachFrom.Right => expr.ComputeLimitDivideEtImpera(x, dist, side),
            ApproachFrom.BothSides =>
                !dist.IsFinite
                // just compute limit with no check for left/right equality
                // here approach left will be ignored anyways, as dist is infinite number
                ? expr.ComputeLimitDivideEtImpera(x, dist, ApproachFrom.Left)
                : expr.ComputeLimitDivideEtImpera(x, dist, ApproachFrom.Left) is { } fromLeft
                  && expr.ComputeLimitDivideEtImpera(x, dist, ApproachFrom.Right) is { } fromRight
                ? fromLeft == fromRight ? fromLeft : Real.NaN
                : null,
            _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(side), (int)side, typeof(ApproachFrom))
        };

        internal static Entity? ComputeLimitImpl(Entity expr, Variable x, Entity dist, ApproachFrom side) => dist switch
        {
            _ when !expr.ContainsNode(x) => expr,
            // avoid NaN values as non finite numbers
            Real { IsNaN: true } => Real.NaN,
            // if x -> -oo just make -x -> +oo
            Real { IsFinite: false, IsNegative: true } => SimplifyAndComputeLimitToInfinity(expr.Substitute(x, -x), x),
            // compute limit for x -> +oo
            Real { IsFinite: false, IsNegative: false } => SimplifyAndComputeLimitToInfinity(expr, x),
            Complex { IsFinite: false } =>
                throw new Core.Exceptions.MathSException($"complex infinities are not supported in limits: lim({x} -> {dist}) {expr}"),
            _ => SimplifyAndComputeLimitToInfinity(side switch
            {
                // lim(x -> 3-) x <=> lim(x -> 0+) 3 - x <=> lim(x -> +oo) 3 - 1 / x
                ApproachFrom.Left => expr.Substitute(x, dist - 1 / x),
                // lim(x -> 3+) x <=> lim(x -> 0+) 3 + x <=> lim(x -> +oo) 3 + 1 / x
                ApproachFrom.Right => expr.Substitute(x, dist + 1 / x),
                _ => throw new System.ArgumentOutOfRangeException(nameof(side), side,
                    $"Only {ApproachFrom.Left} and {ApproachFrom.Right} are supported.")
            }, x)
        };
    }
}
namespace AngouriMath
{
    using Core;
    using static Functions.Algebra.LimitFunctional;
    public abstract partial record Entity
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
        internal virtual Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            => new Limitf(this, x, dist, side);

        public partial record Number
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) => this;
        }

        public partial record Variable
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side);
        }

        public partial record Tensor
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                null; // TODO
        }

        public partial record Sumf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Augend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Augend,
                    Addend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Addend),
                    x, dist, side);
        }

        public partial record Minusf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Subtrahend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Subtrahend,
                    Minuend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Minuend),
                    x, dist, side);
        }

        public partial record Mulf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Multiplier.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Multiplier,
                    Multiplicand.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Multiplicand),
                    x, dist, side);
        }

        public partial record Divf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Dividend.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Dividend,
                    Divisor.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Divisor),
                    x, dist, side);
        }

        public partial record Sinf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Cosf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Tanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Cotanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Logf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Base.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Base,
                    Antilogarithm.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Antilogarithm),
                    x, dist, side);
        }

        public partial record Powf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Base.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Base,
                    Exponent.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Exponent),
                    x, dist, side);
        }

        public partial record Arcsinf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Arccosf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Arctanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Arccotanf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Factorialf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Argument.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Argument),
                    x, dist, side);
        }

        public partial record Derivativef
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Expression.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Expression,
                    Var.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Var,
                    Iterations.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim3 ? lim3 : Iterations),
                    x, dist, side);
        }

        public partial record Integralf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side) =>
                ComputeLimitImpl(this, x, dist, side) is { } lim ? lim
                : ComputeLimitImpl(New(
                    Expression.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim1 ? lim1 : Expression,
                    Var.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim2 ? lim2 : Var,
                    Iterations.ComputeLimitDivideEtImpera(x, dist, side) is { IsFinite: true } lim3 ? lim3 : Iterations),
                    x, dist, side);
        }

        public partial record Limitf
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

        public partial record Signumf
        {
            // TODO:
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
                => new Limitf(this, x, dist, side);
        }

        public partial record Absf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
                => Argument.ComputeLimitDivideEtImpera(x, dist, side)?.Abs();
        }

        partial record Equalsf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                var left = Left.ComputeLimitDivideEtImpera(x, dist, side);
                var right = Right.ComputeLimitDivideEtImpera(x, dist, side);
                if (left is null || right is null)
                    return null;
                return MathS.Equality(left, right);
            }
        }

        partial record Greaterf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                var left = Left.ComputeLimitDivideEtImpera(x, dist, side);
                var right = Right.ComputeLimitDivideEtImpera(x, dist, side);
                if (left is null || right is null)
                    return null;
                return left > right;
            }
        }

        partial record GreaterOrEqualf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                var left = Left.ComputeLimitDivideEtImpera(x, dist, side);
                var right = Right.ComputeLimitDivideEtImpera(x, dist, side);
                if (left is null || right is null)
                    return null;
                return left >= right;
            }
        }

        partial record Lessf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                var left = Left.ComputeLimitDivideEtImpera(x, dist, side);
                var right = Right.ComputeLimitDivideEtImpera(x, dist, side);
                if (left is null || right is null)
                    return null;
                return left < right;
            }
        }

        partial record LessOrEqualf
        {
            internal override Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            {
                var left = Left.ComputeLimitDivideEtImpera(x, dist, side);
                var right = Right.ComputeLimitDivideEtImpera(x, dist, side);
                if (left is null || right is null)
                    return null;
                return left <= right;
            }
        }
    }
}
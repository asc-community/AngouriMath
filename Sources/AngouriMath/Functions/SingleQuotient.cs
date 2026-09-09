//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// Writes an expression as one quotient: a numerator and a denominator with no division
    /// anywhere inside either.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is what other systems call <c>together</c> or <c>ratsimp</c>, and it is deliberately
    /// <b>not</b> part of <see cref="Entity.Simplify(int)"/>. Putting a sum over a common
    /// denominator makes some expressions worse — <c>1/x + 1/y</c> is easier to read than
    /// <c>(x + y)/(x*y)</c> — which is why every system that has it keeps it as an operation you
    /// ask for. <c>Simplify</c> here combines nothing: <c>a + b/c</c> comes back as <c>a + b/c</c>.
    /// https://github.com/asc-community/AngouriMath/issues/1239
    /// </para>
    /// <para>
    /// <b>What it is for.</b> Everything that takes a rational function apart —
    /// <see cref="PartialFractions"/> and the integrator's rational path — wants its input as a
    /// single <see cref="Divf"/> of two polynomials, and declines anything else. So a rewrite that
    /// produces a correct but nested answer is thrown away one step short of being usable. The
    /// half-angle substitution is the case that exposed it: <c>1/(1 - sin(x))</c> rewrites to
    /// <c>2/((t^2 + 1)(1 + (-2)t/(t^2 + 1)))</c>, which is <c>2/(t^2 - 2t + 1)</c> after one
    /// distribution and is integrated at once, and was declined for want of it.
    /// </para>
    /// <para>
    /// <b>No cancellation.</b> The two halves are returned as built, with no common factor taken
    /// out: <c>x/x</c> comes back as <c>(x, x)</c> and not as <c>(1, 1)</c>. Cancelling needs a
    /// gcd, which needs to know what the expression is a polynomial <em>in</em>, and this runs
    /// before anything has decided that. The callers here divide out afterwards anyway, and a
    /// caller that wants the tidy form asks the simplifier for it.
    /// </para>
    /// <para>
    /// <b>It always terminates and never grows without bound</b>, because it recurses only into
    /// the operands of the node it is given and each recursion is on a strictly smaller tree. What
    /// it can do is make the tree bigger — combining a sum of <c>n</c> quotients multiplies the
    /// denominators — so <see cref="Of"/> is a transformation to ask for rather than one to apply
    /// on the way past.
    /// </para>
    /// </remarks>
    internal static class SingleQuotient
    {
        /// <summary>
        /// The expression as <c>Numerator / Denominator</c>. The denominator is <c>1</c> for an
        /// expression that had no division in it, which is the signal that nothing was combined.
        /// </summary>
        internal static (Entity Numerator, Entity Denominator) Of(Entity expr)
        {
            switch (expr)
            {
                case Divf(var dividend, var divisor):
                {
                    var (an, ad) = Of(dividend);
                    var (bn, bd) = Of(divisor);
                    // (an/ad) / (bn/bd) = (an * bd) / (ad * bn)
                    return (Times(an, bd), Times(ad, bn));
                }

                case Mulf(var left, var right):
                {
                    var (an, ad) = Of(left);
                    var (bn, bd) = Of(right);
                    return (Times(an, bn), Times(ad, bd));
                }

                case Sumf(var augend, var addend):
                {
                    var (an, ad) = Of(augend);
                    var (bn, bd) = Of(addend);
                    return (Plus(Times(an, bd), Times(bn, ad)), Times(ad, bd));
                }

                case Minusf(var minuend, var subtrahend):
                {
                    var (an, ad) = Of(minuend);
                    var (bn, bd) = Of(subtrahend);
                    return (Minus(Times(an, bd), Times(bn, ad)), Times(ad, bd));
                }

                // A whole power distributes over the quotient, and a negative one turns it over.
                // Anything else — a symbolic or fractional exponent — is left whole, since
                // (a/b)^(1/2) is not sqrt(a)/sqrt(b) on the branch cut.
                case Powf(var @base, Integer power):
                {
                    var (bn, bd) = Of(@base);
                    if (bd == Integer.One && power.EInteger.Sign >= 0)
                        return (expr, Integer.One);
                    return power.EInteger.Sign >= 0
                        ? (MathS.Pow(bn, power), MathS.Pow(bd, power))
                        : (MathS.Pow(bd, -power), MathS.Pow(bn, -power));
                }

                default:
                    return (expr, Integer.One);
            }
        }

        /// <summary>
        /// <paramref name="expr"/> rewritten as a single quotient, or unchanged where it had no
        /// division in it to combine.
        /// </summary>
        internal static Entity Combine(Entity expr)
        {
            var (numerator, denominator) = Of(expr);
            return denominator == Integer.One ? numerator : numerator / denominator;
        }

        // Multiplying and adding through `1` and `0` rather than building the nodes keeps the
        // result readable, and keeps the common case -- an expression with one division in it --
        // from coming back wrapped in a chain of `* 1`.
        private static Entity Times(Entity a, Entity b) =>
            a == Integer.One ? b : b == Integer.One ? a : a * b;

        private static Entity Plus(Entity a, Entity b) =>
            a == Integer.Create(0) ? b : b == Integer.Create(0) ? a : a + b;

        private static Entity Minus(Entity a, Entity b) =>
            b == Integer.Create(0) ? a : a - b;
    }
}

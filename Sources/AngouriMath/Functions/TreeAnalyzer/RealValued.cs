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
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// Whether the expression is real wherever it is defined, given that <paramref name="x"/>
        /// is. A free variable is read as complex by this library, so one appearing anywhere but
        /// under a modulus settles nothing and the answer is no. Nothing is claimed for a shape
        /// not listed: the list costs coverage and never correctness.
        /// </summary>
        /// <remarks>
        /// Asked by the limit reader, for which a bound or an oscillation is a fact about a real
        /// argument, and by differentiation, for which <c>|f|' = sgn(f) * f'</c> and
        /// <c>sgn(f)' = 0</c> are facts about a real-valued <c>f</c>: <c>|i / x|</c> is
        /// <c>1 / |x|</c>, whose derivative is <c>-sgn(x) / x^2</c>, where the real-line formula
        /// gives <c>sgn(i / x) * (-i / x^2) = sgn(x) / x^2</c> -- the wrong sign, and through
        /// l'Hopital's rule the limit of <c>(i / x) / |i / x|</c> at <c>+oo</c> read as <c>1</c>
        /// where it is <c>i</c>.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1186">#1186</a>
        /// </remarks>
        internal static bool IsRealValued(Entity expr, Variable x)
        {
            if (!expr.ContainsNode(x))
                return expr.Evaled is Real { IsNaN: false };
            switch (expr)
            {
                case Variable variable:
                    return variable == x;
                case Sumf(var augend, var addend):
                    return IsRealValued(augend, x) && IsRealValued(addend, x);
                case Minusf(var minuend, var subtrahend):
                    return IsRealValued(minuend, x) && IsRealValued(subtrahend, x);
                case Mulf(var multiplier, var multiplicand):
                    return IsRealValued(multiplier, x) && IsRealValued(multiplicand, x);
                case Divf(var dividend, var divisor):
                    return IsRealValued(dividend, x) && IsRealValued(divisor, x);
                // Only an integer exponent keeps a real base real: x ^ (1/2) is not real below 0.
                case Powf(var @base, Integer):
                    return IsRealValued(@base, x);
                case Sinf or Cosf or Tanf or Cotanf or Secantf or Cosecantf
                     or Arctanf or Arccotanf or Signumf:
                    return IsRealValued(expr.DirectChildren[0], x);
                // A modulus is real whatever it is taken of.
                case Absf:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Rewrites <c>abs(c * g)</c>, <c>abs(c / g)</c> and <c>abs(g / c)</c>, for a numeric
        /// <c>c</c> off the real line whose modulus is exact, as <c>|c| * abs(g)</c>,
        /// <c>|c| / abs(g)</c> and <c>abs(g) / |c|</c>, with <c>|c|</c> a number. The modulus is
        /// multiplicative on the whole plane, so nothing is assumed.
        /// </summary>
        /// <remarks>
        /// Only where the modulus is exact: <c>|1 + i|</c> is <c>sqrt(2)</c>, which the number
        /// type can only hold rounded, and a rounded coefficient is a different value. Left inside
        /// the modulus it stays exact. A real factor is left where it is as well -- <c>|3 * x|</c>
        /// and <c>3 * |x|</c> are the same size and there is no phase to take out; what this
        /// exists for is the factor that carries one, <c>|i * x| = |x|</c>, which is what lets a
        /// derivative and a limit read <c>|i / x|</c> as <c>1 / |x|</c>.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1186">#1186</a>
        /// </remarks>
        internal static bool TryTakeNumericModulusOut(Entity absArgument, out Entity rewritten)
        {
            switch (absArgument)
            {
                case Mulf(var c, var rest) when ExactModulus(c) is { } modulus:
                    rewritten = modulus * new Absf(rest);
                    return true;
                case Mulf(var rest, var c) when ExactModulus(c) is { } modulus:
                    rewritten = modulus * new Absf(rest);
                    return true;
                case Divf(var c, var rest) when ExactModulus(c) is { } modulus:
                    rewritten = modulus / new Absf(rest);
                    return true;
                case Divf(var rest, var c) when ExactModulus(c) is { } modulus:
                    rewritten = new Absf(rest) / modulus;
                    return true;
                default:
                    rewritten = absArgument;
                    return false;
            }
        }

        /// <summary>
        /// The modulus of a closed expression whose value is a number off the real line, where
        /// that modulus is a rational number; <see langword="null"/> for anything else -- a
        /// symbol, a real value, and a modulus the number type could only hold rounded. Read off
        /// the value rather than the node, since <c>3 + 4i</c> is written as a sum.
        /// </summary>
        internal static Rational? ExactModulus(Entity c)
            => c.Evaled is Complex number and not Real ? number.Abs() as Rational : null;
    }
}

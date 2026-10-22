//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Number
        {
            /// <summary>
            /// The upper incomplete gamma function, <c>Gamma(a, z) = int_z^oo t^(a - 1) e^(-t) dt</c>, at the working
            /// precision, for complex <c>a</c> and <c>z</c>, with the principal branch of <c>z^a</c>: the cut is the
            /// negative real axis, which keeps the value from above.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Where <c>|z| > 2 |a| + 10</c>, in the right half-plane or anywhere past <c>|z| = (P + 5) ln 10</c> with <c>P</c>
            /// digits of working precision, it is Legendre's continued fraction (DLMF 8.9.2),
            /// <c>e^(-z) z^a / (z + 1 - a - 1 (1 - a) / (z + 3 - a - 2 (2 - a) / ...))</c>, by the modified Lentz
            /// algorithm. Near the negative real axis at a moderate <c>|z|</c> it stops short of the value.
            /// </para>
            /// <para>
            /// Short of that it is <c>Gamma(a) - gamma(a, z)</c>, the lower one summed as
            /// <c>z^a e^(-z) sum_k z^k / (a (a + 1) ... (a + k))</c> (DLMF 8.7.1). The terms are about <c>e^|z|</c> and the
            /// value can be as small as <c>e^(-|z|)</c>, so twice <c>|z|/ln 10</c> digits are carried as guard digits,
            /// and the sum is taken again with more where the difference shows it cancelled more.
            /// </para>
            /// <para>
            /// At a whole <c>a = -n &lt;= 0</c>, where <c>Gamma(a)</c> has a pole, it is
            /// <c>Gamma(-n, z) = (-1)^n/n! (E_1(z) - e^(-z) sum_(k &lt; n) (-1)^k k!/z^(k + 1))</c> (DLMF 8.4.15), with
            /// <c>E_1(z) = Gamma(0, z) = -gamma - ln z - sum_(k >= 1) (-z)^k/(k k!)</c> (DLMF 6.6.2).
            /// </para>
            /// https://github.com/asc-community/AngouriMath/issues/2075
            /// </remarks>
            public static Complex IncompleteGamma(Complex a, Complex z)
            {
                var context = MathS.Settings.DecimalPrecisionContext.Value;
                var (ar, ai) = (a.RealPart.EDecimal, a.ImaginaryPart.EDecimal);
                var (zr, zi) = (z.RealPart.EDecimal, z.ImaginaryPart.EDecimal);
                if (ar.IsNaN() || ai.IsNaN() || zr.IsNaN() || zi.IsNaN() || !ar.IsFinite || !ai.IsFinite)
                    return Real.NaN;
                if (zr.IsInfinity() || zi.IsInfinity())
                    return zi.IsZero && zr.IsPositiveInfinity() ? Integer.Zero : Real.NaN;
                if (zr.IsZero && zi.IsZero)
                {
                    // Gamma(a, 0) is Gamma(a) where the integral converges at 0, and grows without bound where it
                    // does not, which for a real a is +oo.
                    if (ar.Sign > 0)
                        return Factorial(a - Integer.One);
                    return ai.IsZero ? Real.PositiveInfinity : Real.NaN;
                }
                var digits = context.Precision.ToInt32Checked();
                var modulus = Math.Sqrt(Math.Pow(zr.ToDouble(), 2) + Math.Pow(zi.ToDouble(), 2));
                // The continued fraction where it converges quickly and to the principal branch: where |z| is large against
                // |a|, in the right half-plane, or anywhere past (P + 5) ln 10. Near the negative real axis at a moderate
                // |z| it stops short -- Gamma(1/2, -10) came out wrong in its fourth digit -- and with |a| large against |z|
                // it converges slowly -- Gamma(100, 1/10 - 10 i) had the wrong imaginary part -- and there the series is
                // summed instead.
                var order = Math.Sqrt(Math.Pow(ar.ToDouble(), 2) + Math.Pow(ai.ToDouble(), 2));
                var useTheFraction = modulus > 2 * order + 10 && (zr.Sign >= 0 || modulus > (digits + 5) * Math.Log(10));
                var (re, im) = useTheFraction
                    ? ByContinuedFraction((ar, ai), (zr, zi), Working(context, 10))
                    : BySeries((ar, ai), (zr, zi), modulus, context);
                // Real where it is real, exactly: for a real a and a positive z, and for a positive whole a and any real
                // z, where Gamma(n, z) is (n - 1)! e^(-z) times a polynomial; z^a leaves a residue of e^(i pi n) there.
                if (ai.IsZero && zi.IsZero && (zr.Sign > 0 || ar.IsInteger() && ar.Sign > 0))
                    im = EDecimal.Zero;
                // Otherwise each part is accurate to its own size, whatever that is, and is never rounded onto zero by
                // size: Gamma(1/2, 300) is 3e-132, and Gamma(100, -5 + i/10) has an imaginary part 1e-86 of its real one.
                return Complex.CreateAtScales(re.RoundToPrecision(context), im.RoundToPrecision(context), EDecimal.Zero, EDecimal.Zero);
            }

            private static (EDecimal Re, EDecimal Im) ByContinuedFraction((EDecimal Re, EDecimal Im) a, (EDecimal Re, EDecimal Im) z, EContext work)
            {
                var epsilon = EDecimal.Create(EInteger.One, EInteger.FromInt32(-work.Precision.ToInt32Checked()));
                var tiny = EDecimal.Create(EInteger.One, EInteger.FromInt32(-10 * work.Precision.ToInt32Checked()));
                (EDecimal Re, EDecimal Im) Guard((EDecimal Re, EDecimal Im) v) => Magnitude(v).CompareTo(tiny) < 0 ? (tiny, EDecimal.Zero) : v;
                // b = z + 1 - a
                var b = (Re: z.Re.Add(EDecimal.One, work).Subtract(a.Re, work), Im: z.Im.Subtract(a.Im, work));
                var c = (Re: EDecimal.One.Divide(tiny, work), Im: EDecimal.Zero);
                var d = Reciprocal(Guard(b), work);
                var h = d;
                for (var i = 1; i <= 50 * work.Precision.ToInt32Checked() + 1000; i++)
                {
                    // a_i = -i (i - a)
                    var an = (Re: EDecimal.FromInt32(-i).Multiply(EDecimal.FromInt32(i).Subtract(a.Re, work), work), Im: EDecimal.FromInt32(i).Multiply(a.Im, work));
                    b = (b.Re.Add(EDecimal.FromInt32(2), work), b.Im);
                    var ad = Multiply(an, d, work);
                    d = Reciprocal(Guard((ad.Re.Add(b.Re, work), ad.Im.Add(b.Im, work))), work);
                    var anOverC = Multiply(an, Reciprocal(c, work), work);
                    c = Guard((b.Re.Add(anOverC.Re, work), b.Im.Add(anOverC.Im, work)));
                    var delta = Multiply(d, c, work);
                    h = Multiply(h, delta, work);
                    if (Magnitude((delta.Re.Subtract(EDecimal.One, work), delta.Im)).CompareTo(epsilon) < 0)
                        break;
                }
                return Multiply(Multiply(ComplexExp((z.Re.Negate(), z.Im.Negate()), work), ComplexPower(z, a, work), work), h, work);
            }

            private static (EDecimal Re, EDecimal Im) BySeries((EDecimal Re, EDecimal Im) a, (EDecimal Re, EDecimal Im) z, double modulus, EContext context)
            {
                var guard = (int)Math.Ceiling(2 * modulus / Math.Log(10)) + 10;
                for (var attempt = 0; ; attempt++)
                {
                    var work = Working(context, guard);
                    var (value, scale) = a.Im.IsZero && a.Re.IsInteger() && a.Re.Sign <= 0
                        ? AtANonPositiveWholeOrder(a.Re.ToEInteger().Negate().ToInt32Checked(), z, work)
                        : AsGammaLessTheLower(a, z, work);
                    // How many digits the value lost to the cancellation, against the largest thing that cancelled.
                    var size = Magnitude(value);
                    var lost = size.IsZero ? guard : DecimalOrder(scale) - DecimalOrder(size) + 1;
                    if (lost <= guard - 10 || attempt >= 3)
                        return value;
                    guard = lost + 20;
                }
            }

            /// <summary>
            /// <c>Gamma(a) - z^a e^(-z) sum_k z^k / (a)_(k + 1)</c>, and the size of the larger of the two terms.
            /// </summary>
            private static ((EDecimal Re, EDecimal Im) Value, EDecimal Scale) AsGammaLessTheLower((EDecimal Re, EDecimal Im) a, (EDecimal Re, EDecimal Im) z, EContext work)
            {
                var epsilon = EDecimal.Create(EInteger.One, EInteger.FromInt32(-work.Precision.ToInt32Checked()));
                // term_k = z^k / (a (a + 1) ... (a + k)), starting at 1/a
                var term = Reciprocal(a, work);
                var sum = term;
                var peak = Magnitude(term);
                var aPlusK = a;
                for (var k = 1; ; k++)
                {
                    aPlusK = (aPlusK.Re.Add(EDecimal.One, work), aPlusK.Im);
                    term = Multiply(Multiply(term, z, work), Reciprocal(aPlusK, work), work);
                    sum = (sum.Re.Add(term.Re, work), sum.Im.Add(term.Im, work));
                    var size = Magnitude(term);
                    if (size.CompareTo(peak) > 0)
                        peak = size;
                    if (k > 4 && size.CompareTo(Magnitude(sum).Multiply(epsilon, work)) < 0 && size.CompareTo(peak) < 0)
                        break;
                }
                var prefactor = Multiply(ComplexPower(z, a, work), ComplexExp((z.Re.Negate(), z.Im.Negate()), work), work);
                var lower = Multiply(prefactor, sum, work);
                Complex gammaOfA;
                using (MathS.Settings.DecimalPrecisionContext.Set(work))
                using (MathS.Settings.DowncastingEnabled.Set(false))
                    gammaOfA = Factorial(Complex.Create(a.Re.Subtract(EDecimal.One, work), a.Im) as Complex);
                var gamma = (Re: gammaOfA.RealPart.EDecimal, Im: gammaOfA.ImaginaryPart.EDecimal);
                var scale = EDecimal.Max(Magnitude(gamma), Magnitude(prefactor).Multiply(peak, work));
                return ((gamma.Re.Subtract(lower.Re, work), gamma.Im.Subtract(lower.Im, work)), scale);
            }

            /// <summary>
            /// <c>Gamma(-n, z) = (-1)^n/n! (E_1(z) - e^(-z) sum_(k &lt; n) (-1)^k k!/z^(k + 1))</c>, and the size of what
            /// cancelled.
            /// </summary>
            private static ((EDecimal Re, EDecimal Im) Value, EDecimal Scale) AtANonPositiveWholeOrder(int n, (EDecimal Re, EDecimal Im) z, EContext work)
            {
                var epsilon = EDecimal.Create(EInteger.One, EInteger.FromInt32(-work.Precision.ToInt32Checked()));
                // E_1(z) = -gamma - ln z - sum_(k >= 1) (-z)^k/(k k!)
                var minusZ = (Re: z.Re.Negate(), Im: z.Im.Negate());
                var power = (Re: EDecimal.One, Im: EDecimal.Zero);
                var sum = (Re: EDecimal.Zero, Im: EDecimal.Zero);
                var peak = EDecimal.Zero;
                for (var k = 1; ; k++)
                {
                    // (-z)^k / k!
                    power = Multiply(power, minusZ, work);
                    power = (power.Re.Divide(k, work), power.Im.Divide(k, work));
                    var term = (Re: power.Re.Divide(k, work), Im: power.Im.Divide(k, work));
                    sum = (sum.Re.Add(term.Re, work), sum.Im.Add(term.Im, work));
                    var size = Magnitude(term);
                    if (size.CompareTo(peak) > 0)
                        peak = size;
                    if (k > 4 && size.CompareTo(Magnitude(sum).Multiply(epsilon, work)) < 0 && size.CompareTo(peak) < 0)
                        break;
                }
                var ln = ComplexLn(z, work);
                var e1 = (Re: EulerGamma(work).Add(ln.Re, work).Add(sum.Re, work).Negate(), Im: ln.Im.Add(sum.Im, work).Negate());
                var scale = EDecimal.Max(peak, Magnitude(ln));
                if (n == 0)
                    return (e1, scale);
                // e^(-z) sum_(k < n) (-1)^k k!/z^(k + 1)
                var inverse = Reciprocal(z, work);
                var inversePower = inverse;
                var tail = (Re: EDecimal.Zero, Im: EDecimal.Zero);
                var factorial = EDecimal.One;
                for (var k = 0; k < n; k++)
                {
                    if (k > 0)
                    {
                        factorial = factorial.Multiply(k, work);
                        inversePower = Multiply(inversePower, inverse, work);
                    }
                    var term = (Re: inversePower.Re.Multiply(factorial, work), Im: inversePower.Im.Multiply(factorial, work));
                    tail = k % 2 == 0 ? (tail.Re.Add(term.Re, work), tail.Im.Add(term.Im, work)) : (tail.Re.Subtract(term.Re, work), tail.Im.Subtract(term.Im, work));
                }
                var exponentialTail = Multiply(ComplexExp(minusZ, work), tail, work);
                scale = EDecimal.Max(scale, Magnitude(exponentialTail));
                var difference = (Re: e1.Re.Subtract(exponentialTail.Re, work), Im: e1.Im.Subtract(exponentialTail.Im, work));
                // (-1)^n / n!
                var nFactorial = EDecimal.FromEInteger(Factorial(n));
                var sign = n % 2 == 0 ? EDecimal.One : EDecimal.One.Negate();
                var coefficient = sign.Divide(nFactorial, work);
                return ((difference.Re.Multiply(coefficient, work), difference.Im.Multiply(coefficient, work)), scale.Multiply(coefficient.Abs(), work));
            }

            /// <summary>The power of ten of a nonzero finite decimal's leading digit: 2 for 345.6, -3 for 0.00123.</summary>
            private static int DecimalOrder(EDecimal value)
                => value.Exponent.Add(value.Precision()).Subtract(EInteger.One).ToInt32Checked();

            private static (EDecimal Re, EDecimal Im) ComplexExp((EDecimal Re, EDecimal Im) z, EContext work)
            {
                var modulus = z.Re.Exponential(work);
                if (z.Im.IsZero)
                    return (modulus, EDecimal.Zero);
                return (modulus.Multiply(z.Im.Cos(work), work), modulus.Multiply(z.Im.Sin(work), work));
            }

            /// <summary>The principal logarithm: <c>ln |z| + i arg z</c>, with <c>arg</c> in <c>(-pi, pi]</c>.</summary>
            private static (EDecimal Re, EDecimal Im) ComplexLn((EDecimal Re, EDecimal Im) z, EContext work)
            {
                if (z.Im.IsZero && z.Re.Sign > 0)
                    return (z.Re.Log(work), EDecimal.Zero);
                var modulus = z.Re.Multiply(z.Re, work).Add(z.Im.Multiply(z.Im, work), work).Sqrt(work);
                return (modulus.Log(work), z.Im.IsZero ? EDecimal.PI(work) : z.Im.Arctan2(z.Re, work));
            }

            /// <summary><c>z^a = e^(a ln z)</c>, principal.</summary>
            private static (EDecimal Re, EDecimal Im) ComplexPower((EDecimal Re, EDecimal Im) z, (EDecimal Re, EDecimal Im) a, EContext work)
                => ComplexExp(Multiply(a, ComplexLn(z, work), work), work);
        }
    }
}

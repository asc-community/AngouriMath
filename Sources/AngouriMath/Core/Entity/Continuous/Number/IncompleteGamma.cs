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
            /// precision, for complex <c>a</c> and <c>z</c>, with the branch of <c>z^a</c> the power takes: the principal
            /// one, whose cut is the negative real axis and which keeps the value from above, and on that axis the real root
            /// where the power takes it, for an exact <c>a</c> of odd denominator below 10.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Where <c>|z| > 2 |a| + 10</c>, in the right half-plane or anywhere past <c>|z| = (P + 5) ln 10</c> with <c>P</c>
            /// digits of working precision, it is Legendre's continued fraction (DLMF 8.9.2),
            /// <c>e^(-z) z^a / (z + 1 - a - 1 (1 - a) / (z + 3 - a - 2 (2 - a) / ...))</c>, by the modified Lentz
            /// algorithm. Near the negative real axis at a moderate <c>|z|</c> it stops short of the value.
            /// </para>
            /// <para>
            /// Short of that it is <c>Gamma(a) - gamma(a, z)</c>, the lower one summed in the right half-plane as
            /// <c>z^a e^(-z) sum_k z^k / (a (a + 1) ... (a + k))</c> (DLMF 8.7.1), whose terms there are about <c>e^|z|</c>
            /// and of one sign on the positive axis, with twice <c>|z|/ln 10</c> guard digits; and in the left half-plane as
            /// <c>z^a sum_k (-z)^k / (k! (a + k))</c> (DLMF 8.7.3), whose terms are of one sign on the negative axis, with
            /// <c>(|z| + Re z)/ln 10</c>. Each is taken again with more where the difference shows it cancelled more.
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
                // Gamma(a, z) is Gamma(a) - z^a g(a, z) with g entire, and its z^a is the power's: on the negative axis, for
                // an exact a of odd denominator q below 10, the power takes the real root, rho = (-1)^p e^(-i pi p/q) times
                // the principal one. With the principal branch there, -z^(a - 1) e^(-z), whose power takes the same root, was
                // not the derivative of the value, and an antiderivative written in the function had the values of one
                // turned by rho: the integral of e^(x^3) from 1 to 2 came out -137.8 + 238.6 i, where it is 275.5.
                // SimplificationContract.md section 6; https://github.com/asc-community/AngouriMath/issues/204
                var turn = TheRealRootsTurn(a, zr, zi);
                var (re, im) = useTheFraction
                    ? ByContinuedFraction((ar, ai), (zr, zi), Working(context, 10))
                    : BySeries((ar, ai), (zr, zi), modulus, context, turn);
                // On the cut, for a real a, the fraction's coefficients are real and it converges to e^(i pi a) times a real
                // function, the mean of the value from above and its conjugate from below, short of the value from above by
                // -i e^(i pi a) sin(pi a) Gamma(a). That is below the precision of a value e^|z| in size, but not of a part it
                // is the whole of: Gamma(a) at a half-whole a, the real part of Gamma(1/2, -300), which is sqrt(pi), and
                // -i (-1)^n pi/n! at a = -n, the imaginary part of Gamma(-3, -300), pi/6. Without it, both are 0.
                if (useTheFraction && turn is null && ai.IsZero && zi.IsZero && zr.Sign < 0)
                {
                    var work = Working(context, 10);
                    if (!ar.IsInteger() && ar.Multiply(EDecimal.FromInt32(2)).IsInteger())
                        re = re.Add(GammaOf((ar, ai), work).Re, work);
                    else if (ar.IsInteger() && ar.Sign <= 0)
                    {
                        var n = ar.ToEInteger().Negate();
                        var piOverFactorial = InternalAMExtensions.ConstantCache.Lookup(work).Pi.Divide(EDecimal.FromEInteger(Factorial(n.ToInt32Checked())), work);
                        im = n.IsEven ? im.Subtract(piOverFactorial, work) : im.Add(piOverFactorial, work);
                    }
                }
                // The continued fraction is used on the negative axis only past (P + 5) ln 10, where the value is e^|z| in
                // size and (1 - rho) Gamma(a) + rho times the principal value cancels nothing.
                if (turn is { } fractionsTurn && useTheFraction)
                {
                    var work = Working(context, 10);
                    var rho = Turned(fractionsTurn, work);
                    var gamma = GammaOf((ar, ai), work);
                    var turned = Multiply(rho, (re, im), work);
                    (re, im) = (gamma.Re.Subtract(gamma.Re.Multiply(rho.Re, work), work).Add(turned.Re, work), turned.Im.Subtract(gamma.Re.Multiply(rho.Im, work), work));
                }
                // Real where it is real, exactly: for a real a and a positive z, for a positive whole a and any real z,
                // where Gamma(n, z) is (n - 1)! e^(-z) times a polynomial and z^a leaves a residue of e^(i pi n), and where
                // z^a is the real root, each factor of Gamma(a) - z^a g(a, z) being real.
                if (ai.IsZero && zi.IsZero && (zr.Sign > 0 || ar.IsInteger() && ar.Sign > 0 || turn is not null))
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

            /// <summary>
            /// Where the power takes the real root of <c>z^a</c>, the turn from the principal value to it, <c>e^(i pi k/q)</c>
            /// as <c>(k, q)</c>: an exact <paramref name="a"/> of odd denominator <c>q</c> below 10 and a negative real
            /// <c>z</c>, the case <see cref="Pow"/> searches for a real root in. <see langword="null"/> elsewhere.
            /// </summary>
            private static (EInteger Turns, EInteger Q)? TheRealRootsTurn(Complex a, EDecimal zr, EDecimal zi)
            {
                if (a is not Rational { ERational: var exponent } || a is Integer || !zi.IsZero || zr.Sign >= 0)
                    return null;
                var q = exponent.Denominator.Abs();
                if (q.IsEven || q.CompareTo(EInteger.FromInt32(10)) >= 0)
                    return null;
                // (-1)^p e^(-i pi p/q) = e^(i pi p (q - 1)/q), the angle taken modulo 2 pi.
                var p = exponent.Denominator.Sign < 0 ? exponent.Numerator.Negate() : exponent.Numerator;
                return (p.Multiply(q.Subtract(EInteger.One)).Mod(q.Multiply(EInteger.FromInt32(2))), q);
            }

            private static (EDecimal Re, EDecimal Im) Turned((EInteger Turns, EInteger Q) turn, EContext work)
            {
                var angle = InternalAMExtensions.ConstantCache.Lookup(work).Pi.Multiply(EDecimal.FromEInteger(turn.Turns), work).Divide(EDecimal.FromEInteger(turn.Q), work);
                return (angle.Cos(work), angle.Sin(work));
            }

            private static (EDecimal Re, EDecimal Im) BySeries((EDecimal Re, EDecimal Im) a, (EDecimal Re, EDecimal Im) z, double modulus, EContext context,
                (EInteger Turns, EInteger Q)? turn)
            {
                // In the left half-plane the lower function is summed as z^a sum_k (-z)^k/(k! (a + k)), as E_1 is, whose terms
                // of size e^|z| sum to e^(-Re z): it loses (|z| + Re z)/ln 10 digits, none on the negative axis, where z^a
                // e^(-z) sum_k z^k/(a)_(k + 1) has terms of alternating sign that sum to e^(-|z|) and lost twice |z|/ln 10.
                // Carrying those digits asked Gamma(a) at a precision of its own, where it is not remembered: Gamma(1/3, -143)
                // took 9.6 seconds, and Gamma(-5/2, -60) 2.4.
                var leftHalf = z.Re.Sign < 0;
                var guard = leftHalf
                    ? (int)Math.Ceiling(Math.Max(0, modulus + z.Re.ToDouble()) / Math.Log(10)) + 12
                    : (int)Math.Ceiling(2 * modulus / Math.Log(10)) + 10;
                for (var attempt = 0; ; attempt++)
                {
                    var work = Working(context, guard);
                    var (value, scale) = a.Im.IsZero && a.Re.IsInteger() && a.Re.Sign <= 0
                        ? AtANonPositiveWholeOrder(a.Re.ToEInteger().Negate().ToInt32Checked(), z, work)
                        : AsGammaLessTheLower(a, z, leftHalf, work, turn);
                    // How many digits the value lost to the cancellation, against the largest thing that cancelled.
                    var size = Magnitude(value);
                    var lost = size.IsZero ? guard : DecimalOrder(scale) - DecimalOrder(size) + 1;
                    if (lost <= guard - 10 || attempt >= 3)
                        return value;
                    guard = lost + 20;
                }
            }

            /// <summary>
            /// <c>Gamma(a) - z^a e^(-z) sum_k z^k / (a)_(k + 1)</c>, or with <paramref name="leftHalf"/>
            /// <c>Gamma(a) - z^a sum_k (-z)^k / (k! (a + k))</c>, and the size of the larger of the two terms.
            /// </summary>
            private static ((EDecimal Re, EDecimal Im) Value, EDecimal Scale) AsGammaLessTheLower((EDecimal Re, EDecimal Im) a, (EDecimal Re, EDecimal Im) z, bool leftHalf,
                EContext work, (EInteger Turns, EInteger Q)? turn)
            {
                var epsilon = EDecimal.Create(EInteger.One, EInteger.FromInt32(-work.Precision.ToInt32Checked()));
                // term_k = z^k / (a (a + 1) ... (a + k)), or (-z)^k / (k! (a + k)), each starting at 1/a, and each term the
                // last times z/(a + k), or times -z (a + k - 1)/(k (a + k))
                var minusZ = (Re: z.Re.Negate(), Im: z.Im.Negate());
                (EDecimal Re, EDecimal Im) sum;
                EDecimal peak;
                if (a.Im.IsZero && z.Im.IsZero)
                {
                    // Real, as a sampled function's arguments are: one product and one quotient a term, where the complex
                    // arithmetic took nineteen operations, and an evaluation four milliseconds.
                    var term = EDecimal.One.Divide(a.Re, work);
                    var real = term;
                    peak = term.Abs();
                    var aPlusK = a.Re;
                    for (var k = 1; ; k++)
                    {
                        var previous = aPlusK;
                        aPlusK = aPlusK.Add(EDecimal.One, work);
                        term = leftHalf
                            ? term.Multiply(minusZ.Re, work).Multiply(previous, work).Divide(aPlusK.Multiply(k, work), work)
                            : term.Multiply(z.Re, work).Divide(aPlusK, work);
                        real = real.Add(term, work);
                        var size = term.Abs();
                        if (size.CompareTo(peak) > 0)
                            peak = size;
                        if (k > 4 && size.CompareTo(real.Abs().Multiply(epsilon, work)) < 0 && size.CompareTo(peak) < 0)
                            break;
                    }
                    sum = (real, EDecimal.Zero);
                }
                else
                {
                    var term = Reciprocal(a, work);
                    sum = term;
                    peak = Magnitude(term);
                    var aPlusK = a;
                    for (var k = 1; ; k++)
                    {
                        var previous = aPlusK;
                        aPlusK = (aPlusK.Re.Add(EDecimal.One, work), aPlusK.Im);
                        term = leftHalf
                            ? Multiply(Multiply(term, minusZ, work), Multiply(previous, Reciprocal((aPlusK.Re.Multiply(k, work), aPlusK.Im.Multiply(k, work)), work), work), work)
                            : Multiply(Multiply(term, z, work), Reciprocal(aPlusK, work), work);
                        sum = (sum.Re.Add(term.Re, work), sum.Im.Add(term.Im, work));
                        var size = Magnitude(term);
                        if (size.CompareTo(peak) > 0)
                            peak = size;
                        if (k > 4 && size.CompareTo(Magnitude(sum).Multiply(epsilon, work)) < 0 && size.CompareTo(peak) < 0)
                            break;
                    }
                }
                var power = ComplexPower(z, a, work);
                if (turn is { } realRoot)
                    power = Multiply(Turned(realRoot, work), power, work);
                var prefactor = leftHalf ? power : Multiply(power, ComplexExp(minusZ, work), work);
                var lower = Multiply(prefactor, sum, work);
                var gamma = GammaOf(a, work);
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

            /// <summary>
            /// <c>Gamma(a)</c> at <paramref name="work"/>'s digits, remembered: it is most of the cost of an evaluation, six
            /// milliseconds at a hundred digits, and a function sampled at points is asked it at one <c>a</c> for every
            /// <c>z</c>.
            /// </summary>
            private static (EDecimal Re, EDecimal Im) GammaOf((EDecimal Re, EDecimal Im) a, EContext work)
            {
                var key = (a.Re, a.Im, work.Precision.ToInt32Checked());
                if (gammaOfTheOrder.TryGetValue(key, out var known))
                    return known;
                Complex value;
                using (MathS.Settings.DecimalPrecisionContext.Set(work))
                using (MathS.Settings.DowncastingEnabled.Set(false))
                    value = Factorial(Complex.Create(a.Re.Subtract(EDecimal.One, work), a.Im) as Complex);
                var gamma = (value.RealPart.EDecimal, value.ImaginaryPart.EDecimal);
                // Emptied rather than grown without end, as the integrator's answers are.
                if (gammaOfTheOrder.Count >= 256)
                    gammaOfTheOrder.Clear();
                gammaOfTheOrder[key] = gamma;
                return gamma;
            }
            [ConcurrentField] private static readonly System.Collections.Concurrent.ConcurrentDictionary<(EDecimal, EDecimal, int), (EDecimal, EDecimal)> gammaOfTheOrder = new();

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
                // On the cut, ln |z| + i pi, with pi from the cache: computed afresh it was a third of an evaluation.
                if (z.Im.IsZero)
                    return (z.Re.Negate().Log(work), InternalAMExtensions.ConstantCache.Lookup(work).Pi);
                var modulus = z.Re.Multiply(z.Re, work).Add(z.Im.Multiply(z.Im, work), work).Sqrt(work);
                return (modulus.Log(work), z.Im.Arctan2(z.Re, work));
            }

            /// <summary><c>z^a = e^(a ln z)</c>, principal.</summary>
            private static (EDecimal Re, EDecimal Im) ComplexPower((EDecimal Re, EDecimal Im) z, (EDecimal Re, EDecimal Im) a, EContext work)
            {
                // On the cut, for a real a, |z|^a (cos pi a + i sin pi a) with each part accurate to its own size. From pi/2
                // rounded to W digits, cos(pi/2) is 10^(-W) where it is 0, and times the lower function's e^143 that was the
                // real part of Gamma(1/2, -143), which is sqrt(pi).
                if (z.Im.IsZero && z.Re.Sign < 0 && a.Im.IsZero)
                {
                    var modulus = z.Re.Negate().Log(work).Multiply(a.Re, work).Exponential(work);
                    var (cos, sin) = CosAndSinOfPiTimes(a.Re, work);
                    return (modulus.Multiply(cos, work), modulus.Multiply(sin, work));
                }
                return ComplexExp(Multiply(a, ComplexLn(z, work), work), work);
            }

            /// <summary>
            /// <c>(cos pi t, sin pi t)</c>, each accurate to its own size: with <c>n</c> the whole number nearest <c>t</c> and
            /// <c>s = 1/2 - |t - n|</c>, both exact, they are <c>(-1)^n sin pi s</c> and <c>(-1)^n sgn(t - n) cos pi s</c>, and
            /// <c>cos pi t</c> is exactly 0 at a half-whole <c>t</c>.
            /// </summary>
            private static (EDecimal Cos, EDecimal Sin) CosAndSinOfPiTimes(EDecimal t, EContext work)
            {
                var n = t.RoundToExponent(EInteger.Zero, ERounding.HalfEven);
                var offset = t.Subtract(n);
                var s = EDecimal.Create(5, -1).Subtract(offset.Abs());
                var sign = n.ToEInteger().IsEven ? EDecimal.One : EDecimal.One.Negate();
                if (s.IsZero)
                    return (EDecimal.Zero, offset.Sign > 0 ? sign : sign.Negate());
                var angle = InternalAMExtensions.ConstantCache.Lookup(work).Pi.Multiply(s, work);
                var cos = angle.Sin(work).Multiply(sign, work);
                var sin = offset.IsZero ? EDecimal.Zero : angle.Cos(work).Multiply(offset.Sign > 0 ? sign : sign.Negate(), work);
                return (cos, sin);
            }
        }
    }
}

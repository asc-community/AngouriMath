//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using NumericsComplex = System.Numerics.Complex;

namespace AngouriMath.Core.Compilation
{
    /// <summary>
    /// The special functions in double precision, for both compilers: <c>erf</c>, <c>erfc</c>,
    /// <c>erfi</c>, <c>Ei</c>, <c>li</c>, <c>Si</c>, <c>Ci</c>, <c>Shi</c> and <c>Chi</c>.
    /// The interpreter's kernels, <see cref="Entity.Number.Erf(Entity.Number.Complex)"/> and the
    /// others, work in arbitrary precision. At 20 digits they take 0.7 to 5.5 ms a call. Newton's
    /// method makes thousands of calls. These take a microsecond or two.
    /// https://github.com/asc-community/AngouriMath/issues/1607
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each is the kernel's function, with the kernel's branch cuts and the kernel's values on the
    /// axes. They were measured against the kernels at 30 digits on some ten thousand points: a
    /// grid of <c>[-14, 14]^2</c>, both axes out to 40 and down to <c>1e-4</c>, either side of the
    /// negative real axis, and circles out to 300. The integrals agree to within <c>6e-14</c> of the
    /// value. The error functions agree to within <c>3e-13</c>, which is the rounding of
    /// <c>z^2</c> in their <c>e^(-z^2)</c> where <c>|z|</c> is large. An imaginary part of
    /// either sign of zero is read as the axis, as the kernels read it.
    /// </para>
    /// <para>
    /// The error functions come from the Faddeeva function
    /// <c>w(z) = e^(-z^2) erfc(-i z)</c>, by Weideman's rational approximation with 40 terms
    /// (J. A. C. Weideman, <i>Computation of the complex error function</i>, SIAM J. Numer. Anal.
    /// 31, 1994), and near the origin from the Taylor series of <c>erf</c>.
    /// </para>
    /// <para>
    /// <c>Ei</c> is summed as its series where the terms cancel little, which is where
    /// <c>|z| - Re z</c> is at most 3 or <c>|z|</c> at most 2. Out to <c>|z| = 40</c> it is
    /// <c>-E1(-z)</c> otherwise, by <c>E1</c>'s continued fraction, plus the branch term. From
    /// 40 on it is the asymptotic series. <c>li</c> is <c>Ei</c> of the logarithm. <c>Shi</c> and
    /// <c>Chi</c> are summed as their own series by the same rule with <c>|Re z|</c>, and
    /// otherwise come from <c>Ei(z)</c> and <c>Ei(-z)</c>. <c>Si</c> and <c>Ci</c> are those at
    /// <c>i z</c>. Every relation is one the kernels state.
    /// </para>
    /// </remarks>
    internal static class SpecialFunctions
    {
        private const double SqrtPi = 1.7724538509055160273;
        private const double EulerGamma = 0.57721566490153286061;

        /// <summary>The real part where the value is real, and NaN where it is not, as <see cref="Math.Sqrt"/> of a negative number is.</summary>
        private static double RealOrNaN(NumericsComplex value) => value.Imaginary == 0 ? value.Real : double.NaN;

        private static bool IsNaN(NumericsComplex z) => double.IsNaN(z.Real) || double.IsNaN(z.Imaginary);
        private static bool IsInfinite(NumericsComplex z) => double.IsInfinity(z.Real) || double.IsInfinity(z.Imaginary);
        [ConstantField] private static readonly NumericsComplex NaN = new(double.NaN, double.NaN);

        public static NumericsComplex Erf(NumericsComplex z)
        {
            if (IsNaN(z)) return NaN;
            if (IsInfinite(z))
                return z.Imaginary == 0 ? (z.Real < 0 ? -1 : 1) : NaN;
            NumericsComplex value;
            if (z.Real * z.Real + z.Imaginary * z.Imaginary < 1)
                value = ErfByTheSeries(z);
            else if (z.Real >= 0)
                value = 1 - ExpTimes(-z * z, Faddeeva(new NumericsComplex(-z.Imaginary, z.Real)));
            else
                value = ExpTimes(-z * z, Faddeeva(new NumericsComplex(z.Imaginary, -z.Real))) - 1;
            // Real on the real line and imaginary on the imaginary axis.
            if (z.Imaginary == 0) return new(value.Real, 0);
            if (z.Real == 0) return new(0, value.Imaginary);
            return value;
        }

        public static NumericsComplex Erfc(NumericsComplex z)
        {
            if (IsNaN(z)) return NaN;
            if (IsInfinite(z))
                return z.Imaginary == 0 ? (z.Real < 0 ? 2 : 0) : NaN;
            NumericsComplex value;
            if (z.Real * z.Real + z.Imaginary * z.Imaginary < 1)
                value = 1 - ErfByTheSeries(z);
            else if (z.Real >= 0)
                value = ExpTimes(-z * z, Faddeeva(new NumericsComplex(-z.Imaginary, z.Real)));
            else
                value = 2 - ExpTimes(-z * z, Faddeeva(new NumericsComplex(z.Imaginary, -z.Real)));
            // Real on the real line, and 1 - i erfi(y) on the imaginary axis.
            if (z.Imaginary == 0) return new(value.Real, 0);
            if (z.Real == 0) return new(1, value.Imaginary);
            return value;
        }

        /// <summary><c>-i erf(i z)</c>.</summary>
        public static NumericsComplex Erfi(NumericsComplex z)
        {
            var erf = Erf(new NumericsComplex(-z.Imaginary, z.Real));
            // -i (a + i b) = b - i a
            var value = new NumericsComplex(erf.Imaginary, -erf.Real);
            if (z.Imaginary == 0) return new(value.Real, 0);
            if (z.Real == 0) return new(0, value.Imaginary);
            return value;
        }

        public static double Erf(double x) => RealOrNaN(Erf(new NumericsComplex(x, 0)));
        public static double Erfc(double x) => RealOrNaN(Erfc(new NumericsComplex(x, 0)));
        public static double Erfi(double x) => RealOrNaN(Erfi(new NumericsComplex(x, 0)));

        /// <summary><c>2/sqrt(pi) sum (-1)^n z^(2n + 1)/(n! (2n + 1))</c>, for <c>|z| &lt; 1</c>.</summary>
        private static NumericsComplex ErfByTheSeries(NumericsComplex z)
        {
            var minusZSquared = -z * z;
            NumericsComplex term = z, sum = z;
            for (var n = 1; n < 100; n++)
            {
                term *= minusZSquared / n;
                var next = term / (2 * n + 1);
                sum += next;
                if (next.Magnitude <= 1e-17 * sum.Magnitude)
                    break;
            }
            return 2 / SqrtPi * sum;
        }

        /// <summary>
        /// <c>w(z)</c> for <c>Im z &gt;= 0</c>, Weideman's
        /// <c>2 p(Z)/(L - i z)^2 + 1/(sqrt(pi) (L - i z))</c> with <c>Z = (L + i z)/(L - i z)</c>.
        /// </summary>
        private static NumericsComplex Faddeeva(NumericsComplex z)
        {
            // L - i z and L + i z, for z = x + i y
            var below = new NumericsComplex(WeidemanL + z.Imaginary, -z.Real);
            var above = new NumericsComplex(WeidemanL - z.Imaginary, z.Real);
            var zz = above / below;
            NumericsComplex p = weideman[WeidemanTerms];
            for (var n = WeidemanTerms - 1; n >= 1; n--)
                p = p * zz + weideman[n];
            return 2 * p / (below * below) + 1 / (SqrtPi * below);
        }

        private const int WeidemanTerms = 40;
        [ConstantField] private static readonly double WeidemanL = Math.Sqrt(WeidemanTerms / Math.Sqrt(2));

        /// <summary>
        /// The polynomial's coefficients, <c>a_n = 1/(4N) sum_k f(t_k) cos(pi n k/(2N))</c> over
        /// <c>|k| &lt; 2N</c>, with <c>t_k = L tan(pi k/(4N))</c> and <c>f(t) = e^(-t^2) (L^2 + t^2)</c>:
        /// the discrete Fourier transform Weideman takes, written out, since <c>f</c> is even.
        /// Index 0 is unused.
        /// </summary>
        [ConstantField] private static readonly double[] weideman = WeidemanCoefficients();

        private static double[] WeidemanCoefficients()
        {
            var m = 2 * WeidemanTerms;
            var coefficients = new double[WeidemanTerms + 1];
            for (var n = 1; n <= WeidemanTerms; n++)
            {
                var sum = 0.0;
                for (var k = -m + 1; k <= m - 1; k++)
                {
                    var t = WeidemanL * Math.Tan(k * Math.PI / (2 * m));
                    sum += Math.Exp(-t * t) * (WeidemanL * WeidemanL + t * t) * Math.Cos(Math.PI * n * k / m);
                }
                coefficients[n] = sum / (4 * WeidemanTerms);
            }
            return coefficients;
        }

        /// <summary>
        /// <c>e^a f</c>. Where <c>e^a</c> overflows, each part of the product is scaled in logarithms,
        /// so that an infinite factor and a finite one make an infinity and not a NaN, and a part with
        /// a small cosine or sine stays finite if it is.
        /// </summary>
        private static NumericsComplex ExpTimes(NumericsComplex a, NumericsComplex f)
        {
            if (a.Real <= 700)
                return NumericsComplex.Exp(a) * f;
            var s = a + NumericsComplex.Log(f);
            return new(Scaled(s.Real, Math.Cos(s.Imaginary)), Scaled(s.Real, Math.Sin(s.Imaginary)));
        }

        private static double Scaled(double exponent, double factor)
            => factor == 0 ? 0 : Math.Sign(factor) * Math.Exp(exponent + Math.Log(Math.Abs(factor)));

        /// <summary>The principal logarithm, with a zero imaginary part of either sign read as the axis.</summary>
        private static NumericsComplex Ln(NumericsComplex z)
            => new(Math.Log(z.Magnitude), z.Imaginary == 0 ? (z.Real < 0 ? Math.PI : 0) : Math.Atan2(z.Imaginary, z.Real));

        /// <summary><c>i pi sgn(Im z)</c>, the term the logarithm's branch adds off the real axis.</summary>
        private static NumericsComplex Branch(NumericsComplex z)
            => z.Imaginary == 0 ? NumericsComplex.Zero : new(0, Math.Sign(z.Imaginary) * Math.PI);

        /// <summary>
        /// <c>Ei(z) = gamma + (ln z - ln(1/z))/2 + sum_(k >= 1) z^k/(k k!)</c>, real on the whole real
        /// line, as <see cref="Entity.Number.Ei(Entity.Number.Complex)"/> defines it.
        /// </summary>
        public static NumericsComplex Ei(NumericsComplex z)
        {
            if (IsNaN(z)) return NaN;
            if (IsInfinite(z))
                return z.Imaginary == 0 ? (z.Real < 0 ? 0 : double.PositiveInfinity) : NaN;
            if (z == NumericsComplex.Zero)
                return double.NegativeInfinity;
            var modulus = z.Magnitude;
            NumericsComplex value;
            if (modulus >= 40)
                value = EiByTheAsymptoticSeries(z) + Branch(z);
            else if (modulus <= 2 || modulus - z.Real <= 3)
                // (ln z - ln(1/z))/2: ln |z| on the negative axis.
                value = EulerGamma + (z.Imaginary == 0 && z.Real < 0 ? Math.Log(-z.Real) : Ln(z)) + ExponentialSeries(z);
            else
                value = Branch(z) - E1ByTheContinuedFraction(-z);
            return z.Imaginary == 0 ? new(value.Real, 0) : value;
        }

        /// <summary><c>li(z) = Ei(ln z)</c>, with the principal logarithm; <c>li(0) = 0</c> and <c>li(1) = -oo</c>.</summary>
        public static NumericsComplex Li(NumericsComplex z)
        {
            if (IsNaN(z)) return NaN;
            if (IsInfinite(z))
                return z.Imaginary == 0 && z.Real > 0 ? double.PositiveInfinity : NaN;
            if (z == NumericsComplex.Zero)
                return NumericsComplex.Zero;
            if (z == NumericsComplex.One)
                return double.NegativeInfinity;
            var value = Ei(Ln(z));
            return z.Imaginary == 0 && z.Real > 0 ? new(value.Real, 0) : value;
        }

        public static double Ei(double x) => RealOrNaN(Ei(new NumericsComplex(x, 0)));
        public static double Li(double x) => RealOrNaN(Li(new NumericsComplex(x, 0)));

        /// <summary><c>sum_(k >= 1) z^k/(k k!)</c>.</summary>
        private static NumericsComplex ExponentialSeries(NumericsComplex z)
        {
            NumericsComplex power = 1, sum = 0;
            var modulus = z.Magnitude;
            for (var k = 1; k < 1000; k++)
            {
                power = power * z / k;
                var term = power / k;
                sum += term;
                if (k > modulus && term.Magnitude <= 1e-17 * (sum.Magnitude + 1))
                    break;
            }
            return sum;
        }

        /// <summary>
        /// <c>E1(w) = e^(-w)/(w + 1 - 1/(w + 3 - 4/(w + 5 - ...)))</c>, by the modified Lentz method.
        /// Used where <c>w</c> is at least 2 out and away from its cut, the negative real axis, and
        /// there it takes at most a couple of hundred steps.
        /// </summary>
        private static NumericsComplex E1ByTheContinuedFraction(NumericsComplex w)
        {
            const double tiny = 1e-300;
            var b = w + 1;
            NumericsComplex c = 1 / tiny, d = 1 / b, h = d;
            for (var i = 1; i < 5000; i++)
            {
                var a = -(double)i * i;
                b += 2;
                d = a * d + b;
                if (d == NumericsComplex.Zero) d = tiny;
                c = b + a / c;
                if (c == NumericsComplex.Zero) c = tiny;
                d = 1 / d;
                var delta = c * d;
                h *= delta;
                if ((delta - 1).Magnitude < 1e-16)
                    break;
            }
            return ExpTimes(-w, h);
        }

        /// <summary>
        /// <c>e^z/z sum_k k!/z^k</c>, cut at its smallest term, which from <c>|z| = 40</c> on is below
        /// <c>1e-16</c> of the sum.
        /// </summary>
        private static NumericsComplex EiByTheAsymptoticSeries(NumericsComplex z)
        {
            NumericsComplex term = 1, sum = 1;
            var previous = 1.0;
            for (var k = 1; k < 200; k++)
            {
                var next = term * k / z;
                var size = next.Magnitude;
                if (size >= previous || size < 1e-17)
                    break;
                sum += next;
                (term, previous) = (next, size);
            }
            return ExpTimes(z, sum / z);
        }

        public static NumericsComplex Si(NumericsComplex z)
        {
            if (IsNaN(z)) return NaN;
            if (IsInfinite(z))
                return z.Imaginary == 0 ? (z.Real < 0 ? -Math.PI / 2 : Math.PI / 2) : NaN;
            // -i Shi(i z)
            var shi = HyperbolicIntegral(new NumericsComplex(-z.Imaginary, z.Real), odd: true);
            return OnTheAxes(z, new NumericsComplex(shi.Imaginary, -shi.Real), odd: true);
        }

        public static NumericsComplex Ci(NumericsComplex z)
        {
            if (IsNaN(z)) return NaN;
            if (z == NumericsComplex.Zero) return double.NegativeInfinity;
            if (IsInfinite(z))
                return z.Imaginary == 0 ? (z.Real > 0 ? NumericsComplex.Zero : new NumericsComplex(0, Math.PI)) : NaN;
            // Chi(i z) + ln z - ln(i z), the last two -i pi/2 where arg z <= pi/2 and 3 i pi/2 past it.
            var chi = HyperbolicIntegral(new NumericsComplex(-z.Imaginary, z.Real), odd: false);
            var argument = z.Imaginary == 0 ? (z.Real < 0 ? Math.PI : 0) : Math.Atan2(z.Imaginary, z.Real);
            return OnTheAxes(z, chi + new NumericsComplex(0, argument <= Math.PI / 2 ? -Math.PI / 2 : 3 * Math.PI / 2), odd: false);
        }

        public static NumericsComplex Shi(NumericsComplex z) => HyperbolicIntegral(z, odd: true);
        public static NumericsComplex Chi(NumericsComplex z) => HyperbolicIntegral(z, odd: false);

        public static double Si(double x) => RealOrNaN(Si(new NumericsComplex(x, 0)));
        public static double Ci(double x) => RealOrNaN(Ci(new NumericsComplex(x, 0)));
        public static double Shi(double x) => RealOrNaN(Shi(new NumericsComplex(x, 0)));
        public static double Chi(double x) => RealOrNaN(Chi(new NumericsComplex(x, 0)));

        /// <summary><c>Shi</c> where <paramref name="odd"/>, and <c>Chi</c> otherwise.</summary>
        private static NumericsComplex HyperbolicIntegral(NumericsComplex z, bool odd)
        {
            if (IsNaN(z)) return NaN;
            if (z == NumericsComplex.Zero)
                return odd ? NumericsComplex.Zero : double.NegativeInfinity;
            if (IsInfinite(z))
            {
                if (z.Imaginary != 0) return NaN;
                return odd ? (z.Real < 0 ? double.NegativeInfinity : double.PositiveInfinity)
                           : (z.Real < 0 ? NaN : double.PositiveInfinity);
            }
            var modulus = z.Magnitude;
            NumericsComplex value;
            if (modulus < 40 && (modulus <= 2 || modulus - Math.Abs(z.Real) <= 3))
                value = odd ? HalfOfTheExponentialSeries(z, odd: true) : EulerGamma + Ln(z) + HalfOfTheExponentialSeries(z, odd: false);
            else
            {
                // Shi(z) = (Ei(z) - Ei(-z))/2 - i pi/2 sgn(Im z), Chi(z) = (Ei(z) + Ei(-z))/2 + i pi/2
                // sgn(Im z), and i pi more for Chi on the negative real axis.
                var plus = Ei(z);
                var minus = Ei(-z);
                value = Half(odd ? plus - minus : plus + minus);
                if (z.Imaginary != 0)
                {
                    var branch = new NumericsComplex(0, Math.Sign(z.Imaginary) * Math.PI / 2);
                    value = odd ? value - branch : value + branch;
                }
                else if (!odd && z.Real < 0)
                    value += new NumericsComplex(0, Math.PI);
            }
            return OnTheAxes(z, value, odd);
        }

        /// <summary>
        /// <c>sum z^(2k + 1)/((2k + 1) (2k + 1)!)</c> where <paramref name="odd"/>, and
        /// <c>sum_(k >= 1) z^(2k)/(2k (2k)!)</c> otherwise.
        /// </summary>
        private static NumericsComplex HalfOfTheExponentialSeries(NumericsComplex z, bool odd)
        {
            NumericsComplex power = 1, sum = 0;
            var modulus = z.Magnitude;
            for (var n = 1; n < 2000; n++)
            {
                power = power * z / n;
                if ((n % 2 == 1) != odd)
                    continue;
                var term = power / n;
                sum += term;
                if (n > modulus && term.Magnitude <= 1e-17 * (sum.Magnitude + 1))
                    break;
            }
            return sum;
        }

        /// <summary>
        /// Half of <paramref name="value"/>, part by part: dividing a complex number by 2 multiplies it
        /// by the complex number 2, and an infinite part times the other's zero makes a NaN.
        /// </summary>
        private static NumericsComplex Half(NumericsComplex value) => new(value.Real / 2, value.Imaginary / 2);

        /// <summary>
        /// What the kernels give exactly on the axes: the odd ones real on the real line and imaginary on
        /// the imaginary axis; the even ones real on the positive real axis, <c>i pi</c> more on the
        /// negative, and <c>i pi/2 sgn(y)</c> more than a real value on the imaginary axis. That holds
        /// however large the value, so an infinite one is given its exact part as well.
        /// </summary>
        private static NumericsComplex OnTheAxes(NumericsComplex z, NumericsComplex value, bool odd)
        {
            if (double.IsNaN(value.Real) && double.IsNaN(value.Imaginary))
                return value;
            if (z.Imaginary == 0)
                return new(value.Real, odd || z.Real >= 0 ? 0 : Math.PI);
            if (z.Real == 0)
                return odd ? new NumericsComplex(0, value.Imaginary) : new NumericsComplex(value.Real, Math.Sign(z.Imaginary) * Math.PI / 2);
            return value;
        }
    }
}

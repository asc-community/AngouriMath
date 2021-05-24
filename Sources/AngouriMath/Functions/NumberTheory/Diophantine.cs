using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    internal static class Diophantine
    {
        // b < a
        internal static IEnumerable<(Integer a, Integer q, Integer b, Integer r)> Decompose(Integer a, Integer b)
        {
            for (var newR = a % b; newR != 0; newR = a % b)
            {
                var newQ = a.IntegerDiv(b);
                yield return (a, newQ, b, newR);
                a = b;
                b = newR;
            }
        }

        // a x + b y = c
        // a > b
        internal static (Integer x, Integer y)? Solve(Integer a, Integer b, Integer c)
        {
            NormalInfo info;
            (a, b, c, info) = NormalInfo.Normalize(a, b, c);

            // http://zimmer.csufresno.edu/~lburger/Math149_diophantine%20I.pdf

            var gcd = MathS.NumberTheory.GreatestCommonDivisor(a, b);

            if (c % gcd != 0)
                return null;
            
            Integer x = 1;
            Integer y = 0;
            var turnOfY = true;

            foreach (var (_, q, _, r) in Decompose(a, b).Reverse())
            {
                if (turnOfY)
                    y += x * -q;
                else
                    x += y * -q;
                turnOfY = !turnOfY;
            }

            var coef = c.IntegerDiv(gcd);
            return info.Compensate(x * coef, y * coef);
        }

        internal struct NormalInfo
        {
            private bool aInverted;
            private bool bInverted;
            private bool cInverted;
            private bool abSwapped;
            internal static (Integer a, Integer b, Integer c, NormalInfo info) Normalize(Integer a, Integer b, Integer c)
            {
                var info = new NormalInfo();

                (a, info.aInverted) = InvertIfNeeded(a);
                (b, info.bInverted) = InvertIfNeeded(b);
                (c, info.cInverted) = InvertIfNeeded(c);
                if (a < b)
                    (a, b, info.abSwapped) = (b, a, true);
                return (a, b, c, info);

                static (Integer, bool) InvertIfNeeded(Integer n)
                    => n < 0 ? (-n, true) : (n, false);
            }

            internal (Integer x, Integer y) Compensate(Integer x, Integer y)
            {
                if (abSwapped)
                    (x, y) = (y, x);
                if (aInverted)
                    x = -x;
                if (bInverted)
                    y = -y;
                if (cInverted)
                    (x, y) = (-x, -y);
                return (x, y);
            }
        }
    }
}

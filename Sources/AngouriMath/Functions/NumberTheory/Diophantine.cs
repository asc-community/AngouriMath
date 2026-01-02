//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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

        // Credit: https://cp-algorithms.com/algebra/linear-diophantine-equation.html
        private static Integer InnerGcd(Integer a, Integer b, out Integer x, out Integer y)
        {
            if (b == 0)
                return ((x, y) = (1, 0)).ReplaceWith(a);
            var d = InnerGcd(b, a % b, out var x1, out var y1);
            x = y1;
            y = x1 - y1 * a.IntegerDiv(b);
            return d;
        }
        
        // Credit: https://cp-algorithms.com/algebra/linear-diophantine-equation.html
        private static (Integer x, Integer y)? FindAny(Integer a, Integer b, Integer c)
        {
            var g = InnerGcd(a, b, out var x0, out var y0);
            if (c % g != 0)
                return null;
            x0 *= c.IntegerDiv(g).Alias(out var cg);
            y0 *= cg;
            return (x0, y0);
        }
        
        
        // a x + b y = c
        internal static (Integer x, Integer y)? Solve(Integer a, Integer b, Integer c)
        {
            NormalInfo info;
            (a, b, c, info) = NormalInfo.Normalize(a, b, c);

            if (FindAny(a, b, c) is not var (x, y))
                return null;
            
            while (true)
            {
                var newX = x - b;
                var newY = y + a;
                if (newX.Abs() + newY.Abs() >= x.Abs() + y.Abs())
                    break;
                x = newX;
                y = newY;
            }
            
            while (true)
            {
                var newX = x + b;
                var newY = y - a;
                if (newX.Abs() + newY.Abs() >= x.Abs() + y.Abs())
                    break;
                x = newX;
                y = newY;
            }

            
            return info.Compensate(x, y);
        }

        private struct NormalInfo
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

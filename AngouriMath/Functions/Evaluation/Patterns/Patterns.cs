using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AngouriMath
{
    using PatType = Entity.PatType;
    internal class Patterns
    {
        static Pattern any1 = new Pattern(0, PatType.COMMON);
        static Pattern any2 = new Pattern(1, PatType.COMMON);
        static Pattern any3 = new Pattern(2, PatType.COMMON);
        static Pattern const1 = new Pattern(3, PatType.NUMBER);
        static Pattern const2 = new Pattern(4, PatType.NUMBER);
        static Pattern const3 = new Pattern(5, PatType.NUMBER);
        static Pattern var1 = new Pattern(6, PatType.VARIABLE);
        static Pattern var2 = new Pattern(7, PatType.VARIABLE);
        static Pattern var3 = new Pattern(8, PatType.VARIABLE);
        static Pattern func1 = new Pattern(9, PatType.FUNCTION);
        static Pattern func2 = new Pattern(10, PatType.FUNCTION);
        static Pattern func3 = new Pattern(11, PatType.FUNCTION);
        private static int InternNumber = 10000;
        static Pattern Num(double a) => new Pattern(++InternNumber, PatType.NUMBER, a.ToString(CultureInfo.InvariantCulture));
        internal static readonly Dictionary<Pattern, Entity> rules = new Dictionary<Pattern, Entity> {
            // (a * f(x)) * g(x) = a * (f(x) * g(x))
            { (const1 * func1) * func2, (func1 * func2) * const1 },

            // a / (b / c) = a * c / b
            { any1 / (any2 / any3), any1 * any3 / any2 },

            // a * (b / c) = (a * b) / c
            { any1 * (any2 / any3), (any1 * any2) / any3 },

            // (a * f(x)) * b = (a * b) * f(x)
            { (const1 * func1) * const2, (const1 * const2) * func1 },

            // (a * f(x)) * (b * g(x)) = (a * b) * (f(x) * g(x))
            { (const1 * func1) * (const2 * func2), (func1 * func2) * (const1 * const2) },

            // (f(x) + {}) + g(x) = (f(x) + g(x)) + {}
            { (func1 + any1) + func2, (func1 + func2) + any1 },

            // g(x) + (f(x) + {}) = (f(x) + g(x)) + {}
            { func2 + (func1 + any1), (func1 + func2) + any1 },

            // sin(:)^2 + cos(:)^2 = 1
            { Powf.PHang(Sinf.PHang(any1), Num(2)) + Powf.PHang(Cosf.PHang(any1), Num(2)), 1 },
            { Powf.PHang(Cosf.PHang(any1), Num(2)) + Powf.PHang(Sinf.PHang(any1), Num(2)), 1 },

            // x * a = a * x
            { var1 * const1, const1 * var1},

            // a + x = x + a
            { const1 + var1, var1 + const1},

            // f(x) * a = a * f(x)
            { func1 * const1, const1 * func1},

            // a + f(x) = f(x) + a
            { const1 + func1, func1 + const1},

            // a * x + b * x = (a + b) * x
            { const1 * var1 + const2 * var1, (const1 + const2) * var1 },

            // a * x - b * x = (a - b) * x
            { const1 * var1 - const2 * var1, (const1 - const2) * var1 },

            // {1} * {2} + {1} * {3} = {1} * ({2} + {3})
            { any1 * any2 + any1 * any3, any1 * (any2 + any3) },

            // {1} * {2} - {1} * {3} = {1} * ({2} - {3})
            { any1 * any2 - any1 * any3, any1 * (any2 - any3) },

            // x * x = x ^ 2
            { var1 * var1, Powf.PHang(var1, Num(2)) },

            // x * {} ^ {} = {} ^ {} * x
            { var1 * Powf.PHang(any1, any2), Powf.PHang(any1, any2) * var1 },

            // {} ^ n * {}
            { Powf.PHang(any1, any2) * any1, Powf.PHang(any1, any2 + Num(1)) },

            // {} ^ n * {} ^ m = {} ^ (n + m)
            { Powf.PHang(any1, any2) * Powf.PHang(any1, any3), Powf.PHang(any1, any2 + any3) },

            // {} ^ n / {} ^ m = {} ^ (n - m)
            { Powf.PHang(any1, any2) / Powf.PHang(any1, any3), Powf.PHang(any1, any2 - any3) },

            // ({} ^ {}) ^ {} = {} ^ ({} * {})
            { Powf.PHang(Powf.PHang(any1, any2), any3), Powf.PHang(any1, any2 * any3) },

            // c ^ log(c, a) = a
            { Powf.PHang(const1, Logf.PHang(any1, const1)), any1 },

            // (a * x) * b
            { (const1 * var1) * const2, (const1 * const2) * var1 },

            // (x + a) + b
            { (var1 + const1) + const2, var1 + (const1 + const2) },

            // b + (x + a)
            { const2 + (var1 + const1), var1 + (const1 + const2) },

            // (a * x) ^ c = a^c * x^c
            { Powf.PHang(const1 * var1, const2), Powf.PHang(const1, const2) * Powf.PHang(var1, const2) },

            // sin({}) * cos({}) = 1/2 * sin(2{})
            { Sinf.PHang(any1) * Cosf.PHang(any1), Num(1.0/2) * Sinf.PHang(Num(2) * any1) },
            { Cosf.PHang(any1) * Sinf.PHang(any1), Num(1.0/2) * Sinf.PHang(Num(2) * any1) },

            // (x - {}) (x + {}) = x2 - {}2
            { (var1 - any1) * (var1 + any1), Powf.PHang(var1, Num(2)) - Powf.PHang(any1, Num(2)) },
            { (var1 + any1) * (var1 - any1), Powf.PHang(var1, Num(2)) - Powf.PHang(any1, Num(2)) },

            // a / a
            { any1 / any1, 1},

            // (a * c) / c
            { (any1 * any2) / any2, any1 },
            { (any2 * any1) / any2, any1 },
            { (any1 * any2) / (any2 * any3), any1 / any3 },
            { (any1 * any2) / (any3 * any2), any1 / any3 },
            { (any2 * any1) / (any2 * any3), any1 / any3 },
            { (any2 * any1) / (any3 * any2), any1 / any3 },
        };
    }
}

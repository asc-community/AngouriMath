using System;
using System.Collections.Generic;
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
        private static int InternNumber = 10000;
        static Pattern Num(double a) => new Pattern(++InternNumber, PatType.NUMBER, a.ToString());
        static VariableEntity x = new VariableEntity("4");
        internal static Dictionary<Pattern, Entity> patterns = new Dictionary<Pattern, Entity> {
            // a / (b / c) = a * c / b
            { any1 / (any2 / any3), any1 * any3 / any2 },

            // sin(:)^2 + cos(:)^2 = 1
            { Powf.PHang(Sinf.PHang(any1), Num(2)) + Powf.PHang(Cosf.PHang(any1), Num(2)), 1 },
            { Powf.PHang(Cosf.PHang(any1), Num(2)) + Powf.PHang(Sinf.PHang(any1), Num(2)), 1 },

            // x * a + a * x
            { var1 * const1, const1 * var1},

            // a * x + b * x = (a + b) * x
            { any1 * var1 + any2 * var1, (any1 + any2) * var1 },

            // x * x = x ^ 2
            { var1 * var1, Powf.PHang(var1, 2) },

            // x * {} ^ {} = {} ^ {} * x
            { var1 * Powf.PHang(any1, any2), Powf.PHang(any1, any2) * var1 },

            // {} ^ n * {}
            { Powf.PHang(any1, any2) * any1, Powf.PHang(any1, any2 + Num(1)) },

            // {} ^ n * {} ^ m = {} ^ (n + m)
            { Powf.PHang(any1, any2) * Powf.PHang(any1, any3), Powf.PHang(any1, any2 + any3) },

            // ({} ^ {}) ^ {} = {} ^ ({} * {})
            { Powf.PHang(Powf.PHang(any1, any2), any3), Powf.PHang(any1, any2 * any3) },

            // c ^ log(c, a) = a
            { Powf.PHang(const1, Logf.PHang(any1, const1)), any1 }
        };
    }
}

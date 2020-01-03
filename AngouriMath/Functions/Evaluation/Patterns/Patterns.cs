using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AngouriMath
{
    using PatType = Entity.PatType;
    class RuleList : Dictionary<Pattern, Entity> { }
    internal static class Patterns
    {
        static readonly Pattern any1 = new Pattern(100, PatType.COMMON);
        static readonly Pattern any2 = new Pattern(101, PatType.COMMON);
        static readonly Pattern any3 = new Pattern(102, PatType.COMMON);
        static readonly Pattern any4 = new Pattern(103, PatType.COMMON);
        static readonly Pattern const1 = new Pattern(200, PatType.NUMBER);
        static readonly Pattern const2 = new Pattern(201, PatType.NUMBER);
        static readonly Pattern var1 = new Pattern(300, PatType.VARIABLE);
        static readonly Pattern func1 = new Pattern(400, PatType.FUNCTION);
        static readonly Pattern func2 = new Pattern(401, PatType.FUNCTION);
        private static int InternNumber = 10000;
        static Pattern Num(double a) => new Pattern(++InternNumber, PatType.NUMBER, a.ToString(CultureInfo.InvariantCulture));
        internal static readonly RuleList CommonRules = new RuleList {
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
            { any1 * any2 + any3 * any1, any1 * (any2 + any3) },
            { any2 * any1 + any3 * any1, any1 * (any2 + any3) },
            { any2 * any1 + any1 * any3, any1 * (any2 + any3) },
            { any1 + any1 * any2, any1 * (Num(1) + any2) },
            { any1 + any2 * any1, any1 * (Num(1) + any2) },
            { any1 * any2 + any1, any1 * (Num(1) + any2) },
            { any2 * any1 + any1, any1 * (Num(1) + any2) },
            { any1 + any1, Num(2) * any1 },              

            // {1} * {2} - {1} * {3} = {1} * ({2} - {3})
            { any1 * any2 - any1 * any3, any1 * (any2 - any3) },

            // x * x = x ^ 2
            { var1 * var1, Powf.PHang(var1, Num(2)) },

            // x * {} ^ {} = {} ^ {} * x
            { var1 * Powf.PHang(any1, any2), Powf.PHang(any1, any2) * var1 },

            // {} ^ n * {}
            { Powf.PHang(any1, any2) * any1, Powf.PHang(any1, any2 + Num(1)) },
            { any1 * Powf.PHang(any1, any2), Powf.PHang(any1, any2 + Num(1)) },

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
            { const2 * (const1 * var1), (const1 * const2) * var1 },
            { (const1 * func1) * const2, (const1 * const2) * func1 },
            { const2 * (const1 * func1), (const1 * const2) * func1 },

            // (x + a) + b
            { (var1 + const1) + const2, var1 + (const1 + const2) },

            // b + (x + a)
            { const2 + (var1 + const1), var1 + (const1 + const2) },

            // x * a * x
            { any1 * (any1 * any2), (any1 * any1) * any2 },
            { any1 * (any2 * any1), (any1 * any1) * any2 },
            { (any1 * any2) * any1, (any1 * any1) * any2 },
            { (any2 * any1) * any1, (any1 * any1) * any2 },
            { Powf.PHang(any1, any3) * (any1 * any2), (Powf.PHang(any1, any3 + Num(1))) * any2 },
            { Powf.PHang(any1, any3) * (any2 * any1), (Powf.PHang(any1, any3 + Num(1))) * any2 },
            { (any1 * any2) * Powf.PHang(any1, any3), (Powf.PHang(any1, any3 + Num(1))) * any2 },
            { (any2 * any1) * Powf.PHang(any1, any3), (Powf.PHang(any1, any3 + Num(1))) * any2 },
            
            // {1} ^ n * {2} ^ n = ({1} * {2}) ^ n
            { Powf.PHang(any1, any3) * Powf.PHang(any2, any3), Powf.PHang(any1 * any2, any3) },
            
            // (a * x) ^ c = a^c * x^c
            { Powf.PHang(const1 * any1, const2), Powf.PHang(const1, const2) * Powf.PHang(any1, const2) },

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

            // ({1} - {2}) / ({2} - {1})
            { (any1 - any2) / (any2 - any1), -1 },

            // ({1} + {2}) / ({2} + {1})
            { (any1 + any2) / (any2 + any1), 1 },

            // a / (b * {1})
            { const1 / (const2 * any1), (const1 / const2) / any1 },
            { const1 / (any1 * const2), (const1 / const2) / any1 },

            // arc1({}) + arc2({}) = pi/2
            { Arcsinf.PHang(any1) + Arccosf.PHang(any1), MathS.pi / 2 },
            { Arccosf.PHang(any1) + Arcsinf.PHang(any1), MathS.pi / 2 },
            { Arctanf.PHang(any1) + Arccotanf.PHang(any1), MathS.pi / 2 },
            { Arccotanf.PHang(any1) + Arctanf.PHang(any1), MathS.pi / 2 },

            // arcfunc(func(x)) = x
            { Arcsinf.PHang(Sinf.PHang(any1)), any1 },
            { Arccosf.PHang(Cosf.PHang(any1)), any1 },
            { Arctanf.PHang(Tanf.PHang(any1)), any1 },
            { Arccotanf.PHang(Cotanf.PHang(any1)), any1 },

            // func(arcfunc(x)) = x
            { Sinf.PHang(Arcsinf.PHang(any1)), any1 },
            { Cosf.PHang(Arccosf.PHang(any1)), any1 },
            { Tanf.PHang(Arctanf.PHang(any1)), any1 },
            { Cotanf.PHang(Arccotanf.PHang(any1)), any1 },

            // a * (b * {}) = (a * b) * {}
            { const1 * (const2 * any1), (const1 * const2) * any1 },

            { any1 - any1, Num(0) },

            // {1} - {2} * {1}
            { any1 - any2 * any1, any1 * (Num(1) - any2) },
            { any1 - any1 * any2, any1 * (Num(1) - any2) },

            // {1} ^ (-1) = 1 / {1}
            { Powf.PHang(any1, Num(-1)), 1 / any1 },

            // -1 * {1} + {2} = {2} - {1}
            { Num(-1) * any1 + any2, any2 - any1 },

            // a / {} * b
            { any1 / any2 * any3, any1 * any3 / any2}
        };

        internal static readonly RuleList ExpandRules = new RuleList
        {
            // (any1 + any2)2
            { Powf.PHang(any1 + any2, Num(2)), Powf.PHang(any1, Num(2)) + Num(2) * any1 * any2 + Powf.PHang(any2, Num(2)) },

            // (any1 - any2)2
            { Powf.PHang(any1 + any2, Num(2)), Powf.PHang(any1, Num(2)) - Num(2) * any1 * any2 + Powf.PHang(any2, Num(2)) },

            // ({1} - {2}) ({1} + {2}) = x2 - {}2
            { (any1 - any2) * (any1 + any2), Powf.PHang(any1, Num(2)) - Powf.PHang(any2, Num(2)) },
            { (any1 + any2) * (any1 - any2), Powf.PHang(any1, Num(2)) - Powf.PHang(any2, Num(2)) },

            // ({1} + {2}) * ({3} + {4}) = {1}{3} + {1}{4} + {2}{3} + {2}{4}
            { (any1 + any2) * (any3 + any4), any1 * any3 + any1 * any4 + any2 * any3 + any2 * any4 },
            { (any1 - any2) * (any3 + any4), any1 * any3 + any1 * any4 - any2 * any3 - any2 * any4 },
            { (any1 + any2) * (any3 - any4), any1 * any3 - any1 * any4 + any2 * any3 - any2 * any4 },
            { (any1 - any2) * (any3 - any4), any1 * any3 - any1 * any4 - any2 * any3 + any2 * any4 },
            
            // {1} * ({2} + {3})
            { any1 * (any2 + any3), any1 * any2 + any1 * any3 },
            { any1 * (any2 - any3), any1 * any2 - any1 * any3 },
            { (any2 + any3) * any1, any1 * any2 + any1 * any3 },
            { (any2 - any3) * any1, any1 * any2 - any1 * any3 },
        };

        internal static readonly RuleList CollapseRules = new RuleList
        {
            // {1}2 - {2}2
            { Powf.PHang(any1, const1) - Powf.PHang(any2, const2), 
                (Powf.PHang(any1, const1 / Num(2)) - Powf.PHang(any2, const2 / Num(2))) *
                (Powf.PHang(any1, const1 / Num(2)) + Powf.PHang(any2, const2 / Num(2))) }
        };
    }
}

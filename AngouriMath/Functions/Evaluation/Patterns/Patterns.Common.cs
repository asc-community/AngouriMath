﻿/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        internal static Entity DivisionPreparingRules(Entity x) => x switch
        {
            Mulf(var any1, Divf(Integer(1), var any2)) => any1 / any2,
            Divf(Mulf(Number const1, var any1), var any2) => const1 * (any1 / any2),
            Mulf(Divf(Number const1, var any1), var any2) => const1 * (any2 / any1),
            _ => x
        };

        internal static Entity CommonRules(Entity x) => x switch
        {
            // (a * f(x)) * g(x) = a * (f(x) * g(x))
            Mulf(Mulf(Number const1, Function func1), Function func2) => func1 * func2 * const1,

            // (a/b) * (c/d) = (a*c)/(b*d)
            Mulf(Divf(var any1, var any2), Divf(var any3, var any4)) => any1 * any3 / (any2 * any4),

            // a / (b / c) = a * c / b
            Divf(var any1, Divf(var any2, var any3)) => any1 * any3 / any2,

            // a / b * c = a * c / b
            Mulf(Divf(var any1, var any2), var any3) => any1 * any3 / any2,

            // a * {1} / b
            Divf(Mulf(Number const1, var any1), Number const2) => const1 / const2 * any1,

            // a / b / c = a / (b * c)
            Divf(Divf(var any1, var any2), var any3) => any1 / (any2 * any3),

            // a * (b / c) = (a * b) / c
            Mulf(var any1, Divf(var any2, var any3)) => any1 * any2 / any3,

            // (a * f(x)) * b = (a * b) * f(x)
            Mulf(Mulf(Number const1, Function func1), Number const2) => const1 * const2 * func1,
            Mulf(Number const2, Mulf(Number const1, Function func1)) => const1 * const2 * func1,

            // (a * f(x)) * (b * g(x)) = (a * b) * (f(x) * g(x))
            Mulf(Mulf(Number const1, Function func1), Mulf(Number const2, Function func2)) =>
                func1 * func2 * (const1 * const2),

            // (f(x) + {}) + g(x) = (f(x) + g(x)) + {}
            Sumf(Sumf(Function func1, var any1), Function func2) => func1 + func2 + any1,

            // g(x) + (f(x) + {}) = (f(x) + g(x)) + {}
            Sumf(Function func2, Sumf(Function func1, var any1)) => func1 + func2 + any1,

            // x * a = a * x
            Mulf(Variable var1, Number const1) => const1 * var1,

            // a + x = x + a
            Sumf(Number const1, Variable var1) => var1 + const1,

            // f(x) * a = a * f(x)
            Mulf(Function func1, Number const1) => const1 * func1,

            // a + f(x) = f(x) + a
            Sumf(Number const1, Function func1) => func1 + const1,

            // a * x + b * x = (a + b) * x
            Sumf(Mulf(Number const1, Variable var1), Mulf(Number const2, Variable var1a))
                when var1 == var1a => (const1 + const2) * var1,

            // a * x - b * x = (a - b) * x
            Minusf(Mulf(Number const1, Variable var1), Mulf(Number const2, Variable var1a))
                when var1 == var1a => (const1 - const2) * var1,

            // {1} * {2} + {1} * {3} = {1} * ({2} + {3})
            Sumf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any2, var any1), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any2, var any1), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 + any2),
            Sumf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 + any2),
            Sumf(Mulf(var any1, var any2), var any1a) when any1 == any1a => any1 * (1 + any2),
            Sumf(Mulf(var any2, var any1), var any1a) when any1 == any1a => any1 * (1 + any2),
            Sumf(var any1, var any1a) when any1 == any1a => 2 * any1,

            Minusf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any2, var any1), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any2, var any1), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 - any2),
            Minusf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 - any2),
            Minusf(Mulf(var any1, var any2), var any1a) when any1 == any1a => any1 * (any2 - 1),
            Minusf(Mulf(var any2, var any1), var any1a) when any1 == any1a => any1 * (any2 - 1),
            Minusf(var any1, var any1a) when any1 == any1a => 0,

            // {1} / {2} + {1} * {3} = {1} * (1 / {2} + {3})
            Sumf(Divf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (1 / any2 + any3),
            Sumf(Divf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (1 / any2 + any3),
            Sumf(Mulf(var any2, var any1), Divf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + 1 / any3),
            Sumf(Mulf(var any1, var any2), Divf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + 1 / any3),
            Sumf(var anyButNot1 and not Integer(1), Divf(var anyButNot1a, var any2))
                when anyButNot1 == anyButNot1a => anyButNot1 * (1 + 1 / any2),
            Sumf(Divf(var anyButNot1 and not Integer(1), var any2), var anyButNot1a)
                when anyButNot1 == anyButNot1a => anyButNot1 * (1 + 1 / any2),

            // {1} * {2} - {1} * {3} = {1} * ({2} - {3})
            Minusf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),

            // x * x = x ^ 2
            Mulf(var any1, var any1a) when any1 == any1a => new Powf(any1, 2),

            // (a * x) * b
            Mulf(Mulf(Number const1, Variable var1), Number const2) => const1 * const2 * var1,
            Mulf(Number const2, Mulf(Number const1, Variable var1)) => const1 * const2 * var1,

            // (x + a) + b
            Sumf(Sumf(Variable var1, Number const1), Number const2) => var1 + (const1 + const2),

            // b + (x + a)
            Sumf(Number const2, Sumf(Variable var1, Number const1)) => var1 + (const1 + const2),

            // x * a * x
            Mulf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(Mulf(var any1, var any2), var any1a) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(Mulf(var any2, var any1), var any1a) when any1 == any1a => new Powf(any1, 2) * any2,

            // -1 * {1} + {2} = {2} - {1}
            Sumf(Mulf(Integer(-1), var any1), var any2) => any2 - any1,
            Sumf(var any1, Mulf(Integer(-1), var any2)) => any1 - any2,

            // (x - {}) (x + {}) = x2 - {}2
            Mulf(Minusf(Variable var1, var any1), Sumf(Variable var1a, var any1a))
                when (var1, any1) == (var1a, any1a) => new Powf(var1, 2) - new Powf(any1, 2),
            Mulf(Sumf(Variable var1, var any1), Minusf(Variable var1a, var any1a))
                when (var1, any1) == (var1a, any1a) => new Powf(var1, 2) - new Powf(any1, 2),

            // a / a
            Divf(var any1, var any1a) when any1 == any1a => 1,

            // (a * c) / c
            Divf(Mulf(var any1, var any2), var any2a) when any2 == any2a => any1,
            Divf(Mulf(var any2, var any1), var any2a) when any2 == any2a => any1,
            Divf(Mulf(var any1, var any2), Mulf(var any2a, var any3)) when any2 == any2a => any1 / any3,
            Divf(Mulf(var any1, var any2), Mulf(var any3, var any2a)) when any2 == any2a => any1 / any3,
            Divf(Mulf(var any2, var any1), Mulf(var any2a, var any3)) when any2 == any2a => any1 / any3,
            Divf(Mulf(var any2, var any1), Mulf(var any3, var any2a)) when any2 == any2a => any1 / any3,

            // ({1} - {2}) / ({2} - {1})
            Divf(Minusf(var any1, var any2), Minusf(var any2a, var any1a)) when (any1, any2) == (any1a, any2a) => -1,

            // ({1} + {2}) / ({2} + {1})
            Divf(Sumf(var any1, var any2), Sumf(var any2a, var any1a)) when (any1, any2) == (any1a, any2a) => 1,

            // a / (b * {1})
            Divf(Number const1, Mulf(Number const2, var any1)) => const1 / const2 / any1,
            Divf(Number const1, Mulf(var any1, Number const2)) => const1 / const2 / any1,

            // a * (b * {}) = (a * b) * {}
            Mulf(Number const1, Mulf(Number const2, var any1)) => const1 * const2 * any1,

            // {1} - {2} * {1}
            Minusf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 - any2),
            Minusf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 - any2),

            Sumf(var any1, Sumf(var any2, var any1a)) when any1 == any1a => 2 * any1 + any2,
            Sumf(var any1, Sumf(var any1a, var any2)) when any1 == any1a => 2 * any1 + any2,
            Sumf(Sumf(var any2, var any1), var any1a) when any1 == any1a => 2 * any1 + any2,
            Sumf(Sumf(var any1, var any2), var any1a) when any1 == any1a => 2 * any1 + any2,

            Minusf(Sumf(var any1, var any2), var any1a) when any1 == any1a => any2,
            Minusf(Sumf(var any2, var any1), var any1a) when any1 == any1a => any2,
            Minusf(var any1, Sumf(var any2, var any1a)) when any1 == any1a => -any2,
            Minusf(var any1, Sumf(var any1a, var any2)) when any1 == any1a => -any2,

            Sumf(var any1, Minusf(var any2, var any1a)) when any1 == any1a => any2,
            Sumf(var any1, Minusf(var any1a, var any2)) when any1 == any1a => 2 * any1 - any2,
            Sumf(Minusf(var any2, var any1), var any1a) when any1 == any1a => any2,
            Sumf(Minusf(var any1, var any2), var any1a) when any1 == any1a => 2 * any1 - any2,

            Minusf(var any1, Minusf(var any2, var any1a)) when any1 == any1a => 2 * any1 - any2,
            Minusf(var any1, Minusf(var any1a, var any2)) when any1 == any1a => any2,
            Minusf(Minusf(var any2, var any1), var any1a) when any1 == any1a => any2 - 2 * any1,
            Minusf(Minusf(var any1, var any2), var any1a) when any1 == any1a => -any2,

            Signumf(Signumf(var any1)) => new Signumf(any1),
            Absf(Absf(var any1)) => new Absf(any1),
            Absf(Signumf(_)) => 1,
            Signumf(Absf(_)) => 1,

            Mulf(Absf(var any1), Absf(var any2)) => new Absf(any1 * any2),
            Divf(Absf(var any1), Absf(var any2)) => new Absf(any1 / any2),
            Mulf(Signumf(var any1), Absf(var any1a)) when any1 == any1a => any1,
            Mulf(Absf(var any1a), Signumf(var any1)) when any1 == any1a => any1,

            _ => x
        };
    }
}
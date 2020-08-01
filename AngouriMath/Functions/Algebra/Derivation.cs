
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */



 using System;
using System.Collections.Generic;
 using AngouriMath.Core.Sys.Interfaces;
 using PeterO.Numbers;

namespace AngouriMath
{
    using DeriveTable = Dictionary<string, Func<List<Entity>, VariableEntity, Entity>>;

    // Adding function Derive to Entity
    public abstract partial class Entity : ILatexiseable
    {
        /// <summary>
        /// Derives over x power times
        /// </summary>
        public Entity Derive(VariableEntity x, EInteger power)
        {
            var ent = this;
            for (var _ = 0; _ < power; _++)
                ent = ent.Derive(x);
            return ent;
        }

        /// <summary>
        /// Derivation over a variable (without simplification)
        /// </summary>
        /// <param name="x">
        /// The variable to derive over
        /// </param>
        /// <returns></returns>
        public Entity Derive(VariableEntity x)
        {
            if (IsLeaf)
                switch (this)
                {
                    case VariableEntity _ when Name == x.Name:
                        return new NumberEntity(1);
                    case NumberEntity { Value: var value } when value == Core.Numerix.RealNumber.NaN:
                        return this;
                    default:
                        return new NumberEntity(0);
                }
            else
                return MathFunctions.InvokeDerive(Name, Children, x);
        }
    }

    // Adding invoke table for eval
    internal static partial class MathFunctions
    {
        internal static readonly DeriveTable deriveTable = new DeriveTable();

        public static Entity InvokeDerive(string typeName, List<Entity> args, VariableEntity x)
        {
            return deriveTable[typeName](args, x);
        }
    }


    // Each function and operator processing
    internal static partial class Sumf
    {
        // (a + b)' = a' + b'
        public static Entity Derive(List<Entity> args, VariableEntity variable) {
            MathFunctions.AssertArgs(args.Count, 2);
            var a = args[0];
            var b = args[1];
            return a.Derive(variable) + b.Derive(variable);
        }
        
    }
    internal static partial class Minusf
    {
        // (a - b)' = a' - b'
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var a = args[0];
            var b = args[1];
            return a.Derive(variable) - b.Derive(variable);
        }
    }
    internal static partial class Mulf
    {
        // (a * b)' = a' * b + b' * a
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var a = args[0];
            var b = args[1];
            return a.Derive(variable) * b + b.Derive(variable) * a;
        }
    }

    internal static partial class Divf
    {
        // (a / b)' = (a' * b - b' * a) / b^2
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var a = args[0];
            var b = args[1];
            return (a.Derive(variable) * b - b.Derive(variable) * a) / (b.Pow(2));
        }
    }
    internal static partial class Powf
    {
        // (a ^ b)' = e ^ (ln(a) * b) * (a' * b / a + ln(a) * b')
        // (a ^ const)' = const * a ^ (const - 1)
        // (const ^ b)' = e^b * b'
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var a = args[0];
            var b = args[1];
            if (b is NumberEntity { Value:var value })
            {
                var cons = value - 1;
                var res = b * (a.Pow(cons)) * a.Derive(variable);
                return res;
            }
            else if(a is NumberEntity)
            {
                return a.Pow(b) * MathS.Ln(a) * b.Derive(variable);
            }
            else
                return a.Pow(b) * (a.Derive(variable) * b / a + MathS.Ln(a) * b.Derive(variable));
        }
    }
    internal static partial class Sinf
    {
        // sin(a) = cos(a) * a'
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return a.Cos() * a.Derive(variable);
        }
    }
    internal static partial class Cosf
    {
        // sin(a) = -sin(a) * a'
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return -1 * a.Sin() * a.Derive(variable);
        }
    }
    internal static partial class Tanf
    {
        // tan(a) = 1 / cos(a) ^ 2
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return 1 / a.Cos().Pow(2) * a.Derive(variable);
        }
    }
    internal static partial class Cotanf
    {
        // tan(a) = -1 / sin(a) ^ 2
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return -1 / a.Sin().Pow(2) * a.Derive(variable);
        }
    }
    internal static partial class Logf
    {
        // log(a, b) = (ln(a) / ln(b))' = (ln(a)' * ln(b) - ln(a) * ln(b)') / ln(b)^2 = (a' / a * ln(b) - ln(a) * b' / b) / ln(b)^2
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var a = args[0];
            var b = args[1];
            return (a.Derive(variable) / a * MathS.Ln(b) - MathS.Ln(a) * b.Derive(variable) / b) / (MathS.Ln(b).Pow(2));
        }
    }
    internal static partial class Arcsinf
    {
        // arcsin(x)' = 1 / sqrt(1 - x^2)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return 1 / MathS.Sqrt(1 - MathS.Sqr(a)) * a.Derive(variable);
        }
    }
    internal static partial class Arccosf
    {
        // arccos(x)' = -1 / sqrt(1 - x^2)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return -1 / MathS.Sqrt(1 - MathS.Sqr(a)) * a.Derive(variable);
        }
    }
    internal static partial class Arctanf
    {
        // arctan(x)' = 1 / (1 + x^2)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return 1 / (1 + MathS.Sqr(a)) * a.Derive(variable);
        }
    }
    internal static partial class Arccotanf
    {
        // arccotan(x)' = -1 / (1 + x^2)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return -1 / (1 + MathS.Sqr(a)) * a.Derive(variable);
        }
    }
    internal static partial class Factorialf
    {
        // (x!)' = Γ(x + 1) polygamma(0, x + 1)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            // TODO: Implementation of symbolic gamma function and polygamma functions needed
            return Core.Numerix.RealNumber.NaN;
        }
    }

    internal static partial class Derivativef
    {
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 3);
            if (args[1] == variable)
                return MathS.Derivative(args[0], args[1], args[2] + 1);
            else
                return MathS.Derivative(MathS.Derivative(args[0], args[1], args[2]), variable, 1);
        }
    }

    internal static partial class Integralf
    {
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 3);
            if (args[1] == variable)
                return MathS.Integral(args[0], args[1], args[2] - 1);
            else
                return MathS.Integral(MathS.Derivative(args[0], args[1], args[2]), variable, 1);
        }
    }
}

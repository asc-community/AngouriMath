using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    using DeriveTable = Dictionary<string, Func<List<Entity>, VariableEntity, Entity>>;

    // Adding function Derive to Entity
    public abstract partial class Entity
    {
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
            {
                if (this.entType == Entity.EntType.VARIABLE && this.Name == x.Name)
                    return new NumberEntity(1);
                else
                    return new NumberEntity(0);
            }
            else
                return MathFunctions.InvokeDerive(Name, Children, x);
        }
    }

    // Adding invoke table for eval
    public static partial class MathFunctions
    {
        internal static readonly DeriveTable deriveTable = new DeriveTable();

        public static Entity InvokeDerive(string typeName, List<Entity> args, VariableEntity x)
        {
            return deriveTable[typeName](args, x);
        }
    }


    // Each function and operator processing
    public static partial class Sumf
    {
        // (a + b)' = a' + b'
        public static Entity Derive(List<Entity> args, VariableEntity variable) {
            MathFunctions.AssertArgs(args.Count, 2);
            var a = args[0];
            var b = args[1];
            return a.Derive(variable) + b.Derive(variable);
        }
        
    }
    public static partial class Minusf
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
    public static partial class Mulf
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

    public static partial class Divf
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
    public static partial class Powf
    {
        // (a ^ b)' = e ^ (ln(a) * b) * (a' * b / a + ln(a) * b')
        // (a ^ const)' = const * a ^ (const - 1)
        // (const ^ b)' = e^b * b'
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var a = args[0];
            var b = args[1];
            if (b.entType == Entity.EntType.NUMBER)
            {
                var cons = (b as NumberEntity).Value - 1;
                var res = b * (a.Pow(cons)) * a.Derive(variable);
                return res;
            }
            else if(a.entType == Entity.EntType.NUMBER)
            {
                return a.Pow(b) * MathS.Ln(a) * b.Derive(variable);
            }
            else
                return a.Pow(b) * (a.Derive(variable) * b / a + MathS.Ln(a) * b.Derive(variable));
        }
    }
    public static partial class Sinf
    {
        // sin(a) = cos(a) * a'
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return a.Cos() * a.Derive(variable);
        }
    }
    public static partial class Cosf
    {
        // sin(a) = -sin(a) * a'
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return -1 * a.Sin() * a.Derive(variable);
        }
    }
    public static partial class Tanf
    {
        // tan(a) = 1 / cos(a) ^ 2
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return 1 / a.Cos().Pow(2) * a.Derive(variable);
        }
    }
    public static partial class Cotanf
    {
        // tan(a) = -1 / sin(a) ^ 2
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return -1 / a.Sin().Pow(2) * a.Derive(variable);
        }
    }
    public static partial class Logf
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
    public static partial class Arcsinf
    {
        // arcsin(x)' = 1 / sqrt(1 - x^2)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return 1 / MathS.Sqrt(1 - MathS.Sqr(a)) * a.Derive(variable);
        }
    }
    public static partial class Arccosf
    {
        // arccos(x)' = -1 / sqrt(1 - x^2)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return -1 / MathS.Sqrt(1 - MathS.Sqr(a)) * a.Derive(variable);
        }
    }
    public static partial class Arctanf
    {
        // arctan(x)' = 1 / (1 + x^2)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return 1 / (1 + MathS.Sqr(a)) * a.Derive(variable);
        }
    }
    public static partial class Arccotanf
    {
        // arccotan(x)' = -1 / (1 + x^2)
        public static Entity Derive(List<Entity> args, VariableEntity variable)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var a = args[0];
            return -1 / (1 + MathS.Sqr(a)) * a.Derive(variable);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

/*
 * Adding evaluation of expression
 * var n1 = MathS.Num(3);
 * var n2 = MathS.Num(4);
 * var c = n1 + n2;
 * Console.WriteLine(c.Eval());
 */

namespace MathSharp
{
    using EvalTable = Dictionary<string, Func<List<Entity>, Entity>>;

    // Adding function Eval to Entity
    public abstract partial class Entity
    {
        public Entity Simplify()
        {
            if (IsLeaf)
            {
                return this;
            }
            else
                return MathFunctions.InvokeEval(Name, children);
        }
    }

    // Adding invoke table for eval
    public static partial class MathFunctions
    {
        internal static EvalTable evalTable = new EvalTable();

        public static Entity InvokeEval(string typeName, List<Entity> args)
        {
            return evalTable[typeName](args);
        }

        public static bool IsOneNumber(List<Entity> args, NumberEntity e)
        {
            return (args[0] is NumberEntity && (args[0] as NumberEntity).Value == e.Value ||
                    args[1] is NumberEntity && (args[1] as NumberEntity).Value == e.Value);
                    
        }
        public static Entity GetAnotherEntity(List<Entity> args, NumberEntity e)
        {
            if (args[0] is NumberEntity && (args[0] as NumberEntity).Value == e.Value)
                return args[1];
            else
                return args[0];
        }
    }

    // Each function and operator processing
    public static partial class Sumf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].Simplify();
            var r2 = args[1].Simplify();
            args = new List<Entity>() { r1, r2 };
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value + (r2 as NumberEntity).Value);
            else
                if (MathFunctions.IsOneNumber(args, 0))
                    return MathFunctions.GetAnotherEntity(args, 0);
                else
                    return r1 + r2;
        }
    }
    public static partial class Minusf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].Simplify();
            var r2 = args[1].Simplify();
            args = new List<Entity>() { r1, r2 };
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value - (r2 as NumberEntity).Value);
            else
                if (r1 != r2)
                    return r1 - r2;
                else
                    return 0;
        }
    }
    public static partial class Mulf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].Simplify();
            var r2 = args[1].Simplify();
            args = new List<Entity>() { r1, r2 };
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value * (r2 as NumberEntity).Value);
            else
                if (MathFunctions.IsOneNumber(args, 1))
                    return MathFunctions.GetAnotherEntity(args, 1);
                else if (MathFunctions.IsOneNumber(args, 0))
                    return 0;
                else
                    return r1 * r2;
        }
    }
    public static partial class Divf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].Simplify();
            var r2 = args[1].Simplify();
            args = new List<Entity>() { r1, r2 };
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value / (r2 as NumberEntity).Value);
            else
                if (r1 is NumberEntity && (r1 as NumberEntity).Value == 0)
                    return 0;
                else if (r2 is NumberEntity && (r2 as NumberEntity).Value == 1)
                    return r1;
                else
                    return r1 / r2;
        }
    }
    public static partial class Powf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].Simplify();
            var r2 = args[1].Simplify();
            args = new List<Entity>() { r1, r2 };
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity(Math.Pow((r1 as NumberEntity).Value, (r2 as NumberEntity).Value));
            else
                if (r1 is NumberEntity && ((r1 as NumberEntity).Value == 0 || (r1 as NumberEntity).Value == 1))
                    return r1;
                else if (r2 is NumberEntity && (r2 as NumberEntity).Value == 1)
                    return r1;
                else
                    return r1 ^ r2; 
        }
    }
    public static partial class Sinf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].Simplify();
            if (r is NumberEntity)
                return new NumberEntity(Math.Sin((r as NumberEntity).Value));
            else
                return r.Sin();
        }
    }
    public static partial class Cosf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].Simplify();
            if (r is NumberEntity)
                return new NumberEntity(Math.Cos((r as NumberEntity).Value));
            else
                return r.Cos();
        }
    }

    public static partial class Logf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r = args[0].Simplify();
            var n = args[1].Simplify();
            args = new List<Entity>() { r, n };
            if (r is NumberEntity && n is NumberEntity)
                return new NumberEntity(Math.Log((r as NumberEntity).Value, (n as NumberEntity).Value));
            else
                return r.Log(args[1]);
        }
    }
}

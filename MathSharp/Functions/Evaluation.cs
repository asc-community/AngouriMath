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
    // Adding function Eval to Entity
    public abstract partial class Entity
    {
        public Entity Eval()
        {
            if (IsLeaf)
            {
                return this;
            }
            else
                return MathFunctions.Invoke(Name, children);
        }
    }

    public static partial class Sumf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.Assert(args.Count, 2);
            var r1 = args[0].Eval();
            var r2 = args[1].Eval();
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value + (r2 as NumberEntity).Value);
            else
                return r1 + r2;
        }
    }
    public static partial class Minusf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.Assert(args.Count, 2);
            var r1 = args[0].Eval();
            var r2 = args[1].Eval();
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value - (r2 as NumberEntity).Value);
            else
                return r1 - r2;
        }
    }
    public static partial class Mulf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.Assert(args.Count, 2);
            var r1 = args[0].Eval();
            var r2 = args[1].Eval();
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value * (r2 as NumberEntity).Value);
            else
                return r1 * r2;
        }
    }
    public static partial class Divf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.Assert(args.Count, 2);
            var r1 = args[0].Eval();
            var r2 = args[1].Eval();
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value / (r2 as NumberEntity).Value);
            else
                return r1 / r2;
        }
    }
    public static partial class Powf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.Assert(args.Count, 2);
            var r1 = args[0].Eval();
            var r2 = args[1].Eval();
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity(Math.Pow((r1 as NumberEntity).Value, (r2 as NumberEntity).Value));
            else
                return r1 ^ r2;
        }
    }
    public static partial class Sinf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.Assert(args.Count, 1);
            var r = args[0].Eval();
            if (r is NumberEntity)
                return new NumberEntity(Math.Sin((r as NumberEntity).Value));
            else
                return r.Sin();
        }
    }
    public static partial class Cosf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.Assert(args.Count, 1);
            var r = args[0].Eval();
            if (r is NumberEntity)
                return new NumberEntity(Math.Cos((r as NumberEntity).Value));
            else
                return r.Cos();
        }
    }
}

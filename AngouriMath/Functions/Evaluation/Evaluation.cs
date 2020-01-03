using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;


namespace AngouriMath
{
    using EvalTable = Dictionary<string, Func<List<Entity>, Entity>>;
    using SortLevel = Core.TreeAnalysis.TreeAnalyzer.SortLevel;

    // Adding function Eval to Entity
    public abstract partial class Entity
    {
        /// <summary>
        /// Expands an equation trying to eliminate all the parentheses ( e. g. 2 * (x + 3) = 2 * x + 2 * 3 )
        /// </summary>
        /// <returns>
        /// An expanded Entity
        /// </returns>
        public Entity Expand() => Expand(2);


        /// <summary>
        /// Collapses an equation trying to eliminate as many power-uses as possible ( e. g. x * 3 + x * y = x * (3 + y) )
        /// </summary>
        /// <returns></returns>
        public Entity Collapse() => Collapse(2);


        /// <summary>
        /// Expands an equation trying to eliminate all the parentheses ( e. g. 2 * (x + 3) = 2 * x + 2 * 3 )
        /// </summary>
        /// <param name="level">
        /// The number of iterations (increase this argument in case if some parentheses remain)
        /// </param>
        /// <returns>
        /// An expanded Entity
        /// </returns>
        public Entity Expand(int level) => level <= 1 ? TreeAnalyzer.Replace(Patterns.ExpandRules, this) : TreeAnalyzer.Replace(Patterns.ExpandRules, this).Expand(level - 1);

        /// <summary>
        /// Collapses an equation trying to eliminate as many power-uses as possible ( e. g. x * 3 + x * y = x * (3 + y) )
        /// </summary>
        /// <param name="level">
        /// The number of iterations (increase this argument if some collapse operations are still available)
        /// </param>
        /// <returns></returns>
        public Entity Collapse(int level) => level <= 1 ? TreeAnalyzer.Replace(Patterns.CollapseRules, this) : TreeAnalyzer.Replace(Patterns.CollapseRules, this).Expand(level - 1);

        /// <summary>
        /// Simplifies an equation (e. g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x)
        /// </summary>
        /// <returns></returns>
        public Entity Simplify() => Simplify(3);

        /// <summary>
        /// Simplifies an equation (e. g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x)
        /// The only difference between SimplifyIntelli and Simplify is number of iterations
        /// which you can specify on your own.
        /// </summary>
        /// <returns></returns>
        public Entity SimplifyIntelli() => Simplify(6);

        /// <summary>
        /// Simplifies an equation (e. g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x)
        /// </summary>
        /// <param name="level">
        /// Increase this argument if you think the equation should be simplified better
        /// </param>
        /// <returns></returns>
        public Entity Simplify(int level)
        {
            var stage1 = this.InnerSimplify();
            Entity res = stage1;
            for (int i = 0; i < level; i++)
            {
                switch (i)
                {
                    case 0: res = res.Sort(SortLevel.HIGH_LEVEL); break;
                    case 2: res = res.Sort(SortLevel.MIDDLE_LEVEL); break;
                    case 4: res = res.Sort(SortLevel.LOW_LEVEL); break;
                }
                res = TreeAnalyzer.Replace(Patterns.CommonRules, res).InnerSimplify();
            }
            return res;
        }
        internal Entity InnerSimplify()
        {
            if (IsLeaf)
            {
                return this;
            }
            else
                return MathFunctions.InvokeEval(Name, Children);
        }

        /// <summary>
        /// Simplification synonim. Recommended to use in case of computing a concrete number
        /// </summary>
        /// <returns></returns>
        public Entity Eval() => Simplify(0);
    }

    // Adding invoke table for eval
    public static partial class MathFunctions
    {
        internal static readonly EvalTable evalTable = new EvalTable();

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
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            args = new List<Entity> { r1, r2 };
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
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            if (r1 is NumberEntity && r2 is NumberEntity)
            {
                return new NumberEntity((r1 as NumberEntity).Value - (r2 as NumberEntity).Value);
            }
            else if (r1 == r2)
            {
                return 0;
            }
            else if (r2 == 0)
            {
                return r1;
            }
            else
            {
                return r1 - r2;
            }
        }
    }
    public static partial class Mulf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            args = new List<Entity> { r1, r2 };
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
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity((r1 as NumberEntity).Value / (r2 as NumberEntity).Value);
            else
                if (r1 == 0)
                    return 0;
                else if (r2 == 1)
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
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            if (r1 is NumberEntity && r2 is NumberEntity)
                return new NumberEntity(Number.Pow((r1 as NumberEntity).Value, (r2 as NumberEntity).Value));
            else
                if (r1 == 0 || r1 == 1)
                    return r1;
                else if (r2 == 1)
                    return r1;
                else if (r2 == 0)
                    return 1;
                else
                    return r1.Pow(r2);
        }
    }
    public static partial class Sinf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            if (r is NumberEntity)
                return new NumberEntity(Number.Sin((r as NumberEntity).Value));
            else
                return r.Sin();
        }
    }
    public static partial class Cosf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            if (r is NumberEntity)
                return new NumberEntity(Number.Cos((r as NumberEntity).Value));
            else
                return r.Cos();
        }
    }
    public static partial class Tanf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            if (r is NumberEntity)
                return new NumberEntity(Number.Tan((r as NumberEntity).Value));
            else
                return r.Tan();
        }
    }
    public static partial class Cotanf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            if (r is NumberEntity)
                return new NumberEntity(Number.Cotan((r as NumberEntity).Value));
            else
                return r.Cotan();
        }
    }

    public static partial class Logf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r = args[0].InnerSimplify();
            var n = args[1].InnerSimplify();
            args = new List<Entity> { r, n };
            if (r is NumberEntity && n is NumberEntity)
                return new NumberEntity(Number.Log((r as NumberEntity).Value, (n as NumberEntity).Value));
            else
                if (r == n)
                {
                    return 1;
                }
                else if (r == 1)
                {
                    return 0;
                }
                else
                {
                    return r.Log(args[1]);
                }
        }
    }
    
    public static partial class Arcsinf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();
            if (arg is NumberEntity)
                return new NumberEntity(Number.Arcsin((arg as NumberEntity).Value));
            else
                return Arcsinf.Hang(arg);
        }
    }
    public static partial class Arccosf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();
            if (arg is NumberEntity)
                return new NumberEntity(Number.Arccos((arg as NumberEntity).Value));
            else
                return Arccosf.Hang(arg);
        }
    }
    public static partial class Arctanf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();
            if (arg is NumberEntity)
                return new NumberEntity(Number.Arctan((arg as NumberEntity).Value));
            else
                return Arctanf.Hang(arg);
        }
    }
    public static partial class Arccotanf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();
            if (arg is NumberEntity)
                return new NumberEntity(Number.Arccotan((arg as NumberEntity).Value));
            else
                return Arccotanf.Hang(arg);
        }
    }
}

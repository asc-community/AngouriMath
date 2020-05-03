
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



﻿using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Sys.Items.Tensors;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Evaluation.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
 using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    public static partial class MathS
    {
        internal static readonly Dictionary<string, Number> ConstantList = new Dictionary<string, Number>
        {
            { "pi", Math.PI },
            { "e", Math.E }
        };

        /// <summary>
        /// Determines whether something is a variable or contant, e. g.
        /// x + 3 - is not a constant
        /// e - is a constant
        /// x - is not a constant
        /// pi + 4 - is not a constant (but pi is)
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static bool IsConstant(Entity expr) => (expr.entType == Entity.EntType.VARIABLE && MathS.ConstantList.ContainsKey(expr.Name));
        public static bool CanBeEvaluated(Entity expr)
        {
            if (expr.IsTensoric())
                return false;
            if (expr.entType == Entity.EntType.VARIABLE)
                return IsConstant(expr);
            for (int i = 0; i < expr.Children.Count; i++)
                if (!CanBeEvaluated(expr.Children[i]))
                    return false;
            return true;
        }
    }
}

namespace AngouriMath
{
    using EvalTable = Dictionary<string, Func<List<Entity>, Entity>>;
    using SortLevel = Core.TreeAnalysis.TreeAnalyzer.SortLevel;

    // Adding function Eval to Entity
    public abstract partial class Entity : ILatexiseable
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
        public Entity Collapse(int level) => level <= 1 ? TreeAnalyzer.Replace(Patterns.CollapseRules, this) : TreeAnalyzer.Replace(Patterns.CollapseRules, this).Collapse(level - 1);

        /// <summary>
        /// Simplifies an equation (e. g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x)
        /// </summary>
        /// <returns></returns>
        public Entity Simplify() => Simplificator.Simplify(this);

        /// <summary>
        /// Simplifies an equation (e. g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x)
        /// </summary>
        /// <param name="level">
        /// Increase this argument if you think the equation should be simplified better
        /// </param>
        /// <returns></returns>
        public Entity Simplify(int level) => Simplificator.Simplify(this, level);

        /// <summary>
        /// Finds all alternative forms of an expression sorted by their complexity
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public Set Alternate(int level) => Simplificator.Alternate(this, level);
        internal abstract Entity InnerSimplify();

        /// <summary>
        /// Fast substitution of some mathematical constants,
        /// e. g. pi * e + 3 => 3.1415 * 2.718 + 3
        /// </summary>
        /// <returns></returns>
        public Entity SubstituteConstants()
        {
            if (MathS.IsConstant(this))
                return MathS.ConstantList[this.Name];
            Entity curr = this.DeepCopy();  // Instead of copying in substitute, 
            // we better copy first and then do inPlace substitute
            foreach (var pair in MathS.ConstantList)
                curr.Substitute(pair.Key, pair.Value, inPlace: true);
            return curr;
        }

        /// <summary>
        /// Simplification synonim. Recommended to use in case of computing a 
        /// concrete number.
        /// </summary>
        /// <returns>
        /// Number since new version
        /// </returns>
        public Number Eval() => SubstituteConstants().Simplify(0).GetValue();

        /// <summary>
        /// Collapses the entire expression into a tensor if possible
        /// ( x y ) + 1 => ( x+1 y+1 )
        /// 
        /// ( 1 2 ) + ( 3 4 ) => ( 4 6 ) vectors pointwise
        /// 
        ///              (( 3 )
        /// (( 1 2 3 )) x ( 4 ) => (( 26 )) Matrices dot product
        ///               ( 5 ))
        ///               
        /// ( 1 2 ) x ( 1 3 ) => ( 1 6 ) Vectors pointwise
        /// </summary>
        /// <returns></returns>
        public Tensor EvalTensor()
        {
            if (!IsTensoric())
                throw new MathSException(
                    "To eval an expression as a tensor, it should contain at least one tensor (matrix, vector)");
            if (entType == EntType.TENSOR)
            {
                Tensor result = this as Tensor;
                TensorFunctional.Apply(result, p => MathS.CanBeEvaluated(p) ? p.Eval() : p);
                return result;
            }
            var r = DeepCopy();
            TensorFunctional.__EvalTensor(ref r);
            if (r.entType == EntType.TENSOR)
            {
                Tensor result = r as Tensor;
                TensorFunctional.Apply(result, p => MathS.CanBeEvaluated(p) ? p.Eval() : p);
                return result;
            }
            else
                throw new SysException("Unexpected behaviour");
        }
    }

    // Adding invoke table for eval
    internal static partial class MathFunctions
    {
        internal static readonly EvalTable evalTable = new EvalTable();

        internal static Entity InvokeEval(string typeName, List<Entity> args)
        {
            return evalTable[typeName](args);
        }

        internal static bool IsOneNumber(List<Entity> args, NumberEntity e)
        {
            return (args[0].entType == Entity.EntType.NUMBER && (args[0] as NumberEntity).Value == e.Value ||
                    args[1].entType == Entity.EntType.NUMBER && (args[1] as NumberEntity).Value == e.Value);
                    
        }
        internal static Entity GetAnotherEntity(List<Entity> args, NumberEntity e)
        {
            if (args[0].entType == Entity.EntType.NUMBER && (args[0] as NumberEntity).Value == e.Value)
                return args[1];
            else
                return args[0];
        }
    }

    // Each function and operator processing
    internal static partial class Sumf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            args = new List<Entity> { r1, r2 };
            if (r1.entType == Entity.EntType.NUMBER && r2.entType == Entity.EntType.NUMBER)
                return new NumberEntity((r1 as NumberEntity).Value + (r2 as NumberEntity).Value);
            else
                if (MathFunctions.IsOneNumber(args, 0))
                    return MathFunctions.GetAnotherEntity(args, 0);
                else
                    return r1 + r2;
        }
    }
    internal static partial class Minusf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            if (r1.entType == Entity.EntType.NUMBER && r2.entType == Entity.EntType.NUMBER)
                return new NumberEntity((r1 as NumberEntity).Value - (r2 as NumberEntity).Value);
            else if (r1 == r2)
                return 0;
            else if (r2 == 0)
                return r1;
            else
                return r1 - r2;
        }
    }
    internal static partial class Mulf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            args = new List<Entity> { r1, r2 };
            if (r1.entType == Entity.EntType.NUMBER && r2.entType == Entity.EntType.NUMBER)
                return new NumberEntity((r1 as NumberEntity).Value * (r2 as NumberEntity).Value);
            else if (MathFunctions.IsOneNumber(args, 1))
                return MathFunctions.GetAnotherEntity(args, 1);
            else if (MathFunctions.IsOneNumber(args, 0))
                return 0;
            else
                return r1 * r2;

        }
    }
    internal static partial class Divf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            if (r1.entType == Entity.EntType.NUMBER && r2.entType == Entity.EntType.NUMBER)
                return new NumberEntity((r1 as NumberEntity).Value / (r2 as NumberEntity).Value);
            else if (r1 == 0)
                return 0;
            else if (r2 == 1)
                return r1;
            else
                return r1 / r2;
        }
    }
    internal static partial class Powf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            if (r1.entType == Entity.EntType.NUMBER && r2.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Pow((r1 as NumberEntity).Value, (r2 as NumberEntity).Value));
            else if (r1 == 0 || r1 == 1)
                return r1;
            else if (r2 == 1)
                return r1;
            else if (r2 == 0)
                return 1;
            else
                return r1.Pow(r2);
        }
    }
    internal static partial class Sinf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            if (r.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Sin((r as NumberEntity).Value));
            else
                return r.Sin();
        }
    }
    internal static partial class Cosf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            if (r.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Cos((r as NumberEntity).Value));
            else
                return r.Cos();
        }
    }
    internal static partial class Tanf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            if (r.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Tan((r as NumberEntity).Value));
            else
                return r.Tan();
        }
    }
    internal static partial class Cotanf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            if (r.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Cotan((r as NumberEntity).Value));
            else
                return r.Cotan();
        }
    }

    internal static partial class Logf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r = args[0].InnerSimplify();
            var n = args[1].InnerSimplify();
            args = new List<Entity> { r, n };
            if (r.entType == Entity.EntType.NUMBER && n.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Log((r as NumberEntity).Value, (n as NumberEntity).Value));
            else if (r == n)
                return 1;
            else if (r == 1)
                return 0;
            else
                return r.Log(args[1]);
        }
    }
    
    internal static partial class Arcsinf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();
            if (arg.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Arcsin((arg as NumberEntity).Value));
            else
                return Arcsinf.Hang(arg);
        }
    }
    internal static partial class Arccosf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();
            if (arg.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Arccos((arg as NumberEntity).Value));
            else
                return Arccosf.Hang(arg);
        }
    }
    internal static partial class Arctanf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();
            if (arg.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Arctan((arg as NumberEntity).Value));
            else
                return Arctanf.Hang(arg);
        }
    }
    internal static partial class Arccotanf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();
            if (arg.entType == Entity.EntType.NUMBER)
                return new NumberEntity(Number.Arccotan((arg as NumberEntity).Value));
            else
                return Arccotanf.Hang(arg);
        }
    }
}

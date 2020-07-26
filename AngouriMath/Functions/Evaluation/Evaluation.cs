
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



using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Sys.Items.Tensors;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Evaluation.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.Sys.Interfaces;
using EvalTable = System.Collections.Generic.Dictionary<string, System.Func<System.Collections.Generic.List<AngouriMath.Entity>, AngouriMath.Entity>>;
namespace AngouriMath
{
    public static partial class MathS
    {
        internal static readonly Dictionary<string, ComplexNumber> ConstantList = new Dictionary<string, ComplexNumber>
        {
            { "pi", MathS.DecimalConst.pi },
            { "e", MathS.DecimalConst.e },
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
        public static bool IsConstant(Entity expr) => (expr is VariableEntity && MathS.ConstantList.ContainsKey(expr.Name));
        public static bool CanBeEvaluated(Entity expr)
        {
            if (expr.IsTensoric())
                return false;
            if (expr is VariableEntity)
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

    // Adding function Eval to Entity
    public abstract partial class Entity : ILatexiseable
    {
        const int DefaultLevel = 2;
        /// <summary>
        /// Expands an equation trying to eliminate all the parentheses ( e. g. 2 * (x + 3) = 2 * x + 2 * 3 )
        /// </summary>
        /// <param name="level">
        /// The number of iterations (increase this argument in case if some parentheses remain)
        /// </param>
        /// <returns>
        /// An expanded Entity if it wasn't too complicated,
        /// current entity otherwise
        /// To change the limit use MathS.Settings.MaxExpansionTermCount
        /// </returns>
        public Entity Expand(int level = DefaultLevel)
        {
            static Entity Expand_(Entity e, int level) =>
                level <= 1
                ? TreeAnalyzer.Replace(Patterns.ExpandRules, e)
                : Expand_(TreeAnalyzer.Replace(Patterns.ExpandRules, e), level - 1);
            var expChildren = new List<Entity>();
            foreach (var linChild in TreeAnalyzer.LinearChildrenOverSum(this))
            {
                var exp = TreeAnalyzer.SmartExpandOver(linChild, entity => true);
                if (exp is { })
                    expChildren.AddRange(exp);
                else
                    return this; // if one is too complicated, return the current one
            }
            var expanded = TreeAnalyzer.MultiHangBinary(expChildren, "sumf", Const.PRIOR_SUM);
            return Expand_(expanded, level).InnerSimplify();
        }

        /// <summary>
        /// Collapses an equation trying to eliminate as many power-uses as possible ( e. g. x * 3 + x * y = x * (3 + y) )
        /// </summary>
        /// <param name="level">
        /// The number of iterations (increase this argument if some collapse operations are still available)
        /// </param>
        /// <returns></returns>
        public Entity Collapse(int level = DefaultLevel) => level <= 1
            ? TreeAnalyzer.Replace(Patterns.CollapseRules, this)
            : TreeAnalyzer.Replace(Patterns.CollapseRules, this).Collapse(level - 1);

        /// <summary>
        /// Simplifies an equation (e. g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x)
        /// </summary>
        /// <param name="level">
        /// Increase this argument if you think the equation should be simplified better
        /// </param>
        /// <returns></returns>
        public Entity Simplify(int level = DefaultLevel) => Simplificator.Simplify(this, level);

        /// <summary>
        /// Finds all alternative forms of an expression sorted by their complexity
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public Set Alternate(int level) => Simplificator.Alternate(this, level);

        internal abstract Entity InnerEval();
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
            Entity curr = this.DeepCopy(); // Instead of copying in substitute, 
            // we better copy first and then do inPlace substitute
            foreach (var pair in MathS.ConstantList)
                curr.Substitute(pair.Key, pair.Value, inPlace: true);
            return curr;
        }

        /// <summary>
        /// Simplification synonym. Recommended to use in case of computing a concrete number.
        /// </summary>
        /// <returns>
        /// <see cref="ComplexNumber"/> since new version
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this entity cannot be represented as a simple number.
        /// <see cref="MathS.CanBeEvaluated(Entity)"/> should be used to check beforehand.
        /// </exception>
        public ComplexNumber Eval() =>
            SubstituteConstants().InnerEval() is NumberEntity { Value: var value } ? value :
                throw new InvalidOperationException
                    ($"Result cannot be represented as a simple number! Use {nameof(MathS.CanBeEvaluated)} to check beforehand.");

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
        /// <exception cref="InvalidOperationException">
        /// Thrown when this entity cannot be represented as a tensor.
        /// <see cref="IsTensoric"/> should be used to check beforehand.
        /// </exception>
        public Tensor EvalTensor()
        {
            if (!IsTensoric())
                throw new InvalidOperationException(
                    "To evaluate an expression as a tensor, it should contain at least one tensor (including matrices and vectors). " +
                    $"Use {nameof(IsTensoric)} to check beforehand.");
            if (this is Tensor result)
            {
                TensorFunctional.Apply(result, p => MathS.CanBeEvaluated(p) ? p.Eval() : p);
                return result;
            }
            var r = DeepCopy();
            TensorFunctional.__EvalTensor(ref r);
            if (r is Tensor t)
            {
                TensorFunctional.Apply(t, p => MathS.CanBeEvaluated(p) ? p.Eval() : p);
                return t;
            }
            else
                throw new SysException("Unexpected behaviour");
        }
    }

    // Adding invoke table for eval & simplify
    internal static partial class MathFunctions
    {
        internal static readonly EvalTable evalTable = new EvalTable();
        internal static readonly EvalTable simplifyTable = new EvalTable();

        internal static Entity InvokeSimplify(string typeName, List<Entity> args)
        {
            return simplifyTable[typeName](args);
        }

        internal static Entity InvokeEval(string typeName, List<Entity> args)
        {
            return evalTable[typeName](args);
        }

        internal static bool IsOneNumber(List<Entity> args, NumberEntity e)
        {
            return args[0] is NumberEntity n0 && n0.Value == e.Value ||
                   args[1] is NumberEntity n1 && n1.Value == e.Value;

        }

        internal static Entity GetAnotherEntity(List<Entity> args, NumberEntity e)
        {
            if (args[0] is NumberEntity n && n.Value == e.Value)
                return args[1];
            else
                return args[0];
        }
    }
}


/*
 *
 * This class contains implementation for evaluation for all operators and functions
 *
 */


namespace AngouriMath
{
    // Each function and operator processing
    internal static partial class Sumf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerEval();
            var r2 = args[1].InnerEval();
            args = new List<Entity> { r1, r2 };
            if (r1 is NumberEntity n1 && r2 is NumberEntity n2)
                return new NumberEntity(n1.Value + n2.Value);
            else
                if (MathFunctions.IsOneNumber(args, 0))
                return MathFunctions.GetAnotherEntity(args, 0);
            else
                return r1 + r2;
        }
    }
    internal static partial class Minusf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerEval();
            var r2 = args[1].InnerEval();
            if (r1 is NumberEntity n1 && r2 is NumberEntity n2)
                return new NumberEntity(n1.Value - n2.Value);
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
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerEval();
            var r2 = args[1].InnerEval();
            args = new List<Entity> { r1, r2 };
            if (r1 is NumberEntity n1 && r2 is NumberEntity n2)
                return new NumberEntity(n1.Value * n2.Value);
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
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerEval();
            var r2 = args[1].InnerEval();
            if (r1 is NumberEntity n1 && r2 is NumberEntity n2)
                return new NumberEntity(n1.Value / n2.Value);
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
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerEval();
            var r2 = args[1].InnerEval();
            if (r1 is NumberEntity n1 && r2 is NumberEntity n2)
                return new NumberEntity(Number.Pow(n1.Value, n2.Value));
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
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerEval();
            if (r is NumberEntity n)
                return new NumberEntity(Number.Sin(n.Value));
            else
                return r.Sin();
        }
    }
    internal static partial class Cosf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerEval();
            if (r is NumberEntity n)
                return new NumberEntity(Number.Cos(n.Value));
            else
                return r.Cos();
        }
    }
    internal static partial class Tanf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerEval();
            if (r is NumberEntity n)
                return new NumberEntity(Number.Tan(n.Value));
            else
                return r.Tan();
        }
    }
    internal static partial class Cotanf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerEval();
            if (r is NumberEntity n)
                return new NumberEntity(Number.Cotan(n.Value));
            else
                return r.Cotan();
        }
    }

    internal static partial class Logf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r = args[0].InnerEval();
            var n = args[1].InnerEval();
            args = new List<Entity> { r, n };
            if (r is NumberEntity rn && n is NumberEntity nn)
                return new NumberEntity(Number.Log(rn.Value, nn.Value));
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
            var arg = args[0].InnerEval();
            if (arg is NumberEntity n)
                return new NumberEntity(Number.Arcsin(n.Value));
            else
                return Arcsinf.Hang(arg);
        }
    }
    internal static partial class Arccosf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerEval();
            if (arg is NumberEntity n)
                return new NumberEntity(Number.Arccos(n.Value));
            else
                return Arccosf.Hang(arg);
        }
    }
    internal static partial class Arctanf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerEval();
            if (arg is NumberEntity n)
                return new NumberEntity(Number.Arctan(n.Value));
            else
                return Arctanf.Hang(arg);
        }
    }
    internal static partial class Arccotanf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerEval();
            if (arg is NumberEntity n)
                return new NumberEntity(Number.Arccotan(n.Value));
            else
                return Arccotanf.Hang(arg);
        }
    }
    internal static partial class Factorialf
    {
        public static Entity Eval(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerEval();
            if (r is NumberEntity n)
                return new NumberEntity(Number.Factorial(n.Value));
            else
                return r.Sin();
        }
    }
}

namespace AngouriMath
{
    public abstract partial class Entity
    {
        internal ComplexNumber? __cachedEvaledValue = null;
    }
}

/*
 *
 * This class contains implementation for basic simplification for all operators and functions
 * This keeps all numbers rational or as an expression so no precision loss occurres
 *
 */

namespace AngouriMath
{
    internal static class InnerSimplifyAdditionalFunctional
    {
        internal static Entity KeepIfBad(ComplexNumber candidate, Entity ifAllBad, params ComplexNumber[] nums)
        {
            if (!candidate.IsFinite)
                return candidate;
            if (IsGood(candidate.Real, nums.Select(n => n.Real).ToArray(), false) &&
                IsGood(candidate.Imaginary, nums.Select(n => n.Imaginary).ToArray(), false))
                return candidate;
            else
                return ifAllBad;
        }

        internal static bool KeepIfBad(ComplexNumber candidate,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out Entity? res, bool disableIrrational, params ComplexNumber[] nums)
        {
            if (!candidate.IsFinite)
            {
                res = candidate;
                return true;
            }
            if (IsGood(candidate.Real, nums.Select(n => n.Real).ToArray(), disableIrrational) &&
                IsGood(candidate.Imaginary, nums.Select(n => n.Imaginary).ToArray(), disableIrrational))
            {
                res = candidate;
                return true;
            }
            else
            {
                res = null;
                return false;
            }
        }

        static bool IsGood(RealNumber cand, RealNumber[] nums, bool disableIrrational)
        {
            static int GetRank(RealNumber num) =>
                num == 0 ? 0 : num switch
                {
                    IntegerNumber _ => 0,
                    RationalNumber _ => 1,
                    RealNumber _ => 2
                };
            var minLevel = nums.Select(GetRank).Min();
            return cand is RationalNumber ||
                   (GetRank(cand) <= minLevel && !disableIrrational) ||
                (cand is RealNumber && !(cand is RationalNumber) && cand.Value.IsZero); // TODO: make im:0 downcastable
        }

        /// <summary>Test for exact value</summary>
        internal static bool IsRationalOrNonFiniteComplex(ComplexNumber num)
            => num.Real is RationalNumber && num.Imaginary == 0
            || num is RationalNumber
            || !num.Real.IsFinite && (num.Imaginary == 0 || !num.Imaginary.IsFinite);
    }

    // Each function and operator processing
    internal static partial class Sumf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();
            ComplexNumber? potentialResult = null;
            if (r1.__cachedEvaledValue is { } && r2.__cachedEvaledValue is { })
            {
                potentialResult = r1.__cachedEvaledValue + r2.__cachedEvaledValue;
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }
            args = new List<Entity> { r1, r2 };
            Entity result;
            if (r1 is NumberEntity { Value: var n1 } && r2 is NumberEntity { Value: var n2 })
            {
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(n1 + n2, r1 + r2, n1, n2);
            }
            else
                if (MathFunctions.IsOneNumber(args, 0))
                result = MathFunctions.GetAnotherEntity(args, 0);
            else
                result = r1 + r2;
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Minusf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (r1.__cachedEvaledValue is { } && r2.__cachedEvaledValue is { })
            {
                potentialResult = r1.__cachedEvaledValue - r2.__cachedEvaledValue;
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (r1 is NumberEntity { Value: var n1 } && r2 is NumberEntity { Value: var n2 })
            {
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(n1 - n2, r1 - r2, n1, n2);
            }
            else if (r1 == r2)
                result = 0;
            else if (r2 == 0)
                result = r1;
            else
                result = r1 - r2;
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Mulf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (r1.__cachedEvaledValue is { } && r2.__cachedEvaledValue is { })
            {
                potentialResult = r1.__cachedEvaledValue * r2.__cachedEvaledValue;
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            args = new List<Entity> { r1, r2 };
            if (MathFunctions.IsOneNumber(args, 1))
                result = MathFunctions.GetAnotherEntity(args, 1);
            else if (MathFunctions.IsOneNumber(args, 0))
                result = 0;
            else if (r1 is NumberEntity { Value: var n1 } && r2 is NumberEntity { Value: var n2 })
            {
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(n1 * n2, r1 * r2, n1, n2);
            }
            else
                result = r1 * r2;
            result.__cachedEvaledValue = potentialResult;
            return result;

        }
    }
    internal static partial class Divf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (r1.__cachedEvaledValue is { } && r2.__cachedEvaledValue is { })
            {
                potentialResult = r1.__cachedEvaledValue / r2.__cachedEvaledValue;
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (r1 is NumberEntity { Value: var n1 } && r2 is NumberEntity { Value: var n2 })
            {
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(n1 / n2, r1 / r2, n1, n2);
            }
            else if (r1 == 0)
                result = 0;
            else if (r2 == 1)
                result = r1;
            else
                result = r1 / r2;
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Powf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r1 = args[0].InnerSimplify();
            var r2 = args[1].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (r1.__cachedEvaledValue is { } && r2.__cachedEvaledValue is { })
            {
                potentialResult = Number.Pow(r1.__cachedEvaledValue, r2.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (r1 is NumberEntity { Value: var n1 } && r2 is NumberEntity { Value: var n2 })
            {
                // TODO: Consider cases like sqrt(12) which could be simplified to 2 sqrt(3)
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Pow(n1, n2), MathS.Pow(r1, r2), n1, n2);
            }
            else if (r1 == 0 || r1 == 1)
                result = r1;
            else if (r2 == 1)
                result = r1;
            else if (r2 == 0)
                result = 1;
            else if (r2 == -1)
                result = 1 / r1;
            else
                result = r1.Pow(r2);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Sinf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            ComplexNumber? evaled = r.__cachedEvaledValue;

            ComplexNumber? potentialResult = null;
            if (r.__cachedEvaledValue is { })
            {
                potentialResult = Number.Sin(r.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (evaled is { })
            {
                var n = evaled;
                if (InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Sin(n), out var res, true, n))
                    result = res;
                else if (Const.TrigonometryTableValues.PullSin(n, out res))
                    result = res;
                else
                    result = MathS.Sin(r);
            }
            else
                result = r.Sin();
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Cosf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            ComplexNumber? evaled = r.__cachedEvaledValue;

            ComplexNumber? potentialResult = null;
            if (r.__cachedEvaledValue is { })
            {
                potentialResult = Number.Cos(r.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (evaled is { })
            {
                var n = evaled;
                if (InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Cos(n), out var res, true, n))
                    result = res;
                else if (Const.TrigonometryTableValues.PullCos(n, out res))
                    result = res;
                else
                    result = r.Cos();
            }
            else
                result = r.Cos();
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Tanf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            ComplexNumber? evaled = r.__cachedEvaledValue;

            ComplexNumber? potentialResult = null;
            if (r.__cachedEvaledValue is { })
            {
                potentialResult = Number.Tan(r.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (evaled is { })
            {
                var n = evaled;
                if (InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Tan(n), out var res, true, n))
                    result = res;
                else if (Const.TrigonometryTableValues.PullTan(n, out res))
                    result = res;
                else
                    result = r.Tan();
            }
            else
                result = r.Tan();
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Cotanf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var r = args[0].InnerSimplify();
            ComplexNumber? evaled = r.__cachedEvaledValue;

            ComplexNumber? potentialResult = null;
            if (r.__cachedEvaledValue is { })
            {
                potentialResult = Number.Cotan(r.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (evaled is { })
            {
                var n = evaled;
                if (InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Cotan(n), out var res, true, n))
                    result = 1 / res;
                else if (Const.TrigonometryTableValues.PullTan(n, out res))
                    result = 1 / res;
                else
                    result = r.Cotan();
            }
            else
                result = r.Cotan();
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }

    internal static partial class Logf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var r = args[0].InnerSimplify();
            var n = args[1].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (r.__cachedEvaledValue is { } && n.__cachedEvaledValue is { })
            {
                potentialResult = Number.Log(r.__cachedEvaledValue, n.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            args = new List<Entity> { r, n };
            if (r is NumberEntity { Value: var n1 } && n is NumberEntity { Value: var n2 })
            {
                return InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Log(n1, n2), MathS.Log(r, n), n1, n2);
            }
            else if (r == n)
                result = 1;
            else if (r == 1)
                result = 0;
            else
                result = r.Log(args[1]);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }

    internal static partial class Arcsinf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (arg.__cachedEvaledValue is { })
            {
                potentialResult = Number.Arcsin(arg.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (arg is NumberEntity { Value: var n })
            {
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Arcsin(n), MathS.Arcsin(arg), n);
            }
            else
                result = Arcsinf.Hang(arg);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Arccosf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (arg.__cachedEvaledValue is { })
            {
                potentialResult = Number.Arccos(arg.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (arg is NumberEntity { Value: var n })
            {
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Arccos(n), MathS.Arccos(arg), n);
            }
            else
                result = Arccosf.Hang(arg);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Arctanf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (arg.__cachedEvaledValue is { })
            {
                potentialResult = Number.Arctan(arg.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (arg is NumberEntity { Value: var n })
            {
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Arctan(n), MathS.Arctan(arg), n);
            }
            else
                result = Arctanf.Hang(arg);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Arccotanf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (arg.__cachedEvaledValue is { })
            {
                potentialResult = Number.Arccotan(arg.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (arg is NumberEntity { Value: var n })
            {
                result = InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Arccotan(n), MathS.Arccotan(arg), n);
            }
            else
                result = Arccotanf.Hang(arg);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    internal static partial class Factorialf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            var arg = args[0].InnerSimplify();

            ComplexNumber? potentialResult = null;
            if (arg.__cachedEvaledValue is { })
            {
                potentialResult = Number.Factorial(arg.__cachedEvaledValue);
                if (InnerSimplifyAdditionalFunctional.IsRationalOrNonFiniteComplex(potentialResult))
                    return new NumberEntity(potentialResult) { __cachedEvaledValue = potentialResult };
            }

            Entity result;
            if (arg.__cachedEvaledValue is { } n)
            {
                if (InnerSimplifyAdditionalFunctional.KeepIfBad(Number.Factorial(n), out var res, true, n))
                    result = 1 / res;
                else if (n is RationalNumber { Value: { Numerator: var num, Denominator: var denom } } && denom.Equals(2))
                {
                    var en = (num + 1) / 2;
                    var positive = en > 0;
                    if (!positive) en = -en;
                    result =
                        positive
                        // (+n - 1/2)! = (2n-1)!/(2^(2n-1)(n-1)!)*sqrt(pi)
                        // also 2n-1 is the numerator
                        ? RationalNumber.Create(num.Factorial(), PeterO.Numbers.EInteger.FromInt32(2).Pow(num) * (en - 1).Factorial())
                        // (-n - 1/2)! = (-4)^n*n!/(2n)!*sqrt(pi)
                        : RationalNumber.Create(PeterO.Numbers.EInteger.FromInt32(-4).Pow(en) * en.Factorial(), (2 * en).Factorial());
                    result *= MathS.Sqrt(MathS.pi);
                }
                else
                    result = arg.Factorial();
            }
            else
                result = arg.Factorial();
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
}

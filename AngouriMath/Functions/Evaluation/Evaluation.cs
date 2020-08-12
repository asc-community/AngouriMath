
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
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Evaluation.Simplification;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AngouriMath.Convenience;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    public static partial class MathS
    {
        public static bool CanBeEvaluated(Entity expr) =>
            expr.All(e => e is not (Tensor or VariableEntity { IsConstant: false }));
    }

    // Adding function Eval to Entity
    public abstract partial record Entity : ILatexiseable
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
        /// To change the limit use <see cref="MathS.Settings.MaxExpansionTermCount"/>
        /// </returns>
        public Entity Expand(int level = DefaultLevel)
        {
            static Entity Expand_(Entity e, int level) =>
                level <= 1
                ? e.Replace(Patterns.ExpandRules)
                : Expand_(e.Replace(Patterns.ExpandRules), level - 1);
            var expChildren = new List<Entity>();
            foreach (var linChild in Sumf.LinearChildren(this))
            {
                var exp = TreeAnalyzer.SmartExpandOver(linChild, entity => true);
                if (exp is { })
                    expChildren.AddRange(exp);
                else
                    return this; // if one is too complicated, return the current one
            }
            var expanded = TreeAnalyzer.MultiHangBinary(expChildren, (a, b) => new Sumf(a, b));
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
            ? this.Replace(Patterns.CollapseRules)
            : this.Replace(Patterns.CollapseRules).Collapse(level - 1);

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
        /// e.g. pi * e + 3 => 3.1415 * 2.718 + 3
        /// </summary>
        public Entity SubstituteConstants() => Substitute(VariableEntity.ConstantList);

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
            SubstituteConstants().InnerEval() is ComplexNumber value ? value :
                throw new InvalidOperationException
                    ($"Result cannot be represented as a simple number! Use {nameof(MathS.CanBeEvaluated)} to check beforehand.");

        /// <summary>
        /// Finds out whether an expression contains at least one tensor
        /// </summary>
        /// <returns></returns>
        public bool IsTensoric() => this.OfType<Tensor>().Any();

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
                Tensor.Apply(result, p => MathS.CanBeEvaluated(p) ? p.Eval() : p);
                return result;
            }
            var r = DeepCopy();
            Tensor.__EvalTensor(ref r);
            if (r is Tensor t)
            {
                Tensor.Apply(t, p => MathS.CanBeEvaluated(p) ? p.Eval() : p);
                return t;
            }
            else
                throw new SysException("Unexpected behaviour");
        }
    }

    public partial record NumberEntity : Entity
    {
        internal override Entity InnerEval() => this;
    }
    public partial record VariableEntity : Entity
    {
        internal override Entity InnerEval() => this;
    }
    public partial record Tensor : Entity
    {
        internal override Entity InnerEval() => Elementwise(e => e.InnerEval());
    }
    // Each function and operator processing
    public partial record Sumf
    {
        internal override Entity InnerEval() => (Augend.InnerEval(), Addend.InnerEval()) switch
        {
            (ComplexNumber n1, ComplexNumber n2) => n1 + n2,
            (IntegerNumber(0), var n) => n,
            (var n, IntegerNumber(0)) => n,
            (var n1, var n2) => n1 == n2 ? 2 * n1 : n1 + n2
        };
    }
    public partial record Minusf
    {
        internal override Entity InnerEval() => (Subtrahend.InnerEval(), Minuend.InnerEval()) switch
        {
            (ComplexNumber n1, ComplexNumber n2) => n1 - n2,
            (IntegerNumber(0), var n) => -n,
            (var n, IntegerNumber(0)) => n,
            (var n1, var n2) => n1 == n2 ? 0 : n1 - n2
        };
    }
    public partial record Mulf
    {
        internal override Entity InnerEval() => (Multiplier.InnerEval(), Multiplicand.InnerEval()) switch
        {
            (ComplexNumber n1, ComplexNumber n2) => n1 * n2,
            (IntegerNumber(0), _) or (_, IntegerNumber(0)) => 0,
            (IntegerNumber(1), var n) => n,
            (var n, IntegerNumber(1)) => n,
            (var n1, var n2) => n1 == n2 ? new Powf(n1, 2) : n1 * n2
        };
    }
    public partial record Divf
    {
        internal override Entity InnerEval() => (Dividend.InnerEval(), Divisor.InnerEval()) switch
        {
            (ComplexNumber n1, ComplexNumber n2) => n1 / n2,
            (IntegerNumber(0), _) => 0,
            (var n, IntegerNumber(1)) => n,
            (var n1, var n2) => n1 == n2 ? 1 : n1 / n2
        };
    }
    public partial record Powf
    {
        internal override Entity InnerEval() => (Base.InnerEval(), Exponent.InnerEval()) switch
        {
            (ComplexNumber n1, ComplexNumber n2) => NumberEntity.Pow(n1, n2),
            (IntegerNumber(0 or 1) n1, _) => n1,
            (var n, IntegerNumber(1)) => n,
            (_, IntegerNumber(0)) => 1,
            (var n1, var n2) => new Powf(n1, n2)
        };
    }
    public partial record Sinf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Sin(n),
            var n => new Sinf(n)
        };
    }
    public partial record Cosf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Cos(n),
            var n => new Cosf(n)
        };
    }
    public partial record Tanf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Tan(n),
            var n => new Tanf(n)
        };
    }
    public partial record Cotanf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Cotan(n),
            var n => new Cotanf(n)
        };
    }

    public partial record Logf
    {
        internal override Entity InnerEval() => (Base.InnerEval(), Antilogarithm.InnerEval()) switch
        {
            (ComplexNumber n1, ComplexNumber n2) => NumberEntity.Log(n1, n2),
            (IntegerNumber(1), _) => 0,
            (var n1, var n2) => n1 == n2 ? 1 : (Entity)new Logf(n1, n2)
        };
    }

    public partial record Arcsinf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Arcsin(n),
            var n => new Arcsinf(n)
        };
    }
    public partial record Arccosf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Arccos(n),
            var n => new Arccosf(n)
        };
    }
    public partial record Arctanf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Arctan(n),
            var n => new Arctanf(n)
        };
}
    public partial record Arccotanf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Arccotan(n),
            var n => new Arccotanf(n)
        };
    }
    public partial record Factorialf
    {
        internal override Entity InnerEval() => Argument.InnerEval() switch
        {
            ComplexNumber n => NumberEntity.Factorial(n),
            var n => new Factorialf(n)
        };
}

    public partial record Derivativef
    {
        internal override Entity InnerEval() => (Expression.InnerEval(), Variable.InnerEval(), Iterations.InnerEval()) switch
        {
            (var expr, _, IntegerNumber(0)) => expr,
            // TODO: consider Integral for negative cases
            (var expr, VariableEntity var, IntegerNumber(var asInt, _)) => expr.Derive(var, asInt),
            _ => this
        };
    }

    public partial record Integralf
    {
        internal override Entity InnerEval() => (Expression.InnerEval(), Variable.InnerEval(), Iterations.InnerEval()) switch
        {
            (var expr, _, IntegerNumber(0)) => expr,
            // TODO: consider Derivative for negative cases
            (var expr, VariableEntity var, IntegerNumber(var asInt, _)) =>
                throw new NotImplementedException("Integration is not implemented yet"),
            _ => this
        };
    }

    public partial record Limitf
    {
        internal override Entity InnerEval() => ApproachFrom switch
        {
            Limits.ApproachFrom.Left => throw new NotImplementedException("1.1.0.4 will bring limits"), // TODO
            Limits.ApproachFrom.BothSides => throw new NotImplementedException("1.1.0.4 will bring limits"), // TODO
            Limits.ApproachFrom.Right => throw new NotImplementedException("1.1.0.4 will bring limits"), // TODO
            _ => this,
        };
    }
}

namespace AngouriMath
{
    public abstract partial record Entity
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
                num switch { IntegerNumber => 0, RationalNumber => 1, RealNumber => 2 };
            var minLevel = nums.Min(GetRank);
            return cand is RationalNumber ||
                   (GetRank(cand) <= minLevel && !disableIrrational) ||
                (cand is RealNumber and not RationalNumber && cand.Value.IsZero); // TODO: make im:0 downcastable
        }

        /// <summary>Test for exact value</summary>
        internal static bool IsRationalOrNonFiniteComplex(ComplexNumber num)
            => num.Real is RationalNumber && num.Imaginary == 0
            || num is RationalNumber
            || !num.Real.IsFinite && (num.Imaginary == 0 || !num.Imaginary.IsFinite);
    }

    // Each function and operator processing
    public partial record Sumf
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
    public partial record Minusf
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
    public partial record Mulf
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
    public partial record Divf
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
    public partial record Powf
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
    public partial record Sinf
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
    public partial record Cosf
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
    public partial record Tanf
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
    public partial record Cotanf
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

    public partial record Logf
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

    public partial record Arcsinf
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
                result = new Arcsinf(arg);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    public partial record Arccosf
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
                result = new Arccosf(arg);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    public partial record Arctanf
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
                result = new Arctanf(arg);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    public partial record Arccotanf
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
                result = new Arccotanf(arg);
            result.__cachedEvaledValue = potentialResult;
            return result;
        }
    }
    public partial record Factorialf
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

    public partial record Derivativef
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 3);
            var ent = args[0].InnerSimplify();
            var x = args[1].InnerSimplify();
            var pow = args[2].InnerSimplify();
            var def = MathS.Derivative(ent, x, pow);
            return x is VariableEntity var && pow is NumberEntity { Value: IntegerNumber { Value: var asInt } }
                   ? ent.Derive(var, asInt) : def;
        }
    }

    public partial record Integralf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 3);
            var ent = args[0].InnerSimplify();
            var x = args[1].InnerSimplify();
            var pow = args[2].InnerSimplify();
            var def = MathS.Integral(ent, x, pow);
            return x is VariableEntity var && pow is NumberEntity { Value: IntegerNumber { Value: var asInt } }
                   ? asInt.IsZero
                     ? ent
                     : throw new NotImplementedException("Integration is not implemented yet")
                   : def;
        }
    }

    public partial record Limitf
    {
        public static Entity Simplify(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 4);
            var expr = args[0].InnerSimplify();
            var x = args[1].InnerSimplify();
            var dest = args[2].InnerSimplify();
            var appr = args[3].InnerSimplify();
            Entity? lim = null;
            if (appr == IntegerNumber.MinusOne)
                throw new NotImplementedException("1.1.0.4 will bring limits"); // TODO
            else if (appr == IntegerNumber.Zero)
                throw new NotImplementedException("1.1.0.4 will bring limits"); // TODO
            else if (appr == IntegerNumber.One)
                throw new NotImplementedException("1.1.0.4 will bring limits"); // TODO
            else
                return new Limitf(expr, x, dest, appr);
        }
    }
}

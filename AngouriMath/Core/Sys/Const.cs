
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
using System.Text;
 using System.Security.Cryptography;
 using AngouriMath.Core.Numerix;
 using AngouriMath.Core.TreeAnalysis;
 using AngouriMath.Functions;
using System.Linq;

namespace AngouriMath
{
    /// <summary>
    /// This class contains some extra functions for different purposes
    /// </summary>
    public static partial class Const
    {
        public enum Priority
        {
            Sum = 2,
            Minus = 2,
            Mul = 4,
            Div = 4,
            Pow = 6,
            Func = 8,
            Var = 10,
            Num = 10,
        }
        internal static readonly string ARGUMENT_DELIMITER = ",";

        /// <summary>
        /// Used for generating linear children over sum
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        internal static OperatorEntity FuncIfSum(Entity child)
        {
            var res = new OperatorEntity("mulf", Const.Priority.Mul);
            res.AddChild(-1);
            res.AddChild(child);
            return res;
        }

        /// <summary>
        /// Used for generating linear children over product
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        internal static OperatorEntity FuncIfMul(Entity child)
        {
            var res = new OperatorEntity("powf", Const.Priority.Pow);
            res.AddChild(child);
            res.AddChild(-1);
            return res;
        }

        /// <summary>
        /// Returns SHA hashcode of a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string HashString(string input)
        {
            using var sha = new SHA256Managed();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] computedByteHash = sha.ComputeHash(bytes);
            return BitConverter.ToString(computedByteHash).Replace("-", String.Empty);
        }

        internal static Entity EvalIfCan(Entity a)
            => MathS.CanBeEvaluated(a) ? a.Eval() : a;

        public static readonly Func<Entity, int> DefaultComplexityCriteria = expr =>
        {
            var res = 0;

            // Number of nodes
            res += expr.Complexity;

            // Number of variables
            res += expr.Count(entity => entity is VariableEntity);

            // Number of divides
            res += expr.Count(entity => entity is Divf) / 2;

            // Number of negative powers
            res += expr.Count(entity => entity is Powf(_, RealNumber { IsNegative: true }));

            return res;
        };
    }
}

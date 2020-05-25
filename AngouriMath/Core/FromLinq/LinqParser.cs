
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



﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.FromString;
 using AngouriMath.Core.Numerix;

namespace AngouriMath.Core.FromLinq
{
    /// <summary>
    /// To parse linq lambda expressions into Entity
    /// </summary>
    internal class LinqParser
    {
        private readonly Expression src;

        internal LinqParser(Expression linq)
        {
            src = linq;
        }

        /// <summary>
        /// Parses the function interpreting "Math.Sqr" as sqr
        /// TODO
        /// </summary>
        /// <param name="linq"></param>
        /// <returns></returns>
        private static Entity InnerParse(Expression linq)
        {
            var unary = linq as UnaryExpression;
            var binary = linq as BinaryExpression;
            switch (linq.NodeType)
            {
                case ExpressionType.Lambda:
                    return InnerParse((linq as LambdaExpression).Body);
                case ExpressionType.Constant:
                    return new NumberEntity((decimal)(linq as ConstantExpression).Value);
                case ExpressionType.Parameter:
                    return new VariableEntity((linq as ParameterExpression).Name);
                case ExpressionType.Negate:
                    return -1 * InnerParse(unary.Operand);
                case ExpressionType.UnaryPlus:
                    return 1 * InnerParse(unary.Operand);
                case ExpressionType.Add:
                    return InnerParse(binary.Left) + InnerParse(binary.Right);
                case ExpressionType.Subtract:
                    return InnerParse(binary.Left) - InnerParse(binary.Right);
                case ExpressionType.Multiply:
                    return InnerParse(binary.Left) * InnerParse(binary.Right);
                case ExpressionType.Divide:
                    return InnerParse(binary.Left) / InnerParse(binary.Right);
                case ExpressionType.Power:
                    return MathS.Pow(InnerParse(binary.Left), InnerParse(binary.Right));
                case ExpressionType.Call:
                    var method = linq as MethodCallExpression;
                    var children = method.Arguments.Select(InnerParse).ToList();
                    var methodInfo = method.Method;
                    var name = methodInfo.Name.ToLower() + "f";
                    if (name == "powf") // The only operator that acts as function
                    {
                        var op = new OperatorEntity(name, Const.PRIOR_POW) {Children = children};
                        return op;
                    }
                    else if (SynonymFunctions.SynFunctions.ContainsKey(name))
                    {
                        return SynonymFunctions.SynFunctions[name](children);
                    }
                    else
                    {
                        var func = new FunctionEntity(name)
                        {
                            Children = children
                        };
                        return func;
                    }
                default:
                    throw new ParseException("Parse error");
            }
        }

        internal Entity Parse()
            => InnerParse(src);

        internal static Entity FromLinq(Expression expr)
        {
            var parser = new LinqParser(expr);
            return parser.Parse();
        }
    }
}
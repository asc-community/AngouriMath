using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.FromString;

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
                    return new NumberEntity(new Number((linq as ConstantExpression).Value));
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
    }
}
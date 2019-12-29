using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AngouriMath.Core.FromLinq
{
    internal class LinqParser
    {
        private Expression src;
        internal LinqParser(Expression linq)
        {
            src = linq;
        }
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
                    var children = new List<Entity>();
                    var method = linq as MethodCallExpression;
                    foreach (var arg in method.Arguments)
                        children.Add(InnerParse(arg));
                    var methodInfo = method.Method;
                    var name = methodInfo.Name.ToLower() + "f";
                    if (SynonimFunctions.SynFunctions.ContainsKey(name))
                        return SynonimFunctions.SynFunctions[name](children);
                    else
                    {
                        var func = new FunctionEntity(name);
                        func.Children = children;
                        return func;
                    }
                default:
                    throw new Exception("Parse error");
            }
        }
        internal Entity Parse()
        {
            return InnerParse(src);
        }
    }
}

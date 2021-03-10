using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    /// <summary>
    /// It is a storage of default constant converters that you can use
    /// </summary>
    public static class CompilationProtocolBuiltinConstantConverters
    {
        /// <summary>
        /// This treats any number as <see cref="Complex"/> and any boolean as <see cref="bool"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Expression ConverterConstant(Entity entity)
            => entity switch
            {
                Number n => Expression.Constant((Complex)n),
                Entity.Boolean b => Expression.Constant((bool)b),
                _ => throw new AngouriBugException("Undefined constant type")
            };

        private static MethodInfo GetDef2(string name)
            => typeof(Complex).GetMethod(name, new[] { typeof(Complex), typeof(Complex) });

        /// <summary>
        /// Applied to a two-argument entities, inherited from <see cref="ITwoArgumentNode"/>
        /// </summary>
        public static Expression TwoArgumentEntity(Expression left, Expression right, Entity typeHolder)
            => typeHolder switch
            {
                Sumf => Expression.Add(left, right),
                Minusf => Expression.Subtract(left, right),
                Mulf => Expression.Multiply(left, right),
                Divf => Expression.Divide(left, right),
                Powf => Expression.Call(GetDef2("Pow"), left, right),
                Logf => Expression.Call(GetDef2("Log"), left, right),
                _ => throw new AngouriBugException("A node seems to be not added")
            };
    }
}

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

        private static Type[] GenerateArrayOfType(int argCount, Type type)
        {
            var l = new List<Type>();
            for (int i = 0; i < argCount; i++)
                l.Add(type);
            return l.ToArray();
        }

        private static MethodInfo GetDef<T>(string name, int argCount)
        {
            if (typeof(T) == typeof(Complex))
                return typeof(Complex).GetMethod(name, GenerateArrayOfType(argCount, typeof(Complex)));
            else
            {
                var matched = typeof(Math).GetMethod(name, GenerateArrayOfType(argCount, typeof(T)));
                if (matched is null)
                    throw new InvalidProtocolProvided($"The given type {typeof(T)} is not supported by default. Provide a custom compilation protocol to resolve the issue");
                return matched;
            }
        }

        /// <summary>
        /// Applied to a two-argument entities, inherited from <see cref="ITwoArgumentNode"/>
        /// </summary>
        public static Func<Expression, Expression, Entity, Expression> CreateTwoArgumentEntity<T>()
            => (left, right, typeHolder) => typeHolder switch
            {
                Sumf => Expression.Add(left, right),
                Minusf => Expression.Subtract(left, right),
                Mulf => Expression.Multiply(left, right),
                Divf => Expression.Divide(left, right),
                Powf => Expression.Call(GetDef<T>("Pow", 2), left, right),
                Logf => Expression.Call(GetDef<T>("Log", 2), left, right),
                _ => throw new AngouriBugException("A node seems to be not added")
            };

        public static Func<Expression, Entity, Expression> CreateOneArgumentEntity<T>()
            => (e, typeHolder) => typeHolder switch
            {
                Sinf => Expression.Call(GetDef<T>("Sin", 1), e),
                Cosf => Expression.Call(GetDef<T>("Cos", 1), e),
                _ => throw new AngouriBugException("A node seems to be not added")
            };

        public static Func<IEnumerable<Expression>, Entity, Expression> CreateAnyArgumentEntity<T>()
            => (en, typeHolder) => typeHolder switch
            {
                // TODO: finite set -> hash set
                _ => throw new AngouriBugException("A node seems to be not added")
                //FiniteSet => Expression.
            };
    }
}

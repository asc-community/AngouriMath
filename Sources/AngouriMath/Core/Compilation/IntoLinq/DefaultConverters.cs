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
        private static object CastToT<T>(Number num)
        {
            if (typeof(T) == typeof(Complex))
                return (Complex)num;
            if (typeof(T) == typeof(double))
                return (double)num;
            if (typeof(T) == typeof(float))
                return (float)num;
            if (typeof(T) == typeof(int))
                return (int)num;
            if (typeof(T) == typeof(long))
                return (long)num;
            if (typeof(T) == typeof(BigInteger))
                return (BigInteger)num;
            throw new InvalidProtocolProvided("Undefined type, provide valid compilation protocol");
        }

        /// <summary>
        /// This treats any number as <see cref="Complex"/> and any boolean as <see cref="bool"/>
        /// </summary>
        public static Func<Entity, Expression> CreateConverterConstant<T>()
            => e => e switch
            {
                Number n => Expression.Constant(CastToT<T>(n)),
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
            => (typeof(T) == typeof(Complex)) ?
                    (typeof(Complex).GetMethod(name, GenerateArrayOfType(argCount, typeof(Complex))) 
                    ?? throw new AngouriBugException("Wrong method for Complex was chosen"))
                :
                    (typeof(Math).GetMethod(name, GenerateArrayOfType(argCount, typeof(T)))
                    ?? throw new InvalidProtocolProvided($"The given type {typeof(T)} is not supported by default. Provide a custom compilation protocol to resolve the issue"));

        /// <summary>
        /// This is a default converter for binary nodes (for those inherited from <see cref="ITwoArgumentNode"/>)
        /// </summary>
        public static Func<Expression, Expression, Entity, Expression> CreateTwoArgumentEntity<T>()
            => (left, right, typeHolder) => typeHolder switch
            {
                Sumf => Expression.Add(left, right),
                Minusf => Expression.Subtract(left, right),
                Mulf => Expression.Multiply(left, right),
                Divf => Expression.Divide(left, right),
                Powf => Expression.Call(GetDef<T>("Pow", 2), left, right),
                Logf => Expression.Divide(
                    Expression.Call(GetDef<T>("Log", 1), right),
                    Expression.Call(GetDef<T>("Log", 1), left)),

                Andf => Expression.And(left, right),
                Orf => Expression.Or(left, right),
                Xorf => Expression.ExclusiveOr(left, right),
                Impliesf => Expression.Or(Expression.Not(left), right),

                Lessf => Expression.LessThan(left, right),
                LessOrEqualf => Expression.LessThanOrEqual(left, right),
                Greaterf => Expression.GreaterThan(left, right),
                GreaterOrEqualf => Expression.GreaterThanOrEqual(left, right),
                Equalsf => Expression.Equal(left, right),

                _ => throw new AngouriBugException("A node seems to be not added")
            };

        private static Expression InvExpression<T>(Expression expr)
        {
            object? c = null;

            if (typeof(T) == typeof(Complex))
                c = new Complex(1, 0);
            if (typeof(T) == typeof(double))
                c = 1d;
            if (typeof(T) == typeof(float))
                c = 1f;
            if (typeof(T) == typeof(long))
                c = 1L;
            if (typeof(T) == typeof(int))
                c = 1;
            if (typeof(T) == typeof(BigInteger))
                c = new BigInteger(1);

            if (c is null)
                throw new InvalidProtocolProvided($"The given type {typeof(T)} is not supported by default. Provide a custom compilation protocol to resolve the issue");
            return Expression.Divide(Expression.Constant(c), expr);
        }

        /// <summary>
        /// This is a default converter for unary nodes (for those inherited from <see cref="IOneArgumentNode"/>)
        /// </summary>
        public static Func<Expression, Entity, Expression> CreateOneArgumentEntity<T>()
            => (e, typeHolder) => typeHolder switch
            {
                Sinf =>   Expression.Call(GetDef<T>("Sin", 1), e),
                Cosf =>   Expression.Call(GetDef<T>("Cos", 1), e),
                Tanf =>   Expression.Call(GetDef<T>("Tan", 1), e),
                Cotanf => InvExpression<T>(Expression.Call(GetDef<T>("Tan", 1), e)),
                Secantf => Expression.Divide(Expression.Constant(1), Expression.Call(GetDef<T>("Cos", 1))),
                Cosecantf => Expression.Divide(Expression.Constant(1), Expression.Call(GetDef<T>("Sin", 1))),

                Arcsinf =>      Expression.Call(GetDef<T>("Asin", 1), e),
                Arccosf =>      Expression.Call(GetDef<T>("Acos", 1), e),
                Arctanf =>      Expression.Call(GetDef<T>("Atan", 1), e),
                Arccotanf =>    Expression.Call(GetDef<T>("Atan", 1), InvExpression<T>(e)),
                Arcsecantf =>   Expression.Call(GetDef<T>("Acos", 1), InvExpression<T>(e)),
                Arccosecantf => Expression.Call(GetDef<T>("Asin", 1), InvExpression<T>(e)),

                Absf => Expression.Call(GetDef<T>("Abs", 1), e),
                Signumf => Expression.Divide(e, Expression.Call(GetDef<T>("Abs", 1), e)),

                _ => throw new AngouriBugException("A node seems to be not added")
            };

        /// <summary>
        /// This is a default converter for other (non-unary and non-binary) nodes
        /// </summary>
        public static Func<IEnumerable<Expression>, Entity, Expression> CreateAnyArgumentEntity<T>()
            => (en, typeHolder) => typeHolder switch
            {
                // TODO: finite set -> hash set
                _ => throw new AngouriBugException("A node seems to be not added")
                //FiniteSet => Expression.
            };
    }
}

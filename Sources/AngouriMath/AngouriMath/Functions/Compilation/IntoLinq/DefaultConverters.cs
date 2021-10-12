//
// Copyright (c) 2019-2021 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    /// <summary>
    /// It is a storage of default constant converters that you can use
    /// </summary>
    public static class CompilationProtocolBuiltinConstantConverters
    {
        private static object DownCast(Number num)
        {
            if (num is Integer)
                return (long)num;
            if (num is Real)
                return (double)num;
            if (num is Number.Complex)
                return (System.Numerics.Complex)num;
            throw new InvalidProtocolProvided("Undefined type, provide valid compilation protocol");
        }

        /// <summary>
        /// This treats any number as <see cref="Complex"/> and any boolean as <see cref="bool"/>
        /// </summary>
        public static Expression ConverterConstant(Entity e)
            => e switch
            {
                Number n => Expression.Constant(DownCast(n)),
                Entity.Boolean b => Expression.Constant((bool)b),
                _ => throw new AngouriBugException("Undefined constant type")
            };
            
        /// <summary>
        /// This converts NaN into an appropriate representation
        /// </summary>
        public static Expression NaNByType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            
            if (type == typeof(System.Numerics.Complex))
                return Expression.Constant(new System.Numerics.Complex(double.NaN, double.NaN));
            if (type == typeof(double))
                return Expression.Constant(double.NaN);
            if (type == typeof(float))
                return Expression.Constant(float.NaN);
            if (type == typeof(long))
                return Expression.Constant(null, typeof(long?));
            if (type == typeof(BigInteger))
                return Expression.Constant(null, typeof(BigInteger?));
            if (type == typeof(int))
                return Expression.Constant(null, typeof(int?));
            throw new InvalidProtocolProvided($"NaN conversion not implemented for {type}");
        }

        private static Type[] GenerateArrayOfType(int argCount, Type type)
        {
            var l = new List<Type>();
            for (int i = 0; i < argCount; i++)
                l.Add(type);
            return l.ToArray();
        }

        private static MethodInfo GetDef(string name, int argCount, Type type)
            => typeof(MathAllMethods).GetMethod(name, GenerateArrayOfType(argCount, type)) ?? throw new AngouriBugException("No null expected");


        [ConstantField] private static readonly Dictionary<Type, int> typeLevelInHierarchy = new()
            {
                { typeof(System.Numerics.Complex), 10 },
                { typeof(double), 9 },
                { typeof(float), 8 },
                { typeof(long), 8 },
                { typeof(BigInteger), 8 },
                { typeof(int), 7 }
            };
        private static Type MaxType(Type a, Type b)
        {
            if (a == b)
                return a;
            if (typeLevelInHierarchy[a] < typeLevelInHierarchy[b])
                return b;
            if (typeLevelInHierarchy[a] > typeLevelInHierarchy[b])
                return a;
            var level = typeLevelInHierarchy[a];
            var res = typeLevelInHierarchy.Where(p => p.Value == level + 1);
            if (res.Count() != 1)
                throw new AngouriBugException("Ambiguous upcast class");
            return res.First().Key;
        }
        internal static Expression Convert(Expression expr, Type type, Func<Type, Expression> nanConverter)
        {
            if (expr.Type == type)
                return expr;
                
            bool exprNullable = (Nullable.GetUnderlyingType(expr.Type) != null);
            bool typeNullable = (Nullable.GetUnderlyingType(type) != null);
            bool exprNaNisNaN = (((ConstantExpression)nanConverter(expr.Type)).Value != null);
            bool typeNaNisNaN = (((ConstantExpression)nanConverter(type)).Value != null);
            
            // ex. long? -> double
            if (exprNullable && typeNaNisNaN)
            {
                return Expression.Condition(Expression.Equal(expr, Expression.Constant(null, expr.Type)),
                                                             nanConverter(type),
                                                             Expression.Convert(expr, type));
            }
            
            // ex. double -> long?
            if (!exprNullable && exprNaNisNaN && typeNullable && !typeNaNisNaN)
            {
                MethodInfo isNaN = expr.Type.GetMethod("IsNaN") ?? throw new AngouriBugException($"IsNaN method expected for type {expr.Type}");
                
                return Expression.Condition(Expression.Call(isNaN, expr),
                                            Expression.Constant(null, type),
                                            Expression.Convert(expr, type));
            }

            return Expression.Convert(expr, type);
        }
        private static (Expression left, Expression right) EqualizeTypesIfAble(Expression left, Expression right, Func<Type, Expression> nanConverter)
        {
            if (left.Type == right.Type)
                return (left, right);
            
            Type? underlyingType = Nullable.GetUnderlyingType(left.Type);
            bool nullable = (underlyingType != null);
            Type leftType = underlyingType ?? left.Type;

            underlyingType = Nullable.GetUnderlyingType(right.Type);
            nullable = nullable || (underlyingType != null);
            Type rightType = underlyingType ?? right.Type;

            if (!typeLevelInHierarchy.ContainsKey(leftType) || !typeLevelInHierarchy.ContainsKey(rightType))
                return (left, right);

            var typeToCastTo = MaxType(leftType, rightType);
            if (nullable && ((ConstantExpression)nanConverter(typeToCastTo)).Value == null)
            {
                typeToCastTo = typeof(Nullable<>).MakeGenericType(typeToCastTo);
            }
            
            left = Convert(left, typeToCastTo, nanConverter);
            right = Convert(right, typeToCastTo, nanConverter);
            
            return (left, right);
        }

        /// <summary>
        /// This is a default converter for binary nodes (for those inherited from <see cref="IBinaryNode"/>)
        /// </summary>
        public static Expression TwoArgumentEntity(Expression left, Expression right, Entity typeHolder, Func<Type, Expression> nanConverter)
        {
            (left, right) = EqualizeTypesIfAble(left, right, nanConverter);
            return typeHolder switch
            {
                Sumf => Expression.Add(left, right),
                Minusf => Expression.Subtract(left, right),
                Mulf => Expression.Multiply(left, right),
                Divf => Expression.Divide(left, right),
                Powf => Expression.Call(GetDef("Pow", 2, left.Type), left, right),
                Logf => Expression.Call(GetDef("Log", 2, right.Type), left, right),

                Andf => Expression.And(left, right),
                Orf => Expression.Or(left, right),
                Xorf => Expression.ExclusiveOr(left, right),
                Impliesf => Expression.Or(Expression.Not(left), right),

                Lessf => Expression.LessThan(left, right),
                LessOrEqualf => Expression.LessThanOrEqual(left, right),
                Greaterf => Expression.GreaterThan(left, right),
                GreaterOrEqualf => Expression.GreaterThanOrEqual(left, right),
                Equalsf => Expression.Equal(left, right),
                
                Providedf => Expression.Condition(right, left, nanConverter(left.Type)),

                _ => throw new AngouriBugException("A binary node seems to be not added")
            };
        }

        /// <summary>
        /// This is a default converter for unary nodes (for those inherited from <see cref="IUnaryNode"/>)
        /// </summary>
        public static Expression OneArgumentEntity(Expression e, Entity typeHolder)
            => typeHolder switch
            {
                Sinf =>         Expression.Call(GetDef("Sin", 1, e.Type), e),
                Cosf =>         Expression.Call(GetDef("Cos", 1, e.Type), e),
                Tanf =>         Expression.Call(GetDef("Tan", 1, e.Type), e),
                Cotanf =>       Expression.Call(GetDef("Cot", 1, e.Type), e),
                Secantf =>      Expression.Call(GetDef("Sec", 1, e.Type), e),
                Cosecantf =>    Expression.Call(GetDef("Csc", 1, e.Type), e),

                Arcsinf =>      Expression.Call(GetDef("Asin", 1, e.Type), e),
                Arccosf =>      Expression.Call(GetDef("Acos", 1, e.Type), e),
                Arctanf =>      Expression.Call(GetDef("Atan", 1, e.Type), e),
                Arccotanf =>    Expression.Call(GetDef("Acot", 1, e.Type), e),
                Arcsecantf =>   Expression.Call(GetDef("Asec", 1, e.Type), e),
                Arccosecantf => Expression.Call(GetDef("Acsc", 1, e.Type), e),

                Absf =>         Expression.Call(GetDef("Abs", 1, e.Type), e),
                Signumf =>      Expression.Call(GetDef("Sgn", 1, e.Type), e),

                Notf =>         Expression.Not(e),

                _ => throw new AngouriBugException("An unary node seems to be not added")
            };

        /// <summary>
        /// This is a default converter for other (non-unary and non-binary) nodes
        /// </summary>
        public static Expression AnyArgumentEntity(IEnumerable<Expression> en, Entity typeHolder, Func<Type, Expression> nanConverter)
            => typeHolder switch
            {
                Piecewise => HandlePiecewise(en, nanConverter),
                // TODO: finite set -> hash set
                _ => throw new AngouriBugException("A node seems to be not added")
                //FiniteSet => Expression.
            };
            
        private static Expression HandlePiecewise(IEnumerable<Expression> en, Func<Type, Expression> nanConverter)
        {
            Expression[] children = en.ToArray();
            
            // 0 cases
            if (children.Length == 0)
                throw new UncompilableNodeException("Zero arg piecewise compilation is not supported");
            
            Type maxType = typeof(int);
            for (int i = 0; i < children.Length; i += 2)
            {
                maxType = MaxType(maxType, children[i].Type);
            }
            
            // last case
            var expression = children[children.Length - 2];
            var predicate = children[children.Length - 1];
            var nan = nanConverter(maxType);
            
            (expression, nan) = EqualizeTypesIfAble(expression, nan, nanConverter);
            
            Expression piecewiseExpr = Expression.Condition(predicate, expression, nan); // can be ConstantExpression in certain cases if values of children could be known
            
            // additional cases
            for (int i = children.Length - 3; i >= 1; i -= 2)
            {
                expression = children[i - 1];
                predicate = children[i];
                
                (expression, piecewiseExpr) = EqualizeTypesIfAble(expression, piecewiseExpr, nanConverter);
                
                piecewiseExpr = Expression.Condition(predicate, expression, piecewiseExpr);
            }
            
            return piecewiseExpr;
        }
    }
}

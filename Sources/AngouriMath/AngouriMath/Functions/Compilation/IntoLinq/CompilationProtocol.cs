//
// Copyright (c) 2019-2022 Angouri.
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
    /// This class describes all the type conversions for 
    /// </summary>
#pragma warning disable SealedOrAbstract
    public partial record CompilationProtocol
#pragma warning restore SealedOrAbstract
    {
        /// <summary>
        /// Change this method if you want a custom converter from number and boolean into the necessary type
        /// </summary>
        [Obsolete("Inherit from CompilationProtocol and override the necessary methods instead")]
        public Func<Entity, Expression>? ConstantConverter { get; init; }

        /// <summary>
        /// Change this if you want a custom conversion of NaN into an appropriate type
        /// </summary>
        [Obsolete("Inherit from CompilationProtocol and override the necessary methods instead")]
        public Func<Type, Expression>? NaNConverter { get; init; }
        
        /// <summary>
        /// Change this if you want a custom conversion between types
        /// </summary>
        [Obsolete("Inherit from CompilationProtocol and override the necessary methods instead")]
        public Func<Expression, Type, Expression>? TypeConverter { get; init; }
        
        /// <summary>
        /// Change this if you want to override compilation node for unary nodes
        /// </summary>
        [Obsolete("Inherit from CompilationProtocol and override the necessary methods instead")]
        public Func<Expression, Entity, Expression>? UnaryNodeConverter { get; init; }
        
        /// <summary>
        /// Change this if you want to override compilation node for binary nodes
        /// </summary>
        [Obsolete("Inherit from CompilationProtocol and override the necessary methods instead")]
        public Func<Expression, Expression, Entity, Expression>? BinaryNodeConverter { get; init; }

        /// <summary>
        /// Change this if you want to override compilation node for non-unary and non-binary nodes
        /// </summary>
        [Obsolete("Inherit from CompilationProtocol and override the necessary methods instead")]
        public Func<IEnumerable<Expression>, Entity, Expression>? AnyArgumentConverter { get; init; }

#pragma warning disable CS0618 // remove when converters above are removed
        /// <summary>
        /// This treats any number as <see cref="Complex"/> and any boolean as <see cref="bool"/>
        /// </summary>
        public virtual Expression ConvertConstant(Entity e)
        {
            if (ConstantConverter is not null)
                return ConstantConverter(e);
            
            return e switch
            {
                Number n => Expression.Constant(DownCast(n)),
                Entity.Boolean b => Expression.Constant((bool)b),
                _ => throw new AngouriBugException("Undefined constant type")
            };
        }
        
        /// <summary>
        /// This provides an appropriate representation of NaN using the given type
        /// </summary>
        public virtual Expression ConvertNaN(Type type)
        {
            if (NaNConverter is not null)
                return NaNConverter(type);
            
            type = Nullable.GetUnderlyingType(type) ?? type;
            
            if (type == typeof(System.Numerics.Complex))
                return Expression.Constant(new System.Numerics.Complex(double.NaN, 0));
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
            if (type == typeof(object))
                return Expression.Constant(null, typeof(object));
            throw new InvalidProtocolProvided($"NaN conversion not implemented for {type}");
        }

        /// <summary>
        /// Converts an expression to a different type
        /// </summary>
        public virtual Expression ConvertType(Expression expr, Type type)
        {
            if (TypeConverter is not null)
                return TypeConverter(expr, type);
            
            if (expr.Type == type)
                return expr;
                
            bool exprNullable = (Nullable.GetUnderlyingType(expr.Type) is not null) || (expr.Type == typeof(object));
            bool typeNullable = (Nullable.GetUnderlyingType(type) is not null) || (type == typeof(object));
            bool exprNaNisNaN = (((ConstantExpression)ConvertNaN(expr.Type)).Value is not null);
            bool typeNaNisNaN = (((ConstantExpression)ConvertNaN(type)).Value is not null);
            
            // ex. long? -> double
            if (exprNullable && typeNaNisNaN)
            {
                return Expression.Condition(Expression.Equal(expr, Expression.Constant(null, expr.Type)),
                                                             ConvertNaN(type),
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
        
        /// <summary>
        /// This is a default converter for unary nodes (for those inherited from <see cref="IUnaryNode"/>)
        /// </summary>
        public virtual Expression ConvertUnaryNode(Expression e, Entity typeHolder)
        {
            if (UnaryNodeConverter is not null)
                return UnaryNodeConverter(e, typeHolder);
            
            return typeHolder switch
            {
                Sinf      when ShouldBeAtLeastDouble(e) is var newE  => Expression.Call(GetDef("Sin", 1, newE.Type), newE),
                Cosf      when ShouldBeAtLeastDouble(e) is var newE  => Expression.Call(GetDef("Cos", 1, newE.Type), newE),
                Tanf      when ShouldBeAtLeastDouble(e) is var newE  => Expression.Call(GetDef("Tan", 1, newE.Type), newE),
                Cotanf    when ShouldBeAtLeastDouble(e) is var newE  => Expression.Call(GetDef("Cot", 1, newE.Type), newE),
                Secantf   when ShouldBeAtLeastDouble(e) is var newE  => Expression.Call(GetDef("Sec", 1, newE.Type), newE),
                Cosecantf when ShouldBeAtLeastDouble(e) is var newE  => Expression.Call(GetDef("Csc", 1, newE.Type), newE),
                
                Arcsinf      when ShouldBeAtLeastDouble(e) is var newE => Expression.Call(GetDef("Asin", 1, newE.Type), newE),
                Arccosf      when ShouldBeAtLeastDouble(e) is var newE => Expression.Call(GetDef("Acos", 1, newE.Type), newE),
                Arctanf      when ShouldBeAtLeastDouble(e) is var newE => Expression.Call(GetDef("Atan", 1, newE.Type), newE),
                Arccotanf    when ShouldBeAtLeastDouble(e) is var newE => Expression.Call(GetDef("Acot", 1, newE.Type), newE),
                Arcsecantf   when ShouldBeAtLeastDouble(e) is var newE => Expression.Call(GetDef("Asec", 1, newE.Type), newE),
                Arccosecantf when ShouldBeAtLeastDouble(e) is var newE => Expression.Call(GetDef("Acsc", 1, newE.Type), newE),
                
                Absf =>         Expression.Call(GetDef("Abs", 1, e.Type), e),
                Signumf =>      Expression.Call(GetDef("Sgn", 1, e.Type), e),
                
                Notf =>         Expression.Not(e),
                
                _ => throw new AngouriBugException("An unary node seems to be not added")
            };
        }

        private Expression ShouldBeAtLeastDouble(Expression expr)
        {
            if (typeLevelInHierarchy[expr.Type] < typeLevelInHierarchy[typeof(double)])
                return ConvertType(expr, typeof(double));
            return expr;
        }

        /// <summary>
        /// This is a default converter for binary nodes (for those inherited from <see cref="IBinaryNode"/>)
        /// </summary>
        public virtual Expression ConvertBinaryNode(Expression left, Expression right, Entity typeHolder)
        {
            if (BinaryNodeConverter is not null)
                return BinaryNodeConverter(left, right, typeHolder);
            
            (left, right) = EqualizeTypesIfAble(left, right);
            return typeHolder switch
            {
                Sumf => Expression.Add(left, right),
                Minusf => Expression.Subtract(left, right),
                Mulf => Expression.Multiply(left, right),
                Divf => Expression.Divide(ShouldBeAtLeastDouble(left), ShouldBeAtLeastDouble(right)),
                Powf when 
                    ShouldBeAtLeastDouble(left) is var newLeft 
                    && ShouldBeAtLeastDouble(right) is var newRight
                    => Expression.Call(GetDef("Pow", 2, newLeft.Type), newLeft, newRight),
                Logf when
                    ShouldBeAtLeastDouble(left) is var newLeft
                    && ShouldBeAtLeastDouble(right) is var newRight
                    => Expression.Call(GetDef("Log", 2, newRight.Type), newLeft, newRight),

                Andf => Expression.And(left, right),
                Orf => Expression.Or(left, right),
                Xorf => Expression.ExclusiveOr(left, right),
                Impliesf => Expression.Or(Expression.Not(left), right),

                Lessf => Expression.LessThan(left, right),
                LessOrEqualf => Expression.LessThanOrEqual(left, right),
                Greaterf => Expression.GreaterThan(left, right),
                GreaterOrEqualf => Expression.GreaterThanOrEqual(left, right),
                Equalsf => Expression.Equal(left, right),
                
                Providedf => HandleProvidedf(left, right),

                _ => throw new AngouriBugException("A binary node seems to be not added")
            };
            
            Expression HandleProvidedf(Expression expr, Expression cond)
            {
                var nan = ConvertNaN(expr.Type);
                (expr, nan) = EqualizeTypesIfAble(expr, nan);
                return Expression.Condition(cond, expr, nan);
            }
        }

        /// <summary>
        /// This is a default converter for other (non-unary and non-binary) nodes
        /// </summary>
        public virtual Expression ConvertOtherNode(IEnumerable<Expression> en, Entity typeHolder)
        {
            if (AnyArgumentConverter is not null)
                return AnyArgumentConverter(en, typeHolder);
            
            return typeHolder switch
            {
                Piecewise => HandlePiecewise(en),
                // TODO: finite set -> hash set
                _ => throw new AngouriBugException("A node seems to be not added")
                //FiniteSet => Expression.
            };
        }
            
        private Expression HandlePiecewise(IEnumerable<Expression> en)
        {
            Expression[] children = en.ToArray();
            
            // 0 cases
            if (children.Length == 0)
                return Expression.Constant(null, typeof(object));
            
            Type maxType = typeof(int);
            for (int i = 0; i < children.Length; i += 2)
            {
                maxType = MaxType(maxType, children[i].Type);
            }
            
            // last case
            var expression = children[^2];
            var predicate = children[^1];
            var nan = ConvertNaN(maxType);
            
            (expression, nan) = EqualizeTypesIfAble(expression, nan);
            
            Expression piecewiseExpr = Expression.Condition(predicate, expression, nan); // could be ConstantExpression in certain cases if values of children could be known
            
            // additional cases
            for (int i = children.Length - 3; i >= 1; i -= 2)
            {
                expression = children[i - 1];
                predicate = children[i];
                
                (expression, piecewiseExpr) = EqualizeTypesIfAble(expression, piecewiseExpr);
                
                piecewiseExpr = Expression.Condition(predicate, expression, piecewiseExpr);
            }
            
            return piecewiseExpr;
        }
#pragma warning restore CS0618
        
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
                { typeof(int), 7 },
                { typeof(object), 6}
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
        
        private (Expression left, Expression right) EqualizeTypesIfAble(Expression left, Expression right)
        {
            if (left.Type == right.Type)
                return (left, right);
            
            Type? underlyingType = Nullable.GetUnderlyingType(left.Type);
            bool nullable = (underlyingType is not null);
            Type leftType = underlyingType ?? left.Type;

            underlyingType = Nullable.GetUnderlyingType(right.Type);
            nullable = nullable || (underlyingType is not null);
            Type rightType = underlyingType ?? right.Type;

            if (!typeLevelInHierarchy.ContainsKey(leftType) || !typeLevelInHierarchy.ContainsKey(rightType))
                return (left, right);

            var typeToCastTo = MaxType(leftType, rightType);
            if (nullable && ((ConstantExpression)ConvertNaN(typeToCastTo)).Value == null)
            {
                typeToCastTo = typeof(Nullable<>).MakeGenericType(typeToCastTo);
            }
            
            left = ConvertType(left, typeToCastTo);
            right = ConvertType(right, typeToCastTo);
            
            return (left, right);
        }
    }
}

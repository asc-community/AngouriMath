//
// Copyright (c) 2019-2021 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq.Expressions;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    /// <summary>
    /// It is a storage of default constant converters that you can use
    /// </summary>
    public static class CompilationProtocolBuiltinConstantConverters
    {
        [ConstantField] private static CompilationProtocol DefaultProtocol = new();
        
        /// <inheritdoc cref="CompilationProtocol.ConvertConstant" />
        public static Expression ConverterConstant(Entity e) => DefaultProtocol.ConvertConstant(e);
        
        /// <inheritdoc cref="CompilationProtocol.ConvertNaN" />
        public static Expression ConverterNaN(Type type) => DefaultProtocol.ConvertNaN(type);

        /// <inheritdoc cref="CompilationProtocol.ConvertType" />
        public static Expression ConverterType(Expression expr, Type type) => DefaultProtocol.ConvertType(expr, type);

        /// <inheritdoc cref="CompilationProtocol.ConvertUnaryNode" />
        public static Expression OneArgumentEntity(Expression e, Entity typeHolder) => DefaultProtocol.ConvertUnaryNode(e, typeHolder);

        /// <inheritdoc cref="CompilationProtocol.ConvertBinaryNode" />
        public static Expression TwoArgumentEntity(Expression left, Expression right, Entity typeHolder) => DefaultProtocol.ConvertBinaryNode(left, right, typeHolder);
        
        /// <inheritdoc cref="CompilationProtocol.ConvertOtherNode" />
        public static Expression AnyArgumentEntity(IEnumerable<Expression> en, Entity typeHolder) => DefaultProtocol.ConvertOtherNode(en, typeHolder);
    }
}

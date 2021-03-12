using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    /// <summary>
    /// This class describes all the type conversions for 
    /// </summary>
    public sealed partial record CompilationProtocol
    {
        /// <summary>
        /// Change this method if you want a custom converter from number and boolean into the necessary type
        /// </summary>
        public Func<Entity, Expression> ConstantConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateConverterConstant();

        /// <summary>
        /// Change this if you want to override compilation node for binary nodes
        /// </summary>
        public Func<Expression, Expression, Entity, Expression> BinaryNodeConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateTwoArgumentEntity();

        /// <summary>
        /// Change this if you want to override compilation node for unary nodes
        /// </summary>
        public Func<Expression, Entity, Expression> UnaryNodeConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateOneArgumentEntity();

        /// <summary>
        /// Change this if you want to override compilation node for non-unary and non-binary nodes
        /// </summary>
        public Func<IEnumerable<Expression>, Entity, Expression> AnyArgumentConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateAnyArgumentEntity();

        internal static CompilationProtocol Create()
            => new()
            {
                ConstantConverter = CompilationProtocolBuiltinConstantConverters.CreateConverterConstant(),
                BinaryNodeConverter = CompilationProtocolBuiltinConstantConverters.CreateTwoArgumentEntity(),
                UnaryNodeConverter = CompilationProtocolBuiltinConstantConverters.CreateOneArgumentEntity(),
                AnyArgumentConverter = CompilationProtocolBuiltinConstantConverters.CreateAnyArgumentEntity()
            };        
    }
}

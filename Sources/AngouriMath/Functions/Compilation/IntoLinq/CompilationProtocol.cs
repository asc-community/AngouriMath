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
        public Func<Entity, Expression> ConstantConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateConverterConstant<Complex>();

        /// <summary>
        /// Change this if you want to override compilation node for binary nodes
        /// </summary>
        public Func<Expression, Expression, Entity, Expression> TwoArgumentConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateTwoArgumentEntity<Complex>();

        /// <summary>
        /// Change this if you want to override compilation node for unary nodes
        /// </summary>
        public Func<Expression, Entity, Expression> OneArgumentConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateOneArgumentEntity<Complex>();

        /// <summary>
        /// Change this if you want to override compilation node for non-unary and non-binary nodes
        /// </summary>
        public Func<IEnumerable<Expression>, Entity, Expression> AnyArgumentConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateAnyArgumentEntity<Complex>();

        [ConstantField] private static readonly HashSet<Type> numericTypes = 
            new() { typeof(Complex), typeof(double), typeof(float),
                    typeof(long), typeof(int),  typeof(BigInteger)};

        internal static CompilationProtocol Create<T>()
            => new()
            {
                ConstantConverter = CompilationProtocolBuiltinConstantConverters.CreateConverterConstant<T>(),
                TwoArgumentConverter = CompilationProtocolBuiltinConstantConverters.CreateTwoArgumentEntity<T>(),
                OneArgumentConverter = CompilationProtocolBuiltinConstantConverters.CreateOneArgumentEntity<T>(),
                AnyArgumentConverter = CompilationProtocolBuiltinConstantConverters.CreateAnyArgumentEntity<T>()
            };        
    }
}

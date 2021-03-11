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
    public sealed record CompilationProtocol
    {
        /// <summary>
        /// Change this method if you want a custom converter from number and boolean into the necessary type
        /// </summary>
        public Func<Entity, Expression> ConstantConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.ConverterConstant;

        public Func<Expression, Expression, Entity, Expression> TwoArgumentConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateTwoArgumentEntity<Complex>();

        public Func<Expression, Entity, Expression> OneArgumentConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateOneArgumentEntity<Complex>();

        public Func<IEnumerable<Expression>, Entity, Expression> AnyArgumentConverter { get; init; } = CompilationProtocolBuiltinConstantConverters.CreateAnyArgumentEntity<Complex>();

    }
}

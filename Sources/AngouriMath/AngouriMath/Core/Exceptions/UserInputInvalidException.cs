//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Compilation.IntoLinq;
using System;

namespace AngouriMath.Core.Exceptions
{
    /// <summary>If one is thrown, the user's input is invalid</summary>
    public abstract class MathSException : AngouriMathBaseException { internal MathSException(string message) : base(message) { } }

    /// <summary>Thrown inside operations related to limits</summary>
    public sealed class LimitOperationNotSupportedException : MathSException { internal LimitOperationNotSupportedException(string message) : base(message) { } }

    /// <summary>Thrown when an invalid number is passed as an argument</summary>
    public sealed class InvalidNumberException : MathSException { internal InvalidNumberException(string msg) : base(msg) { } }

    /// <summary>Quite general, is thrown when too many or too few of whatever is provided</summary>
    public sealed class WrongNumberOfArgumentsException : MathSException { internal WrongNumberOfArgumentsException(string message) : base(message) { } }

    /// <summary>Thrown when an invalid node or combination of nodes in the expression tree is encountered</summary>
    public abstract class TreeException : MathSException { internal TreeException(string message) : base(message) { } }

    /// <summary>Thrown when trying to compile and a node cannot be compiled</summary>
    public sealed class UncompilableNodeException : TreeException { internal UncompilableNodeException(string message) : base(message) { } }

    /// <summary>Is thrown when something cannot be collapsed into a single number or boolean</summary>
    public sealed class CannotEvalException : MathSException { internal CannotEvalException(string msg) : base(msg) { } }

    /// <summary> Cannot figure out whether the entity is in the set </summary>
    public sealed class ElementInSetAmbiguousException : MathSException { internal ElementInSetAmbiguousException(string msg) : base(msg) { } }

    /// <summary> Thrown if instead of a statement another expression is put into Solve </summary>
    public sealed class SolveRequiresStatementException : MathSException { internal SolveRequiresStatementException() : base("There should be statement to be true (e. g. equality, inequality, or some other predicate)") { } }

    /// <summary>
    /// Thrown in matrix operations
    /// </summary>
    public sealed class InvalidMatrixOperationException : MathSException { internal InvalidMatrixOperationException(string msg) : base(msg) { } }

    /// <summary>
    /// Occurs when an invalid numeric system was provided to functions 
    /// <see cref="MathS.ToBaseN(Entity.Number.Real, int)"/> or <see cref="MathS.FromBaseN(string, int)"/>
    /// </summary>
    public sealed class InvalidNumericSystemException : MathSException { internal InvalidNumericSystemException(string msg) : base(msg) { } }

    /// <summary>Thrown when invalid cast encountered</summary>
    public sealed class NumberCastException : MathSException
    { 
        internal NumberCastException(Type expected, Type actual)
            : base($"Cannot cast from {actual} to {expected}") { }
    }

    /// <summary>
    /// Occurs when no custom compilation protocol was provided, while the built-in one
    /// does not have the given type defined for the given operator. To resolve it,
    /// create your own <see cref="CompilationProtocol"/>.
    /// </summary>
    public sealed class InvalidProtocolProvided : MathSException
    {
        internal InvalidProtocolProvided(string msg) : base(msg) { }
    }

    /// <summary>
    /// Happens when matrices don't have a correct shape, e. g.
    /// trying to concat when they have different sizes by one side.
    /// </summary>
    public sealed class BadMatrixShapeException : MathSException
    {
        internal BadMatrixShapeException(string msg) : base(msg) { }
    }
}

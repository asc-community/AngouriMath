using System;

namespace AngouriMath.Core.Exceptions
{
    /// <summary>If one is thrown, the user's input is invalid</summary>
    public class MathSException : ArgumentException { internal MathSException(string message) : base(message) { } }

    /// <summary>Thrown when an invalid node or combination of nodes in the expression tree is encountered</summary>
    public class TreeException : MathSException { internal TreeException(string message) : base(message) { } }

    /// <summary>Thrown when trying to compile and a node cannot be compiled</summary>
    public sealed class UncompilableNodeException : TreeException { internal UncompilableNodeException(string message) : base(message) { } }

    /// <summary>Is thrown when something cannot be collapsed into a single number or boolean</summary>
    public sealed class CannotEvalException : MathSException { internal CannotEvalException(string msg) : base(msg) { } }

    /// <summary> Cannot figure out whether the entity is in the set </summary>
    public sealed class ElementInSetAmbiguousException : MathSException { internal ElementInSetAmbiguousException(string msg) : base(msg) { } }

    /// <summary> Thrown if instead of a statement another expression is put into Solve </summary>
    public sealed class SolveRequiresStatement : MathSException { internal SolveRequiresStatement() : base("There should be statement to be true (e. g. equality, inequality, or some other predicate)") { } }
}

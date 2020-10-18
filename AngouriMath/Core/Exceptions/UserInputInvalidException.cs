using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Exceptions
{
    /// <summary>If one is thrown, the user's input is invalid</summary>
    public class MathSException : ArgumentException { public MathSException(string message) : base(message) { } }

    /// <summary>Thrown when an invalid node or combination of nodes in the expression tree is encountered</summary>
    public class TreeException : MathSException { public TreeException(string message) : base(message) { } }

    /// <summary>Thrown when trying to compile and a node cannot be compiled</summary>
    public sealed class UncompilableNodeException : TreeException { public UncompilableNodeException(string message) : base(message) { } }

    /// <summary>Is thrown when something cannot be collapsed into a single number or boolean</summary>
    public sealed class CannotEvalException : MathSException { public CannotEvalException(string msg) : base(msg) { } }

    /// <summary> Cannot figure out whether the entity is in the set </summary>
    public sealed class ElementInSetAmbiguousException : MathSException { public ElementInSetAmbiguousException(string msg) : base(msg) { } }
}

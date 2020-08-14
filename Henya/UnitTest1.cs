using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
namespace System.Runtime.CompilerServices { class IsExternalInit { } }
public class Henya
{
    public abstract partial record Entity
    {

        public static bool operator ==(Entity? a, Entity? b)
        {
            if (a is null && b is null)
                return true;
            return a?.Equals(b) ?? false;
        }
        public static bool operator !=(Entity? a, Entity? b) => !(a == b);

        public Entity Evaled => _evaledValue ??= InnerEval();
        Entity? _evaledValue;
        protected abstract Entity InnerEval();


        public partial record Variable(string Name) : Entity
        {
            protected override Entity InnerEval() => ConstantList.TryGetValue(this, out var value) ? value : this;
            internal static readonly IReadOnlyDictionary<Variable, Variable> ConstantList =
                new Dictionary<Variable, Variable> { { new("pi"), new("pi2") }, { new("e"), new("e2") } };
            public bool IsConstant => ConstantList.ContainsKey(this);
        }
    }
        [Fact]
    public void a()
    {
        var a = new Entity.Variable("x");
        var b = a.Evaled;
        var c = a.IsConstant;
    }
}

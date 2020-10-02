using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Core
{
    // TODO: is it useful for the user to have this class?
    /// <summary>
    /// Use this class for solvers and other places when a set needs to be built
    /// and you want to avoid using lists
    /// </summary>
    internal sealed class FiniteSetBuilder
    {
        private readonly List<Entity> raw = new();

        public FiniteSetBuilder() { }

        public void Add(Entity element)
            => raw.Add(element);

        public FiniteSet ToFiniteSet()
            => raw.Count == 0 ? Empty : MathS.Sets.Finite(raw);
    }
}

using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static AngouriMath.Entity;

namespace AngouriMath.Core
{
    internal sealed class RecordFieldCache
    {
        internal sealed class EntityCache
        {
            internal Entity[]? directChildren;
            internal int? complexity;
            internal bool? isFinite;
            internal int? simplifiedRate;
            internal Entity? innerSimplified;
            internal Entity? innerEvaled;
            internal HashSet<Variable>? vars;
        }

        private readonly ConditionalWeakTable<Entity, EntityCache> caches = new();

        [return: NotNull]
        internal T GetValue<T>(Entity refer, Func<EntityCache, T> getter, Func<EntityCache, T> setter)
        {
            var rec = caches.GetValue(refer, e => new EntityCache());
            var got = getter(rec);
            if (got is null)
                got = setter(rec);
            return got ?? throw new AngouriBugException("Setter cannot return null");
        }
    }
}

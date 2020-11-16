/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
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
            internal double? simplifiedRate;
            internal Entity? innerSimplified;
            internal Entity? innerEvaled;
            internal HashSet<Variable>? vars;
            internal bool? isSetFinite;
            internal bool? isSetEmpty;
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


/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */



﻿using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AngouriMath
{
    /// <summary>
    /// Custom view for list of entities
    /// </summary>
    public class EntitySet : List<Entity>
    {
        public EntitySet Simplify() => Simplify(3);
        public EntitySet Simplify(int level) => new EntitySet(this.Select(p => p.Simplify(level)));
        private readonly HashSet<string> exsts = new HashSet<string>();

        /// <summary>
        /// Neat output
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + string.Join(", ", this) + "]";
        }
        public new void Add(Entity ent) => Add(ent, true);
        public void Add(Entity ent, bool check)
        {
            if (!check)
            {
                base.Add(ent);
                return;
            }
            if (ent == null)
                return;
            if (ent.entType == Entity.EntType.NUMBER && ent.GetValue().IsNull)
                return;
            ent = MathS.CanBeEvaluated(ent) ? ent.InnerSimplify() : ent;
            var hash = ent.ToString();
            if (!exsts.Contains(hash))
            {
                base.Add(ent);
                exsts.Add(hash);
            }
        }
        public void Merge(IEnumerable<Number> list)
        {
            foreach (var l in list)
                Add(l);
        }
        public void Merge(IEnumerable<Entity> list)
        {
            foreach (var l in list)
                Add(l);
        }
        public EntitySet(params Entity[] entites)
        {
            foreach (var el in entites)
                Add(el);
        }
        public EntitySet(bool check, params Entity[] entities)
        {
            foreach (var el in entities)
                Add(el, check);
        }
        public EntitySet(IEnumerable<Entity> list)
        {
            foreach (var el in list)
                Add(el);
        }
        // TODO: needs optimization
        public static EntitySet operator +(EntitySet set, Entity a) => new EntitySet(set.Select(el => el + a));
        public static EntitySet operator -(EntitySet set, Entity a) => new EntitySet(set.Select(el => el - a));
        public static EntitySet operator *(EntitySet set, Entity a) => new EntitySet(set.Select(el => el * a));
        public static EntitySet operator /(EntitySet set, Entity a) => new EntitySet(set.Select(el => el / a));
        public static EntitySet operator +(Entity a, EntitySet set) => new EntitySet(set.Select(el => a + el));
        public static EntitySet operator -(Entity a, EntitySet set) => new EntitySet(set.Select(el => a - el));
        public static EntitySet operator *(Entity a, EntitySet set) => new EntitySet(set.Select(el => a * el));
        public static EntitySet operator /(Entity a, EntitySet set) => new EntitySet(set.Select(el => a / el));
        public EntitySet Log(Entity ba) => new EntitySet(this.Select(el => el.Log(ba)));
        public EntitySet Ln() => this.Log(MathS.e);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Functions
{
    using static Entity;
    partial class TreeAnalyzer
    {
        private static Entity UnpackProvided(Entity entity)
            => entity is DefinedWhen definedWhen ? definedWhen.Expression : entity;

        private static Entity Unpack1(this Entity entity)
            => UnpackProvided(entity);

        internal static Entity Unpack1Simplify(this Entity entity)
            => Unpack1(entity).InnerSimplified;

        internal static Entity Unpack1Eval(this Entity entity)
            => Unpack1(entity).Evaled;

        private static (Entity, Entity) Unpack2(this (Entity, Entity) entity)
            => (UnpackProvided(entity.Item1), UnpackProvided(entity.Item2));

        internal static (Entity, Entity) Unpack2Simplify(this (Entity, Entity) entity)
        {
            var (item1, item2) = entity.Unpack2();
            return (item1.InnerSimplified, item2.InnerSimplified);
        }

        internal static (Entity, Entity) Unpack2Eval(this (Entity, Entity) entity)
        {
            var (item1, item2) = entity.Unpack2();
            return (item1.Evaled, item2.Evaled);
        }
    }
}

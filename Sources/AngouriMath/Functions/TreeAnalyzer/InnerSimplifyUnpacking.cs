using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Functions
{
    using static Entity;
    partial class TreeAnalyzer
    {
        internal static Entity UnpackProvided(Entity entity)
            => entity is DefinedWhen definedWhen ? definedWhen.Expression : entity;

        internal static Entity Unpack1(this Entity entity)
            => UnpackProvided(entity);

        internal static Entity Unpack1Simplify(this Entity entity)
            => Unpack1(entity).InnerSimplified;

        internal static Entity Unpack1Eval(this Entity entity)
            => Unpack1(entity).Evaled;

        internal static (Entity, Entity) Unpack2(this (Entity, Entity) entity)
            => (UnpackProvided(entity.Item1), UnpackProvided(entity.Item2));

        internal static (Entity, Entity) Unpack1Simplify(this (Entity, Entity) entity)
        {
            var (item1, item2) = 
        }

        internal static (Entity, Entity) Unpack1Eval(this Entity entity)
            => Unpack1(entity).Evaled;
    }
}

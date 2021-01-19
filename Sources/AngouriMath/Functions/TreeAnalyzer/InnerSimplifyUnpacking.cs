using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Functions
{
    using static Entity;
    partial class TreeAnalyzer
    {
        private static Entity UnpackProvided(Entity entity)
            => entity is Providedf definedWhen ? definedWhen.Expression : entity;

        internal static Entity Unpack1(this Entity entity)
            => UnpackProvided(entity);

        internal static Entity Unpack1Simplify(this Entity entity)
            => Unpack1(entity.InnerSimplified);

        internal static Entity Unpack1Eval(this Entity entity)
            => Unpack1(entity.Evaled);

        private static (Entity, Entity) Unpack2(this (Entity, Entity) entity)
            => (UnpackProvided(entity.Item1), UnpackProvided(entity.Item2));

        internal static (Entity, Entity) Unpack2Simplify(this (Entity, Entity) entity)
            => (entity.Item1.Unpack1Simplify(), entity.Item2.Unpack1Simplify());        

        internal static (Entity, Entity) Unpack2Eval(this (Entity, Entity) entity)
            => (entity.Item1.Unpack1Eval(), entity.Item2.Unpack1Eval());

        internal static (Entity, Entity, T) Unpack3SimplifyT<T>(this (Entity, Entity, T) entity)
            => (entity.Item1.Unpack1Simplify(), entity.Item2.Unpack1Simplify(), entity.Item3);

        internal static (Entity, Entity, T) Unpack3EvalT<T>(this (Entity, Entity, T) entity)
        {
            var (item1, item2) = (entity.Item1, entity.Item2).Unpack2Eval();
            return (item1.Evaled, item2.Evaled, entity.Item3);
        }
    }
}

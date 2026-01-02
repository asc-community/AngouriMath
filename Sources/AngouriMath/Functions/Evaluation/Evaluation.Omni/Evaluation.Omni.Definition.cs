//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Sets;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary><see cref="MathS.Apply"/></summary>
        public Entity Apply(params Entity[] args) => new Application(this, LList.Of(args));
        /// <summary><see cref="MathS.Apply"/></summary>
        public Entity Apply(LList<Entity> args) => new Application(this, args);
        /// <summary><see cref="MathS.Lambda"/></summary>
        public Entity LambdaOver(Variable param)
            => new Lambda(param, this);
    }
}
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
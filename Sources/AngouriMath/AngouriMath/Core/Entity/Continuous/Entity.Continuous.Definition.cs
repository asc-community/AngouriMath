//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Any node that might be counted as a number, derived, etc. is continuous
        /// </summary>
        public abstract partial record ContinuousNode : Entity
        {

        }

        /// <summary>
        /// Describes any function that is related to trigonometry
        /// </summary>
        public abstract record TrigonometricFunction : Function
        {

        }

        /// <summary>
        /// Hangs two nodes to a Sum node (i. e. building an expression)
        /// </summary>
        /// <param name="augend">The left node to add</param>
        /// <param name="addend">The right node to add</param>
        public static Entity operator +(Entity augend, Entity addend) => new Sumf(augend, addend);

        /// <summary>
        /// Does not do anything until an unary plus node added
        /// </summary>
        public static Entity operator +(Entity a) => a;

        /// <summary>
        /// Hangs two nodes to a Minus node (i. e. building an expression)
        /// </summary>
        /// <param name="subtrahend">The left node to add</param>
        /// <param name="minuend">The right node to add</param>
        public static Entity operator -(Entity subtrahend, Entity minuend) => new Minusf(subtrahend, minuend);

        /// <summary>
        /// Multiplies the only argument by -1
        /// </summary>
        /// <returns>Mul node of -1 and the only argument</returns>
        public static Entity operator -(Entity a) => new Mulf(-1, a);

        /// <summary>
        /// Hangs two nodes to a Mul node (i. e. building an expression)
        /// </summary>
        /// <param name="multiplier">The left node to add</param>
        /// <param name="multiplicand">The right node to add</param>
        public static Entity operator *(Entity multiplier, Entity multiplicand) => new Mulf(multiplier, multiplicand);

        /// <summary>
        /// Hangs two nodes to a Div node (i. e. building an expression)
        /// </summary>
        /// <param name="dividend">The left node to add</param>
        /// <param name="divisor">The right node to add</param>
        public static Entity operator /(Entity dividend, Entity divisor) => new Divf(dividend, divisor);

        /// <summary><see cref="MathS.Sin(Entity)"/></summary>
        public Entity Sin() => new Sinf(this);
        /// <summary><see cref="MathS.Cos(Entity)"/></summary>
        public Entity Cos() => new Cosf(this);
        /// <summary><see cref="MathS.Tan(Entity)"/></summary>
        public Entity Tan() => new Tanf(this);
        /// <summary><see cref="MathS.Cotan(Entity)"/></summary>
        public Entity Cotan() => new Cotanf(this);
        /// <summary><see cref="MathS.Sec(Entity)"/></summary>
        public Entity Sec() => new Secantf(this);
        /// <summary><see cref="MathS.Cosec(Entity)"/></summary>
        public Entity Cosec() => new Cosecantf(this);
        /// <summary><see cref="MathS.Arcsec(Entity)"/></summary>
        public Entity Arcsec() => new Arcsecantf(this);
        /// <summary><see cref="MathS.Arccosec(Entity)"/></summary>
        public Entity Arccosec() => new Arccosecantf(this);
        /// <summary><see cref="MathS.Pow(Entity, Entity)"/></summary>
        /// <param name="n">Power</param>
        public Entity Pow(Entity n) => new Powf(this, n);
        /// <summary><see cref="MathS.Log(Entity, Entity)"/></summary>
        /// <param name="x">Antilogarithm (value)</param>
        public Entity Log(Entity x) => new Logf(this, x);
        /// <summary><see cref="MathS.Arcsin(Entity)"/></summary>
        public Entity Arcsin() => new Arcsinf(this);
        /// <summary><see cref="MathS.Arccos(Entity)"/></summary>
        public Entity Arccos() => new Arccosf(this);
        /// <summary><see cref="MathS.Arctan(Entity)"/></summary>
        public Entity Arctan() => new Arctanf(this);
        /// <summary><see cref="MathS.Arccotan(Entity)"/></summary>
        public Entity Arccotan() => new Arccotanf(this);
        /// <summary><see cref="MathS.Factorial(Entity)"/></summary>
        public Entity Factorial() => new Factorialf(this);
        /// <summary><see cref="MathS.Signum(Entity)"/></summary>
        public Entity Signum() => new Signumf(this);
        /// <summary><see cref="MathS.Abs(Entity)"/></summary>
        public Entity Abs() => new Absf(this);

        /// <summary>
        /// Describes any node that is a function (e. g. sin, cos, etc.)
        /// but not an operator or leaf
        /// </summary>
        public abstract record Function : ContinuousNode
        {
            internal override Priority Priority => Priority.Func;
        }
    }
}

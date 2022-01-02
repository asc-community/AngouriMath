//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    partial record Entity : ILatexiseable
    {
        /// <summary><para>This <see cref="Entity"/> MUST contain exactly ONE occurance of <paramref name="x"/>,
        /// otherwise this function won't work correctly.</para>
        /// 
        /// This function inverts an expression and returns a <see cref="Set"/>. Here, a represents <paramref name="value"/>.
        /// <list type="table">
        /// <item>x^2 = a ⇒ x = { sqrt(a), -sqrt(a) }</item>
        /// <item>sin(x) = a ⇒ x = { arcsin(a) + 2 pi n, pi - arcsin(a) + 2 pi n }</item>
        /// </list>
        /// </summary>
        /// <returns>A set of possible roots of the expression.</returns>
        internal IEnumerable<Entity> Invert(Entity value, Entity x)
        {
            if (value.InnerSimplified is var simplified && this == x)
                return new[] { simplified };
            else
                return InvertNode(simplified, x).Where(el => el.IsFinite);
        }
        /// <summary>Use <see cref="Invert(Entity, Entity)"/> instead which auto-simplifies <paramref name="value"/></summary>
        private protected abstract IEnumerable<Entity> InvertNode(Entity value, Entity x);
        /// <summary>
        /// Returns true if <paramref name="a"/> is inside a rect with corners <paramref name="from"/>
        /// and <paramref name="to"/>, OR <paramref name="a"/> is an unevaluable expression
        /// </summary>        
        private protected static bool EntityInBounds(Entity a, Complex from, Complex to)
            => a.Evaled is not Complex r ||
                   r.RealPart >= from.RealPart &&
                   r.ImaginaryPart >= from.ImaginaryPart &&
                   r.RealPart <= to.RealPart &&
                   r.ImaginaryPart <= to.ImaginaryPart;
    }
}

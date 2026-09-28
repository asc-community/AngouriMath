//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;

namespace AngouriMath
{
    partial record Entity : ILatexizeable
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
        /// <returns>
        /// A set of possible roots of the expression, or <see langword="null"/> where the
        /// preimage has no written form, which is not the claim that there are no roots.
        /// </returns>
        internal IEnumerable<Entity>? Invert(Entity value, Entity x)
        {
            if (value.InnerSimplified is var simplified && this == x)
                return new[] { simplified };
            else
                return InvertNode(simplified, x)?.Where(el => el.IsFinite);
        }
        /// <summary>
        /// The preimages of each of <paramref name="values"/>, united, or <see langword="null"/>
        /// where any of them has no written form.
        /// </summary>
        internal Set? InvertEach(IEnumerable<Entity> values, Entity x)
        {
            var preimages = new List<Set>();
            foreach (var value in values)
                if (Invert(value, x) is { } roots)
                    preimages.Add(roots.ToSet());
                else
                    return null;
            return (Set)preimages.Unite().InnerSimplified;
        }
        /// <summary>Use <see cref="Invert(Entity, Entity)"/> instead which auto-simplifies <paramref name="value"/></summary>
        /// <remarks>
        /// No roots means the equation has none. A node that cannot write the preimage it is
        /// asked for returns <see langword="null"/> instead, a node that inverts a child passes
        /// the child's <see langword="null"/> on, and the solver answers the equation as
        /// unsolved. <c>x! = 6</c> has the root 3, and no node here inverts a factorial, so
        /// returning no roots answered <c>{ }</c>: a claim that it has none.
        /// </remarks>
        private protected abstract IEnumerable<Entity>? InvertNode(Entity value, Entity x);

        /// <summary>
        /// The preimages of several inversions together, or <see langword="null"/> where any of
        /// them has no written form: a part of the preimage is not the preimage. The parts are
        /// taken one at a time and the first <see langword="null"/> ends it, so that the ones
        /// after it are never computed.
        /// </summary>
        private protected static IEnumerable<Entity>? Together(IEnumerable<IEnumerable<Entity>?> parts)
        {
            var written = new List<IEnumerable<Entity>>();
            foreach (var part in parts)
                if (part is null)
                    return null;
                else
                    written.Add(part);
            return written.SelectMany(part => part);
        }
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

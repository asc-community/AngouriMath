//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity.Set;

namespace AngouriMath.Core
{
    /// <summary>
    /// The codomain an <see cref="Entity"/> node is read over: the values it is allowed to take
    /// before it is <see cref="MathS.NaN"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A member of this enum is a <b>constraint on a node</b>, not a mathematical object. Five of
    /// them also name a set — <see cref="Entity.Set.SpecialSet.Create(Domain)"/> maps those five
    /// to <c>BB</c>, <c>ZZ</c>, <c>QQ</c>, <c>RR</c> and <c>CC</c> — and <see cref="Any"/> does not,
    /// because "no constraint" is not a collection of values. Sets are
    /// <see cref="Entity.Set"/>s and are reasoned about with membership, union and difference;
    /// a domain is not, and the two are deliberately different types.
    /// </para>
    /// <para>
    /// The members are ordered from narrowest to widest and are compared as such — evaluation
    /// asks <c>Codomain &lt; Domain.Complex</c> to mean "narrower than the complex plane", and
    /// <see cref="Entity.DomainConditionIn(Domain)"/> narrows a node whose codomain is
    /// <i>wider</i> than the reading it is asked in. <see cref="Any"/> is the top of that order,
    /// which is what makes it the identity for narrowing rather than a set of everything.
    /// </para>
    /// <para>
    /// Distinct from <see cref="MathS.Settings.Codomain"/>, which is the <b>ambient</b> reading a
    /// question is asked in and applies to nodes that do not constrain themselves. A node's own
    /// codomain says what that node is declared over; the ambient one says what the library
    /// should take an unconstrained node to be.
    /// </para>
    /// </remarks>
    public enum Domain
    {
        /// <summary>
        /// The domain of all boolean values (true, false)
        /// </summary>
        Boolean,

        /// <summary>
        /// The domain of all integer values
        /// </summary>
        Integer,

        /// <summary>
        /// The domain of all rational values
        /// </summary>
        Rational,

        /// <summary>
        /// The domain of all real values
        /// </summary>
        Real,

        /// <summary>
        /// The domain of all complex values
        /// </summary>
        Complex,

        /// <summary>
        /// No constraint: this node is not declared over any narrower codomain, so whatever
        /// reading the question is asked in applies to it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Not a set, and not the universal set.</b> It is the widest member of the order
        /// above and the one <see cref="Entity.Set.SpecialSet.Create(Domain)"/> has no set for,
        /// on purpose: it says that a node imposes no restriction, which is a statement about the
        /// node rather than a claim that some collection contains every value. Nothing may read
        /// it as "the set of everything" — the solution set of an equation is never this, and a
        /// code path that needs a set has to name one.
        /// </para>
        /// <para>
        /// A mathematical set that constrains nothing is already expressible, as a set-builder
        /// whose predicate holds everywhere: <c>{ x : True }</c> parses, prints, round-trips,
        /// compares and answers membership. That is why there is no <c>AA</c>/<c>UU</c>
        /// <see cref="Entity.Set.SpecialSet"/> and why this member is staying.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/996">#996</a>
        /// </para>
        /// <para>
        /// It is the default codomain of a <see cref="Entity.Variable"/>, a
        /// <see cref="Entity.Matrix"/>, a <see cref="Entity.Set.ConditionalSet"/>, a
        /// <see cref="Entity.Lambda"/> and the set operators — every node whose value is not
        /// confined to numbers. It is written as <c>domain(x, Any)</c>, which is a keyword in
        /// that one position and not a set literal.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1048">#1048</a>
        /// </para>
        /// </remarks>
        Any
    }

    internal static class DomainsFunctional
    {
        /// <summary>
        /// Whether a value is not ruled out by a codomain.
        /// </summary>
        /// <remarks>
        /// <see cref="Domain.Any"/> is answered here rather than by asking for its set, because
        /// it has none: a node that constrains nothing rules nothing out, which is a shortcut
        /// past <see cref="Entity.Set.SpecialSet.Create(Domain)"/> and not a membership test
        /// against a universal set.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/996">#996</a>
        /// </remarks>
        public static bool FitsDomainOrNonNumeric(Entity entity, Domain domain)
            => domain == Domain.Any || SpecialSet.Create(domain).MayContain(entity);

    }
}

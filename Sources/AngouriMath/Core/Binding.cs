//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core
{
    using static Entity;
    using static Entity.Number;

    /// <summary>
    /// The name a binder is handed, and what that name means inside it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is one name in the language a binder cannot otherwise bind. <c>i</c> is the
    /// imaginary unit and that is decided in the lexer — <c>NUMBER: ... | 'i'</c> — so it never
    /// reaches the rule that makes variables and cannot be one anywhere. <c>e</c> and <c>pi</c>
    /// are the other way round: they are <see cref="Variable"/>s that carry a value, so a binder
    /// takes the name without trouble — and cannot then stop it meaning the constant, since a
    /// bound <c>e</c> and the constant <c>e</c> are one object. That is why this is about the one
    /// named constant that is not a variable.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/976">#976</a>,
    /// <a href="https://github.com/asc-community/AngouriMath/issues/984">#984</a>
    /// </para>
    /// <para>
    /// Naming <c>i</c> as the bound name says something about the whole binder, so it is
    /// honoured throughout it — through the summand and the bounds, not only in the name
    /// position. Doing it in the name position alone would be worse than not doing it at all:
    /// the index would become a variable while every <c>i</c> beside it stayed the imaginary
    /// unit, nothing would substitute, and <c>sum(i, i, 1, 10)</c> would answer <c>10i</c>
    /// instead of 55 — a wrong answer in place of an unevaluated one.
    /// </para>
    /// <para>
    /// Only inside the binder that declares it. <c>sum(i * k, k, 1, 3)</c> is <c>6i</c>, and the
    /// <c>i</c> outside the sum in <c>sum(i, i, 1, 3) + i</c> is still the imaginary unit.
    /// </para>
    /// <para>
    /// This is a <see langword="struct"/> holding two fields and it is the whole cost of the
    /// feature where nothing is shadowed: <see cref="Of(Entity)"/> is one equality test and
    /// <see cref="In(Entity)"/> hands the expression straight back without walking it.
    /// </para>
    /// </remarks>
    internal readonly struct Binding
    {
        /// <summary>
        /// What <c>i</c> becomes where it is bound. Unchecked, because the checked factory
        /// parses and the parser reads <c>i</c> as the imaginary unit — the very thing that
        /// makes this type necessary.
        /// </summary>
        [ConstantField]
        private static readonly Variable index = Variable.CreateVariableUnchecked("i");

        private readonly Entity given;
        private readonly bool shadowed;

        private Binding(Entity given, bool shadowed) => (this.given, this.shadowed) = (given, shadowed);

        /// <summary>Reads a bound name as the binder was given it.</summary>
        /// <param name="name">Whatever arrived in the name position.</param>
        /// <remarks>
        /// Every binder node runs this on the way in, so it is written to cost nothing in the
        /// case that is nearly all of them: a bound name is a <see cref="Variable"/>, the type
        /// test fails, and the equality is never reached. Measured at 2.2ns as an equality and
        /// 0.5ns this way, against about 10ns to construct the node it guards.
        /// </remarks>
        internal static Binding Of(Entity name) => new(name, name is Complex && name == MathS.i);

        /// <summary>The name to bind: what was given, unless that was the imaginary unit.</summary>
        internal Entity Name => shadowed ? index : given;

        /// <summary>
        /// An expression that is inside this binder, with the bound name meaning what it means
        /// here. Identity unless something is shadowed.
        /// </summary>
        internal Entity In(Entity scope) => shadowed ? scope.Replace(Rename) : scope;

        /// <remarks>
        /// <c>2i</c> is one token — the lexer's <c>NUMBER</c> ends <c>'i'?</c> — so a written
        /// <c>2i</c> arrives as a single number and not as a product with anything to rename.
        /// Under a binder that names <c>i</c> it is nevertheless the writer's <c>2</c> beside
        /// the writer's <c>i</c>, and <c>2e</c> is a product there, so this reads it as one:
        /// <c>sum(2i, i, 1, 3)</c> is 12 rather than <c>6i</c>.
        /// </remarks>
        private static Entity Rename(Entity node)
        {
            // Real derives from Complex, so the second test is the one that says "and it has an
            // imaginary part". NaN and the infinities are Reals and so are left alone.
            if (node is not Complex complex || node is Real)
                return node;
            Entity scaled = complex.ImaginaryPart == Integer.One ? index : complex.ImaginaryPart * index;
            return complex.RealPart.IsZero ? scaled : complex.RealPart + scaled;
        }
    }
}
